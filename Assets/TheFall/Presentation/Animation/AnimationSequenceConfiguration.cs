using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheFall.Presentation.Animation
{
    public enum AnimationBeatEasing
    {
        Linear,
        EaseInOut,
        Anticipate,
    }

    public enum AnimationPreviewProfile
    {
        Portrait,
        Landscape,
        Desktop,
    }

    [Serializable]
    public sealed class AnimationBeatConfiguration
    {
        [SerializeField]
        private ResolvedAnimationStepKind _kind;

        [SerializeField]
        private bool _enabled = true;

        [SerializeField]
        [Min(0f)]
        private float _durationSeconds = 0.2f;

        [SerializeField]
        [Min(0f)]
        private float _delaySeconds;

        [SerializeField]
        private AnimationBeatEasing _easing = AnimationBeatEasing.EaseInOut;

        [SerializeField]
        private Vector3 _trajectoryOffset = new Vector3(0f, 0.08f, 0f);

        [SerializeField]
        [Range(0f, 2f)]
        private float _emphasis = 1f;

        public AnimationBeatConfiguration(
            ResolvedAnimationStepKind kind,
            float durationSeconds,
            float delaySeconds = 0f,
            AnimationBeatEasing easing = AnimationBeatEasing.EaseInOut,
            Vector3 trajectoryOffset = default,
            float emphasis = 1f,
            bool enabled = true)
        {
            _kind = kind;
            _durationSeconds = Mathf.Max(0f, durationSeconds);
            _delaySeconds = Mathf.Max(0f, delaySeconds);
            _easing = easing;
            _trajectoryOffset = trajectoryOffset;
            _emphasis = Mathf.Clamp(emphasis, 0f, 2f);
            _enabled = enabled;
        }

        public ResolvedAnimationStepKind Kind => _kind;

        public bool Enabled => _enabled;

        public float DurationSeconds => _durationSeconds;

        public float DelaySeconds => _delaySeconds;

        public AnimationBeatEasing Easing => _easing;

        public Vector3 TrajectoryOffset => _trajectoryOffset;

        public float Emphasis => _emphasis;

        public AnimationBeatConfiguration Copy()
        {
            return new AnimationBeatConfiguration(
                _kind,
                _durationSeconds,
                _delaySeconds,
                _easing,
                _trajectoryOffset,
                _emphasis,
                _enabled);
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public void SetTiming(float durationSeconds, float delaySeconds)
        {
            _durationSeconds = Mathf.Max(0f, durationSeconds);
            _delaySeconds = Mathf.Max(0f, delaySeconds);
        }

        public void SetVisuals(AnimationBeatEasing easing, Vector3 trajectoryOffset, float emphasis)
        {
            _easing = easing;
            _trajectoryOffset = trajectoryOffset;
            _emphasis = Mathf.Clamp(emphasis, 0f, 2f);
        }
    }

    [CreateAssetMenu(
        fileName = "AnimationSequenceConfiguration",
        menuName = "The Fall/Animation/Sequence Preset")]
    public sealed class AnimationSequenceConfiguration : ScriptableObject
    {
        public const int CurrentPresetVersion = 2;

        [SerializeField]
        private int _presetVersion = CurrentPresetVersion;

        [SerializeField]
        private string _presetName = "Workbench Default";

        [SerializeField]
        private List<AnimationBeatConfiguration> _beats = new List<AnimationBeatConfiguration>();

        [SerializeField]
        [Range(0.1f, 4f)]
        private float _playbackSpeed = 1f;

        [SerializeField]
        private bool _loop;

        [SerializeField]
        [Min(1f)]
        private float _fastForwardMultiplier = 4f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _reducedMotionDurationScale = 0.25f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _reducedMotionTrajectoryScale = 0.15f;

        // Retained serialized fields migrate the issue #9 asset without losing established timing.
        [SerializeField]
        [Min(0f)]
        private float _cardPlaySeconds = 0.22f;

        [SerializeField]
        [Min(0f)]
        private float _normalCaptureSeconds = 0.9f;

        [SerializeField]
        [Min(0f)]
        private float _cascadeStepSeconds = 0.4f;

        [SerializeField]
        [Min(0f)]
        private float _scoreBeatSeconds = 0.28f;

        [SerializeField]
        [Min(0f)]
        private float _turnChangeSeconds = 0.08f;

        public int PresetVersion => _presetVersion;

        public string PresetName => _presetName;

        public IReadOnlyList<AnimationBeatConfiguration> Beats => _beats;

        public float PlaybackSpeed => _playbackSpeed;

        public bool Loop => _loop;

        public float CardPlaySeconds => _cardPlaySeconds;

        public float NormalCaptureSeconds => _normalCaptureSeconds;

        public float CascadeStepSeconds => _cascadeStepSeconds;

        public float ScoreBeatSeconds => _scoreBeatSeconds;

        public float TurnChangeSeconds => _turnChangeSeconds;

        public float FastForwardMultiplier => _fastForwardMultiplier;

        public float ReducedMotionDurationScale => _reducedMotionDurationScale;

        public float ReducedMotionTrajectoryScale => _reducedMotionTrajectoryScale;

        public void EnsureDefaults()
        {
            if (_beats == null)
            {
                _beats = new List<AnimationBeatConfiguration>();
            }

            var defaults = CreateDefaultBeats();
            if (_beats.Count == 0)
            {
                _beats.AddRange(defaults);
                return;
            }

            foreach (var fallback in defaults)
            {
                if (_beats.Find(beat => beat.Kind == fallback.Kind) == null)
                {
                    _beats.Add(fallback);
                }
            }
        }

        public AnimationBeatConfiguration GetBeat(ResolvedAnimationStepKind kind)
        {
            EnsureDefaults();
            return _beats.Find(beat => beat.Kind == kind);
        }

        public IReadOnlyList<ResolvedAnimationStepKind> GetEnabledBeatOrder()
        {
            EnsureDefaults();
            var order = new List<ResolvedAnimationStepKind>();
            foreach (var beat in _beats)
            {
                if (beat.Enabled)
                {
                    order.Add(beat.Kind);
                }
            }

            return order;
        }

        public float GetDuration(
            ResolvedAnimationStepKind stepKind,
            bool fastForward,
            bool reducedMotion)
        {
            var beat = GetBeat(stepKind);
            var duration = beat?.DurationSeconds ?? GetLegacyDuration(stepKind);
            if (reducedMotion)
            {
                duration *= _reducedMotionDurationScale;
            }

            if (fastForward)
            {
                duration /= Mathf.Max(1f, _fastForwardMultiplier);
            }

            return Mathf.Max(0f, duration);
        }

        public float GetStepDuration(
            ResolvedAnimationStep step,
            bool fastForward,
            bool reducedMotion)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            var duration = GetDuration(step.Kind, fastForward, reducedMotion);
            var continuesToCascade =
                step.Kind == ResolvedAnimationStepKind.NormalCapture
                && step.SourceEvent is TheFall.Domain.CardsCapturedEvent captured
                && captured.Cards.Count > 2;
            return continuesToCascade
                ? duration * AnimationCardTreatmentEvaluator.CascadeLeadInDurationScale
                : duration;
        }

        public float GetDelay(ResolvedAnimationStepKind stepKind, bool fastForward)
        {
            var delay = GetBeat(stepKind)?.DelaySeconds ?? 0f;
            return fastForward ? delay / Mathf.Max(1f, _fastForwardMultiplier) : delay;
        }

        public void SetPresetIdentity(string presetName, int version = CurrentPresetVersion)
        {
            _presetName = string.IsNullOrWhiteSpace(presetName) ? name : presetName.Trim();
            _presetVersion = Math.Max(1, version);
        }

        public void SetTransport(float playbackSpeed, bool loop)
        {
            _playbackSpeed = Mathf.Clamp(playbackSpeed, 0.1f, 4f);
            _loop = loop;
        }

        public void ReplaceBeats(IEnumerable<AnimationBeatConfiguration> beats)
        {
            if (beats == null)
            {
                throw new ArgumentNullException(nameof(beats));
            }

            _beats = new List<AnimationBeatConfiguration>();
            foreach (var beat in beats)
            {
                if (beat != null)
                {
                    _beats.Add(beat.Copy());
                }
            }
        }

        public bool MoveBeat(int index, int offset)
        {
            EnsureDefaults();
            var target = index + offset;
            if (index < 0 || index >= _beats.Count || target < 0 || target >= _beats.Count)
            {
                return false;
            }

            var beat = _beats[index];
            _beats.RemoveAt(index);
            _beats.Insert(target, beat);
            return true;
        }

        private float GetLegacyDuration(ResolvedAnimationStepKind stepKind)
        {
            switch (stepKind)
            {
                case ResolvedAnimationStepKind.CardPlay:
                case ResolvedAnimationStepKind.HandReflow:
                case ResolvedAnimationStepKind.TablePlacement:
                case ResolvedAnimationStepKind.OpeningPlacement:
                    return _cardPlaySeconds;
                case ResolvedAnimationStepKind.NormalCapture:
                    return _normalCaptureSeconds;
                case ResolvedAnimationStepKind.CascadeCapture:
                case ResolvedAnimationStepKind.CaptureCollection:
                    return _cascadeStepSeconds;
                case ResolvedAnimationStepKind.FallScore:
                case ResolvedAnimationStepKind.CleanTableScore:
                case ResolvedAnimationStepKind.Score:
                    return _scoreBeatSeconds;
                case ResolvedAnimationStepKind.TurnChanged:
                case ResolvedAnimationStepKind.MatchCompleted:
                    return _turnChangeSeconds;
                case ResolvedAnimationStepKind.SynchronizeFinalState:
                    return 0f;
                default:
                    return 0.12f;
            }
        }

        private List<AnimationBeatConfiguration> CreateDefaultBeats()
        {
            return new List<AnimationBeatConfiguration>
            {
                Beat(ResolvedAnimationStepKind.MatchStarted, 0.08f),
                Beat(ResolvedAnimationStepKind.DealerSelection, 0.55f),
                Beat(ResolvedAnimationStepKind.DealerChoice, 0.12f),
                Beat(ResolvedAnimationStepKind.Deal, 0.16f, new Vector3(0f, 0.12f, 0.04f)),
                Beat(ResolvedAnimationStepKind.OpeningRejection, 0.12f),
                Beat(ResolvedAnimationStepKind.OpeningPlacement, _cardPlaySeconds, new Vector3(0f, 0.1f, 0f)),
                Beat(ResolvedAnimationStepKind.CardPlay, _cardPlaySeconds, new Vector3(0f, 0.14f, 0.03f)),
                Beat(ResolvedAnimationStepKind.HandReflow, 0.16f),
                Beat(ResolvedAnimationStepKind.NormalCapture, _normalCaptureSeconds, new Vector3(0f, 0.18f, 0f), 1.15f),
                Beat(ResolvedAnimationStepKind.CascadeCapture, _cascadeStepSeconds, new Vector3(0f, 0.13f, 0f), 1.1f),
                Beat(ResolvedAnimationStepKind.CaptureCollection, _cascadeStepSeconds, new Vector3(0f, 0.13f, 0f), 1.1f),
                Beat(ResolvedAnimationStepKind.FallScore, _scoreBeatSeconds, default, 1.5f),
                Beat(ResolvedAnimationStepKind.CleanTableScore, _scoreBeatSeconds, default, 1.4f),
                Beat(ResolvedAnimationStepKind.Canto, 0.24f, default, 1.3f),
                Beat(ResolvedAnimationStepKind.Score, _scoreBeatSeconds, default, 1.2f),
                Beat(ResolvedAnimationStepKind.DealCompleted, 0.1f),
                Beat(ResolvedAnimationStepKind.Leftovers, 0.2f, new Vector3(0f, 0.12f, 0f)),
                Beat(ResolvedAnimationStepKind.Round, 0.2f, default, 1.25f),
                Beat(ResolvedAnimationStepKind.DealerRotation, 0.12f),
                Beat(ResolvedAnimationStepKind.TieExtension, 0.24f, default, 1.4f),
                Beat(ResolvedAnimationStepKind.TurnChanged, _turnChangeSeconds),
                Beat(ResolvedAnimationStepKind.MatchCompleted, _turnChangeSeconds, default, 1.6f),
            };
        }

        private static AnimationBeatConfiguration Beat(
            ResolvedAnimationStepKind kind,
            float duration,
            Vector3 trajectoryOffset = default,
            float emphasis = 1f)
        {
            return new AnimationBeatConfiguration(
                kind,
                duration,
                easing: AnimationBeatEasing.EaseInOut,
                trajectoryOffset: trajectoryOffset,
                emphasis: emphasis);
        }

        private void OnValidate()
        {
            _presetVersion = Math.Max(1, _presetVersion);
            _playbackSpeed = Mathf.Clamp(_playbackSpeed, 0.1f, 4f);
            _cardPlaySeconds = Mathf.Max(0f, _cardPlaySeconds);
            _normalCaptureSeconds = Mathf.Max(0f, _normalCaptureSeconds);
            _cascadeStepSeconds = Mathf.Max(0f, _cascadeStepSeconds);
            _scoreBeatSeconds = Mathf.Max(0f, _scoreBeatSeconds);
            _turnChangeSeconds = Mathf.Max(0f, _turnChangeSeconds);
            _fastForwardMultiplier = Mathf.Max(1f, _fastForwardMultiplier);
            _reducedMotionDurationScale = Mathf.Clamp01(_reducedMotionDurationScale);
            _reducedMotionTrajectoryScale = Mathf.Clamp01(_reducedMotionTrajectoryScale);
            EnsureDefaults();
        }
    }
}
