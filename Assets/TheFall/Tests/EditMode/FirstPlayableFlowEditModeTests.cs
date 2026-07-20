using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;

namespace TheFall.Tests.EditMode
{
    public sealed class FirstPlayableFlowEditModeTests
    {
        private static readonly PlayerId HumanId = new PlayerId("human");
        private static readonly PlayerId BotId = new PlayerId("baseline-bot");

        [Test]
        public void Setup_ExposesOnlyDocumentedOptionsWithConsistentDefaults()
        {
            var flow = CreateFlow();

            Assert.That(flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(flow.Setup.CasaCantosEnabled, Is.True);
            Assert.That(flow.Setup.TrivilinWinsImmediately, Is.False);
            Assert.That(flow.TryOpenSetup(), Is.True);
            Assert.That(flow.TryOpenSetup(), Is.False);
            Assert.That(flow.TryConfigure(false, true), Is.True);
            Assert.That(flow.TryStartMatch(), Is.True);

            Assert.That(flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Loading));
            Assert.That(flow.Match.State.Rules.CasaCantosEnabled, Is.False);
            Assert.That(flow.Match.State.Rules.TrivilinWinsImmediately, Is.True);
            Assert.That(flow.Match.State.Rules.VictoryTarget, Is.EqualTo(24));
            Assert.That(flow.Match.State.Players, Has.Count.EqualTo(2));
            Assert.That(flow.Match.State.Players.Count(player => player.Player.Control == PlayerControl.Bot), Is.EqualTo(1));
        }

        [Test]
        public void InvalidAndRepeatedNavigation_DoesNotDuplicateOrReplaceTheCurrentSession()
        {
            var flow = CreateFlow();

            Assert.That(flow.TryStartMatch(), Is.False);
            Assert.That(flow.TryReplay(), Is.False);
            Assert.That(flow.TryFinishLoading(1), Is.False);
            Assert.That(flow.TryReturnHome(), Is.False);

            flow.TryOpenSetup();
            flow.TryStartMatch();
            var match = flow.Match;
            var sessionNumber = flow.SessionNumber;

            Assert.That(flow.TryStartMatch(), Is.False);
            Assert.That(flow.TryFinishLoading(sessionNumber + 1), Is.False);
            Assert.That(flow.Match, Is.SameAs(match));
            Assert.That(flow.SessionNumber, Is.EqualTo(sessionNumber));
            Assert.That(flow.TryFinishLoading(sessionNumber), Is.True);
            Assert.That(flow.TryFinishLoading(sessionNumber), Is.False);
            Assert.That(flow.Match, Is.SameAs(match));
        }

        [Test]
        public void CompleteReplayAndReturnHome_ReplacesMatchAndClearsStaleState()
        {
            var flow = CreateFlow();
            flow.TryOpenSetup();
            flow.TryConfigure(false, true);
            flow.TryStartMatch();
            flow.TryFinishLoading(flow.SessionNumber);

            CompleteMatch(flow);
            Assert.That(flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Result));
            var completedMatch = flow.Match;
            var completedSession = flow.SessionNumber;

            Assert.That(flow.TryReplay(), Is.True);
            Assert.That(flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Loading));
            Assert.That(flow.Match, Is.Not.SameAs(completedMatch));
            Assert.That(flow.SessionNumber, Is.EqualTo(completedSession + 1));
            Assert.That(flow.Match.State.Rules.CasaCantosEnabled, Is.False);
            Assert.That(flow.Match.State.Rules.TrivilinWinsImmediately, Is.True);

            Assert.That(flow.TryReturnHome(), Is.True);
            Assert.That(flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Home));
            Assert.That(flow.Match, Is.Null);
            Assert.That(flow.Setup.CasaCantosEnabled, Is.True);
            Assert.That(flow.Setup.TrivilinWinsImmediately, Is.False);
            Assert.That(flow.TryReturnHome(), Is.False);
        }

        private static FirstPlayableFlow CreateFlow()
        {
            return new FirstPlayableFlow((seed, rules) => FirstPlayableMatchFactory.Create(
                seed,
                new Player(HumanId, "Local Player", Seat.First, TeamId.One, PlayerControl.Human),
                new Player(BotId, "Baseline Bot", Seat.Second, TeamId.Two, PlayerControl.Bot),
                rules));
        }

        private static void CompleteMatch(FirstPlayableFlow flow)
        {
            var humanIntentCount = 0;
            while (flow.Stage == FirstPlayableFlowStage.Match && humanIntentCount < 5000)
            {
                var legal = flow.Match.GetHumanLegalIntents();
                Assert.That(legal, Is.Not.Empty);
                var intent = ChooseHumanIntent(flow.Match.State, legal);
                Assert.That(flow.TrySubmitHumanIntent(intent, out var result), Is.True);
                Assert.That(result.HumanResult.IsAccepted, Is.True);
                humanIntentCount++;
            }

            Assert.That(humanIntentCount, Is.LessThan(5000));
            Assert.That(flow.Match.State.Phase, Is.EqualTo(MatchPhase.Completed));
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
