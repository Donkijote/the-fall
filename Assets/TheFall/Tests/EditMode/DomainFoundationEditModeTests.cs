using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;
using TheFall.Presentation;

namespace TheFall.Tests.EditMode
{
    public sealed class DomainFoundationEditModeTests
    {
        private static readonly PlayerId HumanId = new PlayerId("human");
        private static readonly PlayerId BotId = new PlayerId("bot");
        private static readonly Card HumanSeven = new Card(CardSuit.Clubs, CardRank.Seven);
        private static readonly Card BotTwo = new Card(CardSuit.Coins, CardRank.Two);
        private static readonly Card BotSeven = new Card(CardSuit.Coins, CardRank.Seven);

        [Test]
        public void SpanishDeck_ContainsFortyUniqueCardsAndNoEightOrNine()
        {
            var deck = Deck.CreateSpanishDeck();

            Assert.That(deck.Count, Is.EqualTo(40));
            Assert.That(deck.Cards.Distinct().Count(), Is.EqualTo(40));
            Assert.That(deck.Cards.All(card => (int)card.Rank != 8 && (int)card.Rank != 9), Is.True);
        }

        [Test]
        public void SeededShuffle_WithTheSameSeedProducesTheSameDeckOrder()
        {
            var first = Deck.CreateSpanishDeck().Shuffle(new SeededRandomSource(240719));
            var second = Deck.CreateSpanishDeck().Shuffle(new SeededRandomSource(240719));

            Assert.That(second.Cards, Is.EqualTo(first.Cards));
        }

        [Test]
        public void RecordedIntents_WithTheSameSetupAndSeedProduceTheSameStateAndEvents()
        {
            const int seed = 1977;
            var first = ReplayRepresentativeTurnSequence(seed);
            var second = ReplayRepresentativeTurnSequence(seed);
            var failureContext = string.Join(
                Environment.NewLine,
                $"Seed: {seed}",
                $"Initial state: {first.InitialState}",
                $"Intents: {string.Join(" -> ", first.Intents)}",
                $"First events: {string.Join(" | ", first.EventLog)}",
                $"Second events: {string.Join(" | ", second.EventLog)}");

            Assert.That(Snapshot(second.State), Is.EqualTo(Snapshot(first.State)), failureContext);
            Assert.That(second.EventLog, Is.EqualTo(first.EventLog), failureContext);
        }

        [Test]
        public void PlayCard_CapturesSameRankAndSequentialCascadeThenStopsAtGap()
        {
            var session = new MatchSession(CreateRepresentativeState(1977));

            var result = session.Submit(new PlayCardIntent(BotId, BotTwo));

            var capture = result.Events.OfType<CardsCapturedEvent>().Single();
            Assert.That(result.IsAccepted, Is.True);
            Assert.That(capture.Cards.Select(card => card.Rank), Is.EqualTo(new[]
            {
                CardRank.Two,
                CardRank.Two,
                CardRank.Three,
                CardRank.Four,
            }));
            Assert.That(result.State.Table.Select(card => card.Rank), Is.EqualTo(new[] { CardRank.Six }));
            Assert.That(result.State.GetPlayer(BotId).CapturedCards.Count, Is.EqualTo(4));
        }

        [Test]
        public void ImmediateSameRankCapture_AwardsFallAndCleanTableOutsideFinalDeal()
        {
            var tableCard = new Card(CardSuit.Cups, CardRank.Twelve);
            var playedCard = new Card(CardSuit.Coins, CardRank.Twelve);
            var state = CreateState(
                new[] { new Card(CardSuit.Clubs, CardRank.One) },
                new[] { playedCard },
                new[] { tableCard },
                previousPlay: new PreviousPlay(HumanId, tableCard, false));

            var result = OneVersusOneRules.Resolve(state, new PlayCardIntent(BotId, playedCard));

            var scoreEvents = result.Events.OfType<ScoreChangedEvent>().ToArray();
            Assert.That(scoreEvents.Select(score => score.Reason), Is.EqualTo(new[]
            {
                ScoreReason.Fall,
                ScoreReason.CleanTable,
            }));
            Assert.That(scoreEvents.Select(score => score.PointsAwarded), Is.EqualTo(new[] { 4, 4 }));
            Assert.That(result.State.TeamTwoScore.Value, Is.EqualTo(8));
            Assert.That(result.State.Table, Is.Empty);
        }

        [Test]
        public void InvalidIntent_ReturnsExplicitErrorAndKeepsOriginalState()
        {
            var state = CreateRepresentativeState(1977);
            var wrongTurn = new PlayCardIntent(HumanId, HumanSeven);

            var result = OneVersusOneRules.Resolve(state, wrongTurn);

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(RuleError.NotPlayersTurn));
            Assert.That(result.State, Is.SameAs(state));
            Assert.That(result.Events, Is.Empty);
            Assert.That(state.GetPlayer(HumanId).Hand, Does.Contain(HumanSeven));
        }

        [Test]
        public void HumanAndBotPlayers_UseTheSameLegalIntentSurface()
        {
            var botTurn = new MatchSession(CreateRepresentativeState(1977));
            var botIntents = botTurn.GetLegalIntents(BotId);
            var botResult = botTurn.Submit(botIntents.OfType<PlayCardIntent>().First());

            var humanIntents = botTurn.GetLegalIntents(HumanId);
            var humanResult = botTurn.Submit(humanIntents.OfType<PlayCardIntent>().First());

            Assert.That(botIntents.OfType<PlayCardIntent>().Count(), Is.EqualTo(3));
            Assert.That(humanIntents.OfType<PlayCardIntent>().Count(), Is.EqualTo(3));
            Assert.That(botIntents.OfType<AnnounceCantoIntent>().Count(), Is.EqualTo(7));
            Assert.That(humanIntents.OfType<AnnounceCantoIntent>().Count(), Is.EqualTo(7));
            Assert.That(botResult.IsAccepted, Is.True);
            Assert.That(humanResult.IsAccepted, Is.True);
        }

