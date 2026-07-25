using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheFall.Application;
using TheFall.Domain;

namespace TheFall.Presentation.Animation
{
    /// <summary>
    /// Plays already-accepted first-playable resolution records in authoritative event order.
    /// It owns presentation time only; it never submits an intent or invokes rule resolution.
    /// </summary>
    public sealed class FirstPlayableAnimationPlayer
    {
        private sealed class Transition
        {
            public Transition(
                MatchState initialState,
                MatchState finalState,
                IReadOnlyList<DomainEvent> events)
            {
                InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
                FinalState = finalState ?? throw new ArgumentNullException(nameof(finalState));
                Events = events ?? throw new ArgumentNullException(nameof(events));
            }

            public MatchState InitialState { get; }

            public MatchState FinalState { get; }

            public IReadOnlyList<DomainEvent> Events { get; }
        }

        private readonly AnimationSequenceConfiguration _configuration;
        private readonly List<Transition> _pending = new List<Transition>();
        private readonly List<ResolvedAnimationStepKind> _presentedSteps =
            new List<ResolvedAnimationStepKind>();
        private ResolvedAnimationSequence _sequence;
        private AnimationSequenceTransport _transport;
        private Transition _current;
        private int _pendingIndex;
        private int _lastRenderedStep = -1;
        private bool _lastRenderedDelay;
        private bool _currentStepApplied;
        private int _lastRegisteredStep = -1;
        private MatchState _acceptedFinalState;
        private AnimationPresentationState _transitionInitialRenderedState;
        private long _cpuTicks;
        private long _peakTickTicks;

        public FirstPlayableAnimationPlayer(AnimationSequenceConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _configuration.EnsureDefaults();
        }

        public AnimationPresentationState RenderedState { get; private set; }

        public MatchState RenderedReferenceState { get; private set; }

        public ResolvedAnimationSequence Sequence => _sequence;

        public ResolvedAnimationStep ActiveStep => _sequence == null || _transport == null
            || _transport.Position.StepIndex >= _sequence.Steps.Count - 1
                ? null
                : _sequence.Steps[_transport.Position.StepIndex];

        public float ActiveStepProgress => _transport?.Position.Progress ?? 0f;

        public bool IsDelayingActiveStep => _transport?.Position.IsDelaying ?? false;

        public bool IsBusy { get; private set; }

        public bool FastForward { get; private set; }

        public bool ReducedMotion { get; private set; }

        public AnimationSequenceCompletionReason CompletionReason { get; private set; }

        public int VisualRevision { get; private set; }

        public int FrameCount { get; private set; }

        public double CpuMilliseconds => TicksToMilliseconds(_cpuTicks);

        public double PeakTickCpuMilliseconds => TicksToMilliseconds(_peakTickTicks);

        public IReadOnlyList<ResolvedAnimationStepKind> PresentedSteps => _presentedSteps;

        public bool IsRenderedStateSynchronized => !IsBusy
            && RenderedState != null
            && _acceptedFinalState != null
            && RenderedState.IsSynchronizedWith(_acceptedFinalState);

        public void PlayInitialTrace(MatchTrace trace)
        {
            if (trace == null)
            {
                throw new ArgumentNullException(nameof(trace));
            }

            _presentedSteps.Clear();
            FrameCount = 0;
            _cpuTicks = 0;
            _peakTickTicks = 0;
            var transitions = new List<Transition>();
            if (trace.StartupEvents.Count > 0)
            {
                transitions.Add(new Transition(trace.InitialState, trace.InitialState, trace.StartupEvents));
            }

            foreach (var record in trace.IntentHistory)
            {
                if (record.IsAccepted)
                {
                    transitions.Add(new Transition(record.PriorState, record.ResultingState, record.Events));
                }
            }

            Begin(transitions, trace.FinalState);
        }

        public void PlayAdvance(MatchAdvanceResult advance)
        {
            if (advance == null)
            {
                throw new ArgumentNullException(nameof(advance));
            }

            var transitions = new List<Transition>();
            foreach (var record in advance.Resolutions)
            {
                if (record.IsAccepted)
                {
                    transitions.Add(new Transition(record.PriorState, record.ResultingState, record.Events));
                }
            }

            var finalState = transitions.Count == 0
                ? advance.HumanResult.State
                : transitions[transitions.Count - 1].FinalState;
            Begin(transitions, finalState);
        }

