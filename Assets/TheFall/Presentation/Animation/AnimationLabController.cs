using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TheFall.Application.Animation;
using TheFall.Domain;
using TheFall.Presentation.Cards;
using TheFall.Presentation.Table;
using UnityEngine;

namespace TheFall.Presentation.Animation
{
    public enum AnimationSequenceCompletionReason
    {
        None,
        Completed,
        Skipped,
        Interrupted,
        Cancelled,
    }

    /// <summary>
    /// Runs isolated presentation timing over a recorded domain result. Every exit path copies the
    /// authoritative final MatchState into the rendered snapshot before accepting further input.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimationLabController : MonoBehaviour
    {
        [SerializeField]
        private AnimationSequenceConfiguration _configuration;

        [SerializeField]
        private Camera _gameplayCamera;

        [SerializeField]
        private GameObject _tablePrototypePrefab;

        [SerializeField]
        private CardVisualCatalog _cardVisualCatalog;

        private readonly ResolvedMatchBuffer _resolvedBuffer = new ResolvedMatchBuffer();
        private RepresentativeAnimationTurn _recording;
        private ResolvedAnimationSequence _sequence;
        private AnimationPresentationState _renderedState;
        private AnimationLabView _view;
        private Coroutine _activeSequence;
        private Seat _actingSeat = Seat.First;
        private Vector2Int _viewport;
        private bool _fastForward;
        private bool _reducedMotion;

        public AnimationSequenceConfiguration Configuration => _configuration;

        public Camera GameplayCamera => _gameplayCamera;

        public GameObject TablePrototypePrefab => _tablePrototypePrefab;

        public CardVisualCatalog CardVisualCatalog => _cardVisualCatalog;

        public IReadOnlyList<DomainEvent> ResolvedEvents => _resolvedBuffer.Events;

        public ResolvedAnimationSequence Sequence => _sequence;

        public AnimationPresentationState RenderedState => _renderedState;

        public MatchState FinalState => _resolvedBuffer.State;

        public Seat ActingSeat => _actingSeat;

        public bool IsPlaying { get; private set; }

        public int CurrentStepIndex { get; private set; }

        public AnimationSequenceCompletionReason CompletionReason { get; private set; }

        public float LastSequenceElapsedSeconds { get; private set; }

        public int LastSequenceFrameCount { get; private set; }

        public float LastSequenceCpuMilliseconds { get; private set; }

        public float LastSequencePeakUpdateCpuMilliseconds { get; private set; }

        public int CardViewCount => _view?.CardViewCount ?? 0;

        public TableCompositionProfile CurrentProfile => _view != null
            ? _view.CurrentProfile
            : default;

        public bool IsRenderedStateSynchronized =>
            _renderedState != null &&
            _resolvedBuffer.State != null &&
            _renderedState.IsSynchronizedWith(_resolvedBuffer.State);

        private void OnEnable()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            Initialize(_actingSeat, GetRuntimeViewport());
            StartActiveSequence();
        }

        private void OnDisable()
        {
            if (IsPlaying)
            {
                StopActiveSequence();
                SynchronizeFinalState(AnimationSequenceCompletionReason.Interrupted);
            }

            _view?.Destroy();
            _view = null;
        }

        public void PlayRepresentativeSequence()
        {
            StopActiveSequence();
            Initialize(_actingSeat, _viewport.x > 0 ? _viewport : GetRuntimeViewport());
            StartActiveSequence();
        }

        public void SetFastForward(bool enabled)
        {
            _fastForward = enabled;
        }

        public void SetReducedMotion(bool enabled)
        {
            _reducedMotion = enabled;
        }

        public void SkipToEnd()
        {
            StopActiveSequence();
            SynchronizeFinalState(AnimationSequenceCompletionReason.Skipped);
        }

        public void InterruptAndSynchronize()
        {
            StopActiveSequence();
            SynchronizeFinalState(AnimationSequenceCompletionReason.Interrupted);
        }

        public void CancelAndSynchronize()
        {
            StopActiveSequence();
            SynchronizeFinalState(AnimationSequenceCompletionReason.Cancelled);
        }

        public void ResetForTests(Seat actingSeat, Vector2Int viewport, bool startPlaying = false)
        {
            StopActiveSequence();
            Initialize(actingSeat, viewport);
            if (startPlaying)
            {
                StartActiveSequence();
            }
        }

        public void CompleteImmediatelyForTests()
        {
            StopActiveSequence();
            while (CurrentStepIndex < _sequence.Steps.Count)
            {
                _renderedState.Apply(_sequence.Steps[CurrentStepIndex], _sequence.FinalState);
                CurrentStepIndex++;
            }

            _view.RenderImmediate(_renderedState);
            CompletionReason = AnimationSequenceCompletionReason.Completed;
            _view.SetCompletionCue(CompletionReason);
        }

#if UNITY_EDITOR
        public void Configure(
            AnimationSequenceConfiguration configuration,
            Camera gameplayCamera,
            GameObject tablePrototypePrefab,
            CardVisualCatalog cardVisualCatalog)
        {
            _configuration = configuration;
            _gameplayCamera = gameplayCamera;
            _tablePrototypePrefab = tablePrototypePrefab;
            _cardVisualCatalog = cardVisualCatalog;
        }

