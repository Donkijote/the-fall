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
        [TestCase(390, 844, true, AdaptiveUiProfile.MobilePortrait)]
        [TestCase(844, 390, true, AdaptiveUiProfile.MobileLandscape)]
        public void Resolve_UsesPlatformAndOrientationInsteadOfUniformWidthScaling(
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
        public void ResolvePanelInsets_MapsNotchedPortraitSafeAreaIntoPanelSpace()
        {
            var layout = AdaptiveUiFoundation.Resolve(
                new Vector2Int(390, 844),
                new Rect(0f, 34f, 390f, 776f),
                true);

            var insets = AdaptiveUiFoundation.ResolvePanelInsets(
                layout,
                new Vector2(1920f, 4155f));

            Assert.That(insets.Left, Is.EqualTo(0f).Within(0.01f));
            Assert.That(insets.Right, Is.EqualTo(0f).Within(0.01f));
            Assert.That(insets.Top, Is.EqualTo(167.35f).Within(0.1f));
            Assert.That(insets.Bottom, Is.EqualTo(167.35f).Within(0.1f));
        }

        [Test]
        public void ResolvePanelInsets_ClampsLandscapeCutoutInsets()
        {
            var layout = AdaptiveUiFoundation.Resolve(
                new Vector2Int(844, 390),
                new Rect(36f, 0f, 772f, 390f),
                true);

            var insets = AdaptiveUiFoundation.ResolvePanelInsets(
                layout,
                new Vector2(1920f, 887f));

            Assert.That(insets.Left, Is.EqualTo(81.9f).Within(0.1f));
            Assert.That(insets.Right, Is.EqualTo(81.9f).Within(0.1f));
            Assert.That(insets.Top, Is.EqualTo(0f).Within(0.01f));
            Assert.That(insets.Bottom, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ProfileAndSemanticHelpers_KeepExactlyOneStableClass()
        {
            var element = new VisualElement();

            AdaptiveUiFoundation.ApplyProfileClass(
                element,
                AdaptiveUiProfile.MobilePortrait);
            AdaptiveUiFoundation.ApplySemanticState(
                element,
                AdaptiveUiSemanticState.Selected);
            AdaptiveUiFoundation.ApplyProfileClass(
                element,
                AdaptiveUiProfile.MobileLandscape);
            AdaptiveUiFoundation.ApplySemanticState(
                element,
                AdaptiveUiSemanticState.Rejected);

            Assert.That(element.ClassListContains("profile-mobile-landscape"), Is.True);
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
        }
    }
}