        public void Tick(float unscaledDeltaSeconds)
        {
            if (!IsBusy || _transport == null)
            {
                return;
            }

            var startedAt = Stopwatch.GetTimestamp();
            FrameCount++;
            var deltaSeconds = Math.Max(0f, unscaledDeltaSeconds);
            var positionBeforeTick = _transport.Position;
            if (positionBeforeTick.StepIndex < _sequence.Steps.Count - 1
                && _transport.PlaybackSpeed > 0f)
            {
                var remainingPresentationSeconds = Math.Max(
                    0f,
                    _transport.GetStepEndSeconds(positionBeforeTick.StepIndex) - _transport.ElapsedSeconds);
                deltaSeconds = Math.Min(
                    deltaSeconds,
                    remainingPresentationSeconds / _transport.PlaybackSpeed);
            }

            _transport.Tick(deltaSeconds);
            UpdateRenderedPrefix(false);

            if (!_transport.IsPlaying && _transport.ReachedEnd)
            {
                FinishCurrentTransition();
            }

            var elapsed = Stopwatch.GetTimestamp() - startedAt;
            _cpuTicks += elapsed;
            _peakTickTicks = Math.Max(_peakTickTicks, elapsed);
        }

        public void SetFastForward(bool enabled)
        {
            if (FastForward == enabled)
            {
                return;
            }

            FastForward = enabled;
            RecomposeActiveTransport();
        }

        public void SetReducedMotion(bool enabled)
        {
            if (ReducedMotion == enabled)
            {
                return;
            }

            ReducedMotion = enabled;
            RecomposeActiveTransport();
        }

        public void SkipAndSynchronize()
        {
            SynchronizeAcceptedState(AnimationSequenceCompletionReason.Skipped);
        }

        public void InterruptAndSynchronize()
        {
            SynchronizeAcceptedState(AnimationSequenceCompletionReason.Interrupted);
        }

        public void CancelAndSynchronize()
        {
            SynchronizeAcceptedState(AnimationSequenceCompletionReason.Cancelled);
        }

        private void Begin(IReadOnlyList<Transition> transitions, MatchState acceptedFinalState)
        {
            if (IsBusy)
            {
                throw new InvalidOperationException(
                    "A first-playable presentation batch cannot begin while another batch is active.");
            }

            _pending.Clear();
            for (var index = 0; index < transitions.Count; index++)
            {
                _pending.Add(transitions[index]);
            }

            _pendingIndex = 0;
            _acceptedFinalState = acceptedFinalState ?? throw new ArgumentNullException(nameof(acceptedFinalState));
            CompletionReason = AnimationSequenceCompletionReason.None;
            if (_pending.Count == 0)
            {
                RenderedState = new AnimationPresentationState(
                    _acceptedFinalState,
                    RenderedState);
                RenderedReferenceState = _acceptedFinalState;
                CompletionReason = AnimationSequenceCompletionReason.Completed;
                VisualRevision++;
                return;
            }

            IsBusy = true;
            StartTransition(_pending[0]);
        }

        private void StartTransition(Transition transition)
        {
            _current = transition;
            _sequence = ResolvedAnimationSequence.Create(transition.Events, transition.FinalState);
            ComposeTransport();
            _transitionInitialRenderedState = new AnimationPresentationState(
                transition.InitialState,
                RenderedState);
            RenderedState = _transitionInitialRenderedState;
            RenderedReferenceState = transition.InitialState;
            _transport.Play();
            _lastRegisteredStep = -1;
            VisualRevision++;
            RegisterActiveStep();
        }

        private void ComposeTransport()
        {
            var timings = new List<AnimationBeatTiming>();
            for (var index = 0; index < _sequence.Steps.Count - 1; index++)
            {
                var step = _sequence.Steps[index];
                timings.Add(new AnimationBeatTiming(
                    _configuration.GetDelay(step.Kind, FastForward),
                    _configuration.GetDuration(step.Kind, FastForward, ReducedMotion)));
            }

            _transport = new AnimationSequenceTransport(timings)
            {
                PlaybackSpeed = _configuration.PlaybackSpeed,
            };
            _lastRenderedStep = -1;
            _lastRenderedDelay = false;
            _currentStepApplied = false;
        }

