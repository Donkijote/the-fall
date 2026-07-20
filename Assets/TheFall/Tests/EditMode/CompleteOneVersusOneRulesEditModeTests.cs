using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;

namespace TheFall.Tests.EditMode
{
    public sealed class CompleteOneVersusOneRulesEditModeTests
    {
        private static readonly PlayerId FirstId = new PlayerId("complete-first");
        private static readonly PlayerId SecondId = new PlayerId("complete-second");

        [Test]
        public void DealerSelection_TiesContinueFromRemainingSpreadThenRestoreAndShuffleAllCards()
        {
            var session = CreateSession(2201);
            var tiedRank = session.State.Deck.Cards[0].Rank;
            var tiedCards = session.State.Deck.Cards.Where(card => card.Rank == tiedRank).Take(2).ToArray();

            var first = session.Submit(new SelectDealerCardIntent(FirstId, tiedCards[0]));
            var tie = session.Submit(new SelectDealerCardIntent(SecondId, tiedCards[1]));

            Assert.That(first.IsAccepted, Is.True);
            Assert.That(tie.Events.OfType<DealerSelectionTiedEvent>().Single().Rank, Is.EqualTo(tiedRank));
            Assert.That(tie.State.Deck.Count, Is.EqualTo(38));
            Assert.That(tie.State.CurrentSeat, Is.EqualTo(Seat.First));

            var low = tie.State.Deck.Cards.OrderBy(card => CardRankOrder.GetIndex(card.Rank)).First();
            session.Submit(new SelectDealerCardIntent(FirstId, low));
            var high = session.State.Deck.Cards.OrderByDescending(card => CardRankOrder.GetIndex(card.Rank)).First();
            var resolved = session.Submit(new SelectDealerCardIntent(SecondId, high));

            Assert.That(resolved.State.Phase, Is.EqualTo(MatchPhase.AwaitingDealerChoice));
            Assert.That(resolved.State.DealerSeat, Is.EqualTo(Seat.Second));
            Assert.That(resolved.State.Deck.Count, Is.EqualTo(40));
            Assert.That(resolved.Events.Select(item => item.Kind), Does.Contain(DomainEventKind.DealerSelected));
            Assert.That(resolved.Events.Select(item => item.Kind), Does.Contain(DomainEventKind.DeckShuffled));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DealerChoice_ControlsDealOrderAndProducesUniqueOpeningTable(bool handsBeforeTable)
        {
            var session = CreateSession(2202);
            CompleteDealerSelection(session);
            var dealer = session.State.GetPlayerAt(session.State.DealerSeat).Player.Id;

            var result = session.Submit(new ChooseDealOptionsIntent(
                dealer,
                handsBeforeTable,
                OpeningPattern.Ascending));

            var firstDealIndex = result.Events.ToList().FindIndex(item => item is CardDealtEvent);
            var firstTableIndex = result.Events.ToList().FindIndex(item => item is OpeningCardPlacedEvent);
            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.State.Players, Has.All.Matches<PlayerState>(player => player.Hand.Count == 3));
            Assert.That(result.State.Table.Select(card => card.Rank).Distinct().Count(), Is.EqualTo(4));
            Assert.That(result.State.Deck.Count, Is.EqualTo(30));
            Assert.That(firstDealIndex < firstTableIndex, Is.EqualTo(handsBeforeTable));

            var expectedPoints = result.State.Table
                .Select((card, index) => card.Rank == (CardRank)(index + 1) ? index + 1 : 0)
                .Sum();
            var awarded = result.Events
                .OfType<ScoreChangedEvent>()
                .Where(item => item.Reason == ScoreReason.OpeningPattern)
                .Sum(item => item.PointsAwarded);
            Assert.That(awarded, Is.EqualTo(expectedPoints));
        }

