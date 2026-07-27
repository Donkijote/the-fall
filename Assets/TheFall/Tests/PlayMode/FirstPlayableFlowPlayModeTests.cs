using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Presentation.Bootstrap;
using TheFall.Presentation.Match;
using TheFall.Presentation.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TheFall.Tests.PlayMode
{
    public sealed class FirstPlayableFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator PlayerCanCompleteReplayAndLeaveTheFirstPlayableThroughTheUiAdapter()
        {
            yield return LoadFlow();
            yield return LocalizationSettings.InitializationOperation;
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var root = controller.GetComponent<UIDocument>().rootVisualElement;

            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(controller.Flow.SessionNumber, Is.Zero);
            Assert.That(controller.HasEnteredGateway, Is.False);
            Assert.That(root.Q<VisualElement>("login-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q<VisualElement>("home-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            Assert.That(root.Q<TextField>("login-email").focusable, Is.True);
            Assert.That(root.Q<TextField>("login-password").isPasswordField, Is.True);
            Assert.That(root.Q<Button>("login-enter-button").focusable, Is.True);
            Assert.That(controller.OpenSetup(), Is.False);
            Assert.That(controller.EnterGateway(), Is.True);
            Assert.That(controller.EnterGateway(), Is.False);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(controller.Flow.SessionNumber, Is.Zero);
            Assert.That(root.Q<VisualElement>("login-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            Assert.That(root.Q<VisualElement>("home-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q<Label>("home-step-setup").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("home-step-match").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("home-step-result").text, Is.Not.Empty);
            Assert.That(root.Q<Button>("home-start-button").focusable, Is.True);
            Assert.That(controller.OpenSetup(), Is.True);
            Assert.That(root.Q<Toggle>("casas-toggle").value, Is.True);
            Assert.That(root.Q<Toggle>("trivilin-toggle").value, Is.False);
            Assert.That(root.Q<Label>("casas-state").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("trivilin-state").text, Is.Not.Empty);
            Assert.That(controller.StartMatch(), Is.True);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Loading));
            var loadingMatch = controller.Flow.Match;
            var loadingSession = controller.Flow.SessionNumber;
            Assert.That(controller.StartMatch(), Is.False);
            Assert.That(controller.Flow.Match, Is.SameAs(loadingMatch));
            Assert.That(controller.Flow.SessionNumber, Is.EqualTo(loadingSession));
            Assert.That(root.Q<Label>("loading-session").text, Is.Not.Empty);
            Assert.That(root.Q<Button>("loading-home-button").focusable, Is.True);
            yield return null;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            table.SkipPresentation();
            Assert.That(table.AudioPresenter.ActiveCue, Is.Null);

            var humanIntentCount = 0;
            while (controller.Flow.Stage == FirstPlayableFlowStage.Match && humanIntentCount < 5000)
            {
                var legal = controller.Flow.Match.GetHumanLegalIntents();
                var intent = ChooseHumanIntent(controller.Flow.Match.State, legal);
                Assert.That(controller.SubmitHumanIntent(intent), Is.True);
                Assert.That(table.IsPresentationBusy, Is.True);
                table.SkipPresentation();
                Assert.That(table.AudioPresenter.ActiveCue, Is.Null);
                humanIntentCount++;
            }

            Assert.That(humanIntentCount, Is.LessThan(5000));
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Result));
            var completedMatch = controller.Flow.Match;
            var resultEyebrow = root.Q<Label>("result-eyebrow");
            Assert.That(resultEyebrow.resolvedStyle.whiteSpace, Is.EqualTo(WhiteSpace.NoWrap));
            Assert.That(root.Q<Label>("result-outcome").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("result-score").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("result-rules").text, Is.Not.Empty);
            Assert.That(root.Q<Button>("result-replay-button").focusable, Is.True);
            Assert.That(root.Q<Button>("result-home-button").focusable, Is.True);

            Assert.That(controller.Replay(), Is.True);
            Assert.That(controller.Flow.Match, Is.Not.SameAs(completedMatch));
            Assert.That(table.AudioPresenter.ActiveCue, Is.Null);
            yield return null;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            Assert.That(controller.ReturnHome(), Is.True);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(table.AudioPresenter.ActiveCue, Is.Null);
        }

        [UnityTest]
        public IEnumerator LeavingDuringLoading_CancelsTheTransitionAndCannotRestoreAStaleSession()
        {
            yield return LoadFlow();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();

            Assert.That(controller.EnterGateway(), Is.True);
            Assert.That(controller.OpenSetup(), Is.True);
            Assert.That(controller.StartMatch(), Is.True);
            var abandonedSession = controller.Flow.SessionNumber;

            Assert.That(controller.ReturnHome(), Is.True);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);

            yield return null;

            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(controller.Flow.SessionNumber, Is.EqualTo(abandonedSession));
            Assert.That(controller.Flow.Setup.CasaCantosEnabled, Is.True);
            Assert.That(controller.Flow.Setup.TrivilinWinsImmediately, Is.False);
        }

        [UnityTest]
        public IEnumerator PseudoLocalization_RemainsExpandedReadableAndKeyboardFocusable()
        {
            yield return LoadFlow();
            yield return LocalizationSettings.InitializationOperation;
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var root = controller.GetComponent<UIDocument>().rootVisualElement;
            var screen = root.Q<VisualElement>("home-screen");
            var description = root.Q<Label>("login-description");
            var enter = root.Q<Button>("login-enter-button");
            var proof = root.Q<Label>("login-proof");

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("en"));
            controller.Render();
            var englishText = description.text;

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales
                .OfType<PseudoLocale>()
                .Single(locale => locale.Identifier.Code == "qps-ploc");
            yield return null;
            controller.Render();

            Assert.That(description.text, Is.Not.Empty);
            Assert.That(description.text, Is.Not.EqualTo(englishText));
            Assert.That(enter.text, Is.Not.Empty);
            Assert.That(enter.focusable, Is.True);
            Assert.That(proof.text, Is.Not.Empty);
            Assert.That(screen.layout.width, Is.GreaterThan(0f));
            Assert.That(screen.layout.height, Is.GreaterThan(0f));

            Assert.That(controller.EnterGateway(), Is.True);
            Assert.That(controller.OpenSetup(), Is.True);
            yield return null;
            var setupStage = root.Q<VisualElement>("setup-stage");
            var casasToggle = root.Q<Toggle>("casas-toggle");
            var casasState = root.Q<Label>("casas-state");
            var startMatch = root.Q<Button>("setup-start-button");
            Assert.That(setupStage.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(casasToggle.text, Is.Not.Empty);
            Assert.That(casasToggle.focusable, Is.True);
            Assert.That(casasState.text, Is.Not.Empty);
            Assert.That(startMatch.text, Is.Not.Empty);
            Assert.That(startMatch.worldBound.xMin, Is.GreaterThanOrEqualTo(screen.worldBound.xMin - 1f));
            Assert.That(startMatch.worldBound.xMax, Is.LessThanOrEqualTo(screen.worldBound.xMax + 1f));

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("en"));
        }

        [UnityTest]
        public IEnumerator AdaptiveProfiles_PreserveFlowAndApplyMobileSafeAreas()
        {
            yield return LoadFlow();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var root = controller.GetComponent<UIDocument>().rootVisualElement;
            var screen = root.Q<VisualElement>("home-screen");
            var enterGateway = root.Q<Button>("login-enter-button");
            var casasToggle = root.Q<Toggle>("casas-toggle");
            var startMatch = root.Q<Button>("setup-start-button");

            controller.ApplyViewportForTests(
                new Vector2Int(390, 844),
                new Rect(0f, 34f, 390f, 776f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.MobilePortrait));
            Assert.That(screen.ClassListContains("profile-mobile-portrait"), Is.True);
            Assert.That(controller.CurrentAdaptivePanelInsets.Top, Is.GreaterThan(0f));
            Assert.That(controller.CurrentAdaptivePanelInsets.Bottom, Is.GreaterThan(0f));
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(enterGateway.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            controller.ApplyViewportForTests(
                new Vector2Int(844, 390),
                new Rect(36f, 0f, 772f, 390f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.MobileLandscape));
            Assert.That(screen.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(controller.CurrentAdaptivePanelInsets.Left, Is.GreaterThan(0f));
            Assert.That(controller.CurrentAdaptivePanelInsets.Right, Is.GreaterThan(0f));
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(enterGateway.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            Assert.That(controller.EnterGateway(), Is.True);
            Assert.That(controller.OpenSetup(), Is.True);
            var stage = controller.Flow.Stage;

            controller.ApplyViewportForTests(
                new Vector2Int(390, 844),
                new Rect(0f, 34f, 390f, 776f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.MobilePortrait));
            Assert.That(controller.Flow.Stage, Is.EqualTo(stage));
            Assert.That(casasToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(startMatch.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            controller.ApplyViewportForTests(
                new Vector2Int(844, 390),
                new Rect(36f, 0f, 772f, 390f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.MobileLandscape));
            Assert.That(screen.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(screen.ClassListContains("profile-mobile-portrait"), Is.False);
            Assert.That(controller.CurrentAdaptivePanelInsets.Left, Is.GreaterThan(0f));
            Assert.That(controller.CurrentAdaptivePanelInsets.Right, Is.GreaterThan(0f));
            Assert.That(controller.Flow.Stage, Is.EqualTo(stage));
            Assert.That(casasToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(startMatch.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            controller.ClearViewportOverrideForTests();
        }

        private static IEnumerator LoadFlow()
        {
            if (CompositionRoot.Instance != null)
            {
                Object.Destroy(CompositionRoot.Instance.gameObject);
                yield return null;
            }

            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
            var deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != "Home" && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Home"));
            Assert.That(Object.FindAnyObjectByType<FirstPlayableFlowController>(), Is.Not.Null);
        }

        private static PlayerIntent ChooseHumanIntent(MatchState state, IReadOnlyList<PlayerIntent> legal)
        {
            if (state.Phase == MatchPhase.AwaitingDealerChoice)
            {
                return legal.OfType<ChooseDealOptionsIntent>()
                    .Single(item => item.DealHandsBeforeTable && item.OpeningPattern == OpeningPattern.Ascending);
            }

            return legal.OfType<PlayCardIntent>().FirstOrDefault() ?? legal[0];
        }
    }
}
