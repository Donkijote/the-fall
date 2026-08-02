using NUnit.Framework;
using TheFall.Presentation.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheFall.Tests.EditMode
{
    public sealed class AdaptiveUiFoundationEditModeTests
    {
        [TestCase(1280, 720, false, AdaptiveUiProfile.Desktop)]
        [TestCase(1440, 900, false, AdaptiveUiProfile.Desktop)]
        [TestCase(1920, 1080, false, AdaptiveUiProfile.Desktop)]
        [TestCase(2560, 1440, false, AdaptiveUiProfile.Desktop)]
        [TestCase(390, 844, true, AdaptiveUiProfile.PhoneLandscape)]
        [TestCase(844, 390, true, AdaptiveUiProfile.PhoneLandscape)]
        [TestCase(926, 428, true, AdaptiveUiProfile.PhoneLandscape)]
        [TestCase(1024, 768, true, AdaptiveUiProfile.TabletLandscape)]
        [TestCase(1366, 1024, true, AdaptiveUiProfile.TabletLandscape)]
        public void Resolve_UsesDesktopPhoneOrTabletLandscapeProfile(
            int width,
            int height,
            bool isMobilePlatform,
            AdaptiveUiProfile expected)
        {
            var layout = AdaptiveUiFoundation.Resolve(
                new Vector2Int(width, height),
                new Rect(0f, 0f, width, height),
                isMobilePlatform);

            Assert.That(layout.Profile, Is.EqualTo(expected));
        }

        [Test]
        public void ProfileAndSemanticHelpers_KeepExactlyOneStableClass()
        {
            var element = new VisualElement();

            element.AddToClassList("profile-mobile-portrait");
            AdaptiveUiFoundation.ApplySemanticState(
                element,
                AdaptiveUiSemanticState.Selected);
            AdaptiveUiFoundation.ApplyProfileClass(
                element,
                AdaptiveUiProfile.PhoneLandscape);
            AdaptiveUiFoundation.ApplySemanticState(
                element,
                AdaptiveUiSemanticState.Rejected);

            Assert.That(element.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(element.ClassListContains("profile-phone-landscape"), Is.True);
            Assert.That(element.ClassListContains("profile-tablet-landscape"), Is.False);
            Assert.That(element.ClassListContains("profile-mobile-portrait"), Is.False);
            Assert.That(element.ClassListContains("semantic-rejected"), Is.True);
            Assert.That(element.ClassListContains("semantic-selected"), Is.False);

            AdaptiveUiFoundation.ApplySemanticState(
                element,
                AdaptiveUiSemanticState.Inspected);
            Assert.That(element.ClassListContains("semantic-inspected"), Is.True);
            AdaptiveUiFoundation.ApplySemanticState(
                element,
                AdaptiveUiSemanticState.Cancelled);
            Assert.That(element.ClassListContains("semantic-cancelled"), Is.True);
            Assert.That(element.ClassListContains("semantic-inspected"), Is.False);

            AdaptiveUiFoundation.ApplyProfileClass(
                element,
                AdaptiveUiProfile.TabletLandscape);
            Assert.That(element.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(element.ClassListContains("profile-phone-landscape"), Is.False);
            Assert.That(element.ClassListContains("profile-tablet-landscape"), Is.True);
        }

        [Test]
        public void Resolve_UsesViewportAspectWhenSafeAreaIsUnavailable()
        {
            var layout = AdaptiveUiFoundation.Resolve(
                new Vector2Int(844, 390),
                Rect.zero,
                true);

            Assert.That(layout.Profile, Is.EqualTo(AdaptiveUiProfile.PhoneLandscape));
            Assert.That(layout.NormalizedSafeArea, Is.EqualTo(new Rect(0f, 0f, 1f, 1f)));
        }

        [Test]
        public void MinimumTokens_PreserveReadableAndTouchableFoundation()
        {
            Assert.That(AdaptiveUiFoundation.MinimumEssentialTextPoints, Is.GreaterThanOrEqualTo(16f));
            Assert.That(AdaptiveUiFoundation.MinimumSecondaryTextPoints, Is.GreaterThanOrEqualTo(14f));
            Assert.That(AdaptiveUiFoundation.MinimumTouchTargetPoints, Is.GreaterThanOrEqualTo(44f));
            Assert.That(AdaptiveUiFoundation.MinimumDesktopControlPixels, Is.GreaterThanOrEqualTo(44f));
            Assert.That(AdaptiveUiFoundation.MinimumLocalCardIdentityPoints, Is.GreaterThan(
                AdaptiveUiFoundation.MinimumPublicCardIdentityPoints));
            Assert.That(AdaptiveUiFoundation.MinimumFocusStrokePoints, Is.GreaterThanOrEqualTo(3f));
            Assert.That(AdaptiveUiFoundation.MaximumLocalCardViewportHeight, Is.LessThanOrEqualTo(0.2f));
            Assert.That(AdaptiveUiFoundation.MaximumPublicCardViewportHeight,
                Is.LessThan(AdaptiveUiFoundation.MaximumLocalCardViewportHeight));
            Assert.That(AdaptiveUiFoundation.MaximumDealerCardViewportHeight,
                Is.LessThan(AdaptiveUiFoundation.MaximumLocalCardViewportHeight));
        }
    }
}
