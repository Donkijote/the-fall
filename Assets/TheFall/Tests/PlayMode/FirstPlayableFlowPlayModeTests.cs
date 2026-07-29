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
            Assert.That(controller.CurrentScreenKind, Is.EqualTo(FirstPlayableScreenKind.Login));
            Assert.That(controller.MountedScreenCount, Is.EqualTo(1));
            Assert.That(root.Q<VisualElement>("login-stage").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q<VisualElement>("home-stage"), Is.Null);
            Assert.That(root.Q<TextField>("login-email").focusable, Is.True);
            Assert.That(root.Q<TextField>("login-password").isPasswordField, Is.True);
            Assert.That(root.Q<Button>("login-enter-button").focusable, Is.True);
            Assert.That(root.Q<Button>("login-forgot-button").focusable, Is.True);
            Assert.That(root.Q<Button>("login-google-button").focusable, Is.True);
            Assert.That(root.Q<Button>("login-apple-button").focusable, Is.True);
            Assert.That(root.Q<Button>("login-create-button").focusable, Is.True);
            Assert.That(root.Q<VisualElement>(className: "login-enter-icon"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "login-google-mark"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "login-apple-mark"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "icon-envelope"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "icon-padlock"), Is.Not.Null);
            Assert.That(root.Query<VisualElement>(className: "suit-token-icon").ToList(), Has.Count.EqualTo(4));
            Assert.That(controller.OpenSetup(), Is.False);
            Assert.That(controller.OpenSettings(), Is.False);
            Assert.That(controller.EnterGateway(), Is.True);
            yield return WaitForScene("Hub");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            root = controller.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(controller.EnterGateway(), Is.False);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(controller.Flow.SessionNumber, Is.Zero);
            Assert.That(controller.CurrentScreenKind, Is.EqualTo(FirstPlayableScreenKind.Hub));
            Assert.That(controller.MountedScreenCount, Is.EqualTo(1));
            Assert.That(root.Q<VisualElement>("login-stage"), Is.Null);
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
            Assert.That(root.Q<Button>("home-mail-button").text, Is.Empty);
            Assert.That(root.Q<Button>("home-settings-button").text, Is.Empty);
            Assert.That(root.Q<Button>("home-chat-send-button").text, Is.Empty);
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
            yield return WaitForScene("Match");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            root = controller.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Loading));
            Assert.That(controller.CurrentScreenKind, Is.EqualTo(FirstPlayableScreenKind.Loading));
            Assert.That(controller.MountedScreenCount, Is.EqualTo(1));
            Assert.That(controller.Flow.Setup.CasaCantosEnabled, Is.False);
            Assert.That(controller.Flow.Setup.TrivilinWinsImmediately, Is.True);
            Assert.That(root.Q<VisualElement>("setup-stage"), Is.Null);
            Assert.That(root.Q<VisualElement>("home-stage"), Is.Null);
            var loadingMatch = controller.Flow.Match;
            var loadingSession = controller.Flow.SessionNumber;
            Assert.That(controller.BeginQuest(), Is.False);
            Assert.That(controller.Flow.Match, Is.SameAs(loadingMatch));
            Assert.That(controller.Flow.SessionNumber, Is.EqualTo(loadingSession));
            Assert.That(root.Q<Label>("loading-session").text, Is.Not.Empty);
            Assert.That(root.Q<Button>("loading-home-button").focusable, Is.True);
            var matchReadyDeadline = Time.realtimeSinceStartup + 10f;
            while (controller.Flow.Stage == FirstPlayableFlowStage.Loading
                && Time.realtimeSinceStartup < matchReadyDeadline)
            {
                yield return null;
            }

            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            Assert.That(controller.CurrentScreenKind, Is.EqualTo(FirstPlayableScreenKind.Match));
            Assert.That(controller.MountedScreenCount, Is.EqualTo(1));
            Assert.That(root.Q<VisualElement>("loading-stage"), Is.Null);
            Assert.That(root.Q<VisualElement>(className: "match-header"), Is.Null);
            Assert.That(root.Q<VisualElement>(className: "interaction-strip"), Is.Null);
            Assert.That(root.Q<VisualElement>(className: "match-score-hud"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>(className: "match-status"), Is.Not.Null);
            var matchHomeButton = root.Q<Button>("match-home-button");
            Assert.That(matchHomeButton.ClassListContains("match-home-floating"), Is.True);
            Assert.That(matchHomeButton.focusable, Is.True);
            Assert.That(
                matchHomeButton.worldBound.width,
                Is.LessThan(root.worldBound.width * 0.35f));
            Assert.That(root.Q<Label>("match-score").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("match-progress").text, Is.Not.Empty);
            Assert.That(root.Q<Label>("match-canto").text, Is.Not.Empty);
            Assert.That(root.Q<VisualElement>("match-event-callout"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>("match-feedback-callout"), Is.Not.Null);
            Assert.That(root.Q<Button>("dealer-options-button").text, Is.Empty);
            Assert.That(root.Q<Button>("canto-options-button").text, Is.Empty);
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
            Assert.That(controller.CurrentScreenKind, Is.EqualTo(FirstPlayableScreenKind.Result));
            Assert.That(controller.MountedScreenCount, Is.EqualTo(1));
            Assert.That(root.Q<VisualElement>("match-stage"), Is.Null);
            yield return null;
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
            Assert.That(table.AudioPresenter.ActiveCue, Is.Null);
            yield return WaitForScene("Hub");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            root = controller.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(controller.CurrentScreenKind, Is.EqualTo(FirstPlayableScreenKind.Hub));
            Assert.That(root.Q<Toggle>("home-settings-casas-toggle").value, Is.False);
            Assert.That(root.Q<Toggle>("home-settings-trivilin-toggle").value, Is.True);
        }

        [UnityTest]
        public IEnumerator LeavingDuringLoading_CancelsTheTransitionAndCannotRestoreAStaleSession()
        {
            yield return LoadFlow();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();

            Assert.That(controller.EnterGateway(), Is.True);
            yield return WaitForScene("Hub");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            Assert.That(controller.BeginQuest(), Is.True);
            yield return WaitForScene("Match");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var flow = controller.Flow;
            var abandonedSession = controller.Flow.SessionNumber;

            Assert.That(controller.ReturnHome(), Is.True);
            yield return WaitForScene("Hub");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            Assert.That(flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(flow.Match, Is.Null);

            yield return null;

            Assert.That(flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(flow.Match, Is.Null);
            Assert.That(flow.SessionNumber, Is.EqualTo(abandonedSession));
            Assert.That(flow.Setup.CasaCantosEnabled, Is.True);
            Assert.That(flow.Setup.TrivilinWinsImmediately, Is.False);
        }

        [UnityTest]
        public IEnumerator PseudoLocalization_RemainsExpandedReadableAndKeyboardFocusable()
        {
            yield return LoadFlow();
            yield return LocalizationSettings.InitializationOperation;
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var root = controller.GetComponent<UIDocument>().rootVisualElement;
            var screen = root;
            var description = root.Q<Label>("login-description");
            var enter = root.Q<Button>("login-enter-button");
            var enterLabel = root.Q<Label>("login-enter-label");
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
            Assert.That(enter.text, Is.Empty);
            Assert.That(enterLabel.text, Is.Not.Empty);
            Assert.That(enter.focusable, Is.True);
            Assert.That(proof.text, Is.Not.Empty);
            Assert.That(root.Q<Button>("login-google-button").text, Is.Empty);
            Assert.That(root.Q<Button>("login-google-button").tooltip, Is.Not.Empty);
            Assert.That(root.Q<Button>("login-apple-button").text, Is.Empty);
            Assert.That(root.Q<Button>("login-apple-button").tooltip, Is.Not.Empty);
            Assert.That(root.Q<Button>("login-create-button").text, Is.Not.Empty);
            Assert.That(screen.layout.width, Is.GreaterThan(0f));
            Assert.That(screen.layout.height, Is.GreaterThan(0f));

            Assert.That(controller.EnterGateway(), Is.True);
            yield return WaitForScene("Hub");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            root = controller.GetComponent<UIDocument>().rootVisualElement;
            screen = root;
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
        public IEnumerator AdaptiveProfiles_PreserveFlowWithScreenOwnedSafeAreas()
        {
            yield return LoadFlow();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var root = controller.GetComponent<UIDocument>().rootVisualElement;
            var screen = root;
            var enterGateway = root.Q<Button>("login-enter-button");
            var loginGoogle = root.Q<Button>("login-google-button");
            var loginStage = root.Q<VisualElement>("login-stage");
            var loginBackdrop = root.Q<VisualElement>(className: "login-backdrop");
            var loginContent = root.Q<VisualElement>("login-content");
            var loginPanel = root.Q<VisualElement>(className: "login-panel");

            controller.ApplyViewportForTests(
                new Vector2Int(844, 390),
                new Rect(36f, 0f, 772f, 390f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.PhoneLandscape));
            Assert.That(screen.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(screen.ClassListContains("profile-phone-landscape"), Is.True);
            Assert.That(screen.ClassListContains("profile-mobile-portrait"), Is.False);
            Assert.That(root.Q<VisualElement>("safe-area-content-container"), Is.Not.Null);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(enterGateway.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(loginGoogle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(loginGoogle.worldBound.width, Is.EqualTo(loginGoogle.worldBound.height).Within(4f));
            var loginApple = root.Q<Button>("login-apple-button");
            Assert.That(loginApple.worldBound.width, Is.EqualTo(loginApple.worldBound.height).Within(4f));
            Assert.That(loginStage.worldBound.xMin, Is.EqualTo(screen.worldBound.xMin).Within(1f));
            Assert.That(loginStage.worldBound.xMax, Is.EqualTo(screen.worldBound.xMax).Within(1f));
            Assert.That(loginBackdrop.worldBound.xMin, Is.EqualTo(loginStage.worldBound.xMin).Within(1f));
            Assert.That(loginBackdrop.worldBound.xMax, Is.EqualTo(loginStage.worldBound.xMax).Within(1f));
            Assert.That(loginContent.worldBound.xMin, Is.GreaterThanOrEqualTo(loginStage.worldBound.xMin - 1f));
            Assert.That(loginContent.worldBound.xMax, Is.LessThanOrEqualTo(loginStage.worldBound.xMax + 1f));
            Assert.That(loginPanel.worldBound.yMin, Is.GreaterThanOrEqualTo(loginContent.worldBound.yMin - 1f));
            Assert.That(loginPanel.worldBound.yMax, Is.LessThanOrEqualTo(loginContent.worldBound.yMax + 1f));
            var loginTokens = root.Query<VisualElement>(className: "suit-token").ToList();
            Assert.That(loginTokens, Has.Count.EqualTo(4));
            foreach (var token in loginTokens)
            {
                var icon = token.Q<VisualElement>(className: "suit-token-icon");
                Assert.That(icon.worldBound.yMin, Is.GreaterThan(token.worldBound.yMin));
                Assert.That(icon.worldBound.yMax, Is.LessThan(token.worldBound.yMax));
            }

            var loginInputs = root.Query<VisualElement>(className: "login-input-shell").ToList();
            Assert.That(loginInputs, Has.Count.EqualTo(2));
            Assert.That(enterGateway.worldBound.yMin, Is.GreaterThan(loginInputs[1].worldBound.yMax));

            controller.ApplyViewportForTests(
                new Vector2Int(926, 428),
                new Rect(47f, 0f, 832f, 428f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.PhoneLandscape));
            Assert.That(screen.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(enterGateway.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            Assert.That(controller.EnterGateway(), Is.True);
            yield return WaitForScene("Hub");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            root = controller.GetComponent<UIDocument>().rootVisualElement;
            screen = root;
            controller.ApplyViewportForTests(
                new Vector2Int(926, 428),
                new Rect(47f, 0f, 832f, 428f),
                true);
            yield return null;
            Assert.That(root.Q<VisualElement>("safe-area-content-container"), Is.Not.Null);
            var homeDecks = root.Q<Button>("home-decks-button");
            var homeChatSend = root.Q<Button>("home-chat-send-button");
            var hubLayout = root.Q<VisualElement>(className: "hub-layout");
            var hubTopbar = root.Q<VisualElement>(className: "hub-topbar");
            var hubObjective = root.Q<VisualElement>(className: "hub-objective");
            var hubDock = root.Q<VisualElement>(className: "hub-dock");
            var hubChat = root.Q<VisualElement>(className: "hub-chat");
            var hubChatTabs = root.Q<VisualElement>(className: "hub-chat-tabs");
            var hubChatInputRow = root.Q<VisualElement>(className: "hub-chat-input-row");
            var hubChatTabsList = root.Query<Button>(className: "hub-chat-tab").ToList();
            Assert.That(homeDecks.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(homeChatSend.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(hubChat.resolvedStyle.position, Is.EqualTo(Position.Absolute));
            Assert.That(hubTopbar.worldBound.xMin - hubLayout.worldBound.xMin, Is.LessThanOrEqualTo(25f));
            Assert.That(hubLayout.worldBound.xMax - hubTopbar.worldBound.xMax, Is.LessThanOrEqualTo(25f));
            Assert.That(hubObjective.worldBound.yMax, Is.LessThanOrEqualTo(hubLayout.worldBound.yMax + 1f));
            Assert.That(hubDock.worldBound.yMax, Is.LessThanOrEqualTo(hubLayout.worldBound.yMax + 1f));
            Assert.That(hubChat.worldBound.yMax, Is.LessThanOrEqualTo(hubDock.worldBound.yMin + 1f));
            Assert.That(hubChat.worldBound.xMax, Is.LessThanOrEqualTo(hubLayout.worldBound.xMax + 1f));
            Assert.That(hubChatTabs.worldBound.yMin, Is.GreaterThanOrEqualTo(hubChat.worldBound.yMin));
            Assert.That(hubChatTabs.worldBound.xMin, Is.GreaterThan(hubChat.worldBound.xMin));
            Assert.That(hubChatTabs.worldBound.xMax, Is.LessThan(hubChat.worldBound.xMax));
            Assert.That(hubChatInputRow.worldBound.xMin, Is.GreaterThan(hubChat.worldBound.xMin));
            Assert.That(hubChatInputRow.worldBound.xMax, Is.LessThan(hubChat.worldBound.xMax));
            Assert.That(hubChatInputRow.worldBound.yMax, Is.LessThan(hubChat.worldBound.yMax));
            Assert.That(homeChatSend.worldBound.xMax, Is.LessThan(hubChat.worldBound.xMax));
            Assert.That(homeChatSend.worldBound.yMax, Is.LessThan(hubChat.worldBound.yMax));
            foreach (var chatTab in hubChatTabsList)
            {
                Assert.That(chatTab.worldBound.xMin, Is.GreaterThanOrEqualTo(hubChatTabs.worldBound.xMin));
                Assert.That(chatTab.worldBound.xMax, Is.LessThanOrEqualTo(hubChatTabs.worldBound.xMax));
            }
            Assert.That(controller.OpenSettings(), Is.True);
            var casasToggle = root.Q<Toggle>("home-settings-casas-toggle");
            var audioToggle = root.Q<Toggle>("home-settings-audio-master-toggle");
            var stage = controller.Flow.Stage;

            controller.ApplyViewportForTests(
                new Vector2Int(1024, 768),
                new Rect(0f, 24f, 1024f, 720f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.TabletLandscape));
            Assert.That(screen.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(screen.ClassListContains("profile-tablet-landscape"), Is.True);
            Assert.That(screen.ClassListContains("profile-phone-landscape"), Is.False);
            Assert.That(screen.ClassListContains("profile-mobile-portrait"), Is.False);
            Assert.That(controller.Flow.Stage, Is.EqualTo(stage));
            Assert.That(casasToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(audioToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));

            controller.ApplyViewportForTests(
                new Vector2Int(844, 390),
                new Rect(36f, 0f, 772f, 390f),
                true);
            yield return null;

            Assert.That(controller.CurrentAdaptiveLayout.Profile, Is.EqualTo(AdaptiveUiProfile.PhoneLandscape));
            Assert.That(screen.ClassListContains("profile-mobile-landscape"), Is.True);
            Assert.That(screen.ClassListContains("profile-phone-landscape"), Is.True);
            Assert.That(screen.ClassListContains("profile-tablet-landscape"), Is.False);
            Assert.That(screen.ClassListContains("profile-mobile-portrait"), Is.False);
            Assert.That(controller.Flow.Stage, Is.EqualTo(stage));
            Assert.That(casasToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(audioToggle.worldBound.height, Is.GreaterThanOrEqualTo(AdaptiveUiFoundation.MinimumTouchTargetPoints));
            Assert.That(hubObjective.worldBound.yMax, Is.LessThanOrEqualTo(hubLayout.worldBound.yMax + 1f));
            Assert.That(hubDock.worldBound.yMax, Is.LessThanOrEqualTo(hubLayout.worldBound.yMax + 1f));
            Assert.That(hubChat.worldBound.yMax, Is.LessThanOrEqualTo(hubDock.worldBound.yMin + 1f));
            Assert.That(hubChat.worldBound.xMax, Is.LessThanOrEqualTo(hubLayout.worldBound.xMax + 1f));

            Assert.That(controller.BeginQuest(), Is.True);
            yield return WaitForScene("Match");
            controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            root = controller.GetComponent<UIDocument>().rootVisualElement;
            screen = root;
            controller.ApplyViewportForTests(
                new Vector2Int(844, 390),
                new Rect(36f, 0f, 772f, 390f),
                true);
            var adaptiveMatchDeadline = Time.realtimeSinceStartup + 10f;
            while (controller.Flow.Stage == FirstPlayableFlowStage.Loading
                && Time.realtimeSinceStartup < adaptiveMatchDeadline)
            {
                yield return null;
            }

            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            yield return null;
            Assert.That(root.Q<VisualElement>("safe-area-content-container"), Is.Not.Null);
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

            Assert.That(controller.ReturnHome(), Is.True);
            yield return WaitForScene("Hub");
            Object.FindAnyObjectByType<FirstPlayableFlowController>().ClearViewportOverrideForTests();
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
            while (SceneManager.GetActiveScene().name != "Login" && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Login"));
            Assert.That(Object.FindAnyObjectByType<FirstPlayableFlowController>(), Is.Not.Null);
        }

        private static IEnumerator WaitForScene(string sceneName)
        {
            var deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != sceneName
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
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
