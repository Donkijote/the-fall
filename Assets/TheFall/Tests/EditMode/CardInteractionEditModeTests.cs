using System.Linq;
using NUnit.Framework;
using TheFall.Application.Interaction;
using TheFall.Domain;
using TheFall.Presentation.Input;

namespace TheFall.Tests.EditMode
{
    public sealed class CardInteractionEditModeTests
    {
        [Test]
        public void TouchAndDesktop_ProduceTheSameConfirmedApplicationIntentSequence()
        {
            var touchTurn = RepresentativeCardTurn.Create();
            var touch = CreateAdapter(touchTurn);
            var touchCard = touchTurn.LocalHand[0];

            touch.TouchInspect(touchCard);
            touch.TouchTap(touchCard);
            var touchResult = touch.TouchTap(touchCard);

            var desktopTurn = RepresentativeCardTurn.Create();
            var desktop = CreateAdapter(desktopTurn);
            var desktopCard = desktopTurn.LocalHand[0];

            desktop.MouseInspect(desktopCard);
            desktop.MouseSelect(desktopCard);
            var desktopResult = desktop.KeyboardConfirm();

            var touchSequence = touchTurn.Interaction.IntentHistory.Select(intent => intent.Kind).ToArray();
            var desktopSequence = desktopTurn.Interaction.IntentHistory.Select(intent => intent.Kind).ToArray();

            Assert.That(touchSequence, Is.EqualTo(new[]
            {
                CardInteractionIntentKind.Inspect,
                CardInteractionIntentKind.Select,
                CardInteractionIntentKind.Confirm,
                CardInteractionIntentKind.Play,
            }));
            Assert.That(desktopSequence, Is.EqualTo(touchSequence));
            Assert.That(touchResult.IsAccepted, Is.True);
            Assert.That(desktopResult.IsAccepted, Is.True);
            Assert.That(touchResult.RuleResult, Is.Not.Null);
            Assert.That(desktopResult.RuleResult, Is.Not.Null);
            Assert.That(touchResult.RuleResult.Events.OfType<CardsCapturedEvent>().Single().Cards, Has.Count.EqualTo(4));
            Assert.That(desktopResult.RuleResult.Events.Select(resolvedEvent => resolvedEvent.Kind),
                Is.EqualTo(touchResult.RuleResult.Events.Select(resolvedEvent => resolvedEvent.Kind)));
        }

        [Test]
        public void KeyboardOnly_CanInspectSelectAndConfirmTheFocusedCard()
        {
            var turn = RepresentativeCardTurn.Create();
            var keyboard = CreateAdapter(turn);

            keyboard.KeyboardInspect();
            keyboard.KeyboardSelect();
            var result = keyboard.KeyboardConfirm();

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(turn.Interaction.State.Feedback, Is.EqualTo(CardInteractionFeedback.Confirmed));
            Assert.That(turn.Interaction.IntentHistory.Select(intent => intent.Kind), Is.EqualTo(new[]
            {
                CardInteractionIntentKind.Inspect,
                CardInteractionIntentKind.Select,
                CardInteractionIntentKind.Confirm,
                CardInteractionIntentKind.Play,
            }));
        }

        [Test]
        public void InvalidCard_IsRejectedImmediatelyWithoutChangingDomainState()
        {
            var turn = RepresentativeCardTurn.Create();
            var initialMatchState = turn.Interaction.MatchState;
            var unavailable = new Card(CardSuit.Cups, CardRank.Eleven);

            var result = turn.Interaction.Submit(
                new SelectCardInteractionIntent(turn.Interaction.LocalPlayerId, unavailable));

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.State.Feedback, Is.EqualTo(CardInteractionFeedback.Rejected));
            Assert.That(result.State.FeedbackReason, Is.EqualTo(CardInteractionFeedbackReason.CardUnavailable));
            Assert.That(result.State.FeedbackLocalizationKey, Is.EqualTo("interaction.feedback.card-unavailable"));
            Assert.That(turn.Interaction.MatchState, Is.SameAs(initialMatchState));
        }

        [Test]
        public void TemporaryBlockAndCancel_PreserveThenExplicitlyClearSelectionWithoutPlaying()
        {
            var turn = RepresentativeCardTurn.Create();
            var adapter = CreateAdapter(turn);
            var card = turn.LocalHand[1];
            adapter.MouseSelect(card);
            var matchStateBeforeBlock = turn.Interaction.MatchState;

            turn.Interaction.SetTemporarilyBlocked(true);
            var blocked = adapter.KeyboardConfirm();

            Assert.That(blocked.IsAccepted, Is.False);
            Assert.That(blocked.State.Feedback, Is.EqualTo(CardInteractionFeedback.TemporarilyBlocked));
            Assert.That(blocked.State.SelectedCard, Is.EqualTo(card));
            Assert.That(turn.Interaction.MatchState, Is.SameAs(matchStateBeforeBlock));
            Assert.That(turn.Interaction.IntentHistory.All(intent => intent.Kind != CardInteractionIntentKind.Play), Is.True);

            turn.Interaction.SetTemporarilyBlocked(false);
            var cancelled = adapter.Cancel();

            Assert.That(cancelled.IsAccepted, Is.True);
            Assert.That(cancelled.State.SelectedCard, Is.Null);
            Assert.That(turn.Interaction.MatchState, Is.SameAs(matchStateBeforeBlock));
        }

        private static CardInteractionInputAdapter CreateAdapter(RepresentativeCardTurn turn)
        {
            var adapter = new CardInteractionInputAdapter(turn.Interaction);
            adapter.SetCards(turn.LocalHand);
            return adapter;
        }
    }
}
