using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public static class OneVersusOneRules
    {
        private const int CapturedCardQuota = 20;

        public static RuleResult StartMatch(
            Player first,
            Player second,
            RuleConfiguration rules,
            IRandomSource randomSource)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            var shuffledDeck = Deck.CreateSpanishDeck().Shuffle(randomSource);
            var state = new MatchState(
                new[]
                {
                    new PlayerState(first, Array.Empty<Card>()),
                    new PlayerState(second, Array.Empty<Card>()),
                },
                Seat.First,
                Seat.First,
                Array.Empty<Card>(),
                shuffledDeck,
                default,
                default,
                rules ?? RuleConfiguration.Standard,
                false,
                null,
                MatchPhase.DealerSelection,
                null,
                0,
                0,
                false,
                null,
                Array.Empty<Card>(),
                Array.Empty<DealerCardSelection>(),
                Array.Empty<CantoAnnouncement>(),
                null,
                null);

            return RuleResult.Accepted(
                state,
                new DomainEvent[] { new MatchStartedEvent(shuffledDeck.Count) });
        }

        public static IReadOnlyList<PlayerIntent> GetLegalIntents(MatchState state, PlayerId playerId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Phase == MatchPhase.Completed)
            {
                return Array.Empty<PlayerIntent>();
            }

            PlayerState player;
            try
            {
                player = state.GetPlayer(playerId);
            }
            catch (ArgumentException)
            {
                return Array.Empty<PlayerIntent>();
            }

            if (player.Player.Seat != state.CurrentSeat)
            {
                return Array.Empty<PlayerIntent>();
            }

            if (state.Phase == MatchPhase.DealerSelection)
            {
                var selectionIntents = new PlayerIntent[state.Deck.Count];
                for (var index = 0; index < state.Deck.Count; index++)
                {
                    selectionIntents[index] = new SelectDealerCardIntent(playerId, state.Deck.Cards[index]);
                }

                return Array.AsReadOnly(selectionIntents);
            }

            if (state.Phase == MatchPhase.AwaitingDealerChoice)
            {
                if (player.Player.Seat != state.DealerSeat)
                {
                    return Array.Empty<PlayerIntent>();
                }

                return Array.AsReadOnly(new PlayerIntent[]
                {
                    new ChooseDealOptionsIntent(playerId, true, OpeningPattern.Ascending),
                    new ChooseDealOptionsIntent(playerId, true, OpeningPattern.Descending),
                    new ChooseDealOptionsIntent(playerId, false, OpeningPattern.Ascending),
                    new ChooseDealOptionsIntent(playerId, false, OpeningPattern.Descending),
                });
            }

            if (state.Phase != MatchPhase.Active)
            {
                return Array.Empty<PlayerIntent>();
            }

            var intents = new List<PlayerIntent>();
            if (player.Hand.Count == 3 && !HasCantoAnnouncement(state.CantoAnnouncements, playerId))
            {
                foreach (CantoKind cantoKind in Enum.GetValues(typeof(CantoKind)))
                {
                    intents.Add(new AnnounceCantoIntent(playerId, cantoKind));
                }
            }

            for (var index = 0; index < player.Hand.Count; index++)
            {
                intents.Add(new PlayCardIntent(playerId, player.Hand[index]));
            }

            return Array.AsReadOnly(intents.ToArray());
        }

        public static RuleResult Resolve(MatchState state, PlayerIntent intent)
        {
            return Resolve(state, intent, null);
        }

        public static RuleResult Resolve(MatchState state, PlayerIntent intent, IRandomSource randomSource)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            if (state.Phase == MatchPhase.Completed)
            {
                return RuleResult.Rejected(state, RuleError.MatchAlreadyCompleted);
            }

            PlayerState player;
            try
            {
                player = state.GetPlayer(intent.PlayerId);
            }
            catch (ArgumentException)
            {
                return RuleResult.Rejected(state, RuleError.UnknownPlayer);
            }

            if (intent is SelectDealerCardIntent selectDealerCard)
            {
                return ResolveDealerSelection(state, player, selectDealerCard, randomSource);
            }

            if (intent is ChooseDealOptionsIntent chooseDealOptions)
            {
                return ResolveDealerChoice(state, player, chooseDealOptions, randomSource);
            }

            if (intent is AnnounceCantoIntent announceCanto)
            {
                return ResolveCantoAnnouncement(state, player, announceCanto);
            }

            if (intent is PlayCardIntent playCard)
            {
                return ResolvePlayCard(state, player, playCard, randomSource);
            }

            return RuleResult.Rejected(state, RuleError.UnsupportedIntent);
        }

        private static RuleResult ResolveDealerSelection(
            MatchState state,
            PlayerState player,
            SelectDealerCardIntent intent,
            IRandomSource randomSource)
        {
            if (state.Phase != MatchPhase.DealerSelection)
            {
                return RuleResult.Rejected(state, RuleError.WrongPhase);
            }

            if (player.Player.Seat != state.CurrentSeat)
            {
                return RuleResult.Rejected(state, RuleError.NotPlayersTurn);
            }

            if (!Contains(state.Deck.Cards, intent.Card))
            {
                return RuleResult.Rejected(state, RuleError.CardNotInDealerSpread);
            }

            if (HasDealerSelection(state.CurrentDealerSelections, player.Player.Id))
            {
                return RuleResult.Rejected(state, RuleError.PlayerAlreadySelectedDealerCard);
            }

            var selections = new List<DealerCardSelection>(state.CurrentDealerSelections)
            {
                new DealerCardSelection(player.Player.Id, intent.Card),
            };
            if (selections.Count == 2
                && selections[0].Card.Rank != selections[1].Card.Rank
                && randomSource == null)
            {
                return RuleResult.Rejected(state, RuleError.RandomSourceRequired);
            }

            var builder = new StateBuilder(state);
            builder.Deck.Remove(intent.Card);
            builder.DealerSelectionCards.Add(intent.Card);
            builder.CurrentDealerSelections.Add(new DealerCardSelection(player.Player.Id, intent.Card));
            var events = new List<DomainEvent>
            {
                new DealerCardSelectedEvent(player.Player.Id, intent.Card),
            };

            if (builder.CurrentDealerSelections.Count == 1)
            {
                builder.CurrentSeat = OtherSeat(state.CurrentSeat);
                events.Add(new TurnChangedEvent(state.CurrentSeat, builder.CurrentSeat));
                return RuleResult.Accepted(builder.Build(), events);
            }

            var firstSelection = builder.CurrentDealerSelections[0];
            var secondSelection = builder.CurrentDealerSelections[1];
            if (firstSelection.Card.Rank == secondSelection.Card.Rank)
            {
                builder.CurrentDealerSelections.Clear();
                builder.CurrentSeat = Seat.First;
                events.Add(new DealerSelectionTiedEvent(firstSelection.Card.Rank));
                if (state.CurrentSeat != builder.CurrentSeat)
                {
                    events.Add(new TurnChangedEvent(state.CurrentSeat, builder.CurrentSeat));
                }

                return RuleResult.Accepted(builder.Build(), events);
            }

            var winningSelection = CardRankOrder.GetIndex(firstSelection.Card.Rank)
                > CardRankOrder.GetIndex(secondSelection.Card.Rank)
                ? firstSelection
                : secondSelection;
            var dealer = builder.GetPlayer(winningSelection.PlayerId);
            builder.Deck.AddRange(builder.DealerSelectionCards);
            builder.Deck = new List<Card>(new Deck(builder.Deck).Shuffle(randomSource).Cards);
            builder.DealerSelectionCards.Clear();
            builder.CurrentDealerSelections.Clear();
            builder.DealerSeat = dealer.Player.Seat;
            builder.CurrentSeat = dealer.Player.Seat;
            builder.Phase = MatchPhase.AwaitingDealerChoice;
            builder.RoundNumber = 1;
            builder.DealNumber = 0;
            events.Add(new DealerSelectedEvent(dealer.Player.Id, dealer.Player.Seat));
            events.Add(new DeckShuffledEvent(builder.RoundNumber, builder.Deck.Count));
            return RuleResult.Accepted(builder.Build(), events);
        }

        private static RuleResult ResolveDealerChoice(
            MatchState state,
            PlayerState player,
            ChooseDealOptionsIntent intent,
            IRandomSource randomSource)
        {
            if (state.Phase != MatchPhase.AwaitingDealerChoice)
            {
                return RuleResult.Rejected(state, RuleError.WrongPhase);
            }

            if (player.Player.Seat != state.DealerSeat)
            {
                return RuleResult.Rejected(state, RuleError.NotDealer);
            }

            if (randomSource == null)
            {
                return RuleResult.Rejected(state, RuleError.RandomSourceRequired);
            }

            var builder = new StateBuilder(state)
            {
                Phase = MatchPhase.Active,
                CurrentSeat = OtherSeat(state.DealerSeat),
                DealNumber = 1,
                IsFinalDeal = false,
                PreviousPlay = null,
                LastCapturer = null,
                DealHandsBeforeTable = intent.DealHandsBeforeTable,
                OpeningPattern = intent.OpeningPattern,
            };
            builder.Table.Clear();
            builder.CantoAnnouncements.Clear();
            var events = new List<DomainEvent>
            {
                new DealerChoiceMadeEvent(player.Player.Id, intent.DealHandsBeforeTable, intent.OpeningPattern),
                new DealStartedEvent(builder.RoundNumber, builder.DealNumber, false),
            };

            if (intent.DealHandsBeforeTable)
            {
                DealHands(builder, events);
                DealOpeningTable(builder, events, randomSource);
            }
            else
            {
                DealOpeningTable(builder, events, randomSource);
                if (builder.Phase != MatchPhase.Completed)
                {
                    DealHands(builder, events);
                }
            }

            if (builder.Phase != MatchPhase.Completed)
            {
                events.Add(new TurnChangedEvent(state.CurrentSeat, builder.CurrentSeat));
            }

            return RuleResult.Accepted(builder.Build(), events);
        }

        private static RuleResult ResolveCantoAnnouncement(
            MatchState state,
            PlayerState player,
            AnnounceCantoIntent intent)
        {
            if (state.Phase != MatchPhase.Active)
            {
                return RuleResult.Rejected(state, RuleError.WrongPhase);
            }

            if (player.Player.Seat != state.CurrentSeat)
            {
                return RuleResult.Rejected(state, RuleError.NotPlayersTurn);
            }

            if (player.Hand.Count != 3)
            {
                return RuleResult.Rejected(state, RuleError.CantoOpportunityClosed);
            }

            if (HasCantoAnnouncement(state.CantoAnnouncements, player.Player.Id))
            {
                return RuleResult.Rejected(state, RuleError.CantoAlreadyAnnounced);
            }

            var builder = new StateBuilder(state);
            var announcement = new CantoAnnouncement(player.Player.Id, intent.ClaimedKind, player.Hand);
            var wasOnlyAnnouncement = builder.CantoAnnouncements.Count == 0;
            builder.CantoAnnouncements.Add(announcement);
            var events = new List<DomainEvent>
            {
                new CantoAnnouncedEvent(player.Player.Id, intent.ClaimedKind),
            };
            var classification = CantoRules.Classify(player.Hand, state.Rules);
            var isValid = classification != null && classification.Kind == intent.ClaimedKind;
            if (isValid && classification.WinsImmediately)
            {
                events.Add(new CantoResolvedEvent(player.Player.Id, intent.ClaimedKind, true, true));
                CompleteMatch(builder, player.Player.TeamId, events);
            }
            else if (isValid
                && wasOnlyAnnouncement
                && builder.GetScore(player.Player.TeamId).Value + classification.Points >= state.Rules.VictoryTarget)
            {
                AwardScore(builder, player.Player.TeamId, classification.Points, ScoreReason.Canto, events);
                events.Add(new CantoResolvedEvent(player.Player.Id, intent.ClaimedKind, true, true));
                CompleteMatch(builder, player.Player.TeamId, events);
            }

            return RuleResult.Accepted(builder.Build(), events);
        }

        private static RuleResult ResolvePlayCard(
            MatchState state,
            PlayerState player,
            PlayCardIntent intent,
            IRandomSource randomSource)
        {
            if (state.Phase != MatchPhase.Active)
            {
                return RuleResult.Rejected(state, RuleError.WrongPhase);
            }

            if (player.Player.Seat != state.CurrentSeat)
            {
                return RuleResult.Rejected(state, RuleError.NotPlayersTurn);
            }

            if (!Contains(player.Hand, intent.Card))
            {
                return RuleResult.Rejected(state, RuleError.CardNotInHand);
            }

            if (state.Deck.Count == 0 && CountCardsInHands(state.Players) == 1 && randomSource == null)
            {
                return RuleResult.Rejected(state, RuleError.RandomSourceRequired);
            }

            var builder = new StateBuilder(state);
            var events = new List<DomainEvent>
            {
                new CardPlayedEvent(player.Player.Id, intent.Card),
            };
            var table = builder.Table;
            var sameRankIndex = FindRank(table, intent.Card.Rank);
            if (sameRankIndex < 0)
            {
                table.Add(intent.Card);
                builder.SetPlayer(player.Play(intent.Card, null));
                builder.PreviousPlay = new PreviousPlay(player.Player.Id, intent.Card, false);
                events.Add(new CardPlacedOnTableEvent(player.Player.Id, intent.Card));
            }
            else
            {
                var captured = new List<Card> { intent.Card, table[sameRankIndex] };
                table.RemoveAt(sameRankIndex);
                var cascadeRank = intent.Card.Rank;
                while (CardRankOrder.TryGetNext(cascadeRank, out var nextRank))
                {
                    var cascadeIndex = FindRank(table, nextRank);
                    if (cascadeIndex < 0)
                    {
                        break;
                    }

                    captured.Add(table[cascadeIndex]);
                    table.RemoveAt(cascadeIndex);
                    cascadeRank = nextRank;
                }

                builder.SetPlayer(player.Play(intent.Card, captured));
                builder.LastCapturer = player.Player.Id;
                builder.PreviousPlay = new PreviousPlay(player.Player.Id, intent.Card, true);
                events.Add(new CardsCapturedEvent(player.Player.Id, captured));

                if (IsFall(state.PreviousPlay, player.Player.Id, intent.Card))
                {
                    AwardScore(
                        builder,
                        player.Player.TeamId,
                        CardRankOrder.GetFallPoints(intent.Card.Rank),
                        ScoreReason.Fall,
                        events);
                }

                if (table.Count == 0 && !state.IsFinalDeal)
                {
                    AwardScore(builder, player.Player.TeamId, 4, ScoreReason.CleanTable, events);
                }

                if (TryGetUniqueWinner(builder, out var immediateWinner))
                {
                    CompleteMatch(builder, immediateWinner, events);
                    return RuleResult.Accepted(builder.Build(), events);
                }
            }

            if (CountCardsInHands(builder.Players) > 0)
            {
                var previousSeat = builder.CurrentSeat;
                builder.CurrentSeat = OtherSeat(builder.CurrentSeat);
                events.Add(new TurnChangedEvent(previousSeat, builder.CurrentSeat));
                return RuleResult.Accepted(builder.Build(), events);
            }

            events.Add(new DealCompletedEvent(builder.RoundNumber, builder.DealNumber));
            ResolveCantos(builder, events);
            if (builder.Phase == MatchPhase.Completed)
            {
                return RuleResult.Accepted(builder.Build(), events);
            }

            if (builder.Deck.Count > 0)
            {
                BeginNextDeal(builder, events);
            }
            else
            {
                CompleteRound(builder, events, randomSource);
            }

            return RuleResult.Accepted(builder.Build(), events);
        }

        private static void DealOpeningTable(
            StateBuilder builder,
            List<DomainEvent> events,
            IRandomSource randomSource)
        {
            for (var position = 0; position < 4; position++)
            {
                Card acceptedCard;
                while (true)
                {
                    var drawnCard = DrawTop(builder.Deck);
                    if (FindRank(builder.Table, drawnCard.Rank) < 0)
                    {
                        acceptedCard = drawnCard;
                        break;
                    }

                    var reinsertionIndex = randomSource.NextInt(builder.Deck.Count + 1);
                    builder.Deck.Insert(reinsertionIndex, drawnCard);
                    events.Add(new OpeningCardRejectedEvent(drawnCard, position, reinsertionIndex));
                }

                builder.Table.Add(acceptedCard);
                events.Add(new OpeningCardPlacedEvent(acceptedCard, position));
                var expectedRank = GetExpectedOpeningRank(builder.OpeningPattern.Value, position);
                if (acceptedCard.Rank != expectedRank)
                {
                    continue;
                }

                var dealer = builder.GetPlayerAt(builder.DealerSeat);
                AwardScore(builder, dealer.Player.TeamId, (int)expectedRank, ScoreReason.OpeningPattern, events);
                if (TryGetUniqueWinner(builder, out var winner))
                {
                    CompleteMatch(builder, winner, events);
                    return;
                }
            }
        }

        private static void DealHands(StateBuilder builder, List<DomainEvent> events)
        {
            var firstSeat = OtherSeat(builder.DealerSeat);
            var secondSeat = builder.DealerSeat;
            var firstHand = new List<Card>();
            var secondHand = new List<Card>();
            for (var handPosition = 0; handPosition < 3; handPosition++)
            {
                var firstCard = DrawTop(builder.Deck);
                firstHand.Add(firstCard);
                events.Add(new CardDealtEvent(builder.GetPlayerAt(firstSeat).Player.Id, firstCard, handPosition));

                var secondCard = DrawTop(builder.Deck);
                secondHand.Add(secondCard);
                events.Add(new CardDealtEvent(builder.GetPlayerAt(secondSeat).Player.Id, secondCard, handPosition));
            }

            builder.SetPlayer(builder.GetPlayerAt(firstSeat).Deal(firstHand));
            builder.SetPlayer(builder.GetPlayerAt(secondSeat).Deal(secondHand));
            builder.IsFinalDeal = builder.Deck.Count == 0;
        }

        private static void BeginNextDeal(StateBuilder builder, List<DomainEvent> events)
        {
            builder.DealNumber++;
            builder.CantoAnnouncements.Clear();
            var previousSeat = builder.CurrentSeat;
            builder.CurrentSeat = OtherSeat(builder.DealerSeat);
            var isFinalDeal = builder.Deck.Count == 6;
            events.Add(new DealStartedEvent(builder.RoundNumber, builder.DealNumber, isFinalDeal));
            DealHands(builder, events);
            events.Add(new TurnChangedEvent(previousSeat, builder.CurrentSeat));
        }

        private static void ResolveCantos(StateBuilder builder, List<DomainEvent> events)
        {
            var valid = new List<ResolvedCanto>();
            foreach (var announcement in builder.CantoAnnouncements)
            {
                var classification = CantoRules.Classify(announcement.Hand, builder.Rules);
                if (classification == null || classification.Kind != announcement.ClaimedKind)
                {
                    events.Add(new CantoResolvedEvent(
                        announcement.PlayerId,
                        announcement.ClaimedKind,
                        false,
                        false));
                    var player = builder.GetPlayer(announcement.PlayerId);
                    var before = builder.GetScore(player.Player.TeamId);
                    var after = before.SubtractClamped(1);
                    builder.SetScore(player.Player.TeamId, after);
                    events.Add(new ScoreChangedEvent(
                        player.Player.TeamId,
                        after.Value - before.Value,
                        after,
                        ScoreReason.FalseCantoPenalty));
                    continue;
                }

                valid.Add(new ResolvedCanto(announcement, classification));
            }

            ResolvedCanto winner = null;
            foreach (var candidate in valid)
            {
                if (winner == null)
                {
                    winner = candidate;
                    continue;
                }

                var candidateSeat = builder.GetPlayer(candidate.Announcement.PlayerId).Player.Seat;
                var winnerSeat = builder.GetPlayer(winner.Announcement.PlayerId).Player.Seat;
                if (CantoRules.Compare(
                        candidate.Classification,
                        candidateSeat,
                        winner.Classification,
                        winnerSeat,
                        builder.DealerSeat) > 0)
                {
                    winner = candidate;
                }
            }

            foreach (var resolved in valid)
            {
                var didScore = ReferenceEquals(resolved, winner);
                events.Add(new CantoResolvedEvent(
                    resolved.Announcement.PlayerId,
                    resolved.Announcement.ClaimedKind,
                    true,
                    didScore));
            }

            if (winner != null)
            {
                var player = builder.GetPlayer(winner.Announcement.PlayerId);
                if (winner.Classification.WinsImmediately)
                {
                    CompleteMatch(builder, player.Player.TeamId, events);
                }
                else
                {
                    AwardScore(
                        builder,
                        player.Player.TeamId,
                        winner.Classification.Points,
                        ScoreReason.Canto,
                        events);
                }
            }

            builder.CantoAnnouncements.Clear();
            if (builder.Phase != MatchPhase.Completed && TryGetUniqueWinner(builder, out var scoreWinner))
            {
                CompleteMatch(builder, scoreWinner, events);
            }
        }

        private static void CompleteRound(
            StateBuilder builder,
            List<DomainEvent> events,
            IRandomSource randomSource)
        {
            if (builder.Table.Count > 0)
            {
                var collector = builder.LastCapturer.HasValue
                    ? builder.GetPlayer(builder.LastCapturer.Value)
                    : builder.GetPlayerAt(OtherSeat(builder.DealerSeat));
                var leftovers = builder.Table.ToArray();
                builder.SetPlayer(collector.Collect(leftovers));
                builder.Table.Clear();
                events.Add(new LeftoversCollectedEvent(collector.Player.Id, leftovers));
            }

            var countingOrder = new[] { OtherSeat(builder.DealerSeat), builder.DealerSeat };
            foreach (var seat in countingOrder)
            {
                var player = builder.GetPlayerAt(seat);
                var excess = Math.Max(0, player.CapturedCards.Count - CapturedCardQuota);
                if (excess > 0)
                {
                    AwardScore(builder, player.Player.TeamId, excess, ScoreReason.CapturedCards, events);
                }
            }

            events.Add(new RoundCompletedEvent(builder.RoundNumber));
            if (TryGetUniqueWinner(builder, out var winner))
            {
                CompleteMatch(builder, winner, events);
                return;
            }

            var tieExtension = builder.TeamOneScore.Value >= builder.Rules.VictoryTarget
                && builder.TeamOneScore.Value == builder.TeamTwoScore.Value;
            var previousDealer = builder.DealerSeat;
            var nextDealer = OtherSeat(previousDealer);
            builder.DealerSeat = nextDealer;
            builder.CurrentSeat = nextDealer;
            builder.RoundNumber++;
            builder.DealNumber = 0;
            builder.IsTieExtension = tieExtension || builder.IsTieExtension;
            builder.IsFinalDeal = false;
            builder.PreviousPlay = null;
            builder.LastCapturer = null;
            builder.Phase = MatchPhase.AwaitingDealerChoice;
            builder.WinnerTeam = null;
            builder.Table.Clear();
            builder.CantoAnnouncements.Clear();
            builder.DealHandsBeforeTable = null;
            builder.OpeningPattern = null;
            for (var index = 0; index < builder.Players.Count; index++)
            {
                builder.Players[index] = builder.Players[index].ResetRound();
            }

            builder.Deck = new List<Card>(Deck.CreateSpanishDeck().Shuffle(randomSource).Cards);
            if (tieExtension)
            {
                events.Add(new TieExtensionStartedEvent(builder.RoundNumber, builder.TeamOneScore));
            }

            events.Add(new DealerRotatedEvent(previousDealer, nextDealer));
            events.Add(new DeckShuffledEvent(builder.RoundNumber, builder.Deck.Count));
        }

        private static void AwardScore(
            StateBuilder builder,
            TeamId teamId,
            int points,
            ScoreReason reason,
            List<DomainEvent> events)
        {
            var score = builder.GetScore(teamId).Add(points);
            builder.SetScore(teamId, score);
            events.Add(new ScoreChangedEvent(teamId, points, score, reason));
        }

        private static void CompleteMatch(StateBuilder builder, TeamId winnerTeam, List<DomainEvent> events)
        {
            builder.Phase = MatchPhase.Completed;
            builder.WinnerTeam = winnerTeam;
            events.Add(new MatchCompletedEvent(winnerTeam));
        }

        private static bool TryGetUniqueWinner(StateBuilder builder, out TeamId winnerTeam)
        {
            if (builder.TeamOneScore.Value >= builder.Rules.VictoryTarget
                && builder.TeamOneScore.Value > builder.TeamTwoScore.Value)
            {
                winnerTeam = TeamId.One;
                return true;
            }

            if (builder.TeamTwoScore.Value >= builder.Rules.VictoryTarget
                && builder.TeamTwoScore.Value > builder.TeamOneScore.Value)
            {
                winnerTeam = TeamId.Two;
                return true;
            }

            winnerTeam = default;
            return false;
        }

        private static CardRank GetExpectedOpeningRank(OpeningPattern pattern, int position)
        {
            var ascending = new[] { CardRank.One, CardRank.Two, CardRank.Three, CardRank.Four };
            return pattern == OpeningPattern.Ascending ? ascending[position] : ascending[3 - position];
        }

        private static Card DrawTop(List<Card> deck)
        {
            if (deck.Count == 0)
            {
                throw new InvalidOperationException("The deck does not contain enough cards for the requested deal.");
            }

            var card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        private static bool IsFall(PreviousPlay previousPlay, PlayerId currentPlayerId, Card playedCard)
        {
            return previousPlay != null
                && !previousPlay.WasCapture
                && previousPlay.PlayerId != currentPlayerId
                && previousPlay.Card.Rank == playedCard.Rank;
        }

        private static bool Contains(IReadOnlyList<Card> cards, Card expected)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                if (cards[index] == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private static int FindRank(IReadOnlyList<Card> cards, CardRank rank)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                if (cards[index].Rank == rank)
                {
                    return index;
                }
            }

            return -1;
        }

        private static int CountCardsInHands(IReadOnlyList<PlayerState> players)
        {
            var total = 0;
            for (var index = 0; index < players.Count; index++)
            {
                total += players[index].Hand.Count;
            }

            return total;
        }

        private static bool HasDealerSelection(
            IReadOnlyList<DealerCardSelection> selections,
            PlayerId playerId)
        {
            for (var index = 0; index < selections.Count; index++)
            {
                if (selections[index].PlayerId == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCantoAnnouncement(
            IReadOnlyList<CantoAnnouncement> announcements,
            PlayerId playerId)
        {
            for (var index = 0; index < announcements.Count; index++)
            {
                if (announcements[index].PlayerId == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        private static Seat OtherSeat(Seat seat)
        {
            if (seat == Seat.First)
            {
                return Seat.Second;
            }

            if (seat == Seat.Second)
            {
                return Seat.First;
            }

            throw new ArgumentOutOfRangeException(nameof(seat), "1v1 only supports the first and second seats.");
        }

        private sealed class ResolvedCanto
        {
            public ResolvedCanto(CantoAnnouncement announcement, CantoClassification classification)
            {
                Announcement = announcement;
                Classification = classification;
            }

            public CantoAnnouncement Announcement { get; }

            public CantoClassification Classification { get; }
        }

        private sealed class StateBuilder
        {
            public StateBuilder(MatchState state)
            {
                Players = new List<PlayerState>(state.Players);
                DealerSeat = state.DealerSeat;
                CurrentSeat = state.CurrentSeat;
                Table = new List<Card>(state.Table);
                Deck = new List<Card>(state.Deck.Cards);
                TeamOneScore = state.TeamOneScore;
                TeamTwoScore = state.TeamTwoScore;
                Rules = state.Rules;
                IsFinalDeal = state.IsFinalDeal;
                PreviousPlay = state.PreviousPlay;
                Phase = state.Phase;
                WinnerTeam = state.WinnerTeam;
                RoundNumber = state.RoundNumber;
                DealNumber = state.DealNumber;
                IsTieExtension = state.IsTieExtension;
                LastCapturer = state.LastCapturer;
                DealerSelectionCards = new List<Card>(state.DealerSelectionCards);
                CurrentDealerSelections = new List<DealerCardSelection>(state.CurrentDealerSelections);
                CantoAnnouncements = new List<CantoAnnouncement>(state.CantoAnnouncements);
                DealHandsBeforeTable = state.DealHandsBeforeTable;
                OpeningPattern = state.OpeningPattern;
            }

            public List<PlayerState> Players { get; }
            public Seat DealerSeat { get; set; }
            public Seat CurrentSeat { get; set; }
            public List<Card> Table { get; }
            public List<Card> Deck { get; set; }
            public Score TeamOneScore { get; set; }
            public Score TeamTwoScore { get; set; }
            public RuleConfiguration Rules { get; }
            public bool IsFinalDeal { get; set; }
            public PreviousPlay PreviousPlay { get; set; }
            public MatchPhase Phase { get; set; }
            public TeamId? WinnerTeam { get; set; }
            public int RoundNumber { get; set; }
            public int DealNumber { get; set; }
            public bool IsTieExtension { get; set; }
            public PlayerId? LastCapturer { get; set; }
            public List<Card> DealerSelectionCards { get; }
            public List<DealerCardSelection> CurrentDealerSelections { get; }
            public List<CantoAnnouncement> CantoAnnouncements { get; }
            public bool? DealHandsBeforeTable { get; set; }
            public OpeningPattern? OpeningPattern { get; set; }

            public PlayerState GetPlayer(PlayerId playerId)
            {
                for (var index = 0; index < Players.Count; index++)
                {
                    if (Players[index].Player.Id == playerId)
                    {
                        return Players[index];
                    }
                }

                throw new InvalidOperationException($"Unknown player {playerId}.");
            }

            public PlayerState GetPlayerAt(Seat seat)
            {
                for (var index = 0; index < Players.Count; index++)
                {
                    if (Players[index].Player.Seat == seat)
                    {
                        return Players[index];
                    }
                }

                throw new InvalidOperationException($"No player occupies {seat}.");
            }

            public void SetPlayer(PlayerState player)
            {
                for (var index = 0; index < Players.Count; index++)
                {
                    if (Players[index].Player.Id == player.Player.Id)
                    {
                        Players[index] = player;
                        return;
                    }
                }

                throw new InvalidOperationException($"Unknown player {player.Player.Id}.");
            }

            public Score GetScore(TeamId teamId)
            {
                return teamId == TeamId.One ? TeamOneScore : TeamTwoScore;
            }

            public void SetScore(TeamId teamId, Score score)
            {
                if (teamId == TeamId.One)
                {
                    TeamOneScore = score;
                }
                else
                {
                    TeamTwoScore = score;
                }
            }

            public MatchState Build()
            {
                return new MatchState(
                    Players.ToArray(),
                    DealerSeat,
                    CurrentSeat,
                    Table.ToArray(),
                    new Deck(Deck),
                    TeamOneScore,
                    TeamTwoScore,
                    Rules,
                    IsFinalDeal,
                    PreviousPlay,
                    Phase,
                    WinnerTeam,
                    RoundNumber,
                    DealNumber,
                    IsTieExtension,
                    LastCapturer,
                    DealerSelectionCards.ToArray(),
                    CurrentDealerSelections.ToArray(),
                    CantoAnnouncements.ToArray(),
                    DealHandsBeforeTable,
                    OpeningPattern);
            }
        }
    }
}
