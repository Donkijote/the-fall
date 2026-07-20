using System.Collections;
using System.Linq;
using NUnit.Framework;
using TheFall.Application.Interaction;
using TheFall.Presentation.Interaction;
using TheFall.Presentation.Table;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheFall.Tests.PlayMode
{
    public sealed class CardInteractionPlayModeTests
    {
        [UnityTest]
        public IEnumerator TouchAndDesktop_CompleteTheSameRepresentativeTurn()
        {
            yield return SceneManager.LoadSceneAsync("MatchPrototype", LoadSceneMode.Single);

            var prototype = Object.FindAnyObjectByType<CardInteractionPrototype>();
            Assert.That(prototype, Is.Not.Null);
            var card = prototype.LocalHand[0];

            prototype.InputAdapter.TouchInspect(card);
            prototype.InputAdapter.TouchTap(card);
            var touchResult = prototype.InputAdapter.TouchTap(card);
            var touchSequence = prototype.Interaction.IntentHistory.Select(intent => intent.Kind).ToArray();

            prototype.ResetRepresentativeTurnForTests();
            card = prototype.LocalHand[0];
            prototype.InputAdapter.MouseInspect(card);
            prototype.InputAdapter.MouseSelect(card);
            var desktopResult = prototype.InputAdapter.KeyboardConfirm();
            var desktopSequence = prototype.Interaction.IntentHistory.Select(intent => intent.Kind).ToArray();

            Assert.That(touchResult.IsAccepted, Is.True);
            Assert.That(desktopResult.IsAccepted, Is.True);
            Assert.That(desktopSequence, Is.EqualTo(touchSequence));
            Assert.That(desktopSequence, Is.EqualTo(new[]
            {
                CardInteractionIntentKind.Inspect,
                CardInteractionIntentKind.Select,
                CardInteractionIntentKind.Confirm,
                CardInteractionIntentKind.Play,
            }));
        }

        [UnityTest]
        public IEnumerator PortraitRecomposition_PreservesSelectionWithoutPlayingOrDuplicatingIt()
        {
            yield return SceneManager.LoadSceneAsync("MatchPrototype", LoadSceneMode.Single);

            var prototype = Object.FindAnyObjectByType<CardInteractionPrototype>();
            var selectedCard = prototype.LocalHand[1];
            prototype.InputAdapter.TouchTap(selectedCard);
            var stateRevision = prototype.Interaction.State.Revision;
            var intentCount = prototype.Interaction.IntentHistory.Count;
            var matchState = prototype.Interaction.MatchState;

            prototype.TableComposition.ApplyViewportForTests(
                new Vector2Int(844, 390),
                new Rect(36f, 0f, 772f, 390f));
            prototype.TableComposition.ApplyViewportForTests(
                new Vector2Int(390, 844),
                new Rect(0f, 34f, 390f, 776f));

            Assert.That(prototype.Interaction.State.SelectedCard, Is.EqualTo(selectedCard));
            Assert.That(prototype.Interaction.State.Revision, Is.EqualTo(stateRevision));
            Assert.That(prototype.Interaction.IntentHistory, Has.Count.EqualTo(intentCount));
            Assert.That(prototype.Interaction.MatchState, Is.SameAs(matchState));
            Assert.That(prototype.TableComposition.LocalHandCardViews
                .Single(view => view.HandIndex == 1).VisualState,
                Is.EqualTo(PrototypeCardVisualState.Selected));
        }

        [UnityTest]
        public IEnumerator CardViews_ShowLegalSelectedBlockedConfirmedAndRejectedStates()
        {
            yield return SceneManager.LoadSceneAsync("MatchPrototype", LoadSceneMode.Single);

            var prototype = Object.FindAnyObjectByType<CardInteractionPrototype>();
            var selectedCard = prototype.LocalHand[0];
            var selectedView = prototype.TableComposition.LocalHandCardViews.Single(view => view.HandIndex == 0);
            var otherView = prototype.TableComposition.LocalHandCardViews.Single(view => view.HandIndex == 1);

            Assert.That(prototype.TableComposition.LocalHandCardViews
                .All(view => view.VisualState == PrototypeCardVisualState.Legal), Is.True);

            prototype.InputAdapter.MouseSelect(selectedCard);
            Assert.That(selectedView.VisualState, Is.EqualTo(PrototypeCardVisualState.Selected));

            prototype.SetTemporarilyBlockedForTests(true);
            Assert.That(selectedView.VisualState, Is.EqualTo(PrototypeCardVisualState.TemporarilyBlocked));

            prototype.SetTemporarilyBlockedForTests(false);
            prototype.InputAdapter.KeyboardConfirm();
            Assert.That(selectedView.VisualState, Is.EqualTo(PrototypeCardVisualState.Confirmed));

            prototype.InputAdapter.MouseSelect(prototype.LocalHand[1]);
            Assert.That(otherView.VisualState, Is.EqualTo(PrototypeCardVisualState.Rejected));
            Assert.That(prototype.Interaction.State.FeedbackReason,
                Is.EqualTo(CardInteractionFeedbackReason.CardUnavailable));
        }
    }
}
