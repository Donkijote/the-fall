using System;
using System.Collections.Generic;
using TheFall.Domain;
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

    /// <summary>
    /// Shared bounded table-card placement used by the workbench and integrated game.
    /// Slot selection is deterministic for replay, but deliberately avoids insertion-order rows.
    /// </summary>
    public static class AnimationTableCardLayoutEvaluator
    {
        public const int Capacity = 10;

        private static readonly Vector2[] Anchors =
        {
            new Vector2(-0.36f, -0.18f),
            new Vector2(0.03f, 0.23f),
            new Vector2(0.34f, -0.10f),
            new Vector2(-0.09f, -0.27f),
            new Vector2(-0.39f, 0.12f),
            new Vector2(0.27f, 0.24f),
            new Vector2(0.02f, -0.05f),
            new Vector2(0.40f, 0.07f),
            new Vector2(-0.23f, 0.25f),
            new Vector2(0.22f, -0.26f),
        };

        private static readonly int[] ProbeStrides = { 1, 3, 7, 9 };

        public static int ResolveAvailableIndex(Card card, IEnumerable<int> occupiedIndices)
        {
            var occupied = new bool[Capacity];
            foreach (var index in occupiedIndices)
            {
                if (index >= 0 && index < Capacity)
                {
                    occupied[index] = true;
                }
            }

            var seed = ResolveCardSeed(card);
            var start = (int)((uint)seed % Capacity);
            var stride = ProbeStrides[(int)(((uint)seed >> 8) % ProbeStrides.Length)];
            for (var offset = 0; offset < Capacity; offset++)
            {
                var candidate = (start + offset * stride) % Capacity;
                if (!occupied[candidate])
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                $"The table cannot contain more than {Capacity} rank slots.");
        }

        public static Vector3 ResolveLocalPosition(
            int layoutIndex,
            Card card,
            float baseHeight = 0f)
        {
            if (layoutIndex < 0 || layoutIndex >= Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(layoutIndex));
            }

            var seed = ResolveCardSeed(card);
            var position = ResolveAnchorPosition(layoutIndex, baseHeight);
            position.x += ResolveSignedVariation(seed) * 0.016f;
            position.z += ResolveSignedVariation(seed * 31 + 17) * 0.012f;
            return position;
        }

        private static Vector3 ResolveAnchorPosition(
            int layoutIndex,
            float baseHeight = 0f)
        {
            if (layoutIndex < 0 || layoutIndex >= Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(layoutIndex));
            }

            var anchor = Anchors[layoutIndex];
            return new Vector3(
                anchor.x,
                baseHeight + layoutIndex * 0.0015f,
                anchor.y);
        }

        public static float ResolveYaw(Card card)
        {
            return ResolveSignedVariation(ResolveCardSeed(card) * 47 + 23) * 7f;
        }

        private static int ResolveCardSeed(Card card)
        {
            return ((int)card.Suit + 1) * 397 ^ ((int)card.Rank + 1) * 97;
        }

        private static float ResolveSignedVariation(int seed)
        {
            unchecked
            {
                var value = (uint)seed;
                value ^= value >> 16;
                value *= 0x7FEB352Du;
                value ^= value >> 15;
                value *= 0x846CA68Bu;
                value ^= value >> 16;
                return (value & 0xFFFFu) / 32767.5f - 1f;
            }
        }
    }
}
