using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;

namespace TheFall.Tests.EditMode
{
    public sealed class FirstPlayableMatchOrchestrationEditModeTests
    {
        private static readonly PlayerId HumanId = new PlayerId("human");
        private static readonly PlayerId BotId = new PlayerId("baseline-bot");

        [Test]
        public void HumanFacingSession_CompletesWhileTheBotSuppliesEveryOpponentChoice()
        {
            var match = PlayCompleteMatch(2301);

            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Completed));
            Assert.That(match.State.WinnerTeam, Is.Not.Null);
            Assert.That(match.Trace.FinalState, Is.SameAs(match.State));
            Assert.That(match.Trace.IntentHistory, Has.Some.Matches<IntentResolutionRecord>(item => item.Actor == IntentActor.Human));
            Assert.That(match.Trace.IntentHistory, Has.Some.Matches<IntentResolutionRecord>(item => item.Actor == IntentActor.Bot));
            Assert.That(match.Trace.IntentHistory.Count, Is.LessThan(5000));
        }

        [Test]
        public void BotSubmissions_AreAcceptedMembersOfTheSharedLegalIntentSurface()
        {
            var match = PlayCompleteMatch(2302);
            var botRecords = match.Trace.IntentHistory.Where(item => item.Actor == IntentActor.Bot).ToArray();

            Assert.That(botRecords, Is.Not.Empty);
            foreach (var record in botRecords)
            {
                var legal = OneVersusOneRules.GetLegalIntents(record.PriorState, BotId);
                Assert.That(record.IsAccepted, Is.True, $"Seed {match.Trace.Seed}: {record.Intent} -> {record.Error}");
                Assert.That(legal.Any(item => Describe(item) == Describe(record.Intent)), Is.True, record.Intent.ToString());
            }

            Assert.That(botRecords.Select(item => item.Intent), Has.Some.TypeOf<SelectDealerCardIntent>());
            Assert.That(botRecords.Select(item => item.Intent), Has.Some.TypeOf<PlayCardIntent>());
        }

        [Test]
        public void BotTurnView_PublicContractOmitsOpponentHandsAndHiddenDeckState()
        {
            var publicProperties = typeof(BotTurnView)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .ToArray();
            var publicConstructors = typeof(BotTurnView)
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            Assert.That(publicConstructors, Is.Empty);
            Assert.That(publicProperties, Does.Contain(nameof(BotTurnView.OwnHand)));
            Assert.That(publicProperties, Does.Contain(nameof(BotTurnView.Table)));
            Assert.That(publicProperties, Does.Not.Contain("Deck"));
            Assert.That(publicProperties, Does.Not.Contain("Players"));
            Assert.That(publicProperties, Does.Not.Contain("OpponentHand"));
            Assert.That(publicProperties, Does.Not.Contain("LegalIntents"));
        }

        [Test]
        public void SameSeedAndHumanChoices_ReplayIdenticalBotChoicesEventsAndFinalState()
        {
            var first = PlayCompleteMatch(2303);
            var second = PlayCompleteMatch(2303);

            Assert.That(
                second.Trace.IntentHistory.Select(item => $"{item.Actor}:{Describe(item.Intent)}"),
                Is.EqualTo(first.Trace.IntentHistory.Select(item => $"{item.Actor}:{Describe(item.Intent)}")));
            Assert.That(
                second.Trace.Events.Select(Describe),
                Is.EqualTo(first.Trace.Events.Select(Describe)));
            Assert.That(Snapshot(second.State), Is.EqualTo(Snapshot(first.State)));
        }

        [Test]
        public void RejectedHumanIntent_RetainsSeedIntentStateAndEmptyEventsForDiagnosis()
        {
            var match = CreateMatch(2304);
            var priorState = match.State;
            var invalidIntent = new ChooseDealOptionsIntent(HumanId, true, OpeningPattern.Ascending);

            var advance = match.SubmitHumanIntent(invalidIntent);
            var record = advance.Resolutions.Single();

            Assert.That(advance.HumanResult.IsAccepted, Is.False);
            Assert.That(advance.HumanResult.Error, Is.EqualTo(RuleError.WrongPhase));
            Assert.That(match.Trace.Seed, Is.EqualTo(2304));
            Assert.That(record.Intent, Is.SameAs(invalidIntent));
            Assert.That(record.PriorState, Is.SameAs(priorState));
            Assert.That(record.ResultingState, Is.SameAs(priorState));
            Assert.That(record.Events, Is.Empty);
            Assert.That(match.Trace.FinalState, Is.SameAs(priorState));
        }

        [Test]
        public void SeededSimulationMatrix_CompletesWithoutInvalidBotIntentsOrDeadlock()
        {
            var botIntentTypes = new HashSet<Type>();
            for (var seed = 1; seed <= 24; seed++)
            {
                var match = PlayCompleteMatch(seed);
                var invalidBotRecords = match.Trace.IntentHistory
                    .Where(item => item.Actor == IntentActor.Bot && !item.IsAccepted)
                    .ToArray();
                foreach (var botRecord in match.Trace.IntentHistory.Where(item => item.Actor == IntentActor.Bot))
                {
                    botIntentTypes.Add(botRecord.Intent.GetType());
                }

                Assert.That(invalidBotRecords, Is.Empty, $"Seed {seed}");
                Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Completed), $"Seed {seed}");
                Assert.That(match.State.WinnerTeam, Is.Not.Null, $"Seed {seed}");
                Assert.That(match.Trace.FinalState, Is.SameAs(match.State), $"Seed {seed}");
            }

            Assert.That(botIntentTypes, Does.Contain(typeof(SelectDealerCardIntent)));
            Assert.That(botIntentTypes, Does.Contain(typeof(ChooseDealOptionsIntent)));
            Assert.That(botIntentTypes, Does.Contain(typeof(AnnounceCantoIntent)));
            Assert.That(botIntentTypes, Does.Contain(typeof(PlayCardIntent)));
        }

        private static FirstPlayableMatchOrchestrator PlayCompleteMatch(int seed)
        {
            var match = CreateMatch(seed);
            var humanIntentCount = 0;
            while (match.State.Phase != MatchPhase.Completed && humanIntentCount < 5000)
            {
                var legal = match.GetHumanLegalIntents();
                Assert.That(legal, Is.Not.Empty, $"Seed {seed}, phase {match.State.Phase}");
                var intent = ChooseHumanIntent(match.State, legal);
                var result = match.SubmitHumanIntent(intent);
                Assert.That(result.HumanResult.IsAccepted, Is.True, $"Seed {seed}: {intent} -> {result.HumanResult.Error}");
                humanIntentCount++;
            }

            Assert.That(humanIntentCount, Is.LessThan(5000), $"Seed {seed} deadlocked.");
            return match;
        }

        private static FirstPlayableMatchOrchestrator CreateMatch(int seed)
        {
            return FirstPlayableMatchFactory.Create(
                seed,
                new Player(HumanId, "Local Player", Seat.First, TeamId.One, PlayerControl.Human),
                new Player(BotId, "Baseline Bot", Seat.Second, TeamId.Two, PlayerControl.Bot),
                RuleConfiguration.Standard);
        }

        private static PlayerIntent ChooseHumanIntent(MatchState state, IReadOnlyList<PlayerIntent> legal)
        {
            if (state.Phase == MatchPhase.AwaitingDealerChoice)
            {
                return legal.OfType<ChooseDealOptionsIntent>()
                    .Single(item => item.DealHandsBeforeTable && item.OpeningPattern == OpeningPattern.Ascending);
            }

            var play = legal.OfType<PlayCardIntent>().FirstOrDefault();
            return play ?? legal[0];
        }

        private static string Describe(PlayerIntent intent)
        {
            return intent.ToString();
        }

        private static string Describe(DomainEvent resolvedEvent)
        {
            if (resolvedEvent is CardPlayedEvent played)
            {
                return $"{played.Kind}:{played.PlayerId}:{played.Card}";
            }

            if (resolvedEvent is ScoreChangedEvent score)
            {
                return $"{score.Kind}:{score.TeamId}:{score.Reason}:{score.PointsAwarded}:{score.Total.Value}";
            }

            if (resolvedEvent is DealerCardSelectedEvent selection)
            {
                return $"{selection.Kind}:{selection.PlayerId}:{selection.Card}";
            }

            if (resolvedEvent is MatchCompletedEvent completed)
            {
                return $"{completed.Kind}:{completed.WinnerTeam}";
            }

            return resolvedEvent.Kind.ToString();
        }

        private static string Snapshot(MatchState state)
        {
            return string.Join(
                ";",
                state.Phase,
                state.WinnerTeam?.ToString() ?? "none",
                state.RoundNumber,
                state.DealNumber,
                state.DealerSeat,
                state.CurrentSeat,
                state.TeamOneScore.Value,
                state.TeamTwoScore.Value,
                state.IsTieExtension,
                string.Join(",", state.Deck.Cards),
                string.Join(",", state.Table),
                string.Join("|", state.Players.Select(player =>
                    $"{player.Player.Id}:{string.Join(",", player.Hand)}:{string.Join(",", player.CapturedCards)}")));
        }
    }
}
