using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Presentation.Animation;
using TheFall.Presentation.Match;
using TheFall.Presentation.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace TheFall.Presentation.Diagnostics
{
    /// <summary>
    /// Opt-in development-player probe for issues #28 and #31. It observes the real integrated
    /// flow, records fixed-memory frame and process metrics, and can drive deterministic matches
    /// through the existing application/presentation boundary. It is inert unless the
    /// --first-playable-acceptance command-line flag is present.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class FirstPlayableAcceptanceProbe : MonoBehaviour
    {
        private const string EnableArgument = "--first-playable-acceptance";
        private const string EnableEnvironmentVariable = "THE_FALL_FIRST_PLAYABLE_ACCEPTANCE";
        private const string ModeEnvironmentVariable = "THE_FALL_ACCEPTANCE_MODE";
        private const string CommitEnvironmentVariable = "THE_FALL_ACCEPTANCE_COMMIT";
        private const string WarmupEnvironmentVariable = "THE_FALL_ACCEPTANCE_WARMUP_SECONDS";
        private const string MeasureEnvironmentVariable = "THE_FALL_ACCEPTANCE_MEASURE_SECONDS";
        private const string ReadinessArgument = "--acceptance-readiness-only";
        private const string OutputArgument = "--acceptance-output";
        private const string CommitArgument = "--acceptance-commit";
        private const string WarmupArgument = "--acceptance-warmup-seconds";
        private const string MeasureArgument = "--acceptance-measure-seconds";

        private static readonly Vector2Int[] RequiredResolutions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1440, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
        };

        private readonly AcceptanceSampleHistogram _wallFrameTimes = new AcceptanceSampleHistogram();
        private readonly AcceptanceSampleHistogram _cpuFrameTimes = new AcceptanceSampleHistogram();
        private readonly AcceptanceSampleHistogram _gpuFrameTimes = new AcceptanceSampleHistogram();
        private readonly FrameTiming[] _latestFrameTiming = new FrameTiming[1];
        private readonly HashSet<string> _observedRuleConfigurations = new HashSet<string>();
        private readonly HashSet<string> _observedDealerSeats = new HashSet<string>();
        private readonly HashSet<string> _observedCompletionReasons = new HashSet<string>();
        private readonly HashSet<string> _observedResolutions = new HashSet<string>();
        private readonly HashSet<string> _observedOrientations = new HashSet<string>();
        private readonly HashSet<string> _observedThermalStates = new HashSet<string>();
        private readonly HashSet<DomainEventKind> _observedEventKinds = new HashSet<DomainEventKind>();

        private Stopwatch _runtime;
        private FirstPlayableFlowController _controller;
        private FirstPlayableTablePresentation _table;
        private string _outputPath;
        private string _candidateCommit;
        private double _warmupSeconds;
        private double _measurementSeconds;
        private double _homeReadySeconds = -1d;
        private double _homeToMatchSeconds = -1d;
        private double _measurementStartedAt = -1d;
        private double _lastMemorySampleAt = -1d;
        private double _lastReportAt = -1d;
        private long _peakWorkingSetBytes;
        private long _peakUnityAllocatedBytes;
        private int _completedMatches;
        private int _maximumRound;
        private int _cantoEventCount;
        private int _tieExtensionEventCount;
        private int _authoritativeMismatchCount;
        private int _submittedHumanIntents;
        private int _matchIndex;
        private int _resolutionIndex;
        private int _previousSleepTimeout;
        private int _worstThermalState = AcceptancePlatformMetrics.ThermalStateUnavailable;
        private bool _readinessOnly;
        private bool _pendingSkip;
        private bool _pendingInterrupt;
        private bool _pendingCancel;
        private bool _isQuitting;

        public static void AttachWhenRequested(GameObject host)
        {
            var isRequested = Environment.GetCommandLineArgs().Contains(EnableArgument)
                || string.Equals(
                    Environment.GetEnvironmentVariable(EnableEnvironmentVariable),
                    "1",
                    StringComparison.Ordinal);
            if (!Debug.isDebugBuild
                || host == null
                || !isRequested
                || host.GetComponent<FirstPlayableAcceptanceProbe>() != null)
            {
                return;
            }

            host.AddComponent<FirstPlayableAcceptanceProbe>();
        }

        private void Awake()
        {
            var arguments = Environment.GetCommandLineArgs();
            _readinessOnly = arguments.Contains(ReadinessArgument)
                || string.Equals(
                    Environment.GetEnvironmentVariable(ModeEnvironmentVariable),
                    "readiness",
                    StringComparison.OrdinalIgnoreCase);
            _outputPath = ReadStringArgument(
                arguments,
                OutputArgument,
                Path.Combine(UnityEngine.Application.persistentDataPath, "first-playable-acceptance.json"));
            _candidateCommit = ReadStringArgument(
                arguments,
                CommitArgument,
                ReadEnvironmentVariable(CommitEnvironmentVariable, "unrecorded"));
            _warmupSeconds = ReadDoubleArgument(
                arguments,
                WarmupArgument,
                ReadDoubleEnvironmentVariable(WarmupEnvironmentVariable, 300d));
            _measurementSeconds = ReadDoubleArgument(
                arguments,
                MeasureArgument,
                ReadDoubleEnvironmentVariable(MeasureEnvironmentVariable, 900d));
            _runtime = Stopwatch.StartNew();
            _previousSleepTimeout = Screen.sleepTimeout;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_outputPath)) ?? ".");
            UnityEngine.Application.targetFrameRate = UnityEngine.Application.isMobilePlatform ? 60 : -1;
            if (!UnityEngine.Application.isMobilePlatform)
            {
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            }
        }

        private IEnumerator Start()
        {
            yield return WaitForIntegratedHome();
            _homeReadySeconds = SecondsSinceProcessStart();
            WriteReport("home-ready", false);

            var matchLoadStartedAt = _runtime.Elapsed.TotalSeconds;
            StartConfiguredMatch();
            yield return WaitForUsableMatch();
            _homeToMatchSeconds = _runtime.Elapsed.TotalSeconds - matchLoadStartedAt;
            WriteReport("match-ready", false);

            if (_readinessOnly)
            {
                QuitSuccessfully("readiness-complete");
                yield break;
            }

            _measurementStartedAt = _runtime.Elapsed.TotalSeconds + _warmupSeconds;
            var finishAt = _measurementStartedAt + _measurementSeconds;
            while (_runtime.Elapsed.TotalSeconds < _measurementStartedAt)
            {
                DriveRepresentativeLoop();
                yield return null;
            }

            if (!UnityEngine.Application.isMobilePlatform)
            {
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
                _observedResolutions.Add("1920x1080");
            }
            while (_runtime.Elapsed.TotalSeconds < finishAt)
            {
                DriveRepresentativeLoop();
                yield return null;
            }

            QuitSuccessfully("endurance-complete");
        }

        private void Update()
        {
            if (_runtime == null || _isQuitting)
            {
                return;
            }

            var elapsed = _runtime.Elapsed.TotalSeconds;
            ObserveDisplay();
            SampleMemory(elapsed);
            FrameTimingManager.CaptureFrameTimings();

            if (_measurementStartedAt >= 0d && elapsed >= _measurementStartedAt)
            {
                _wallFrameTimes.Add(Time.unscaledDeltaTime * 1000d);
                if (FrameTimingManager.GetLatestTimings(1, _latestFrameTiming) > 0)
                {
                    _cpuFrameTimes.Add(_latestFrameTiming[0].cpuFrameTime);
                    _gpuFrameTimes.Add(_latestFrameTiming[0].gpuFrameTime);
                }
            }

            if (_table != null
                && !_table.IsPresentationBusy
                && _controller?.Flow?.Match != null
                && _table.RenderedState != null
                && !ReferenceEquals(_table.RenderedState, _controller.Flow.Match.State))
            {
                _authoritativeMismatchCount++;
            }

            if (elapsed - _lastReportAt >= 30d)
            {
                WriteReport("running", false);
                _lastReportAt = elapsed;
            }
        }

        private void OnApplicationQuit()
        {
            Screen.sleepTimeout = _previousSleepTimeout;
            if (!_isQuitting)
            {
                WriteReport("application-quit", false);
            }
        }

        private void OnDestroy()
        {
            Screen.sleepTimeout = _previousSleepTimeout;
        }

        private IEnumerator WaitForIntegratedHome()
        {
            while (_controller == null
                || _table == null
                || !LocalizationSettings.InitializationOperation.IsDone
                || _controller.Flow == null
                || _controller.Flow.Stage != FirstPlayableFlowStage.Home)
            {
                _controller = FindAnyObjectByType<FirstPlayableFlowController>();
                _table = FindAnyObjectByType<FirstPlayableTablePresentation>();
                yield return null;
            }
        }

        private IEnumerator WaitForUsableMatch()
        {
            while (_controller.Flow.Stage != FirstPlayableFlowStage.Match
                || _controller.IsPresentationBusy
                || _controller.Flow.Match == null
                || _controller.Flow.Match.GetHumanLegalIntents().Count == 0)
            {
                if (_table != null && _table.IsPresentationBusy)
                {
                    _table.SetFastForward(true);
                }

                yield return null;
            }
        }

        private void StartConfiguredMatch()
        {
            if (_controller.Flow.Stage != FirstPlayableFlowStage.Home || !_controller.OpenSetup())
            {
                throw new InvalidOperationException("Acceptance probe could not open first-playable setup.");
            }

            var root = _controller.GetComponent<UIDocument>().rootVisualElement;
            var casasEnabled = _matchIndex % 2 == 0;
            var trivilinWins = _matchIndex % 2 != 0;
            root.Q<Toggle>("casas-toggle").value = casasEnabled;
            root.Q<Toggle>("trivilin-toggle").value = trivilinWins;
            _observedRuleConfigurations.Add(
                $"casas={casasEnabled.ToString().ToLowerInvariant()},trivilin-wins={trivilinWins.ToString().ToLowerInvariant()}");
            if (!_controller.StartMatch())
            {
                throw new InvalidOperationException("Acceptance probe could not start the configured match.");
            }
        }

        private void DriveRepresentativeLoop()
        {
            if (_controller == null || _table == null)
            {
                return;
            }

            if (_controller.Flow.Stage == FirstPlayableFlowStage.Result && !_table.IsPresentationBusy)
            {
                RecordCompletedMatch();
                if (!_controller.ReturnHome())
                {
                    throw new InvalidOperationException("Acceptance probe could not return Home after a match.");
                }

                _matchIndex++;
                StartConfiguredMatch();
                return;
            }

            if (_controller.Flow.Stage != FirstPlayableFlowStage.Match)
            {
                return;
            }

            ConfigureTransportForMatch();
            if (_table.IsPresentationBusy)
            {
                ExerciseEarlyExitWhenScheduled();
                return;
            }

            var legal = _controller.Flow.Match.GetHumanLegalIntents();
            if (legal.Count == 0)
            {
                return;
            }

            var intent = ChooseHumanIntent(_controller.Flow.Match.State, legal);
            if (!_controller.SubmitHumanIntent(intent))
            {
                throw new InvalidOperationException($"Acceptance probe failed to submit legal intent {intent}.");
            }

            _submittedHumanIntents++;
            if (_runtime.Elapsed.TotalSeconds < _measurementStartedAt
                && _submittedHumanIntents % 20 == 0)
            {
                ApplyNextResolution();
            }

            ScheduleEarlyExit();
        }

        private PlayerIntent ChooseHumanIntent(MatchState state, IReadOnlyList<PlayerIntent> legal)
        {
            var dealerSelections = legal.OfType<SelectDealerCardIntent>().ToArray();
            if (dealerSelections.Length > 0)
            {
                return _matchIndex % 2 == 0
                    ? dealerSelections.OrderByDescending(item => CardRankOrder.GetIndex(item.Card.Rank)).First()
                    : dealerSelections.OrderBy(item => CardRankOrder.GetIndex(item.Card.Rank)).First();
            }

            if (state.Phase == MatchPhase.AwaitingDealerChoice)
            {
                var handsBeforeTable = _matchIndex % 2 == 0;
                var pattern = _matchIndex % 2 == 0
                    ? OpeningPattern.Ascending
                    : OpeningPattern.Descending;
                return legal.OfType<ChooseDealOptionsIntent>()
                    .Single(item =>
                        item.DealHandsBeforeTable == handsBeforeTable
                        && item.OpeningPattern == pattern);
            }

            var cantos = legal.OfType<AnnounceCantoIntent>().ToArray();
            if (cantos.Length > 0 && (_submittedHumanIntents + _matchIndex) % 3 == 0)
            {
                var human = state.GetPlayerAt(Seat.First);
                var classified = CantoRules.Classify(human.Hand, state.Rules);
                return classified == null
                    ? cantos[0]
                    : cantos.Single(item => item.ClaimedKind == classified.Kind);
            }

            return legal.OfType<PlayCardIntent>().FirstOrDefault() ?? legal[0];
        }

        private void ConfigureTransportForMatch()
        {
            var mode = _matchIndex % 4;
            _table.SetFastForward(mode == 1 || mode == 2 || mode == 3);
            _table.SetReducedMotion(mode == 2);
        }

        private void ScheduleEarlyExit()
        {
            var mode = _submittedHumanIntents % 24;
            _pendingSkip = mode == 6;
            _pendingInterrupt = mode == 12;
            _pendingCancel = mode == 18;
        }

        private void ExerciseEarlyExitWhenScheduled()
        {
            if (_pendingSkip)
            {
                _table.SkipPresentation();
                _observedCompletionReasons.Add(AnimationSequenceCompletionReason.Skipped.ToString());
                _pendingSkip = false;
            }
            else if (_pendingInterrupt)
            {
                _table.InterruptPresentation();
                _observedCompletionReasons.Add(AnimationSequenceCompletionReason.Interrupted.ToString());
                _pendingInterrupt = false;
            }
            else if (_pendingCancel)
            {
                _table.CancelPresentation();
                _observedCompletionReasons.Add(AnimationSequenceCompletionReason.Cancelled.ToString());
                _pendingCancel = false;
            }
        }

        private void ApplyNextResolution()
        {
            if (UnityEngine.Application.isMobilePlatform)
            {
                ObserveDisplay();
                return;
            }

            var resolution = RequiredResolutions[_resolutionIndex % RequiredResolutions.Length];
            _resolutionIndex++;
            Screen.SetResolution(
                resolution.x,
                resolution.y,
                FullScreenMode.Windowed);
            _observedResolutions.Add($"{resolution.x}x{resolution.y}");
        }

        private void RecordCompletedMatch()
        {
            var match = _controller.Flow.Match;
            _completedMatches++;
            _maximumRound = Math.Max(_maximumRound, match.State.RoundNumber);
            _observedCompletionReasons.Add(_table.AnimationCompletionReason.ToString());
            foreach (var resolvedEvent in match.Trace.Events)
            {
                _observedEventKinds.Add(resolvedEvent.Kind);
                if (resolvedEvent is DealerSelectedEvent dealer)
                {
                    _observedDealerSeats.Add(dealer.DealerSeat.ToString());
                }
                else if (resolvedEvent is CantoAnnouncedEvent || resolvedEvent is CantoResolvedEvent)
                {
                    _cantoEventCount++;
                }
                else if (resolvedEvent is TieExtensionStartedEvent)
                {
                    _tieExtensionEventCount++;
                }
            }
        }

        private void SampleMemory(double elapsed)
        {
            if (elapsed - _lastMemorySampleAt < 1d)
            {
                return;
            }

            _lastMemorySampleAt = elapsed;
            _peakWorkingSetBytes = Math.Max(
                _peakWorkingSetBytes,
                AcceptancePlatformMetrics.AppMemoryBytes());

            _peakUnityAllocatedBytes = Math.Max(
                _peakUnityAllocatedBytes,
                Profiler.GetTotalAllocatedMemoryLong());

            var thermalState = AcceptancePlatformMetrics.ThermalState();
            _worstThermalState = Math.Max(_worstThermalState, thermalState);
            _observedThermalStates.Add(AcceptancePlatformMetrics.ThermalStateName(thermalState));
        }

        private void ObserveDisplay()
        {
            _observedResolutions.Add($"{Screen.width}x{Screen.height}");
            _observedOrientations.Add(Screen.orientation.ToString());
        }

        private void QuitSuccessfully(string status)
        {
            if (_isQuitting)
            {
                return;
            }

            _isQuitting = true;
            WriteReport(status, true);
            Screen.sleepTimeout = _previousSleepTimeout;
            UnityEngine.Application.Quit(0);
        }

        private void WriteReport(string status, bool completed)
        {
            if (string.IsNullOrWhiteSpace(_outputPath))
            {
                return;
            }

            var report = new StringBuilder();
            report.AppendLine("{");
            AppendJson(report, "status", status, true);
            AppendJson(report, "completed", completed, true);
            AppendJson(report, "candidateCommit", _candidateCommit, true);
            AppendJson(report, "utc", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture), true);
            AppendJson(report, "unityVersion", UnityEngine.Application.unityVersion, true);
            AppendJson(report, "operatingSystem", SystemInfo.operatingSystem, true);
            AppendJson(report, "processorType", SystemInfo.processorType, true);
            AppendJson(report, "processorCount", SystemInfo.processorCount, true);
            AppendJson(report, "systemMemoryMiB", SystemInfo.systemMemorySize, true);
            AppendJson(report, "graphicsDevice", SystemInfo.graphicsDeviceName, true);
            AppendJson(report, "graphicsMemoryMiB", SystemInfo.graphicsMemorySize, true);
            AppendJson(report, "displayRefreshRateHz", Screen.currentResolution.refreshRateRatio.value, true);
            AppendJson(report, "targetFrameRate", UnityEngine.Application.targetFrameRate, true);
            AppendJson(report, "screenWidth", Screen.width, true);
            AppendJson(report, "screenHeight", Screen.height, true);
            AppendJson(report, "screenOrientation", Screen.orientation.ToString(), true);
            AppendJson(report, "homeReadySeconds", _homeReadySeconds, true);
            AppendJson(report, "homeToUsableMatchSeconds", _homeToMatchSeconds, true);
            AppendJson(report, "warmupSeconds", _warmupSeconds, true);
            AppendJson(report, "measurementSeconds", _measurementSeconds, true);
            AppendJson(report, "wallFrameSamples", _wallFrameTimes.Count, true);
            AppendJson(report, "wallFrameMeanMilliseconds", _wallFrameTimes.MeanMilliseconds, true);
            AppendJson(report, "wallFrameMedianMilliseconds", _wallFrameTimes.Percentile(0.5d), true);
            AppendJson(report, "wallFrameP95Milliseconds", _wallFrameTimes.Percentile(0.95d), true);
            AppendJson(report, "wallFrameMaximumMilliseconds", _wallFrameTimes.MaximumMilliseconds, true);
            AppendJson(report, "framesOver100Milliseconds", _wallFrameTimes.OverOneHundredMillisecondsCount, true);
            AppendJson(report, "cpuFrameSamples", _cpuFrameTimes.Count, true);
            AppendJson(report, "cpuFrameMedianMilliseconds", _cpuFrameTimes.Percentile(0.5d), true);
            AppendJson(report, "cpuFrameP95Milliseconds", _cpuFrameTimes.Percentile(0.95d), true);
            AppendJson(report, "gpuFrameSamples", _gpuFrameTimes.Count, true);
            AppendJson(report, "gpuFrameMedianMilliseconds", _gpuFrameTimes.Percentile(0.5d), true);
            AppendJson(report, "gpuFrameP95Milliseconds", _gpuFrameTimes.Percentile(0.95d), true);
            AppendJson(report, "peakWorkingSetBytes", _peakWorkingSetBytes, true);
            AppendJson(report, "peakAppMemoryBytes", _peakWorkingSetBytes, true);
            AppendJson(report, "peakUnityAllocatedBytes", _peakUnityAllocatedBytes, true);
            AppendJson(
                report,
                "worstThermalState",
                AcceptancePlatformMetrics.ThermalStateName(_worstThermalState),
                true);
            AppendJson(report, "completedMatches", _completedMatches, true);
            AppendJson(report, "maximumRound", _maximumRound, true);
            AppendJson(report, "cantoEventCount", _cantoEventCount, true);
            AppendJson(report, "tieExtensionEventCount", _tieExtensionEventCount, true);
            AppendJson(report, "submittedHumanIntents", _submittedHumanIntents, true);
            AppendJson(report, "authoritativeMismatchCount", _authoritativeMismatchCount, true);
            AppendJsonArray(report, "ruleConfigurations", _observedRuleConfigurations, true);
            AppendJsonArray(report, "dealerSeats", _observedDealerSeats, true);
            AppendJsonArray(report, "completionReasons", _observedCompletionReasons, true);
            AppendJsonArray(report, "resolutions", _observedResolutions, true);
            AppendJsonArray(report, "orientations", _observedOrientations, true);
            AppendJsonArray(report, "thermalStates", _observedThermalStates, true);
            AppendJsonArray(
                report,
                "eventKinds",
                _observedEventKinds.Select(item => item.ToString()),
                false);
            report.AppendLine("}");

            var temporaryPath = _outputPath + ".tmp";
            File.WriteAllText(temporaryPath, report.ToString());
            if (File.Exists(_outputPath))
            {
                File.Delete(_outputPath);
            }

            File.Move(temporaryPath, _outputPath);
        }

        private static string ReadStringArgument(
            IReadOnlyList<string> arguments,
            string name,
            string fallback)
        {
            for (var index = 0; index < arguments.Count; index++)
            {
                if (arguments[index] == name && index + 1 < arguments.Count)
                {
                    return arguments[index + 1];
                }

                var prefix = name + "=";
                if (arguments[index].StartsWith(prefix, StringComparison.Ordinal))
                {
                    return arguments[index].Substring(prefix.Length);
                }
            }

            return fallback;
        }

        private static double ReadDoubleArgument(
            IReadOnlyList<string> arguments,
            string name,
            double fallback)
        {
            var value = ReadStringArgument(arguments, name, string.Empty);
            return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed)
                ? Math.Max(0d, parsed)
                : fallback;
        }

        private static string ReadEnvironmentVariable(string name, string fallback)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static double ReadDoubleEnvironmentVariable(string name, double fallback)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed)
                ? Math.Max(0d, parsed)
                : fallback;
        }

        private static double SecondsSinceProcessStart()
        {
            return AcceptancePlatformMetrics.ProcessUptimeSeconds();
        }

        private static void AppendJson(
            StringBuilder target,
            string name,
            string value,
            bool comma)
        {
            target.Append("  \"").Append(Escape(name)).Append("\": \"")
                .Append(Escape(value ?? string.Empty)).Append('"');
            target.AppendLine(comma ? "," : string.Empty);
        }

        private static void AppendJson(
            StringBuilder target,
            string name,
            bool value,
            bool comma)
        {
            AppendRawJson(target, name, value ? "true" : "false", comma);
        }

        private static void AppendJson(
            StringBuilder target,
            string name,
            long value,
            bool comma)
        {
            AppendRawJson(target, name, value.ToString(CultureInfo.InvariantCulture), comma);
        }

        private static void AppendJson(
            StringBuilder target,
            string name,
            int value,
            bool comma)
        {
            AppendRawJson(target, name, value.ToString(CultureInfo.InvariantCulture), comma);
        }

        private static void AppendJson(
            StringBuilder target,
            string name,
            double value,
            bool comma)
        {
            AppendRawJson(target, name, value.ToString("0.###", CultureInfo.InvariantCulture), comma);
        }

        private static void AppendJsonArray(
            StringBuilder target,
            string name,
            IEnumerable<string> values,
            bool comma)
        {
            target.Append("  \"").Append(Escape(name)).Append("\": [");
            var ordered = values.OrderBy(item => item, StringComparer.Ordinal).ToArray();
            for (var index = 0; index < ordered.Length; index++)
            {
                if (index > 0)
                {
                    target.Append(", ");
                }

                target.Append('"').Append(Escape(ordered[index])).Append('"');
            }

            target.Append(']');
            target.AppendLine(comma ? "," : string.Empty);
        }

        private static void AppendRawJson(
            StringBuilder target,
            string name,
            string value,
            bool comma)
        {
            target.Append("  \"").Append(Escape(name)).Append("\": ").Append(value);
            target.AppendLine(comma ? "," : string.Empty);
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }
    }
}
