using System;
using UnityEngine;

namespace TheFall.Presentation.UI
{
    public enum AdaptiveUiProfile
    {
        Desktop,
        PhoneLandscape,
        TabletLandscape,
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
        public const float MinimumPublicCardIdentityPoints = 26f;
        public const float MinimumLocalCardIdentityPoints = 34f;
        public const float MinimumCharacterHeadPoints = 64f;
        public const float MinimumFocusStrokePoints = 3f;
        public const float MaximumLocalCardViewportHeight = 0.19f;
        public const float MaximumPublicCardViewportHeight = 0.15f;
        public const float MaximumDealerCardViewportHeight = 0.18f;

        private static readonly string[] ProfileClasses =
        {
            "profile-desktop",
            "profile-mobile-portrait",
            "profile-mobile-landscape",
            "profile-phone-landscape",
            "profile-tablet-landscape",
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

            var profile = AdaptiveUiProfile.Desktop;
            if (isMobilePlatform)
            {
                var profileWidth = safeAreaPixels.width > 0f
                    ? safeAreaPixels.width
                    : viewportPixels.x;
                var profileHeight = safeAreaPixels.height > 0f
                    ? safeAreaPixels.height
                    : viewportPixels.y;
                var longSide = Mathf.Max(profileWidth, profileHeight);
                var shortSide = Mathf.Max(1f, Mathf.Min(profileWidth, profileHeight));
                profile = longSide / shortSide <= 1.7f
                    ? AdaptiveUiProfile.TabletLandscape
                    : AdaptiveUiProfile.PhoneLandscape;
            }

            return new AdaptiveUiLayout(
                profile,
                viewportPixels,
                NormalizeSafeArea(viewportPixels, safeAreaPixels));
        }

        public static void ApplyProfileClass(
            UnityEngine.UIElements.VisualElement element,
            AdaptiveUiProfile profile)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            RemoveProfileClasses(element);
            switch (profile)
            {
                case AdaptiveUiProfile.Desktop:
                    element.AddToClassList("profile-desktop");
                    break;
                case AdaptiveUiProfile.PhoneLandscape:
                    element.AddToClassList("profile-mobile-landscape");
                    element.AddToClassList("profile-phone-landscape");
                    break;
                case AdaptiveUiProfile.TabletLandscape:
                    element.AddToClassList("profile-mobile-landscape");
                    element.AddToClassList("profile-tablet-landscape");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(profile));
            }
        }

        public static void RemoveProfileClasses(
            UnityEngine.UIElements.VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            foreach (var className in ProfileClasses)
            {
                element.RemoveFromClassList(className);
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
                case AdaptiveUiProfile.PhoneLandscape:
                    return "profile-phone-landscape";
                case AdaptiveUiProfile.TabletLandscape:
                    return "profile-tablet-landscape";
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