        [Test]
        public void PresentationBuffer_ConsumesResolvedStateAndEventsWithoutRuleEvaluation()
        {
            var result = OneVersusOneRules.Resolve(
                CreateRepresentativeState(1977),
                new PlayCardIntent(BotId, BotTwo));
            var buffer = new ResolvedMatchBuffer();

            buffer.Consume(result);

            Assert.That(buffer.State, Is.SameAs(result.State));
            Assert.That(buffer.Events, Is.SameAs(result.Events));
            Assert.That(buffer.Events.Select(resolvedEvent => resolvedEvent.Kind), Is.EqualTo(
                result.Events.Select(resolvedEvent => resolvedEvent.Kind)));
        }

        private static ReplayResult ReplayRepresentativeTurnSequence(int seed)
        {
            var initialState = CreateRepresentativeState(seed);
            var session = new MatchSession(initialState);
            var eventLog = new List<string>();
            var intents = new PlayerIntent[]
            {
                new PlayCardIntent(BotId, BotTwo),
                new PlayCardIntent(HumanId, HumanSeven),
                new PlayCardIntent(BotId, BotSeven),
            };

            foreach (var intent in intents)
            {
                var result = session.Submit(intent);
                Assert.That(result.IsAccepted, Is.True, intent.ToString());
                eventLog.AddRange(result.Events.Select(Describe));
            }

            return new ReplayResult(
                Snapshot(initialState),
                intents.Select(intent => intent.ToString()).ToArray(),
                session.State,
                eventLog);
        }

        private static MatchState CreateRepresentativeState(int seed)
        {
            var humanHand = new[]
            {
                HumanSeven,
                new Card(CardSuit.Cups, CardRank.Ten),
                new Card(CardSuit.Swords, CardRank.Eleven),
            };
            var botHand = new[]
            {
                BotTwo,
                BotSeven,
                new Card(CardSuit.Swords, CardRank.Twelve),
            };
            var table = new[]
            {
                new Card(CardSuit.Cups, CardRank.Two),
                new Card(CardSuit.Clubs, CardRank.Three),
                new Card(CardSuit.Coins, CardRank.Four),
                new Card(CardSuit.Cups, CardRank.Six),
            };
            var usedCards = humanHand.Concat(botHand).Concat(table);
            var remainingDeck = Deck.CreateSpanishDeck()
                .Remove(usedCards)
                .Shuffle(new SeededRandomSource(seed));

            return CreateState(humanHand, botHand, table, remainingDeck);
        }

        private static MatchState CreateState(
            IEnumerable<Card> humanHand,
            IEnumerable<Card> botHand,
            IEnumerable<Card> table,
            Deck deck = null,
            PreviousPlay previousPlay = null)
        {
            var human = new Player(
                HumanId,
                "Human",
                Seat.First,
                TeamId.One,
                PlayerControl.Human);
            var bot = new Player(
                BotId,
                "Bot",
                Seat.Second,
                TeamId.Two,
                PlayerControl.Bot);

            return MatchState.CreateOneVersusOne(
                new PlayerState(human, humanHand),
                new PlayerState(bot, botHand),
                Seat.First,
                Seat.Second,
                table,
                deck ?? new Deck(Array.Empty<Card>()),
                previousPlay: previousPlay);
        }

        private static string Snapshot(MatchState state)
        {
            var players = string.Join("|", state.Players.Select(player =>
                $"{player.Player.Id}:{string.Join(",", player.Hand)}:{string.Join(",", player.CapturedCards)}"));
            return string.Join(
                ";",
                state.CurrentSeat,
                string.Join(",", state.Table),
                players,
                string.Join(",", state.Deck.Cards),
                state.TeamOneScore.Value,
                state.TeamTwoScore.Value,
                state.Phase,
                state.WinnerTeam?.ToString() ?? "none");
        }

        private static string Describe(DomainEvent resolvedEvent)
        {
            if (resolvedEvent is CardPlayedEvent played)
            {
                return $"played:{played.PlayerId}:{played.Card}";
            }

            if (resolvedEvent is CardPlacedOnTableEvent placed)
            {
                return $"placed:{placed.PlayerId}:{placed.Card}";
            }

            if (resolvedEvent is CardsCapturedEvent captured)
            {
                return $"captured:{captured.PlayerId}:{string.Join(",", captured.Cards)}";
            }

            if (resolvedEvent is ScoreChangedEvent score)
            {
                return $"score:{score.TeamId}:{score.Reason}:{score.PointsAwarded}:{score.Total.Value}";
            }

            if (resolvedEvent is TurnChangedEvent turn)
            {
                return $"turn:{turn.PreviousSeat}:{turn.CurrentSeat}";
            }

            if (resolvedEvent is MatchCompletedEvent completed)
            {
                return $"completed:{completed.WinnerTeam}";
            }

            throw new ArgumentOutOfRangeException(nameof(resolvedEvent));
        }

        private sealed class ReplayResult
        {
            public ReplayResult(
                string initialState,
                IReadOnlyList<string> intents,
                MatchState state,
                IReadOnlyList<string> eventLog)
            {
                InitialState = initialState;
                Intents = intents;
                State = state;
                EventLog = eventLog;
            }

            public string InitialState { get; }

            public IReadOnlyList<string> Intents { get; }

            public MatchState State { get; }

            public IReadOnlyList<string> EventLog { get; }
        }
    }
}
