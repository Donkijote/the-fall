using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheFall.Presentation.Table
{
    public enum TableSeatingMode
    {
        OneVersusOne,
        ThreePlayer,
        TwoVersusTwo,
    }

    public enum TableCompositionProfileKind
    {
        Portrait,
        StandardLandscape,
        WideLandscape,
    }

    public readonly struct PrototypeSeatLayout
    {
        public PrototypeSeatLayout(
            int logicalIndex,
            int teamIndex,
            float anchorAngleDegrees,
            string displayName,
            bool isLocal)
        {
            LogicalIndex = logicalIndex;
            TeamIndex = teamIndex;
            AnchorAngleDegrees = anchorAngleDegrees;
            DisplayName = displayName;
            IsLocal = isLocal;
        }

        public int LogicalIndex { get; }

        public int TeamIndex { get; }

        public float AnchorAngleDegrees { get; }

        public string DisplayName { get; }

        public bool IsLocal { get; }

        public bool IsHandPrivate => !IsLocal;
    }

    public readonly struct TableCompositionProfile
    {
        public TableCompositionProfile(
            TableCompositionProfileKind kind,
            float contentScale,
            float seatRadiusMetres)
        {
            Kind = kind;
            ContentScale = contentScale;
            SeatRadiusMetres = seatRadiusMetres;
        }

        public TableCompositionProfileKind Kind { get; }

        public float ContentScale { get; }

        public float SeatRadiusMetres { get; }
    }

    /// <summary>
    /// Authored presentation data for the fixed table prototype. Logical indices always advance
    /// counter-clockwise from the local player at the bottom of the composition.
    /// </summary>
    public static class TableCompositionLayout
    {
        private static readonly IReadOnlyList<PrototypeSeatLayout> OneVersusOneSeats =
            Array.AsReadOnly(new[]
            {
                new PrototypeSeatLayout(0, 0, 0f, "P1", true),
                new PrototypeSeatLayout(1, 1, 180f, "P2", false),
            });

        private static readonly IReadOnlyList<PrototypeSeatLayout> ThreePlayerSeats =
            Array.AsReadOnly(new[]
            {
                new PrototypeSeatLayout(0, 0, 0f, "P1", true),
                new PrototypeSeatLayout(1, 1, 120f, "P2", false),
                new PrototypeSeatLayout(2, 2, 240f, "P3", false),
            });

        private static readonly IReadOnlyList<PrototypeSeatLayout> TwoVersusTwoSeats =
            Array.AsReadOnly(new[]
            {
                new PrototypeSeatLayout(0, 0, 0f, "P1", true),
                new PrototypeSeatLayout(1, 1, 90f, "P2", false),
                new PrototypeSeatLayout(2, 0, 180f, "P3", false),
                new PrototypeSeatLayout(3, 1, 270f, "P4", false),
            });

        public static IReadOnlyList<PrototypeSeatLayout> GetSeats(TableSeatingMode mode)
        {
            switch (mode)
            {
                case TableSeatingMode.OneVersusOne:
                    return OneVersusOneSeats;
                case TableSeatingMode.ThreePlayer:
                    return ThreePlayerSeats;
                case TableSeatingMode.TwoVersusTwo:
                    return TwoVersusTwoSeats;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static TableCompositionProfile ResolveProfile(Vector2Int viewportSize)
        {
            if (viewportSize.x <= 0 || viewportSize.y <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(viewportSize), "Viewport dimensions must be positive.");
            }

            if (viewportSize.x < viewportSize.y)
            {
                return new TableCompositionProfile(TableCompositionProfileKind.Portrait, 0.72f, 1.5f);
            }

            var aspect = (float)viewportSize.x / viewportSize.y;
            return aspect >= 1.7f
                ? new TableCompositionProfile(TableCompositionProfileKind.WideLandscape, 1.35f, 1.65f)
                : new TableCompositionProfile(TableCompositionProfileKind.StandardLandscape, 1.15f, 1.6f);
        }

        public static Rect NormalizeSafeArea(Vector2Int viewportSize, Rect safeAreaPixels)
        {
            if (viewportSize.x <= 0 || viewportSize.y <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(viewportSize), "Viewport dimensions must be positive.");
            }

            var xMin = Mathf.Clamp01(safeAreaPixels.xMin / viewportSize.x);
            var yMin = Mathf.Clamp01(safeAreaPixels.yMin / viewportSize.y);
            var xMax = Mathf.Clamp01(safeAreaPixels.xMax / viewportSize.x);
            var yMax = Mathf.Clamp01(safeAreaPixels.yMax / viewportSize.y);

            if (xMax <= xMin || yMax <= yMin)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Vector3 PositionAt(float angleDegrees, float radiusMetres, float heightMetres = 0f)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Sin(radians) * radiusMetres,
                heightMetres,
                -Mathf.Cos(radians) * radiusMetres);
        }
    }
}
