using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;
using TheFall.Presentation.Diagnostics;

namespace TheFall.Tests.EditMode
{
    public sealed class FirstPlayableAcceptanceEditModeTests
    {
        private static readonly PlayerId HumanId = new PlayerId("acceptance-human");
        private static readonly PlayerId BotId = new PlayerId("acceptance-bot");

        [Test]
        public void SampleHistogram_ReportsConservativeMedianP95AndHitchesWithFixedMemory()
        {
            var histogram = new AcceptanceSampleHistogram();
            for (var index = 1; index <= 100; index++)
            {
                histogram.Add(index / 10d);
            }

            histogram.Add(120d);
            histogram.Add(double.NaN);
            histogram.Add(-1d);

            Assert.That(histogram.Count, Is.EqualTo(101));
            Assert.That(histogram.MeanMilliseconds, Is.EqualTo(6.188118d).Within(0.000001d));
            Assert.That(histogram.Percentile(0.5d), Is.EqualTo(5.1d).Within(0.000001d));
            Assert.That(histogram.Percentile(0.95d), Is.EqualTo(9.6d).Within(0.000001d));
            Assert.That(histogram.MaximumMilliseconds, Is.EqualTo(120d));
            Assert.That(histogram.OverOneHundredMillisecondsCount, Is.EqualTo(1));
        }

        [Test]
        public void SeededAcceptanceMatrix_CoversDealerOptionsCantosRepeatedRoundsAndTieExtension()
        {
            var dealerSeats = new HashSet<Seat>();
            var configurations = new HashSet<string>();
            var sawCantoAnnouncement = false;
            var sawValidCanto = false;
            var sawFalseCanto = false;
            var sawRepeatedRound = false;

            for (var seed = 1; seed <= 256; seed++)
            {
                var rules = new RuleConfiguration(
                    casaCantosEnabled: seed % 2 == 0,
                    trivilinWinsImmediately: seed % 2 != 0);
                configurations.Add(
                    $"{rules.CasaCantosEnabled}:{rules.TrivilinWinsImmediately}");
                var match = FirstPlayableMatchFactory.Create(
                    seed,
                    new Player(HumanId, "Local Player", Seat.First, TeamId.One, PlayerControl.Human),
                    new Player(BotId, "Baseline Bot", Seat.Second, TeamId.Two, PlayerControl.Bot),
                    rules);

                var humanIntentCount = 0;
                while (match.State.Phase != MatchPhase.Completed && humanIntentCount < 5000)
                {
                    var legal = match.GetHumanLegalIntents();
                    Assert.That(legal, Is.Not.Empty, $"Seed {seed}, phase {match.State.Phase}");
                    var intent = ChooseAcceptanceIntent(seed, match.State, legal);
                    var advance = match.SubmitHumanIntent(intent);
                    Assert.That(
                        advance.HumanResult.IsAccepted,
                        Is.True,
                        $"Seed {seed}: {intent} -> {advance.HumanResult.Error}");
                    humanIntentCount++;
                }

                Assert.That(humanIntentCount, Is.LessThan(5000), $"Seed {seed} deadlocked.");
                Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Completed), $"Seed {seed}");
                Assert.That(match.State.WinnerTeam, Is.Not.Null, $"Seed {seed}");
                Assert.That(
                    match.Trace.IntentHistory.Where(item => item.Actor == IntentActor.Bot),
                    Has.All.Matches<IntentResolutionRecord>(item => item.IsAccepted),
                    $"Seed {seed}");

                foreach (var resolvedEvent in match.Trace.Events)
                {
                    if (resolvedEvent is DealerSelectedEvent dealer)
                    {
                        dealerSeats.Add(dealer.DealerSeat);
                    }
                    else if (resolvedEvent is CantoAnnouncedEvent)
                    {
                        sawCantoAnnouncement = true;
                    }
                    else if (resolvedEvent is CantoResolvedEvent canto)
                    {
                        sawValidCanto |= canto.IsValid;
                        sawFalseCanto |= !canto.IsValid;
                    }
                    else if (resolvedEvent is RoundCompletedEvent round)
                    {
                        sawRepeatedRound |= round.RoundNumber > 1;
                    }
                }
            }

