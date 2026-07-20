using System;
using UnityEngine;

namespace TheFall.Presentation.Animation
{
    [CreateAssetMenu(
        fileName = "AnimationSequenceConfiguration",
        menuName = "The Fall/Animation/Sequence Configuration")]
    public sealed class AnimationSequenceConfiguration : ScriptableObject
    {
        [SerializeField]
        [Min(0f)]
        private float _cardPlaySeconds = 0.22f;

        [SerializeField]
        [Min(0f)]
        private float _normalCaptureSeconds = 0.28f;

        [SerializeField]
        [Min(0f)]
        private float _cascadeStepSeconds = 0.14f;

        [SerializeField]
        [Min(0f)]
        private float _scoreBeatSeconds = 0.28f;

        [SerializeField]
        [Min(0f)]
        private float _turnChangeSeconds = 0.08f;

        [SerializeField]
        [Min(1f)]
        private float _fastForwardMultiplier = 4f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _reducedMotionDurationScale = 0.25f;

        public float CardPlaySeconds => _cardPlaySeconds;

        public float NormalCaptureSeconds => _normalCaptureSeconds;

        public float CascadeStepSeconds => _cascadeStepSeconds;

        public float ScoreBeatSeconds => _scoreBeatSeconds;

        public float TurnChangeSeconds => _turnChangeSeconds;

        public float FastForwardMultiplier => _fastForwardMultiplier;

        public float ReducedMotionDurationScale => _reducedMotionDurationScale;

        public float GetDuration(
            ResolvedAnimationStepKind stepKind,
            bool fastForward,
            bool reducedMotion)
        {
            float duration;
            switch (stepKind)
            {
                case ResolvedAnimationStepKind.CardPlay:
                case ResolvedAnimationStepKind.TablePlacement:
                    duration = _cardPlaySeconds;
                    break;
                case ResolvedAnimationStepKind.NormalCapture:
                    duration = _normalCaptureSeconds;
                    break;
                case ResolvedAnimationStepKind.CascadeCapture:
                    duration = _cascadeStepSeconds;
                    break;
                case ResolvedAnimationStepKind.FallScore:
                case ResolvedAnimationStepKind.CleanTableScore:
                    duration = _scoreBeatSeconds;
                    break;
                case ResolvedAnimationStepKind.TurnChanged:
                case ResolvedAnimationStepKind.MatchCompleted:
                    duration = _turnChangeSeconds;
                    break;
                case ResolvedAnimationStepKind.SynchronizeFinalState:
                    return 0f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stepKind), stepKind, null);
            }

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

        private void OnValidate()
        {
            _cardPlaySeconds = Mathf.Max(0f, _cardPlaySeconds);
            _normalCaptureSeconds = Mathf.Max(0f, _normalCaptureSeconds);
            _cascadeStepSeconds = Mathf.Max(0f, _cascadeStepSeconds);
            _scoreBeatSeconds = Mathf.Max(0f, _scoreBeatSeconds);
            _turnChangeSeconds = Mathf.Max(0f, _turnChangeSeconds);
            _fastForwardMultiplier = Mathf.Max(1f, _fastForwardMultiplier);
            _reducedMotionDurationScale = Mathf.Clamp01(_reducedMotionDurationScale);
        }
    }
}
