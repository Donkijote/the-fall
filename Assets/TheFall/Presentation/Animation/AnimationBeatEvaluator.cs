using System;
using UnityEngine;

namespace TheFall.Presentation.Animation
{
    /// <summary>
    /// Shared transform-path evaluator used by Edit Mode wireframes and runtime playback.
    /// Authoritative state supplies the endpoints; presentation presets supply the path offset.
    /// </summary>
    public static class AnimationBeatEvaluator
    {
        public static float EvaluateEasedProgress(float progress, AnimationBeatEasing easing)
        {
            var clamped = Mathf.Clamp01(progress);
            switch (easing)
            {
                case AnimationBeatEasing.Linear:
                    return clamped;
                case AnimationBeatEasing.EaseInOut:
                    return Mathf.SmoothStep(0f, 1f, clamped);
                case AnimationBeatEasing.Anticipate:
                    return clamped * clamped * (2.70158f * clamped - 1.70158f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(easing), easing, null);
            }
        }

        public static Vector3 EvaluatePosition(
            Vector3 start,
            Vector3 target,
            float progress,
            AnimationBeatEasing easing,
            Vector3 trajectoryOffset)
        {
            var clamped = Mathf.Clamp01(progress);
            var eased = EvaluateEasedProgress(clamped, easing);
            return Vector3.LerpUnclamped(start, target, eased) +
                Mathf.Sin(clamped * Mathf.PI) * trajectoryOffset;
        }
    }

    public readonly struct AnimationMotionPreview
    {
        public AnimationMotionPreview(
            Vector3 startWorld,
            Vector3 targetWorld,
            Transform presentationRoot)
        {
            StartWorld = startWorld;
            TargetWorld = targetWorld;
            PresentationRoot = presentationRoot;
        }

        public Vector3 StartWorld { get; }

        public Vector3 TargetWorld { get; }

        public Transform PresentationRoot { get; }
    }
}