        private void UpdateRenderedPrefix(bool force)
        {
            var position = _transport.Position;
            var shouldApplyCurrent = position.StepIndex < _sequence.Steps.Count - 1
                && !position.IsDelaying
                && position.Progress > 0f;
            if (!force
                && position.StepIndex == _lastRenderedStep
                && position.IsDelaying == _lastRenderedDelay
                && shouldApplyCurrent == _currentStepApplied)
            {
                return;
            }

            var rendered = new AnimationPresentationState(
                _current.InitialState,
                _transitionInitialRenderedState);
            var animatableCount = _sequence.Steps.Count - 1;
            var completedCount = Math.Min(position.StepIndex, animatableCount);
            for (var index = 0; index < completedCount; index++)
            {
                rendered.Apply(_sequence.Steps[index], _current.FinalState);
            }

            if (position.StepIndex >= animatableCount)
            {
                rendered.Apply(_sequence.Steps[animatableCount], _current.FinalState);
                RenderedReferenceState = _current.FinalState;
            }
            else
            {
                RenderedReferenceState = _current.InitialState;
                if (shouldApplyCurrent)
                {
                    rendered.Apply(_sequence.Steps[position.StepIndex], _current.FinalState);
                }
            }

            RenderedState = rendered;
            _lastRenderedStep = position.StepIndex;
            _lastRenderedDelay = position.IsDelaying;
            _currentStepApplied = shouldApplyCurrent;
            VisualRevision++;
            RegisterActiveStep();
        }

        private void FinishCurrentTransition()
        {
            RenderedState = new AnimationPresentationState(
                _current.FinalState,
                RenderedState);
            RenderedReferenceState = _current.FinalState;
            VisualRevision++;
            _pendingIndex++;
            if (_pendingIndex < _pending.Count)
            {
                StartTransition(_pending[_pendingIndex]);
                return;
            }

            IsBusy = false;
            CompletionReason = AnimationSequenceCompletionReason.Completed;
            _transport = null;
            _sequence = null;
            _current = null;
        }

        private void SynchronizeAcceptedState(AnimationSequenceCompletionReason reason)
        {
            if (_acceptedFinalState == null)
            {
                return;
            }

            RenderedState = ResolveAcceptedRenderedState();
            RenderedReferenceState = _acceptedFinalState;
            IsBusy = false;
            CompletionReason = reason;
            _transport = null;
            _sequence = null;
            _current = null;
            _transitionInitialRenderedState = null;
            _pending.Clear();
            _pendingIndex = 0;
            VisualRevision++;
        }

        private AnimationPresentationState ResolveAcceptedRenderedState()
        {
            if (_current == null)
            {
                return new AnimationPresentationState(
                    _acceptedFinalState,
                    RenderedState);
            }

            var rendered = new AnimationPresentationState(
                _current.InitialState,
                _transitionInitialRenderedState ?? RenderedState);
            for (var transitionIndex = _pendingIndex;
                 transitionIndex < _pending.Count;
                 transitionIndex++)
            {
                var transition = _pending[transitionIndex];
                var sequence = ResolvedAnimationSequence.Create(
                    transition.Events,
                    transition.FinalState);
                for (var stepIndex = 0; stepIndex < sequence.Steps.Count; stepIndex++)
                {
                    rendered.Apply(
                        sequence.Steps[stepIndex],
                        transition.FinalState);
                }
            }

            return new AnimationPresentationState(
                _acceptedFinalState,
                rendered);
        }

        private void RecomposeActiveTransport()
        {
            if (!IsBusy || _transport == null || _sequence == null)
            {
                return;
            }

            var previousTransport = _transport;
            var previousPosition = previousTransport.Position;
            var delayProgress = 0f;
            if (previousPosition.IsDelaying)
            {
                var delayStart = previousTransport.GetStepStartSeconds(previousPosition.StepIndex);
                var delayEnd = previousTransport.GetStepMotionStartSeconds(previousPosition.StepIndex);
                delayProgress = delayEnd <= delayStart
                    ? 1f
                    : (previousTransport.ElapsedSeconds - delayStart) / (delayEnd - delayStart);
            }

            ComposeTransport();
            if (previousPosition.IsDelaying)
            {
                var delayStart = _transport.GetStepStartSeconds(previousPosition.StepIndex);
                var delayEnd = _transport.GetStepMotionStartSeconds(previousPosition.StepIndex);
                _transport.Seek(delayStart + (delayEnd - delayStart) * delayProgress);
            }
            else
            {
                _transport.SeekToStep(previousPosition.StepIndex, previousPosition.Progress);
            }

            _transport.Play();
            UpdateRenderedPrefix(true);
        }

        private void RegisterActiveStep()
        {
            var step = ActiveStep;
            var stepIndex = _transport?.Position.StepIndex ?? -1;
            if (step == null || stepIndex == _lastRegisteredStep)
            {
                return;
            }

            _lastRegisteredStep = stepIndex;
            _presentedSteps.Add(step.Kind);
        }

        private static double TicksToMilliseconds(long ticks)
        {
            return ticks * 1000d / Stopwatch.Frequency;
        }
    }
}
