using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;
using TheFall.Presentation.Match;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFall.Tests.EditMode
{
    public sealed class FirstPlayableTableEditModeTests
    {
        private const string HomeScenePath = "Assets/TheFall/Presentation/Scenes/Home.unity";

        [Test]
        public void HomeSceneExposesThePersistentRuntimeAuthoringLayout()
        {
            var scene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
            var layout = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableTableLayout>(true))
                .Single();
            var presentation = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableTablePresentation>(true))
                .Single();

            Assert.That(layout.gameObject.activeSelf, Is.True);
            Assert.That(layout.IsConfigured, Is.True);
            Assert.That(presentation.AuthoredLayout, Is.SameAs(layout));
            Assert.That(layout.Table.name, Does.StartWith("RoundCardTable"));
            Assert.That(layout.CardScale.x / layout.CardScale.z, Is.EqualTo(63f / 88f).Within(0.0001f));
        }

        [Test]
        public void SnapshotProjectsAuthoritativePublicStateWithoutOpponentHandIdentities()
        {
            var match = CreateMatch(2525);
            AdvanceToHumanPlay(match);
            var state = match.State;

            var snapshot = FirstPlayableTableSnapshot.Create(state);

            Assert.That(snapshot.AuthoritativeState, Is.SameAs(state));
            Assert.That(snapshot.LocalHand, Is.EqualTo(state.GetPlayerAt(Seat.First).Hand));
            Assert.That(snapshot.OpponentHandCount, Is.EqualTo(state.GetPlayerAt(Seat.Second).Hand.Count));
            Assert.That(snapshot.TableCards, Is.EqualTo(state.Table));
            Assert.That(snapshot.LocalCapturedCards, Is.EqualTo(state.GetPlayerAt(Seat.First).CapturedCards));
            Assert.That(snapshot.OpponentCapturedCards, Is.EqualTo(state.GetPlayerAt(Seat.Second).CapturedCards));
            Assert.That(snapshot.LocalScore, Is.EqualTo(state.TeamOneScore.Value));
            Assert.That(snapshot.OpponentScore, Is.EqualTo(state.TeamTwoScore.Value));
            Assert.That(
                typeof(FirstPlayableTableSnapshot).GetProperties()
                    .Any(property => property.Name.Contains("OpponentHand") && property.PropertyType != typeof(int)),
                Is.False,
                "Presentation must receive only the opponent hand count, never its card identities.");
        }

        [Test]
        public void CompleteMatchSnapshotsAlwaysReferenceTheResultingAuthoritativeState()
        {
            var match = CreateMatch(2526);
            var humanIntentCount = 0;

            while (match.State.Phase != MatchPhase.Completed && humanIntentCount < 5000)
            {
                var before = FirstPlayableTableSnapshot.Create(match.State);
                Assert.That(before.AuthoritativeState, Is.SameAs(match.State));

                var legal = match.GetHumanLegalIntents();
                var result = match.SubmitHumanIntent(ChooseHumanIntent(match.State, legal));
                Assert.That(result.HumanResult.IsAccepted, Is.True);

                var after = FirstPlayableTableSnapshot.Create(match.State);
                Assert.That(after.AuthoritativeState, Is.SameAs(match.State));
                Assert.That(after.Phase, Is.EqualTo(match.State.Phase));
                Assert.That(after.TableCards, Is.EqualTo(match.State.Table));
                humanIntentCount++;
            }

            Assert.That(humanIntentCount, Is.LessThan(5000));
            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Completed));
            var completed = FirstPlayableTableSnapshot.Create(match.State);
            Assert.That(completed.WinnerTeam, Is.EqualTo(match.State.WinnerTeam));
        }

        private static FirstPlayableMatchOrchestrator CreateMatch(int seed)
        {
            return FirstPlayableMatchFactory.Create(
                seed,
                new Player(new PlayerId("human"), "Local Player", Seat.First, TeamId.One, PlayerControl.Human),
                new Player(new PlayerId("bot"), "Baseline Bot", Seat.Second, TeamId.Two, PlayerControl.Bot),
                RuleConfiguration.Standard);
        }

        private static void AdvanceToHumanPlay(FirstPlayableMatchOrchestrator match)
        {
            var safety = 0;
            while (!match.GetHumanLegalIntents().OfType<PlayCardIntent>().Any() && safety++ < 100)
            {
                var legal = match.GetHumanLegalIntents();
                match.SubmitHumanIntent(ChooseHumanIntent(match.State, legal));
            }

            Assert.That(match.GetHumanLegalIntents().OfType<PlayCardIntent>().Any(), Is.True);
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