        [Test]
        public void OpeningDuplicate_IsReinsertedBeforeAUniqueReplacementOccupiesThePosition()
        {
            RuleResult resultWithDuplicate = null;
            for (var seed = 1; seed <= 64 && resultWithDuplicate == null; seed++)
            {
                var session = CreateSession(seed);
                CompleteDealerSelection(session);
                var dealer = session.State.GetPlayerAt(session.State.DealerSeat).Player.Id;
                var result = session.Submit(new ChooseDealOptionsIntent(
                    dealer,
                    false,
                    OpeningPattern.Descending));
                if (result.Events.OfType<OpeningCardRejectedEvent>().Any())
                {
                    resultWithDuplicate = result;
                }
            }

            Assert.That(resultWithDuplicate, Is.Not.Null, "Expected a deterministic seed with an opening duplicate.");
            Assert.That(resultWithDuplicate.State.Table.Select(card => card.Rank).Distinct().Count(), Is.EqualTo(4));
            Assert.That(resultWithDuplicate.Events.OfType<OpeningCardRejectedEvent>(), Is.Not.Empty);
        }

        [Test]
        public void CantoClassification_CoversOptionsAdjacencyAndFallbacks()
        {
            AssertCanto(CantoKind.CasaGrande, 12, RuleConfiguration.Standard, CardRank.Twelve, CardRank.Twelve, CardRank.One);
            AssertCanto(CantoKind.CasaChica, 10, RuleConfiguration.Standard, CardRank.Eleven, CardRank.Eleven, CardRank.One);
            AssertCanto(CantoKind.Registro, 8, RuleConfiguration.Standard, CardRank.Twelve, CardRank.Eleven, CardRank.One);
            AssertCanto(CantoKind.Vigia, 7, RuleConfiguration.Standard, CardRank.Seven, CardRank.Seven, CardRank.Ten);
            AssertCanto(CantoKind.Patrulla, 6, RuleConfiguration.Standard, CardRank.Six, CardRank.Seven, CardRank.Ten);
            AssertCanto(CantoKind.Trivilin, 5, RuleConfiguration.Standard, CardRank.Five, CardRank.Five, CardRank.Five);
            AssertCanto(CantoKind.Ronda, 4, new RuleConfiguration(casaCantosEnabled: false), CardRank.Twelve, CardRank.Twelve, CardRank.One);
            Assert.That(
                CantoRules.Classify(CreateHand(CardRank.Twelve, CardRank.One, CardRank.Two), RuleConfiguration.Standard),
                Is.Null,
                "Patrulla must not wrap from 12 to 1.");
        }

