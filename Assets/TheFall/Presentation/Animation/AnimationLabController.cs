using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheFall.Application.Animation;
using TheFall.Domain;
using TheFall.Presentation.Cards;
using TheFall.Presentation.Table;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    /// Real-time workbench over one immutable scenario recording. Presets and transport alter only
    /// how the accepted result is presented; every terminal path copies the authoritative state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimationLabController : MonoBehaviour
    {
        [SerializeField]
        private AnimationSequenceConfiguration _configuration;

        [SerializeField]
        private AnimationSequenceConfiguration[] _presets = Array.Empty<AnimationSequenceConfiguration>();

        [SerializeField]
        private Camera _gameplayCamera;

        [SerializeField]
        private GameObject _tablePrototypePrefab;

        [SerializeField]
        private CardVisualCatalog _cardVisualCatalog;

        [SerializeField]
        private bool _showWorkbench = true;

        private readonly ResolvedMatchBuffer _resolvedBuffer = new ResolvedMatchBuffer();
        private AnimationScenarioRecording _recording;
        private ResolvedAnimationSequence _sequence;
        private AnimationPresentationState _renderedState;
        private AnimationLabView _view;
        private AnimationSequenceConfiguration _workingConfiguration;
        private AnimationSequenceTransport _transport;
        private AnimationScenarioKind _scenarioKind = AnimationScenarioKind.FallCascadeAndCleanTable;
        private AnimationPreviewProfile _previewProfile = AnimationPreviewProfile.Desktop;
        private Seat _actingSeat = Seat.First;
        private Vector2Int _viewport;
        private bool _fastForward;
        private bool _reducedMotion;
        private int _activePresetIndex;
        private int _lastRenderedStep = -1;
        private bool _lastRenderedDelay;
        private bool _hasPreparedTransition;
        private double _sequenceStartedAt;
        private long _cpuTicks;
        private long _peakUpdateTicks;
        private int _frameCount;
        private Vector2 _workbenchScroll;

        public AnimationSequenceConfiguration Configuration => _configuration;

        public IReadOnlyList<AnimationSequenceConfiguration> Presets => _presets;

        public AnimationSequenceConfiguration WorkingConfiguration => _workingConfiguration;

        public Camera GameplayCamera => _gameplayCamera;

        public GameObject TablePrototypePrefab => _tablePrototypePrefab;

        public CardVisualCatalog CardVisualCatalog => _cardVisualCatalog;

        public IReadOnlyList<DomainEvent> ResolvedEvents => _resolvedBuffer.Events;

        public ResolvedAnimationSequence Sequence => _sequence;

        public AnimationPresentationState RenderedState => _renderedState;

        public MatchState FinalState => _resolvedBuffer.State;

        public Seat ActingSeat => _actingSeat;

        public AnimationScenarioKind ScenarioKind => _scenarioKind;

        public AnimationPreviewProfile PreviewProfile => _previewProfile;

        public bool IsPlaying => _transport?.IsPlaying ?? false;

        public int CurrentStepIndex => _transport?.Position.StepIndex ?? 0;

        public float ElapsedSeconds => _transport?.ElapsedSeconds ?? 0f;

        public float DurationSeconds => _transport?.DurationSeconds ?? 0f;

        public float NormalizedPosition => _transport?.NormalizedPosition ?? 0f;

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
            RestartSequence();
        }

        private void Update()
        {
            if (_transport == null || !_transport.IsPlaying)
            {
                return;
            }

            var updateStartedAt = Stopwatch.GetTimestamp();
            var wasPlaying = _transport.IsPlaying;
            var previousElapsed = _transport.ElapsedSeconds;
            _transport.Tick(Time.unscaledDeltaTime);
            _frameCount++;
            var looped = _transport.Loop && _transport.ElapsedSeconds < previousElapsed;
            RenderTransportPosition(looped);
            RecordCpuSample(updateStartedAt);

            if (wasPlaying && !_transport.IsPlaying && _transport.ReachedEnd)
            {
                CompletePlayback(AnimationSequenceCompletionReason.Completed);
            }
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
            DestroyWorkingConfiguration();
        }

        private void OnGUI()
        {
            if (!_showWorkbench || _workingConfiguration == null || _sequence == null)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(12f, 12f, 390f, Mathf.Max(300f, Screen.height - 24f)), GUI.skin.window);
            _workbenchScroll = GUILayout.BeginScrollView(_workbenchScroll);
            GUILayout.Label("ANIMATIONLAB · SEQUENCE WORKBENCH");
            GUILayout.Label($"Scenario: {_recording.DisplayName}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Scenario"))
            {
                SelectNextScenario();
            }

            if (GUILayout.Button($"Seat {_actingSeat}"))
            {
                SetActingSeat(_actingSeat == Seat.First ? Seat.Second : Seat.First);
            }

            if (GUILayout.Button(_previewProfile.ToString()))
            {
                SetPreviewProfile((AnimationPreviewProfile)(((int)_previewProfile + 1) % 3));
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Preset"))
            {
                LoadPreset((_activePresetIndex + 1) % Math.Max(1, _presets.Length));
            }

            GUILayout.Label($"{_workingConfiguration.PresetName} v{_workingConfiguration.PresetVersion}");
#if UNITY_EDITOR
            if (GUILayout.Button("Save", GUILayout.Width(56f)))
            {
                SaveWorkingPreset();
            }
#endif
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(IsPlaying ? "Pause" : "Play"))
            {
                if (IsPlaying)
                {
                    Pause();
                }
                else
                {
                    Resume();
                }
            }

            if (GUILayout.Button("Step"))
            {
                SingleStep();
            }

            if (GUILayout.Button("Restart"))
            {
                RestartSequence();
            }

            if (GUILayout.Button("Skip"))
            {
                SkipToEnd();
            }

            if (GUILayout.Button("Reset"))
            {
                ResetToStart();
            }

            GUILayout.EndHorizontal();

            var seek = GUILayout.HorizontalSlider(NormalizedPosition, 0f, 1f);
            if (!Mathf.Approximately(seek, NormalizedPosition))
            {
                SeekNormalized(seek);
            }

            GUILayout.Label($"{ElapsedSeconds:F2}s / {DurationSeconds:F2}s · beat {Math.Min(CurrentStepIndex + 1, _sequence.Steps.Count)}/{_sequence.Steps.Count}");
            GUILayout.BeginHorizontal();
            var loop = GUILayout.Toggle(_workingConfiguration.Loop, "Loop");
            if (loop != _workingConfiguration.Loop)
            {
                _workingConfiguration.SetTransport(_workingConfiguration.PlaybackSpeed, loop);
            }

            _fastForward = GUILayout.Toggle(_fastForward, "Fast");
            _reducedMotion = GUILayout.Toggle(_reducedMotion, "Reduced motion");
            GUILayout.EndHorizontal();
            var speed = GUILayout.HorizontalSlider(_workingConfiguration.PlaybackSpeed, 0.1f, 4f);
            _workingConfiguration.SetTransport(speed, _workingConfiguration.Loop);
            GUILayout.Label($"Playback {speed:F2}x · changes apply on Restart");

            GUILayout.Space(8f);
            GUILayout.Label("COMPOSED BEATS");
            var beats = _workingConfiguration.Beats;
            for (var index = 0; index < beats.Count; index++)
            {
                var beat = beats[index];
                GUILayout.BeginHorizontal();
                var enabled = GUILayout.Toggle(beat.Enabled, string.Empty, GUILayout.Width(20f));
                if (enabled != beat.Enabled)
                {
                    beat.SetEnabled(enabled);
                }

                GUILayout.Label(beat.Kind.ToString(), GUILayout.Width(150f));
                if (GUILayout.Button("↑", GUILayout.Width(30f)))
                {
                    _workingConfiguration.MoveBeat(index, -1);
                }

                if (GUILayout.Button("↓", GUILayout.Width(30f)))
                {
                    _workingConfiguration.MoveBeat(index, 1);
                }

                GUILayout.Label($"{beat.DurationSeconds:F2}s", GUILayout.Width(52f));
                GUILayout.EndHorizontal();
            }

            var activeBeat = GetActiveBeat();
            if (activeBeat != null)
            {
                GUILayout.Space(8f);
                GUILayout.Label($"ACTIVE TUNING · {activeBeat.Kind}");
                var duration = GUILayout.HorizontalSlider(activeBeat.DurationSeconds, 0f, 2f);
                var delay = GUILayout.HorizontalSlider(activeBeat.DelaySeconds, 0f, 1f);
                var emphasis = GUILayout.HorizontalSlider(activeBeat.Emphasis, 0f, 2f);
                var trajectoryHeight = GUILayout.HorizontalSlider(activeBeat.TrajectoryOffset.y, 0f, 0.5f);
                activeBeat.SetTiming(duration, delay);
                activeBeat.SetVisuals(
                    activeBeat.Easing,
                    new Vector3(
                        activeBeat.TrajectoryOffset.x,
                        trajectoryHeight,
                        activeBeat.TrajectoryOffset.z),
                    emphasis);
                GUILayout.Label($"duration {duration:F2}s · delay {delay:F2}s · emphasis {emphasis:F2} · arc {trajectoryHeight:F2}m");
                if (GUILayout.Button($"Easing: {activeBeat.Easing}"))
                {
                    var next = (AnimationBeatEasing)(((int)activeBeat.Easing + 1) % 3);
                    activeBeat.SetVisuals(next, activeBeat.TrajectoryOffset, activeBeat.Emphasis);
                }
            }

            GUILayout.Space(8f);
            GUILayout.Label("DIAGNOSTICS");
            GUILayout.Label($"Rendered ↔ authoritative: {(IsRenderedStateSynchronized ? "AGREE" : "IN FLIGHT")}");
            for (var index = 0; index < ResolvedEvents.Count; index++)
            {
                var marker = GetActiveStep()?.SourceEventIndex == index ? "▶" : " ";
                GUILayout.Label($"{marker} {index:00}  {ResolvedEvents[index].Kind}");
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public void PlayRepresentativeSequence()
        {
            RestartSequence();
        }

        public void Pause()
        {
            _transport?.Pause();
        }

        public void Resume()
        {
            if (_transport == null)
            {
                ComposeTransport();
            }

            CompletionReason = AnimationSequenceCompletionReason.None;
            _transport.Play();
            if (_sequenceStartedAt <= 0d)
            {
                BeginMetrics();
            }
        }

        public void RestartSequence()
        {
            ComposeTransport();
            CompletionReason = AnimationSequenceCompletionReason.None;
            _transport.Restart();
            BeginMetrics();
            RenderTransportPosition(true);
        }

        public void ResetToStart()
        {
            _transport?.Reset();
            CompletionReason = AnimationSequenceCompletionReason.None;
            RenderTransportPosition(true);
        }

        public void SingleStep()
        {
            _transport?.StepForward();
            RenderTransportPosition(true);
            if (_transport != null && _transport.ReachedEnd)
            {
                CompletePlayback(AnimationSequenceCompletionReason.Completed);
            }
        }

        public void SeekNormalized(float normalizedPosition)
        {
            _transport?.SeekNormalized(normalizedPosition);
            RenderTransportPosition(true);
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
            _transport?.SkipToEnd();
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

        public void SetActingSeat(Seat actingSeat)
        {
            StopActiveSequence();
            InitializeRecording(actingSeat);
            ComposeTransport();
            RenderTransportPosition(true);
        }

        public void SetScenario(AnimationScenarioKind scenarioKind)
        {
            StopActiveSequence();
            _scenarioKind = scenarioKind;
            InitializeRecording(_actingSeat);
            ComposeTransport();
            RenderTransportPosition(true);
        }

        public void SetPreviewProfile(AnimationPreviewProfile profile)
        {
            _previewProfile = profile;
            _viewport = GetProfileViewport(profile);
            RenderTransportPosition(true);
        }

        public void LoadPreset(int presetIndex)
        {
            if (_presets == null || _presets.Length == 0)
            {
                presetIndex = 0;
            }
            else if (presetIndex < 0 || presetIndex >= _presets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(presetIndex));
            }

            StopActiveSequence();
            LoadWorkingConfiguration(presetIndex);
            ComposeTransport();
            RenderTransportPosition(true);
        }

        public void ResetForTests(Seat actingSeat, Vector2Int viewport, bool startPlaying = false)
        {
            StopActiveSequence();
            Initialize(actingSeat, viewport);
            ComposeTransport();
            RenderTransportPosition(true);
            if (startPlaying)
            {
                _transport.Restart();
                BeginMetrics();
            }
        }

        public void CompleteImmediatelyForTests()
        {
            _transport?.SkipToEnd();
            SynchronizeFinalState(AnimationSequenceCompletionReason.Completed);
        }

#if UNITY_EDITOR
        public void Configure(
            AnimationSequenceConfiguration configuration,
            AnimationSequenceConfiguration[] presets,
            Camera gameplayCamera,
            GameObject tablePrototypePrefab,
            CardVisualCatalog cardVisualCatalog)
        {
            _configuration = configuration;
            _presets = presets ?? Array.Empty<AnimationSequenceConfiguration>();
            _gameplayCamera = gameplayCamera;
            _tablePrototypePrefab = tablePrototypePrefab;
            _cardVisualCatalog = cardVisualCatalog;
        }

        public void BuildEditorPreview(Seat actingSeat, Vector2Int viewport, bool resolvedState)
        {
            StopActiveSequence();
            Initialize(actingSeat, viewport);
            ComposeTransport();
            if (resolvedState)
            {
                CompleteImmediatelyForTests();
            }
            else
            {
                RenderTransportPosition(true);
            }
        }

        public void ClearEditorPreview()
        {
            StopActiveSequence();
            _view?.Destroy();
        }

        public void SaveWorkingPreset()
        {
            if (_workingConfiguration == null || _configuration == null)
            {
                return;
            }

            var destination = _presets != null && _presets.Length > _activePresetIndex
                ? _presets[_activePresetIndex]
                : _configuration;
            Undo.RecordObject(destination, "Save animation workbench preset");
            EditorUtility.CopySerialized(_workingConfiguration, destination);
            EditorUtility.SetDirty(destination);
            AssetDatabase.SaveAssets();
        }
#endif

        private void Initialize(Seat actingSeat, Vector2Int viewport)
        {
            ValidateReferences();
            _viewport = viewport;
            _previewProfile = ResolvePreviewProfile(viewport);
            if (_workingConfiguration == null)
            {
                LoadWorkingConfiguration(0);
            }

            InitializeRecording(actingSeat);
            _view = _view ?? new AnimationLabView(
                transform,
                _gameplayCamera,
                _tablePrototypePrefab,
                _cardVisualCatalog);
        }

        private void InitializeRecording(Seat actingSeat)
        {
            _actingSeat = actingSeat;
            _recording = AnimationScenarioRecording.Create(_scenarioKind, actingSeat);
            _resolvedBuffer.Consume(_recording.Result);
        }

        private void ComposeTransport()
        {
            _workingConfiguration.EnsureDefaults();
            _sequence = ResolvedAnimationSequence.Create(
                _resolvedBuffer.Events,
                _resolvedBuffer.State,
                _workingConfiguration.GetEnabledBeatOrder());
            var timings = new List<AnimationBeatTiming>();
            for (var index = 0; index < _sequence.Steps.Count; index++)
            {
                var step = _sequence.Steps[index];
                if (step.Kind == ResolvedAnimationStepKind.SynchronizeFinalState)
                {
                    continue;
                }

                timings.Add(new AnimationBeatTiming(
                    _workingConfiguration.GetDelay(step.Kind, _fastForward),
                    _workingConfiguration.GetDuration(step.Kind, _fastForward, _reducedMotion)));
            }

            _transport = new AnimationSequenceTransport(timings)
            {
                Loop = _workingConfiguration.Loop,
                PlaybackSpeed = _workingConfiguration.PlaybackSpeed,
            };
            _lastRenderedStep = -1;
            _lastRenderedDelay = false;
            _hasPreparedTransition = false;
            CompletionReason = AnimationSequenceCompletionReason.None;
        }

        private void RenderTransportPosition(bool forceRebuild)
        {
            if (_transport == null || _sequence == null)
            {
                return;
            }

            var position = _transport.Position;
            var rebuild = forceRebuild ||
                position.StepIndex != _lastRenderedStep ||
                position.IsDelaying != _lastRenderedDelay ||
                !position.IsDelaying && position.Progress > 0f && !_hasPreparedTransition;
            if (!rebuild && position.StepIndex < _sequence.Steps.Count - 1)
            {
                _view.ApplyTransition(position.Progress);
                return;
            }

            _renderedState = new AnimationPresentationState(_recording.InitialState);
            var animatableCount = _sequence.Steps.Count - 1;
            var completedCount = Math.Min(position.StepIndex, animatableCount);
            for (var index = 0; index < completedCount; index++)
            {
                _renderedState.Apply(_sequence.Steps[index], _sequence.FinalState);
            }

            _view.Build(_renderedState, _recording.ActingPlayerId, _viewport);
            _hasPreparedTransition = false;
            if (position.StepIndex >= animatableCount)
            {
                _renderedState.Apply(_sequence.Steps[_sequence.Steps.Count - 1], _sequence.FinalState);
                _view.RenderImmediate(_renderedState);
            }
            else if (!position.IsDelaying && position.Progress > 0f)
            {
                var step = _sequence.Steps[position.StepIndex];
                _renderedState.Apply(step, _sequence.FinalState);
                _view.PrepareTransition(
                    _renderedState,
                    step,
                    _workingConfiguration.GetBeat(step.Kind),
                    _reducedMotion,
                    _workingConfiguration.ReducedMotionTrajectoryScale);
                _view.ApplyTransition(position.Progress);
                _hasPreparedTransition = true;
            }

            _lastRenderedStep = position.StepIndex;
            _lastRenderedDelay = position.IsDelaying;
        }

        private void StopActiveSequence()
        {
            _transport?.Pause();
        }

        private void SynchronizeFinalState(AnimationSequenceCompletionReason reason)
        {
            if (_sequence == null || _resolvedBuffer.State == null)
            {
                return;
            }

            _renderedState = new AnimationPresentationState(_resolvedBuffer.State);
            _view?.Build(_renderedState, _recording.ActingPlayerId, _viewport);
            _view?.RenderImmediate(_renderedState);
            CompletionReason = reason;
            _view?.SetCompletionCue(reason);
        }

        private void CompletePlayback(AnimationSequenceCompletionReason reason)
        {
            SynchronizeFinalState(reason);
            LastSequenceElapsedSeconds = _sequenceStartedAt <= 0d
                ? 0f
                : (float)(Time.realtimeSinceStartupAsDouble - _sequenceStartedAt);
            LastSequenceFrameCount = _frameCount;
            LastSequenceCpuMilliseconds = TicksToMilliseconds(_cpuTicks);
            LastSequencePeakUpdateCpuMilliseconds = TicksToMilliseconds(_peakUpdateTicks);
            _sequenceStartedAt = 0d;
        }

        private void BeginMetrics()
        {
            _sequenceStartedAt = Time.realtimeSinceStartupAsDouble;
            _frameCount = 0;
            _cpuTicks = 0;
            _peakUpdateTicks = 0;
            LastSequenceElapsedSeconds = 0f;
            LastSequenceFrameCount = 0;
            LastSequenceCpuMilliseconds = 0f;
            LastSequencePeakUpdateCpuMilliseconds = 0f;
        }

        private void RecordCpuSample(long startedAt)
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - startedAt;
            _cpuTicks += elapsedTicks;
            _peakUpdateTicks = Math.Max(_peakUpdateTicks, elapsedTicks);
        }

        private void LoadWorkingConfiguration(int presetIndex)
        {
            DestroyWorkingConfiguration();
            _activePresetIndex = presetIndex;
            var source = _presets != null && _presets.Length > presetIndex && _presets[presetIndex] != null
                ? _presets[presetIndex]
                : _configuration;
            source.EnsureDefaults();
            _workingConfiguration = Instantiate(source);
            _workingConfiguration.name = $"{source.name} (Workbench Copy)";
            _workingConfiguration.hideFlags = HideFlags.DontSave;
            _workingConfiguration.EnsureDefaults();
        }

        private void DestroyWorkingConfiguration()
        {
            if (_workingConfiguration == null)
            {
                return;
            }

            if (UnityEngine.Application.isPlaying)
            {
                Destroy(_workingConfiguration);
            }
            else
            {
                DestroyImmediate(_workingConfiguration);
            }

            _workingConfiguration = null;
        }

        private AnimationBeatConfiguration GetActiveBeat()
        {
            return GetActiveStep() is ResolvedAnimationStep step
                ? _workingConfiguration.GetBeat(step.Kind)
                : null;
        }

        private ResolvedAnimationStep GetActiveStep()
        {
            if (_sequence == null || CurrentStepIndex < 0 || CurrentStepIndex >= _sequence.Steps.Count - 1)
            {
                return null;
            }

            return _sequence.Steps[CurrentStepIndex];
        }

        private void SelectNextScenario()
        {
            var count = Enum.GetValues(typeof(AnimationScenarioKind)).Length;
            SetScenario((AnimationScenarioKind)(((int)_scenarioKind + 1) % count));
        }

        private void ValidateReferences()
        {
            if (_configuration == null ||
                _gameplayCamera == null ||
                _tablePrototypePrefab == null ||
                _cardVisualCatalog == null)
            {
                throw new MissingReferenceException(
                    "AnimationLab requires presentation presets, camera, approved table, and card catalog references.");
            }
        }

        private static AnimationPreviewProfile ResolvePreviewProfile(Vector2Int viewport)
        {
            if (viewport.x < viewport.y)
            {
                return AnimationPreviewProfile.Portrait;
            }

            return (float)viewport.x / Math.Max(1, viewport.y) >= 1.7f
                ? AnimationPreviewProfile.Desktop
                : AnimationPreviewProfile.Landscape;
        }

        private static Vector2Int GetProfileViewport(AnimationPreviewProfile profile)
        {
            switch (profile)
            {
                case AnimationPreviewProfile.Portrait:
                    return new Vector2Int(390, 844);
                case AnimationPreviewProfile.Landscape:
                    return new Vector2Int(1440, 1080);
                case AnimationPreviewProfile.Desktop:
                    return new Vector2Int(1920, 1080);
                default:
                    throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }
        }

        private static Vector2Int GetRuntimeViewport()
        {
            return Screen.width >= 64 && Screen.height >= 64
                ? new Vector2Int(Screen.width, Screen.height)
                : new Vector2Int(1920, 1080);
        }

        private static float TicksToMilliseconds(long ticks)
        {
            return ticks * 1000f / Stopwatch.Frequency;
        }
    }
}
