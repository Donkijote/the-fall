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
            Assert.That(root.Q<Button>("login-forgot-button").focusable, Is.True);
            Assert.That(root.Q<Button>("login-google-button").focusable, Is.True);
            Assert.That(root.Q<Button>("login-apple-button").focusable, Is.True);
            Assert.That(root.Q<Button>("login-create-button").focusable, Is.True);
            Assert.That(root.Q<VisualElement>(className: "icon-envelope"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "icon-padlock"), Is.Not.Null);
            Assert.That(root.Query<VisualElement>(className: "suit-token-icon").ToList(), Has.Count.EqualTo(4));
            Assert.That(controller.OpenSetup(), Is.False);
            Assert.That(controller.OpenSettings(), Is.False);
            Assert.That(controller.EnterGateway(), Is.True);
            Assert.That(controller.EnterGateway(), Is.False);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(controller.Flow.SessionNumber, Is.Zero);
            Assert.That(root.Q<VisualElement>("login-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            Assert.That(root.Q<VisualElement>("home-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q<Label>("home-objective-title").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("home-stat-target-value").text, Is.EqualTo("500"));
            Assert.That(root.Q<Label>("home-stat-mode-label"), Is.Null);
            Assert.That(root.Q<Label>("home-stat-target-label"), Is.Null);
            Assert.That(root.Q<Label>("home-stat-deck-label"), Is.Null);
            Assert.That(root.Q<Button>("home-mail-button").focusable, Is.True);
            var mailIcon = root.Q<Button>("home-mail-button").Q<VisualElement>(className: "icon-envelope");
            Assert.That(mailIcon.Q<VisualElement>(className: "hub-mail-dot"), Is.Not.Null);
            Assert.That(root.Q<Button>("home-settings-button").focusable, Is.True);
            Assert.That(root.Q<Button>("home-decks-button").focusable, Is.True);
            Assert.That(root.Q<Button>("home-bag-button").focusable, Is.True);
            Assert.That(root.Q<Button>("home-shop-button").focusable, Is.True);
            Assert.That(root.Q<Button>("home-rank-button").focusable, Is.True);
            Assert.That(root.Q<Button>("home-mail-button").tooltip, Is.Not.Empty);
            Assert.That(root.Q<Button>("home-settings-button").tooltip, Is.Not.Empty);
            Assert.That(root.Q<Button>("home-decks-button").Q<VisualElement>(className: "icon-decks"), Is.Not.Null);
            Assert.That(root.Q<Button>("home-bag-button").Q<VisualElement>(className: "icon-bag"), Is.Not.Null);
            Assert.That(root.Q<Button>("home-shop-button").Q<VisualElement>(className: "icon-shop"), Is.Not.Null);
            Assert.That(root.Q<Button>("home-rank-button").Q<VisualElement>(className: "icon-rank"), Is.Not.Null);
            Assert.That(root.Q<Button>("home-chat-global-button").focusable, Is.True);
            Assert.That(root.Q<TextField>("home-chat-input").focusable, Is.True);
            Assert.That(root.Q<VisualElement>(className: "hub-topbar"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "hub-bottombar"), Is.Not.Null);
            Assert.That(root.Q<Button>("home-start-button").focusable, Is.True);
            Assert.That(controller.OpenSettings(), Is.True);
            var settingsContent = root.Q<VisualElement>("hub-settings-content");
            var homeCasas = root.Q<Toggle>("home-settings-casas-toggle");
            var homeTrivilin = root.Q<Toggle>("home-settings-trivilin-toggle");
            var homeMasterAudio = root.Q<Toggle>("home-settings-audio-master-toggle");
            var homeReducedMotion = root.Q<Toggle>("home-settings-animation-reduced-toggle");
            Assert.That(settingsContent.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(homeCasas.value, Is.True);
            Assert.That(homeTrivilin.value, Is.False);
            Assert.That(homeCasas.focusable, Is.True);
            Assert.That(homeMasterAudio.focusable, Is.True);
            Assert.That(homeReducedMotion.focusable, Is.True);
            Assert.That(root.Q<Toggle>("home-settings-audio-effects-toggle").focusable, Is.True);
            Assert.That(root.Q<Toggle>("home-settings-audio-music-toggle").focusable, Is.True);
            Assert.That(root.Q<Toggle>("home-settings-animation-fast-toggle").focusable, Is.True);
            homeCasas.value = false;
            homeTrivilin.value = true;
            homeMasterAudio.value = false;
            homeReducedMotion.value = true;
            Assert.That(controller.AudioMasterEnabled, Is.False);
            Assert.That(controller.BeginQuest(), Is.True);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Loading));
            Assert.That(controller.Flow.Setup.CasaCantosEnabled, Is.False);
            Assert.That(controller.Flow.Setup.TrivilinWinsImmediately, Is.True);
            Assert.That(root.Q<VisualElement>("setup-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            var loadingMatch = controller.Flow.Match;
            var loadingSession = controller.Flow.SessionNumber;
            Assert.That(controller.BeginQuest(), Is.False);
            Assert.That(controller.Flow.Match, Is.SameAs(loadingMatch));
            Assert.That(controller.Flow.SessionNumber, Is.EqualTo(loadingSession));
            Assert.That(root.Q<Label>("loading-session").text, Is.Not.Empty);
            Assert.That(root.Q<Button>("loading-home-button").focusable, Is.True);
            yield return null;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            Assert.That(root.Q<VisualElement>(className: "match-header"), Is.Null);
            Assert.That(root.Q<VisualElement>(className: "interaction-strip"), Is.Null);
            Assert.That(root.Q<VisualElement>(className: "match-score-hud"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "match-status"), Is.Not.Null);
            var matchHomeButton = root.Q<Button>("match-home-button");
            Assert.That(matchHomeButton.ClassListContains("match-home-floating"), Is.True);
            Assert.That(matchHomeButton.focusable, Is.True);
            Assert.That(
                matchHomeButton.worldBound.width,
                Is.LessThan(root.Q<VisualElement>("home-screen").worldBound.width * 0.35f));
            Assert.That(root.Q<Label>("match-score").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("match-progress").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("match-canto").text, Is.Not.Empty);
            Assert.That(root.Q<VisualElement>("match-event-callout"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>("match-feedback-callout"), Is.Not.Null);
            Assert.That(root.Q<Toggle>("audio-master-toggle"), Is.Null);
            Assert.That(root.Q<Toggle>("animation-fast-toggle"), Is.Null);
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
            Assert.That(homeCasas.value, Is.False);
            Assert.That(homeTrivilin.value, Is.True);
        }

        [UnityTest]
        public IEnumerator LeavingDuringLoading_CancelsTheTransitionAndCannotRestoreAStaleSession()
        {
            yield return LoadFlow();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();

            Assert.That(controller.EnterGateway(), Is.True);
            Assert.That(controller.BeginQuest(), Is.True);
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
            Assert.That(root.Q<Button>("login-google-button").text, Is.Not.Empty);
            Assert.That(root.Q<Button>("login-create-button").text, Is.Not.Empty);
            Assert.That(screen.layout.width, Is.GreaterThan(0f));
            Assert.That(screen.layout.height, Is.GreaterThan(0f));

            Assert.That(controller.EnterGateway(), Is.True);
            Assert.That(root.Q<Button>("home-decks-button").text, Is.Not.Empty);
            Assert.That(root.Q<Button>("home-chat-system-button").text, Is.Not.Empty);
            Assert.That(controller.OpenSettings(), Is.True);
            yield return null;
            var settingsContent = root.Q<VisualElement>("hub-settings-content");
            var casasToggle = root.Q<Toggle>("home-settings-casas-toggle");
            var audioToggle = root.Q<Toggle>("home-settings-audio-master-toggle");
            Assert.That(settingsContent.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(casasToggle.text, Is.Not.Empty);
            Assert.That(casasToggle.focusable, Is.True);
            Assert.That(audioToggle.text, Is.Not.Empty);
            Assert.That(audioToggle.focusable, Is.True);
            Assert.That(casasToggle.worldBound.xMin, Is.GreaterThanOrEqualTo(screen.worldBound.xMin - 1f));
            Assert.That(casasToggle.worldBound.xMax, Is.LessThanOrEqualTo(screen.worldBound.xMax + 1f));

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
            var loginGoogle = root.Q<Button>("login-google-button");
            var homeDecks = root.Q<Button>("home-decks-button");
            var homeChatSend = root.Q<Button>("home-chat-send-button");
            var casasToggle = root.Q<Toggle>("home-settings-casas-toggle");
            var audioToggle = root.Q<Toggle>("home-settings-audio-master-toggle");

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
            Assert.That(loginGoogle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

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
            yield return null;
            Assert.That(homeDecks.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(homeChatSend.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(controller.OpenSettings(), Is.True);
            var stage = controller.Flow.Stage;

            controller.ApplyViewportForTests(
                new Vector2Int(390, 844),
                new Rect(0f, 34f, 390f, 776f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.MobilePortrait));
            Assert.That(controller.Flow.Stage, Is.EqualTo(stage));
            Assert.That(casasToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(audioToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

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
            Assert.That(audioToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            Assert.That(controller.BeginQuest(), Is.True);
            yield return null;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            yield return null;
            var scoreHud = root.Q<VisualElement>(className: "match-score-hud");
            var matchStatus = root.Q<VisualElement>(className: "match-status");
            var matchHome = root.Q<Button>("match-home-button");
            var matchSkip = root.Q<Button>("animation-skip-button");
            Assert.That(root.Q<VisualElement>(className: "match-header"), Is.Null);
            Assert.That(root.Q<VisualElement>(className: "interaction-strip"), Is.Null);
            Assert.That(scoreHud.worldBound.xMin, Is.GreaterThanOrEqualTo(screen.worldBound.xMin - 1f));
            Assert.That(scoreHud.worldBound.xMax, Is.LessThanOrEqualTo(screen.worldBound.xMax + 1f));
            Assert.That(matchStatus.worldBound.width, Is.LessThan(screen.worldBound.width * 0.5f));
            Assert.That(matchHome.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(matchSkip.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            controller.ApplyViewportForTests(
                new Vector2Int(390, 844),
                new Rect(0f, 34f, 390f, 776f),
                true);
            yield return null;
            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.MobilePortrait));
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            Assert.That(scoreHud.worldBound.xMin, Is.GreaterThanOrEqualTo(screen.worldBound.xMin - 1f));
            Assert.That(scoreHud.worldBound.xMax, Is.LessThanOrEqualTo(screen.worldBound.xMax + 1f));
            Assert.That(matchHome.worldBound.xMax, Is.LessThanOrEqualTo(screen.worldBound.xMax + 1f));
            Assert.That(matchStatus.worldBound.width, Is.GreaterThan(screen.worldBound.width * 0.9f));
            Assert.That(matchHome.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(matchSkip.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            Assert.That(controller.ReturnHome(), Is.True);
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
