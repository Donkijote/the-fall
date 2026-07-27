using System;
using UnityEngine;

namespace TheFall.Presentation.UI
{
    public enum AdaptiveUiProfile
    {
        Desktop,
        MobilePortrait,
        MobileLandscape,
    }

    public enum AdaptiveUiSemanticState
    {
        Neutral,
        Legal,
        Inspected,
        Selected,
        Confirmed,
        Cancelled,
        Rejected,
        Blocked,
    }

    public readonly struct AdaptiveUiInsets : IEquatable<AdaptiveUiInsets>
    {
        public AdaptiveUiInsets(float left, float top, float right, float bottom)
        {
            Left = Mathf.Max(0f, left);
            Top = Mathf.Max(0f, top);
            Right = Mathf.Max(0f, right);
            Bottom = Mathf.Max(0f, bottom);
        }

        public float Left { get; }

        public float Top { get; }

        public float Right { get; }

        public float Bottom { get; }

        public bool Equals(AdaptiveUiInsets other)
        {
            return Mathf.Approximately(Left, other.Left)
                && Mathf.Approximately(Top, other.Top)
                && Mathf.Approximately(Right, other.Right)
                && Mathf.Approximately(Bottom, other.Bottom);
        }

        public override bool Equals(object obj)
        {
            return obj is AdaptiveUiInsets other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }
    }

    public readonly struct AdaptiveUiLayout : IEquatable<AdaptiveUiLayout>
    {
        public AdaptiveUiLayout(
            AdaptiveUiProfile profile,
            Vector2Int viewportPixels,
            Rect normalizedSafeArea)
        {
            Profile = profile;
            ViewportPixels = viewportPixels;
            NormalizedSafeArea = normalizedSafeArea;
        }

        public AdaptiveUiProfile Profile { get; }

        public Vector2Int ViewportPixels { get; }

        public Rect NormalizedSafeArea { get; }

        public bool Equals(AdaptiveUiLayout other)
        {
            return Profile == other.Profile
                && ViewportPixels == other.ViewportPixels
                && NormalizedSafeArea == other.NormalizedSafeArea;
        }

        public override bool Equals(object obj)
        {
            return obj is AdaptiveUiLayout other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Profile, ViewportPixels, NormalizedSafeArea);
        }
    }

    public static class AdaptiveUiFoundation
    {
        public const float MinimumEssentialTextPoints = 16f;
        public const float MinimumSecondaryTextPoints = 14f;
        public const float MinimumTouchTargetPoints = 44f;
        public const float MinimumDesktopControlPixels = 44f;
        public const float MinimumPublicCardIdentityPoints = 48f;
        public const float MinimumLocalCardIdentityPoints = 72f;
        public const float MinimumCharacterHeadPoints = 64f;
        public const float MinimumFocusStrokePoints = 3f;

        private static readonly string[] ProfileClasses =
        {
            "profile-desktop",
            "profile-mobile-portrait",
            "profile-mobile-landscape",
        };

        private static readonly string[] SemanticClasses =
        {
            "semantic-neutral",
            "semantic-legal",
            "semantic-inspected",
            "semantic-selected",
            "semantic-confirmed",
            "semantic-cancelled",
            "semantic-rejected",
            "semantic-blocked",
        };

        public static AdaptiveUiLayout Resolve(
            Vector2Int viewportPixels,
            Rect safeAreaPixels,
            bool isMobilePlatform)
        {
            if (viewportPixels.x <= 0 || viewportPixels.y <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewportPixels),
                    "Adaptive UI requires a positive viewport.");
            }

            var profile = !isMobilePlatform
                ? AdaptiveUiProfile.Desktop
                : viewportPixels.y >= viewportPixels.x
                    ? AdaptiveUiProfile.MobilePortrait
                    : AdaptiveUiProfile.MobileLandscape;

            return new AdaptiveUiLayout(
                profile,
                viewportPixels,
                NormalizeSafeArea(viewportPixels, safeAreaPixels));
        }

        public static AdaptiveUiInsets ResolvePanelInsets(
            AdaptiveUiLayout layout,
            Vector2 panelSize)
        {
            if (panelSize.x <= 0f || panelSize.y <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(panelSize),
                    "Adaptive UI requires a positive panel size.");
            }

            var safeArea = layout.NormalizedSafeArea;
            return new AdaptiveUiInsets(
                safeArea.xMin * panelSize.x,
                (1f - safeArea.yMax) * panelSize.y,
                (1f - safeArea.xMax) * panelSize.x,
                safeArea.yMin * panelSize.y);
        }

        public static void ApplyProfileClass(
            UnityEngine.UIElements.VisualElement element,
            AdaptiveUiProfile profile)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            foreach (var className in ProfileClasses)
            {
                element.EnableInClassList(className, className == ProfileClass(profile));
            }
        }

        public static void ApplySemanticState(
            UnityEngine.UIElements.VisualElement element,
            AdaptiveUiSemanticState state)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            foreach (var className in SemanticClasses)
            {
                element.EnableInClassList(className, className == SemanticClass(state));
            }
        }

        public static string ProfileClass(AdaptiveUiProfile profile)
        {
            switch (profile)
            {
                case AdaptiveUiProfile.Desktop:
                    return "profile-desktop";
                case AdaptiveUiProfile.MobilePortrait:
                    return "profile-mobile-portrait";
                case AdaptiveUiProfile.MobileLandscape:
                    return "profile-mobile-landscape";
                default:
                    throw new ArgumentOutOfRangeException(nameof(profile));
            }
        }

        public static string SemanticClass(AdaptiveUiSemanticState state)
        {
            switch (state)
            {
                case AdaptiveUiSemanticState.Neutral:
                    return "semantic-neutral";
                case AdaptiveUiSemanticState.Legal:
                    return "semantic-legal";
                case AdaptiveUiSemanticState.Inspected:
                    return "semantic-inspected";
                case AdaptiveUiSemanticState.Selected:
                    return "semantic-selected";
                case AdaptiveUiSemanticState.Confirmed:
                    return "semantic-confirmed";
                case AdaptiveUiSemanticState.Cancelled:
                    return "semantic-cancelled";
                case AdaptiveUiSemanticState.Rejected:
                    return "semantic-rejected";
                case AdaptiveUiSemanticState.Blocked:
                    return "semantic-blocked";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private static Rect NormalizeSafeArea(Vector2Int viewportPixels, Rect safeAreaPixels)
        {
            if (safeAreaPixels.width <= 0f || safeAreaPixels.height <= 0f)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            var xMin = Mathf.Clamp01(safeAreaPixels.xMin / viewportPixels.x);
            var yMin = Mathf.Clamp01(safeAreaPixels.yMin / viewportPixels.y);
            var xMax = Mathf.Clamp01(safeAreaPixels.xMax / viewportPixels.x);
            var yMax = Mathf.Clamp01(safeAreaPixels.yMax / viewportPixels.y);
            return Rect.MinMaxRect(
                Mathf.Min(xMin, xMax),
                Mathf.Min(yMin, yMax),
                Mathf.Max(xMin, xMax),
                Mathf.Max(yMin, yMax));
        }
    }
}