            Assert.That(configurations, Is.EquivalentTo(new[] { "True:False", "False:True" }));
            Assert.That(dealerSeats, Is.EquivalentTo(new[] { Seat.First, Seat.Second }));
            Assert.That(sawCantoAnnouncement, Is.True);
            Assert.That(sawValidCanto, Is.True);
            Assert.That(sawFalseCanto, Is.True);
            Assert.That(sawRepeatedRound, Is.True);
            Assert.That(CompleteTieExtensionScenario(), Is.True);
        }

        private static PlayerIntent ChooseAcceptanceIntent(
            int seed,
            MatchState state,
            IReadOnlyList<PlayerIntent> legal)
        {
            var dealerSelections = legal.OfType<SelectDealerCardIntent>().ToArray();
            if (dealerSelections.Length > 0)
            {
                return seed % 2 == 0
                    ? dealerSelections.OrderByDescending(item => CardRankOrder.GetIndex(item.Card.Rank)).First()
                    : dealerSelections.OrderBy(item => CardRankOrder.GetIndex(item.Card.Rank)).First();
            }

            if (state.Phase == MatchPhase.AwaitingDealerChoice)
            {
                var handsBeforeTable = seed % 2 == 0;
                var pattern = seed % 2 == 0
                    ? OpeningPattern.Ascending
                    : OpeningPattern.Descending;
                return legal.OfType<ChooseDealOptionsIntent>()
                    .Single(item =>
                        item.DealHandsBeforeTable == handsBeforeTable
                        && item.OpeningPattern == pattern);
            }

            var cantos = legal.OfType<AnnounceCantoIntent>().ToArray();
            if (cantos.Length > 0)
            {
                var human = state.GetPlayerAt(Seat.First);
                var classified = CantoRules.Classify(human.Hand, state.Rules);
                if (classified != null && (state.DealNumber + seed) % 2 == 0)
                {
                    return cantos.Single(item => item.ClaimedKind == classified.Kind);
                }

                if ((state.DealNumber + seed) % 3 == 0)
                {
                    return cantos[0];
                }
            }

            return legal.OfType<PlayCardIntent>().FirstOrDefault() ?? legal[0];
        }

        private static bool CompleteTieExtensionScenario()
        {
            var played = new Card(CardSuit.Coins, CardRank.Two);
            var state = MatchState.CreateOneVersusOne(
                new PlayerState(
                    new Player(HumanId, "Local Player", Seat.First, TeamId.One, PlayerControl.Human),
                    new[] { played }),
                new PlayerState(
                    new Player(BotId, "Baseline Bot", Seat.Second, TeamId.Two, PlayerControl.Bot),
                    Array.Empty<Card>()),
                Seat.Second,
                Seat.First,
                new[] { new Card(CardSuit.Cups, CardRank.Five) },
                new Deck(Array.Empty<Card>()),
                isFinalDeal: true,
                teamOneScore: new Score(24),
                teamTwoScore: new Score(24));
            var session = new MatchSession(state, new SeededRandomSource(2828));
            var kickoff = session.Submit(new PlayCardIntent(HumanId, played));
            var sawTieExtension = kickoff.Events.OfType<TieExtensionStartedEvent>().Any();

            var intentCount = 0;
            while (session.State.Phase != MatchPhase.Completed && intentCount < 5000)
            {
                var player = session.State.GetPlayerAt(session.State.CurrentSeat).Player;
                var legal = session.GetLegalIntents(player.Id);
                Assert.That(legal, Is.Not.Empty);
                var intent = session.State.Phase == MatchPhase.AwaitingDealerChoice
                    ? legal.OfType<ChooseDealOptionsIntent>()
                        .Single(item =>
                            item.DealHandsBeforeTable
                            && item.OpeningPattern == OpeningPattern.Ascending)
                    : legal.First(item => !(item is AnnounceCantoIntent));
                var result = session.Submit(intent);
                Assert.That(result.IsAccepted, Is.True, $"{intent}: {result.Error}");
                sawTieExtension |= result.Events.OfType<TieExtensionStartedEvent>().Any();
                intentCount++;
            }

            Assert.That(intentCount, Is.LessThan(5000));
            Assert.That(session.State.Phase, Is.EqualTo(MatchPhase.Completed));
            Assert.That(session.State.WinnerTeam, Is.Not.Null);
            Assert.That(session.State.RoundNumber, Is.GreaterThan(1));
            return sawTieExtension;
        }
    }
}
