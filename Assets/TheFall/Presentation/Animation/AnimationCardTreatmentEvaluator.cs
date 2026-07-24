using UnityEngine;

namespace TheFall.Presentation.Animation
{
    public readonly struct AnimationCardTreatmentPose
    {
        public AnimationCardTreatmentPose(
            Vector3 position,
            float flipDegrees,
            bool faceUp)
        {
            Position = position;
            FlipDegrees = flipDegrees;
            FaceUp = faceUp;
        }

        public Vector3 Position { get; }

        public float FlipDegrees { get; }

        public bool FaceUp { get; }
    }

    /// <summary>
    /// Single choreography evaluator used by both AnimationLab and the integrated table.
    /// Callers own card views and coordinate spaces; this type owns treatment phase,
    /// trajectory, flip, and face-visibility evaluation.
    /// </summary>
    public static class AnimationCardTreatmentEvaluator
    {
        public const float CapturePlayEndProgress = 0.38f;
        public const float CapturePickupStartProgress = 0.46f;

        public static Vector3 EvaluateTranslation(
            Vector3 start,
            Vector3 target,
            float progress,
            AnimationBeatEasing easing,
            Vector3 trajectory)
        {
            return Vector3.SqrMagnitude(target - start) <= 0.000001f
                ? target
                : AnimationBeatEvaluator.EvaluatePosition(
                    start,
                    target,
                    progress,
                    easing,
                    trajectory);
        }

        public static AnimationCardTreatmentPose EvaluateRevealMove(
            Vector3 start,
            Vector3 target,
            float progress,
            AnimationBeatEasing easing,
            Vector3 trajectory,
            bool revealFace,
            float flipDirection = 1f)
        {
            var clamped = Mathf.Clamp01(progress);
            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(clamped, easing);
            return new AnimationCardTreatmentPose(
                EvaluateTranslation(start, target, clamped, easing, trajectory),
                revealFace ? eased * 180f * flipDirection : 0f,
                revealFace && clamped >= 0.5f);
        }

        public static AnimationCardTreatmentPose EvaluateHideMove(
            Vector3 start,
            Vector3 target,
            float progress,
            AnimationBeatEasing easing,
            Vector3 trajectory)
        {
            var clamped = Mathf.Clamp01(progress);
            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(clamped, easing);
            return new AnimationCardTreatmentPose(
                EvaluateTranslation(start, target, clamped, easing, trajectory),
                180f + eased * 180f,
                clamped < 0.5f);
        }

        public static AnimationCardTreatmentPose EvaluateNormalCapture(
            Vector3 start,
            Vector3 stack,
            Vector3 target,
            float progress,
            AnimationBeatEasing easing,
            Vector3 trajectory,
            bool isPlayedCard,
            bool revealsPlayedCard,
            bool continuesToCascade)
        {
            var clamped = Mathf.Clamp01(progress);
            var playProgress = Mathf.InverseLerp(
                0f,
                CapturePlayEndProgress,
                clamped);
            var isCollecting = clamped >= CapturePickupStartProgress;
            var collectionProgress = Mathf.InverseLerp(
                CapturePickupStartProgress,
                1f,
                clamped);
            var position = !isCollecting
                ? isPlayedCard
                    ? EvaluateTranslation(
                        start,
                        stack,
                        playProgress,
                        easing,
                        trajectory * 0.55f)
                    : stack
                : continuesToCascade
                    ? stack
                    : EvaluateTranslation(
                        stack,
                        target,
                        collectionProgress,
                        easing,
                        trajectory);
            var playEased = AnimationBeatEvaluator.EvaluateEasedProgress(
                playProgress,
                easing);
            var collectionEased = AnimationBeatEvaluator.EvaluateEasedProgress(
                collectionProgress,
                easing);
            var flipDegrees = revealsPlayedCard && !isCollecting
                ? playEased * 180f
                : continuesToCascade
                    ? 180f
                    : 180f + collectionEased * 180f;
            var faceUp = revealsPlayedCard && !isCollecting
                ? playProgress >= 0.5f
                : continuesToCascade
                    || !isCollecting
                    || collectionProgress < 0.5f;
            return new AnimationCardTreatmentPose(
                position,
                flipDegrees,
                faceUp);
        }

        public static AnimationCardTreatmentPose EvaluateCascade(
            Vector3 start,
            Vector3 target,
            float progress,
            AnimationBeatEasing easing,
            Vector3 trajectory,
            bool stationaryTarget,
            bool completesCapture)
        {
            var clamped = Mathf.Clamp01(progress);
            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(clamped, easing);
            return new AnimationCardTreatmentPose(
                stationaryTarget
                    ? target
                    : EvaluateTranslation(start, target, clamped, easing, trajectory),
                completesCapture ? 180f + eased * 180f : 180f,
                !completesCapture || clamped < 0.5f);
        }
    }
}