        public void BuildEditorPreview(Seat actingSeat, Vector2Int viewport, bool resolvedState)
        {
            StopActiveSequence();
            Initialize(actingSeat, viewport);
            if (resolvedState)
            {
                CompleteImmediatelyForTests();
            }
        }

        public void ClearEditorPreview()
        {
            StopActiveSequence();
            _view?.Destroy();
        }
#endif

        private void Initialize(Seat actingSeat, Vector2Int viewport)
        {
            ValidateReferences();
            _actingSeat = actingSeat;
            _viewport = viewport;
            _recording = RepresentativeAnimationTurn.Create(actingSeat);
            _resolvedBuffer.Consume(_recording.Result);
            _sequence = ResolvedAnimationSequence.Create(_resolvedBuffer.Events, _resolvedBuffer.State);
            _renderedState = new AnimationPresentationState(_recording.InitialState);
            CurrentStepIndex = 0;
            CompletionReason = AnimationSequenceCompletionReason.None;
            LastSequenceElapsedSeconds = 0f;
            LastSequenceFrameCount = 0;
            LastSequenceCpuMilliseconds = 0f;
            LastSequencePeakUpdateCpuMilliseconds = 0f;

            _view = _view ?? new AnimationLabView(
                transform,
                _gameplayCamera,
                _tablePrototypePrefab,
                _cardVisualCatalog);
            _view.Build(_renderedState, _recording.ActingPlayerId, viewport);
        }

        private void StartActiveSequence()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            IsPlaying = true;
            _activeSequence = StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            var startedAt = Time.realtimeSinceStartupAsDouble;
            var frameCount = 0;
            long cpuTicks = 0;
            long peakUpdateTicks = 0;
            while (CurrentStepIndex < _sequence.Steps.Count)
            {
                var updateStartedAt = Stopwatch.GetTimestamp();
                var step = _sequence.Steps[CurrentStepIndex];
                _renderedState.Apply(step, _sequence.FinalState);
                _view.PrepareTransition(_renderedState, step);
                RecordCpuSample(updateStartedAt, ref cpuTicks, ref peakUpdateTicks);
                var duration = _configuration.GetDuration(step.Kind, _fastForward, _reducedMotion);
                var elapsed = 0f;

                while (elapsed < duration)
                {
                    updateStartedAt = Stopwatch.GetTimestamp();
                    elapsed += Time.unscaledDeltaTime;
                    frameCount++;
                    _view.ApplyTransition(duration <= 0f ? 1f : elapsed / duration);
                    RecordCpuSample(updateStartedAt, ref cpuTicks, ref peakUpdateTicks);
                    yield return null;
                }

                updateStartedAt = Stopwatch.GetTimestamp();
                _view.ApplyTransition(1f);
                CurrentStepIndex++;
                RecordCpuSample(updateStartedAt, ref cpuTicks, ref peakUpdateTicks);
            }

            var synchronizationStartedAt = Stopwatch.GetTimestamp();
            _renderedState.Synchronize(_sequence.FinalState);
            _view.RenderImmediate(_renderedState);
            RecordCpuSample(synchronizationStartedAt, ref cpuTicks, ref peakUpdateTicks);
            IsPlaying = false;
            _activeSequence = null;
            CompletionReason = AnimationSequenceCompletionReason.Completed;
            LastSequenceElapsedSeconds = (float)(Time.realtimeSinceStartupAsDouble - startedAt);
            LastSequenceFrameCount = frameCount;
            LastSequenceCpuMilliseconds = TicksToMilliseconds(cpuTicks);
            LastSequencePeakUpdateCpuMilliseconds = TicksToMilliseconds(peakUpdateTicks);
            _view.SetCompletionCue(CompletionReason);
        }

        private void StopActiveSequence()
        {
            if (_activeSequence != null)
            {
                StopCoroutine(_activeSequence);
                _activeSequence = null;
            }

            IsPlaying = false;
        }

        private void SynchronizeFinalState(AnimationSequenceCompletionReason reason)
        {
            if (_sequence == null || _renderedState == null)
            {
                return;
            }

            _renderedState.Synchronize(_sequence.FinalState);
            CurrentStepIndex = _sequence.Steps.Count;
            CompletionReason = reason;
            _view?.RenderImmediate(_renderedState);
            _view?.SetCompletionCue(reason);
        }

        private void ValidateReferences()
        {
            if (_configuration == null ||
                _gameplayCamera == null ||
                _tablePrototypePrefab == null ||
                _cardVisualCatalog == null)
            {
                throw new MissingReferenceException(
                    "AnimationLab requires presentation configuration, camera, approved table, and card catalog references.");
            }
        }

        private static Vector2Int GetRuntimeViewport()
        {
            return Screen.width >= 64 && Screen.height >= 64
                ? new Vector2Int(Screen.width, Screen.height)
                : new Vector2Int(1920, 1080);
        }

        private static void RecordCpuSample(
            long startedAt,
            ref long totalTicks,
            ref long peakTicks)
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - startedAt;
            totalTicks += elapsedTicks;
            peakTicks = Math.Max(peakTicks, elapsedTicks);
        }

        private static float TicksToMilliseconds(long ticks)
        {
            return ticks * 1000f / Stopwatch.Frequency;
        }
    }
}
