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
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();

            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(controller.OpenSetup(), Is.True);
            Assert.That(controller.StartMatch(), Is.True);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Loading));
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
        public IEnumerator PseudoLocalization_RemainsExpandedReadableAndKeyboardFocusable()
        {
            yield return LoadFlow();
            yield return LocalizationSettings.InitializationOperation;
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var root = controller.GetComponent<UIDocument>().rootVisualElement;
            var screen = root.Q<VisualElement>("home-screen");
            var subtitle = root.Q<Label>("home-subtitle");
            var start = root.Q<Button>("home-start-button");

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("en"));
            controller.Render();
            var englishText = subtitle.text;

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales
                .OfType<PseudoLocale>()
                .Single(locale => locale.Identifier.Code == "qps-ploc");
            yield return null;
            controller.Render();

            Assert.That(subtitle.text, Is.Not.Empty);
            Assert.That(subtitle.text, Is.Not.EqualTo(englishText));
            Assert.That(start.text, Is.Not.Empty);
            Assert.That(start.focusable, Is.True);
            Assert.That(screen.layout.width, Is.GreaterThan(0f));
            Assert.That(screen.layout.height, Is.GreaterThan(0f));

            controller.OpenSetup();
            yield return null;
            var setupStage = root.Q<VisualElement>("setup-stage");
            var casasToggle = root.Q<Toggle>("casas-toggle");
            var startMatch = root.Q<Button>("setup-start-button");
            Assert.That(setupStage.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(casasToggle.text, Is.Not.Empty);
            Assert.That(casasToggle.focusable, Is.True);
            Assert.That(startMatch.text, Is.Not.Empty);
            Assert.That(startMatch.worldBound.xMin, Is.GreaterThanOrEqualTo(screen.worldBound.xMin - 1f));
            Assert.That(startMatch.worldBound.xMax, Is.LessThanOrEqualTo(screen.worldBound.xMax + 1f));

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("en"));
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
