using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Application.Interaction;
using TheFall.Domain;
using TheFall.Presentation.Bootstrap;
using TheFall.Presentation.Interaction;
using TheFall.Presentation.Match;
using TheFall.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TheFall.Tests.PlayMode
{
    public sealed class FirstPlayableTablePlayModeTests
    {
        [UnityTest]
        public IEnumerator CompleteMatchPresentationAlwaysMatchesAuthoritativeStateAndPrivacy()
        {
            yield return LoadMatch();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            Assert.That(table, Is.Not.Null);
            Assert.That(table.TablePrototypePrefab, Is.Not.Null);
            Assert.That(table.AuthoredLayout, Is.Not.Null);
            Assert.That(table.AuthoredLayout.IsConfigured, Is.True);
            Assert.That(table.CardCatalog.Entries.Count, Is.EqualTo(40));
            Assert.That(table.GameplayCamera.transform.position, Is.EqualTo(FirstPlayableTablePresentation.CameraPosition));
            Assert.That(table.GameplayCamera.transform.rotation, Is.EqualTo(FirstPlayableTablePresentation.CameraRotation));
            Assert.That(table.GameplayCamera.fieldOfView,
                Is.EqualTo(FirstPlayableTablePresentation.CameraFieldOfView).Within(0.001f));
            Assert.That(table.GameplayCamera.transform.eulerAngles.x, Is.GreaterThan(70f));
            var ui = controller.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(ui.Q("match-actions"), Is.Null);
            Assert.That(ui.Q("match-choices"), Is.Null);

            var humanIntentCount = 0;
            var sawDealerContext = false;
            var sawCantoContext = false;
            while (controller.Flow.Stage == FirstPlayableFlowStage.Match && humanIntentCount < 5000)
            {
                AssertPresentation(table, controller.Flow.Match.State);
                var legal = controller.Flow.Match.GetHumanLegalIntents();
                if (legal.OfType<ChooseDealOptionsIntent>().Any())
                {
                    sawDealerContext = true;
                    Assert.That(ui.Q<VisualElement>("dealer-context").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
                    Assert.That(ui.Q<VisualElement>("dealer-options-menu").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
                    Assert.That(ui.Q<VisualElement>("dealer-options").childCount, Is.EqualTo(4));
                }

                if (legal.OfType<AnnounceCantoIntent>().Any())
                {
                    sawCantoContext = true;
                    Assert.That(ui.Q<VisualElement>("canto-context").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
                }

                Assert.That(controller.SubmitHumanIntent(ChooseHumanIntent(controller.Flow.Match.State, legal)), Is.True);
                Assert.That(table.IsPresentationBusy, Is.True);
                table.SkipPresentation();
                humanIntentCount++;
            }

            Assert.That(humanIntentCount, Is.LessThan(5000));
            Assert.That(sawDealerContext, Is.True);
            Assert.That(sawCantoContext, Is.True);
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Result));
            AssertPresentation(table, controller.Flow.Match.State);
            Assert.That(table.Snapshot.WinnerTeam, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ContextualDealerCardsAndCantoReplaceThePersistentActionPanel()
        {
            yield return LoadMatch();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            var ui = controller.GetComponent<UIDocument>().rootVisualElement;

            Assert.That(controller.Flow.Match.State.Phase, Is.EqualTo(MatchPhase.DealerSelection));
            var dealerIntents = controller.Flow.Match.GetHumanLegalIntents().OfType<SelectDealerCardIntent>().ToArray();
            var dealerCards = table.RenderedCards
                .Where(card => card.Zone == FirstPlayableCardZone.DealerSpread)
                .OrderBy(card => card.InteractionIndex)
                .ToArray();
            Assert.That(dealerCards, Has.Length.EqualTo(dealerIntents.Length));
            Assert.That(dealerCards.All(card => !card.IsFaceUp && !card.Card.HasValue), Is.True);
            Assert.That(dealerCards.All(card => card.GetComponent<Collider>() != null), Is.True);

            var expectedDealerCard = dealerIntents[3].Card;
            var expectedMotionStart = dealerCards[3].transform.position;
            Assert.That(table.ActivateDealerCard(3), Is.True);
            Assert.That(table.IsPresentationBusy, Is.True);
            var deadline = Time.realtimeSinceStartup + 5f;
            while ((!table.TryGetActiveDealerSelectionMotion(out _)
                    || table.AnimationPlayer.ActiveStepProgress < 0.1f)
                && table.IsPresentationBusy
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(table.TryGetActiveDealerSelectionMotion(out var dealerMotion), Is.True);
            Assert.That(Vector3.Distance(dealerMotion.StartWorld, expectedMotionStart), Is.LessThan(0.0001f));
            Assert.That(dealerMotion.TargetWorld.x, Is.EqualTo(dealerMotion.StartWorld.x).Within(0.0001f));
            Assert.That(dealerMotion.TargetWorld.z, Is.EqualTo(dealerMotion.StartWorld.z).Within(0.0001f));
            Assert.That(dealerMotion.TargetWorld.y, Is.GreaterThan(dealerMotion.StartWorld.y));
            var revealedCard = table.RenderedCards
                .Single(card => card.Zone == FirstPlayableCardZone.DealerSelection);
            var earlyFlip = table.DealerCardFlipDegrees;
            Assert.That(earlyFlip, Is.GreaterThan(0f).And.LessThan(90f));
            Assert.That(revealedCard.IsFaceUp, Is.False);
            Assert.That(revealedCard.Card, Is.Null);

            table.ApplyViewportForTests(
                new Vector2Int(1440, 900),
                new Rect(0f, 0f, 1440f, 900f));
            revealedCard = table.RenderedCards
                .Single(card => card.Zone == FirstPlayableCardZone.DealerSelection);
            Assert.That(table.DealerCardFlipDegrees, Is.GreaterThan(0f).And.LessThan(90f));
            Assert.That(revealedCard.IsFaceUp, Is.False);
            Assert.That(revealedCard.Card, Is.Null);

            while (table.AnimationPlayer.ActiveStepProgress < 0.65f
                && table.IsPresentationBusy
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            revealedCard = table.RenderedCards
                .Single(card => card.Zone == FirstPlayableCardZone.DealerSelection);
            Assert.That(table.DealerCardFlipDegrees, Is.GreaterThan(90f).And.LessThan(180f));
            Assert.That(
                Vector3.Distance(revealedCard.transform.position, dealerMotion.StartWorld),
                Is.GreaterThan(0.005f));
            Assert.That(revealedCard.IsFaceUp, Is.True);
            Assert.That(revealedCard.Card, Is.EqualTo(expectedDealerCard));
            table.SkipPresentation();
            Assert.That(controller.Flow.Match.Trace.IntentHistory
                .Last(record => record.Actor == IntentActor.Human).Intent,
                Is.TypeOf<SelectDealerCardIntent>());
            Assert.That(((SelectDealerCardIntent)controller.Flow.Match.Trace.IntentHistory
                .Last(record => record.Actor == IntentActor.Human).Intent).Card,
                Is.EqualTo(expectedDealerCard));

            AdvanceToHumanPlay(controller);
            var renderedTables = controller.GetComponentsInChildren<Transform>(true)
                .Where(item => item.name == "RoundCardTable")
                .ToArray();
            Assert.That(renderedTables, Is.Not.Empty);
            var authoredTableScale = table.AuthoredLayout.Table.transform.localScale;
            Assert.That(renderedTables.All(item => item.localScale == authoredTableScale), Is.True);

            var localHandCard = table.RenderedCards.First(card => card.Zone == FirstPlayableCardZone.LocalHand);
            var publicTableCard = table.RenderedCards.First(card => card.Zone == FirstPlayableCardZone.Table);
            var opponentCard = table.RenderedCards.First(card => card.Zone == FirstPlayableCardZone.OpponentHand);
            var deckCard = table.RenderedCards.First(card => card.Zone == FirstPlayableCardZone.Deck);
            var expectedScale = localHandCard.transform.localScale;
            Assert.That(publicTableCard.transform.localScale, Is.EqualTo(expectedScale));
            Assert.That(opponentCard.transform.localScale, Is.EqualTo(expectedScale));
            Assert.That(deckCard.transform.localScale, Is.EqualTo(expectedScale));
            Assert.That(expectedScale.x / expectedScale.z, Is.EqualTo(63f / 88f).Within(0.0001f));
            Assert.That(localHandCard.transform.localEulerAngles.z, Is.EqualTo(180f).Within(0.001f));
            Assert.That(publicTableCard.transform.localEulerAngles.z, Is.EqualTo(180f).Within(0.001f));
            Assert.That(deckCard.transform.localEulerAngles.z, Is.EqualTo(0f).Within(0.001f));

            var cantoIntents = controller.Flow.Match.GetHumanLegalIntents().OfType<AnnounceCantoIntent>().ToArray();
            Assert.That(cantoIntents, Is.Not.Empty);
            Assert.That(ui.Q<VisualElement>("canto-context").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(ui.Q<VisualElement>("canto-options-menu").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));

            controller.ToggleCantoOptions();
            Assert.That(ui.Q<VisualElement>("canto-options-menu").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(ui.Q<VisualElement>("canto-options").childCount, Is.EqualTo(cantoIntents.Length));

            var intentCount = controller.Flow.Match.Trace.IntentHistory.Count;
            var card = table.Snapshot.LocalHand[0];
            Assert.That(table.InputAdapter.TouchTap(card).IsAccepted, Is.True);
            Assert.That(table.Interaction.State.SelectedCard, Is.EqualTo(card));
            Assert.That(table.InputAdapter.TouchTap(card).IsAccepted, Is.True);
            Assert.That(controller.Flow.Match.Trace.IntentHistory
                .Skip(intentCount)
                .Where(record => record.Actor == IntentActor.Human)
                .Select(record => record.Intent)
                .OfType<AnnounceCantoIntent>(),
                Is.Empty);
        }

        [UnityTest]
        public IEnumerator DesktopRecompositionPreservesSelectionIntentAndCameraThenConfirmedPlayAdvancesOnce()
        {
            yield return LoadMatch();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            AdvanceToHumanPlay(controller);

            var card = table.Snapshot.LocalHand[0];
            table.InputAdapter.MouseInspect(card);
            table.InputAdapter.MouseSelect(card);
            var state = controller.Flow.Match.State;
            var traceCount = controller.Flow.Match.Trace.IntentHistory.Count;
            var interactionRevision = table.Interaction.State.Revision;
            var interactionIntentCount = table.Interaction.IntentHistory.Count;
            var cameraPosition = table.GameplayCamera.transform.position;
            var cameraRotation = table.GameplayCamera.transform.rotation;

            foreach (var viewport in new[]
            {
                new Vector2Int(1280, 720),
                new Vector2Int(1440, 900),
                new Vector2Int(1920, 1080),
                new Vector2Int(2560, 1440),
            })
            {
                table.ApplyViewportForTests(viewport, new Rect(0f, 0f, viewport.x, viewport.y));
                Assert.That(controller.Flow.Match.State, Is.SameAs(state));
                Assert.That(controller.Flow.Match.Trace.IntentHistory, Has.Count.EqualTo(traceCount));
                Assert.That(table.Interaction.State.SelectedCard, Is.EqualTo(card));
                Assert.That(table.Interaction.State.Revision, Is.EqualTo(interactionRevision));
                Assert.That(table.Interaction.IntentHistory, Has.Count.EqualTo(interactionIntentCount));
                Assert.That(table.LocalHandViews.Single(view => view.HandIndex == 0).VisualState,
                    Is.EqualTo(PrototypeCardVisualState.Selected));
            }

            Assert.That(table.GameplayCamera.transform.position, Is.EqualTo(cameraPosition));
            Assert.That(table.GameplayCamera.transform.rotation, Is.EqualTo(cameraRotation));

            var cancelled = table.InputAdapter.Cancel();
            Assert.That(cancelled.IsAccepted, Is.True);
            Assert.That(table.Interaction.State.SelectedCard, Is.Null);
            var rejected = table.InputAdapter.KeyboardConfirm();
            Assert.That(rejected.IsAccepted, Is.False);
            Assert.That(rejected.State.Feedback, Is.EqualTo(CardInteractionFeedback.Rejected));
            Assert.That(rejected.State.FeedbackReason, Is.EqualTo(CardInteractionFeedbackReason.NoSelection));
            Assert.That(controller.Flow.Match.Trace.IntentHistory, Has.Count.EqualTo(traceCount));

            table.InputAdapter.MouseSelect(card);
            table.SetTemporarilyBlocked(true);
            Assert.That(table.LocalHandViews.Single(view => view.HandIndex == 0).VisualState,
                Is.EqualTo(PrototypeCardVisualState.TemporarilyBlocked));
            var blocked = table.InputAdapter.KeyboardConfirm();
            Assert.That(blocked.IsAccepted, Is.False);
            Assert.That(controller.Flow.Match.State, Is.SameAs(state));
            Assert.That(controller.Flow.Match.Trace.IntentHistory, Has.Count.EqualTo(traceCount));

            table.SetTemporarilyBlocked(false);
            var confirmed = table.InputAdapter.KeyboardConfirm();
            Assert.That(confirmed.IsAccepted, Is.True);
            Assert.That(table.IsPresentationBusy, Is.True);
            Assert.That(controller.Flow.Match.Trace.IntentHistory.Count, Is.GreaterThan(traceCount));
            Assert.That(controller.Flow.Match.Trace.IntentHistory
                .Skip(traceCount)
                .Count(record => record.Actor == IntentActor.Human && record.Intent is PlayCardIntent), Is.EqualTo(1));
            Assert.That(table.Interaction.IntentHistory.Select(intent => intent.Kind), Does.Contain(CardInteractionIntentKind.Inspect));
            Assert.That(table.Interaction.IntentHistory.Select(intent => intent.Kind), Does.Contain(CardInteractionIntentKind.Select));
            Assert.That(table.Interaction.IntentHistory.Select(intent => intent.Kind), Does.Contain(CardInteractionIntentKind.Confirm));
            Assert.That(table.Interaction.IntentHistory.Select(intent => intent.Kind), Does.Contain(CardInteractionIntentKind.Cancel));
            Assert.That(table.Interaction.IntentHistory.Select(intent => intent.Kind), Does.Contain(CardInteractionIntentKind.Play));
            table.SkipPresentation();
            Assert.That(table.RenderedState, Is.SameAs(controller.Flow.Match.State));
        }

        private static void AssertPresentation(FirstPlayableTablePresentation table, MatchState state)
        {
            Assert.That(table.RenderedState, Is.SameAs(state));
            Assert.That(table.Snapshot.LocalScore, Is.EqualTo(state.TeamOneScore.Value));
            Assert.That(table.Snapshot.OpponentScore, Is.EqualTo(state.TeamTwoScore.Value));
            Assert.That(table.Snapshot.DealerSeat, Is.EqualTo(state.DealerSeat));
            Assert.That(table.Snapshot.ActiveSeat, Is.EqualTo(state.CurrentSeat));
            Assert.That(table.Snapshot.RoundNumber, Is.EqualTo(state.RoundNumber));
            Assert.That(table.Snapshot.DealNumber, Is.EqualTo(state.DealNumber));

            var localHand = table.RenderedCards.Where(card => card.Zone == FirstPlayableCardZone.LocalHand).ToArray();
            Assert.That(localHand, Has.Length.EqualTo(state.GetPlayerAt(Seat.First).Hand.Count));
            Assert.That(localHand.Select(card => card.Card.Value), Is.EqualTo(state.GetPlayerAt(Seat.First).Hand));

            var opponentHand = table.RenderedCards.Where(card => card.Zone == FirstPlayableCardZone.OpponentHand).ToArray();
            Assert.That(opponentHand, Has.Length.EqualTo(state.GetPlayerAt(Seat.Second).Hand.Count));
            Assert.That(opponentHand.All(card => !card.IsFaceUp && !card.Card.HasValue), Is.True);

            var tableCards = table.RenderedCards.Where(card => card.Zone == FirstPlayableCardZone.Table).ToArray();
            Assert.That(tableCards.Select(card => card.Card.Value), Is.EqualTo(state.Table));
            var localCaptured = table.RenderedCards
                .Where(card => card.Zone == FirstPlayableCardZone.LocalCaptured)
                .ToArray();
            Assert.That(localCaptured, Has.Length.EqualTo(state.GetPlayerAt(Seat.First).CapturedCards.Count));
            Assert.That(localCaptured.All(card => !card.IsFaceUp && !card.Card.HasValue), Is.True);

            var opponentCaptured = table.RenderedCards
                .Where(card => card.Zone == FirstPlayableCardZone.OpponentCaptured)
                .ToArray();
            Assert.That(opponentCaptured, Has.Length.EqualTo(state.GetPlayerAt(Seat.Second).CapturedCards.Count));
            Assert.That(opponentCaptured.All(card => !card.IsFaceUp && !card.Card.HasValue), Is.True);
            Assert.That(table.RenderedCards.Count(card => card.Zone == FirstPlayableCardZone.DealerSpread),
                Is.EqualTo(state.Phase == MatchPhase.DealerSelection ? state.Deck.Count : 0));
            Assert.That(table.RenderedCards
                    .Where(card => card.Zone == FirstPlayableCardZone.DealerSelection)
                    .Select(card => card.Card.Value),
                Is.EqualTo(state.Phase == MatchPhase.DealerSelection
                    ? state.DealerSelectionCards
                    : System.Array.Empty<Card>()));
            Assert.That(table.RenderedCards.Count(card => card.Zone == FirstPlayableCardZone.Deck),
                Is.EqualTo(state.Phase == MatchPhase.DealerSelection ? 0 : state.Deck.Count));
            Assert.That(table.Snapshot.Cantos.Count, Is.EqualTo(state.CantoAnnouncements.Count));
        }

        private static void AdvanceToHumanPlay(FirstPlayableFlowController controller)
        {
            var safety = 0;
            while (!controller.Flow.Match.GetHumanLegalIntents().OfType<PlayCardIntent>().Any() && safety++ < 100)
            {
                var legal = controller.Flow.Match.GetHumanLegalIntents();
                Assert.That(controller.SubmitHumanIntent(ChooseHumanIntent(controller.Flow.Match.State, legal)), Is.True);
                Object.FindAnyObjectByType<FirstPlayableTablePresentation>().SkipPresentation();
            }

            Assert.That(controller.Flow.Match.GetHumanLegalIntents().OfType<PlayCardIntent>().Any(), Is.True);
        }

        private static IEnumerator LoadMatch()
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

            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.OpenSetup(), Is.True);
            Assert.That(controller.StartMatch(), Is.True);
            yield return null;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            Assert.That(table, Is.Not.Null);
            table.SkipPresentation();
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