        [Test]
        public void FalseCantoPenalty_ResolvesBeforeTheStrongestValidCantoScores()
        {
            var firstHand = CreateHand(CardRank.Two, CardRank.Four, CardRank.Six);
            var secondHand = CreateHand(CardRank.Ten, CardRank.Ten, CardRank.One);
            var state = CreateActiveState(
                firstHand,
                secondHand,
                new[] { new Card(CardSuit.Clubs, CardRank.Twelve) },
                isFinalDeal: true,
                firstScore: new Score(2));
            var session = new MatchSession(state, new SeededRandomSource(2203));
            var events = new List<DomainEvent>();

            Submit(session, new AnnounceCantoIntent(FirstId, CantoKind.CasaGrande), events);
            Submit(session, new PlayCardIntent(FirstId, firstHand[0]), events);
            Submit(session, new AnnounceCantoIntent(SecondId, CantoKind.Ronda), events);
            Submit(session, new PlayCardIntent(SecondId, secondHand[0]), events);
            Submit(session, new PlayCardIntent(FirstId, firstHand[1]), events);
            Submit(session, new PlayCardIntent(SecondId, secondHand[1]), events);
            Submit(session, new PlayCardIntent(FirstId, firstHand[2]), events);
            Submit(session, new PlayCardIntent(SecondId, secondHand[2]), events);

            var scoreEvents = events.OfType<ScoreChangedEvent>().ToArray();
            var penaltyIndex = Array.FindIndex(scoreEvents, item => item.Reason == ScoreReason.FalseCantoPenalty);
            var cantoIndex = Array.FindIndex(scoreEvents, item => item.Reason == ScoreReason.Canto);
            Assert.That(penaltyIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(cantoIndex, Is.GreaterThan(penaltyIndex));
            Assert.That(session.State.TeamOneScore.Value, Is.EqualTo(1));
            Assert.That(session.State.TeamTwoScore.Value, Is.EqualTo(2));
        }

        [Test]
        public void EqualCantos_UseUnderlyingStrengthThenDealerRightOrder()
        {
            var firstHand = new[]
            {
                new Card(CardSuit.Coins, CardRank.Ten),
                new Card(CardSuit.Cups, CardRank.Ten),
                new Card(CardSuit.Coins, CardRank.Two),
            };
            var secondHand = new[]
            {
                new Card(CardSuit.Swords, CardRank.Ten),
                new Card(CardSuit.Clubs, CardRank.Ten),
                new Card(CardSuit.Cups, CardRank.Four),
            };
            var state = CreateActiveState(
                firstHand,
                secondHand,
                new[] { new Card(CardSuit.Clubs, CardRank.Twelve) },
                isFinalDeal: true);
            var session = new MatchSession(state, new SeededRandomSource(2210));
            var events = new List<DomainEvent>();

            Submit(session, new AnnounceCantoIntent(FirstId, CantoKind.Ronda), events);
            Submit(session, new PlayCardIntent(FirstId, firstHand[0]), events);
            Submit(session, new AnnounceCantoIntent(SecondId, CantoKind.Ronda), events);
            Submit(session, new PlayCardIntent(SecondId, secondHand[2]), events);
            Submit(session, new PlayCardIntent(FirstId, firstHand[1]), events);
            Submit(session, new PlayCardIntent(SecondId, secondHand[0]), events);
            Submit(session, new PlayCardIntent(FirstId, firstHand[2]), events);
            Submit(session, new PlayCardIntent(SecondId, secondHand[1]), events);

            var resolved = events.OfType<CantoResolvedEvent>().Where(item => item.IsValid).ToArray();
            Assert.That(resolved.Single(item => item.PlayerId == FirstId).DidScore, Is.True);
            Assert.That(resolved.Single(item => item.PlayerId == SecondId).DidScore, Is.False);
            Assert.That(session.State.TeamOneScore.Value, Is.EqualTo(2));
            Assert.That(session.State.TeamTwoScore.Value, Is.EqualTo(0));
        }

        [Test]
        public void ImmediateTrivilinOption_CompletesTheMatchAtAnnouncement()
        {
            var rules = new RuleConfiguration(trivilinWinsImmediately: true);
            var triple = CreateHand(CardRank.Five, CardRank.Five, CardRank.Five);
            var state = CreateActiveState(triple, CreateHand(CardRank.One, CardRank.Three, CardRank.Six), Array.Empty<Card>(), rules);

            var result = OneVersusOneRules.Resolve(
                state,
                new AnnounceCantoIntent(FirstId, CantoKind.Trivilin));

            Assert.That(result.State.Phase, Is.EqualTo(MatchPhase.Completed));
            Assert.That(result.State.WinnerTeam, Is.EqualTo(TeamId.One));
            Assert.That(result.Events.OfType<MatchCompletedEvent>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void CapturedCards_AwardOnlyExcessAboveTwentyBeforeVictoryCheck()
        {
            var played = new Card(CardSuit.Coins, CardRank.Twelve);
            var tableCard = new Card(CardSuit.Cups, CardRank.Twelve);
            var captured = Deck.CreateSpanishDeck().Cards.Take(20).ToArray();
            var state = MatchState.CreateOneVersusOne(
                new PlayerState(CreateFirstPlayer(), new[] { played }, captured),
                new PlayerState(CreateSecondPlayer(), Array.Empty<Card>()),
                Seat.Second,
                Seat.First,
                new[] { tableCard },
                new Deck(Array.Empty<Card>()),
                isFinalDeal: true,
                teamOneScore: new Score(23));

            var result = OneVersusOneRules.Resolve(
                state,
                new PlayCardIntent(FirstId, played),
                new SeededRandomSource(2204));

            var countScore = result.Events.OfType<ScoreChangedEvent>()
                .Single(item => item.Reason == ScoreReason.CapturedCards);
            Assert.That(countScore.PointsAwarded, Is.EqualTo(2));
            Assert.That(result.State.TeamOneScore.Value, Is.EqualTo(25));
            Assert.That(result.State.WinnerTeam, Is.EqualTo(TeamId.One));
        }

        [Test]
        public void EqualLeadersAtTarget_StartAFullTieExtensionWithRotatedDealer()
        {
            var played = new Card(CardSuit.Coins, CardRank.Two);
            var state = MatchState.CreateOneVersusOne(
                new PlayerState(CreateFirstPlayer(), new[] { played }),
                new PlayerState(CreateSecondPlayer(), Array.Empty<Card>()),
                Seat.Second,
                Seat.First,
                new[] { new Card(CardSuit.Cups, CardRank.Five) },
                new Deck(Array.Empty<Card>()),
                isFinalDeal: true,
                teamOneScore: new Score(24),
                teamTwoScore: new Score(24));

            var result = OneVersusOneRules.Resolve(
                state,
                new PlayCardIntent(FirstId, played),
                new SeededRandomSource(2205));

            Assert.That(result.State.Phase, Is.EqualTo(MatchPhase.AwaitingDealerChoice));
            Assert.That(result.State.IsTieExtension, Is.True);
            Assert.That(result.State.RoundNumber, Is.EqualTo(2));
            Assert.That(result.State.DealerSeat, Is.EqualTo(Seat.First));
            Assert.That(result.State.Deck.Count, Is.EqualTo(40));
            Assert.That(result.Events.OfType<TieExtensionStartedEvent>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void InvalidIntent_ReturnsExplicitErrorWithoutMutatingSetupState()
        {
            var session = CreateSession(2206);
            var state = session.State;

            var result = OneVersusOneRules.Resolve(
                state,
                new ChooseDealOptionsIntent(FirstId, true, OpeningPattern.Ascending),
                new SeededRandomSource(2206));

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(RuleError.WrongPhase));
            Assert.That(result.State, Is.SameAs(state));
            Assert.That(result.Events, Is.Empty);
            Assert.That(state.Deck.Count, Is.EqualTo(40));
        }

        [Test]
        public void CompleteSeededMatch_ReplaysToTheSameUniqueWinnerStateAndOrderedEvents()
        {
            var first = PlayCompleteMatch(22);
            var second = PlayCompleteMatch(22);

            Assert.That(first.State.Phase, Is.EqualTo(MatchPhase.Completed));
            Assert.That(first.State.WinnerTeam, Is.Not.Null);
            Assert.That(first.IntentCount, Is.LessThan(5000));
            Assert.That(Snapshot(second.State), Is.EqualTo(Snapshot(first.State)));
            Assert.That(second.Events, Is.EqualTo(first.Events));
        }

        private static MatchReplay PlayCompleteMatch(int seed)
        {
            var session = CreateSession(seed);
            var eventLog = new List<string>(session.StartupEvents.Select(Describe));
            var intentCount = 0;
            while (session.State.Phase != MatchPhase.Completed && intentCount < 5000)
            {
                var player = session.State.GetPlayerAt(session.State.CurrentSeat).Player;
                var legal = session.GetLegalIntents(player.Id);
                PlayerIntent intent;
                if (session.State.Phase == MatchPhase.AwaitingDealerChoice)
                {
                    intent = legal.OfType<ChooseDealOptionsIntent>()
                        .Single(item => item.DealHandsBeforeTable && item.OpeningPattern == OpeningPattern.Ascending);
                }
                else
                {
                    intent = legal.First(item => !(item is AnnounceCantoIntent));
                }

                var result = session.Submit(intent);
                Assert.That(result.IsAccepted, Is.True, $"Seed {seed}, intent {intent}, error {result.Error}");
                eventLog.AddRange(result.Events.Select(Describe));
                intentCount++;
            }

            return new MatchReplay(session.State, intentCount, eventLog);
        }

        private static MatchSession CreateSession(int seed)
        {
            return new MatchSession(
                CreateFirstPlayer(),
                CreateSecondPlayer(),
                RuleConfiguration.Standard,
                new SeededRandomSource(seed));
        }

        private static void CompleteDealerSelection(MatchSession session)
        {
            var low = session.State.Deck.Cards.OrderBy(card => CardRankOrder.GetIndex(card.Rank)).First();
            var first = session.Submit(new SelectDealerCardIntent(FirstId, low));
            Assert.That(first.IsAccepted, Is.True);
            var high = session.State.Deck.Cards.OrderByDescending(card => CardRankOrder.GetIndex(card.Rank)).First();
            var second = session.Submit(new SelectDealerCardIntent(SecondId, high));
            Assert.That(second.IsAccepted, Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(MatchPhase.AwaitingDealerChoice));
        }

        private static void Submit(MatchSession session, PlayerIntent intent, ICollection<DomainEvent> events)
        {
            var result = session.Submit(intent);
            Assert.That(result.IsAccepted, Is.True, $"{intent}: {result.Error}");
            foreach (var resolvedEvent in result.Events)
            {
                events.Add(resolvedEvent);
            }
        }

        private static void AssertCanto(
            CantoKind kind,
            int points,
            RuleConfiguration rules,
            CardRank first,
            CardRank second,
            CardRank third)
        {
            var classification = CantoRules.Classify(CreateHand(first, second, third), rules);
            Assert.That(classification, Is.Not.Null);
            Assert.That(classification.Kind, Is.EqualTo(kind));
            Assert.That(classification.Points, Is.EqualTo(points));
        }

        private static Card[] CreateHand(CardRank first, CardRank second, CardRank third)
        {
            return new[]
            {
                new Card(CardSuit.Coins, first),
                new Card(CardSuit.Cups, second),
                new Card(CardSuit.Swords, third),
            };
        }

        private static MatchState CreateActiveState(
            IEnumerable<Card> firstHand,
            IEnumerable<Card> secondHand,
            IEnumerable<Card> table,
            RuleConfiguration rules = null,
            bool isFinalDeal = false,
            Score firstScore = default,
            Score secondScore = default)
        {
            return MatchState.CreateOneVersusOne(
                new PlayerState(CreateFirstPlayer(), firstHand),
                new PlayerState(CreateSecondPlayer(), secondHand),
                Seat.Second,
                Seat.First,
                table,
                new Deck(Array.Empty<Card>()),
                rules,
                isFinalDeal,
                firstScore,
                secondScore);
        }

        private static Player CreateFirstPlayer()
        {
            return new Player(FirstId, "First", Seat.First, TeamId.One, PlayerControl.Human);
        }

        private static Player CreateSecondPlayer()
        {
            return new Player(SecondId, "Second", Seat.Second, TeamId.Two, PlayerControl.Bot);
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

            if (resolvedEvent is TurnChangedEvent turn)
            {
                return $"{turn.Kind}:{turn.PreviousSeat}:{turn.CurrentSeat}";
            }

            if (resolvedEvent is DealerSelectedEvent dealer)
            {
                return $"{dealer.Kind}:{dealer.PlayerId}:{dealer.DealerSeat}";
            }

            if (resolvedEvent is MatchCompletedEvent completed)
            {
                return $"{completed.Kind}:{completed.WinnerTeam}";
            }

            return resolvedEvent.Kind.ToString();
        }

        private sealed class MatchReplay
        {
            public MatchReplay(MatchState state, int intentCount, IReadOnlyList<string> events)
            {
                State = state;
                IntentCount = intentCount;
                Events = events;
            }

            public MatchState State { get; }

            public int IntentCount { get; }

            public IReadOnlyList<string> Events { get; }
        }
    }
}
