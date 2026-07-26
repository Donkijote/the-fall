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
        public const float CascadeLeadInEndProgress = 0.90f;
        public const float CollectionFlipEndProgress = 0.68f;

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
            var playEndProgress = continuesToCascade
                ? CascadeLeadInEndProgress
                : CapturePlayEndProgress;
            var playProgress = Mathf.InverseLerp(
                0f,
                playEndProgress,
                clamped);
            var isCollecting = !continuesToCascade
                && clamped >= CapturePickupStartProgress;
            var collectionProgress = Mathf.InverseLerp(
                CapturePickupStartProgress,
                1f,
                clamped);
            Vector3 position;
            if (continuesToCascade)
            {
                position = isPlayedCard
                    ? EvaluateTranslation(
                        start,
                        stack,
                        playProgress,
                        easing,
                        trajectory * 0.55f)
                    : stack;
            }
            else if (!isCollecting)
            {
                position = isPlayedCard
                    ? EvaluateTranslation(
                        start,
                        stack,
                        playProgress,
                        easing,
                        trajectory * 0.55f)
                    : stack;
            }
            else
            {
                position = EvaluateTranslation(
                    stack,
                    target,
                    collectionProgress,
                    easing,
                    trajectory);
            }

            var playEased = AnimationBeatEvaluator.EvaluateEasedProgress(
                playProgress,
                easing);
            var collectionFlipProgress = Mathf.Clamp01(
                collectionProgress / CollectionFlipEndProgress);
            var collectionFlipEased = AnimationBeatEvaluator.EvaluateEasedProgress(
                collectionFlipProgress,
                easing);
            var flipDegrees = revealsPlayedCard && !isCollecting
                ? playEased * 180f
                : continuesToCascade
                    ? 180f
                    : 180f + collectionFlipEased * 180f;
            var faceUp = revealsPlayedCard && !isCollecting
                ? playProgress >= 0.5f
                : continuesToCascade
                    || !isCollecting
                    || collectionFlipProgress < 0.5f;
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
            var collectionFlipProgress = completesCapture
                ? Mathf.Clamp01(clamped / CollectionFlipEndProgress)
                : 0f;
            var collectionFlipEased = completesCapture
                ? AnimationBeatEvaluator.EvaluateEasedProgress(
                    collectionFlipProgress,
                    easing)
                : 0f;
            return new AnimationCardTreatmentPose(
                stationaryTarget
                    ? target
                    : EvaluateTranslation(start, target, clamped, easing, trajectory),
                completesCapture ? 180f + collectionFlipEased * 180f : 180f,
                !completesCapture || collectionFlipProgress < 0.5f);
        }
    }

    public readonly struct AnimationHandCardLayout
    {
        public AnimationHandCardLayout(
            float lateralOffset,
            float outwardOffset,
            float heightOffset,
            float fanYawDegrees)
        {
            LateralOffset = lateralOffset;
            OutwardOffset = outwardOffset;
            HeightOffset = heightOffset;
            FanYawDegrees = fanYawDegrees;
        }

        public float LateralOffset { get; }

        public float OutwardOffset { get; }

        public float HeightOffset { get; }

        public float FanYawDegrees { get; }
    }

    /// <summary>
    /// Shared three-card hand fan used by the workbench and integrated table.
    /// Offsets are expressed in seat-relative axes: lateral across the hand and outward from table.
    /// </summary>
    public static class AnimationHandCardLayoutEvaluator
    {
        public const float CardSpacing = 0.235f;
        public const float ArcDepth = 0.04f;
        public const float FanYawStepDegrees = 10f;
        public const float LayerStep = 0.0025f;

        public static AnimationHandCardLayout Resolve(
            int layoutIndex,
            int layoutSlotCount)
        {
            if (layoutIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(layoutIndex));
            }

            var slotCount = Math.Max(1, layoutSlotCount);
            if (layoutIndex >= slotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(layoutIndex));
            }

            var centeredIndex = layoutIndex - (slotCount - 1) * 0.5f;
            return new AnimationHandCardLayout(
                centeredIndex * CardSpacing,
                centeredIndex * centeredIndex * ArcDepth,
                layoutIndex * LayerStep,
                centeredIndex * FanYawStepDegrees);
        }

        public static Vector3 ResolveLocalPosition(
            int layoutIndex,
            int layoutSlotCount,
            Seat seat)
        {
            var layout = Resolve(layoutIndex, layoutSlotCount);
            switch (seat)
            {
                case Seat.First:
                    return new Vector3(
                        layout.LateralOffset,
                        layout.HeightOffset,
                        -layout.OutwardOffset);
                case Seat.Second:
                    return new Vector3(
                        layout.LateralOffset,
                        layout.HeightOffset,
                        layout.OutwardOffset);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(seat),
                        seat,
                        "The first-playable hand supports the two 1v1 seats.");
            }
        }

        public static float ResolveRestingYawDegrees(
            int layoutIndex,
            int layoutSlotCount,
            Seat seat)
        {
            var yaw = Resolve(layoutIndex, layoutSlotCount).FanYawDegrees;
            switch (seat)
            {
                case Seat.First:
                    return yaw;
                case Seat.Second:
                    return -yaw;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(seat),
                        seat,
                        "The first-playable hand supports the two 1v1 seats.");
            }
        }
    }

    /// <summary>
    /// Places the live draw deck beside the dealer, on that player's own right.
    /// </summary>
    public static class AnimationDealerDeckLayoutEvaluator
    {
        public const float LateralOffset = 0.72f;
        public const float SeatOffset = 0.62f;

        public static Vector3 Resolve(Seat dealerSeat, float height)
        {
            switch (dealerSeat)
            {
                case Seat.First:
                    return new Vector3(LateralOffset, height, -SeatOffset);
                case Seat.Second:
                    return new Vector3(-LateralOffset, height, SeatOffset);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(dealerSeat),
                        dealerSeat,
                        "The first-playable deck supports the two 1v1 seats.");
            }
        }
    }

    /// <summary>
    /// Shared bounded table-card placement used by the workbench and integrated game.
    /// Slot selection is deterministic for replay, but deliberately avoids insertion-order rows.
    /// </summary>
    public static class AnimationTableCardLayoutEvaluator
    {
        public const int Capacity = 16;

        private static readonly Vector2[] Anchors =
        {
            new Vector2(-0.13f, -0.15f),
            new Vector2(0.13f, -0.15f),
            new Vector2(-0.13f, 0.15f),
            new Vector2(0.13f, 0.15f),
            new Vector2(-0.45f, -0.31f),
            new Vector2(-0.14f, -0.28f),
            new Vector2(0.18f, -0.31f),
            new Vector2(0.46f, -0.27f),
            new Vector2(-0.40f, -0.01f),
            new Vector2(-0.11f, 0.02f),
            new Vector2(0.20f, -0.02f),
            new Vector2(0.47f, 0.01f),
            new Vector2(-0.46f, 0.27f),
            new Vector2(-0.17f, 0.31f),
            new Vector2(0.14f, 0.28f),
            new Vector2(0.44f, 0.31f),
        };

        private static readonly int[] ProbeStrides = { 1, 3, 7, 9 };

        public static int ResolveAvailableIndex(
            Card card,
            IEnumerable<int> occupiedIndices,
            bool preferOpeningGrid = false)
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
            if (preferOpeningGrid)
            {
                for (var openingIndex = 0; openingIndex < 4; openingIndex++)
                {
                    if (!occupied[openingIndex])
                    {
                        return openingIndex;
                    }
                }
            }

            var bestCandidate = -1;
            var bestClearance = float.NegativeInfinity;
            var firstCandidate = preferOpeningGrid ? 0 : 4;
            var candidateCount = preferOpeningGrid ? Capacity : Capacity - firstCandidate;
            for (var offset = 0; offset < candidateCount; offset++)
            {
                var candidate = firstCandidate
                    + (start + offset) % candidateCount;
                if (occupied[candidate])
                {
                    continue;
                }

                var clearance = float.PositiveInfinity;
                for (var occupiedIndex = 0; occupiedIndex < Capacity; occupiedIndex++)
                {
                    if (!occupied[occupiedIndex])
                    {
                        continue;
                    }

                    clearance = Mathf.Min(
                        clearance,
                        Vector2.SqrMagnitude(
                            Anchors[candidate] - Anchors[occupiedIndex]));
                }

                if (clearance > bestClearance)
                {
                    bestClearance = clearance;
                    bestCandidate = candidate;
                }
            }

            if (bestCandidate >= 0)
            {
                return bestCandidate;
            }

            if (!preferOpeningGrid)
            {
                for (var offset = 0; offset < 4; offset++)
                {
                    var candidate = (start + offset * stride) % 4;
                    if (!occupied[candidate])
                    {
                        return candidate;
                    }
                }
            }

            throw new InvalidOperationException(
                "The table has no remaining presentation slot.");
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

            var position = ResolveAnchorPosition(layoutIndex, baseHeight);
            if (layoutIndex < 4)
            {
                return position;
            }

            var seed = ResolveCardSeed(card);
            position.x += ResolveSignedVariation(seed) * 0.010f;
            position.z += ResolveSignedVariation(seed * 31 + 17) * 0.008f;
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

        public static float ResolveYaw(Card card, int layoutIndex = -1)
        {
            if (layoutIndex >= 0 && layoutIndex < 4)
            {
                return 0f;
            }

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
