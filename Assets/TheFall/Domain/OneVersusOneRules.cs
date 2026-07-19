using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public static class OneVersusOneRules
    {
        public static IReadOnlyList<PlayerIntent> GetLegalIntents(MatchState state, PlayerId playerId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Phase != MatchPhase.Active)
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

            var intents = new PlayerIntent[player.Hand.Count];
            for (var index = 0; index < player.Hand.Count; index++)
            {
                intents[index] = new PlayCardIntent(playerId, player.Hand[index]);
            }

            return Array.AsReadOnly(intents);
        }

        public static RuleResult Resolve(MatchState state, PlayerIntent intent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            if (state.Phase != MatchPhase.Active)
            {
                return RuleResult.Rejected(state, RuleError.MatchAlreadyCompleted);
            }

            if (!(intent is PlayCardIntent playCard))
            {
                return RuleResult.Rejected(state, RuleError.UnsupportedIntent);
            }

            PlayerState player;
            try
            {
                player = state.GetPlayer(playCard.PlayerId);
            }
            catch (ArgumentException)
            {
                return RuleResult.Rejected(state, RuleError.UnknownPlayer);
            }

            if (player.Player.Seat != state.CurrentSeat)
            {
                return RuleResult.Rejected(state, RuleError.NotPlayersTurn);
            }

            if (!Contains(player.Hand, playCard.Card))
            {
                return RuleResult.Rejected(state, RuleError.CardNotInHand);
            }

            return ResolvePlayCard(state, player, playCard.Card);
        }

        private static RuleResult ResolvePlayCard(MatchState state, PlayerState player, Card playedCard)
        {
            var events = new List<DomainEvent>
            {
                new CardPlayedEvent(player.Player.Id, playedCard),
            };
            var table = new List<Card>(state.Table);
            var sameRankIndex = FindRank(table, playedCard.Rank);
            var players = state.CopyPlayers();
            var playerIndex = FindPlayerIndex(players, player.Player.Id);

            if (sameRankIndex < 0)
            {
                table.Add(playedCard);
                players[playerIndex] = player.Play(playedCard, null);
                events.Add(new CardPlacedOnTableEvent(player.Player.Id, playedCard));
                return CompleteAcceptedTurn(
                    state,
                    players,
                    table.ToArray(),
                    state.TeamOneScore,
                    state.TeamTwoScore,
                    new PreviousPlay(player.Player.Id, playedCard, false),
                    events);
            }

            var captured = new List<Card> { playedCard, table[sameRankIndex] };
            table.RemoveAt(sameRankIndex);

            var cascadeRank = playedCard.Rank;
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

            players[playerIndex] = player.Play(playedCard, captured);
            events.Add(new CardsCapturedEvent(player.Player.Id, captured));

            var teamOneScore = state.TeamOneScore;
            var teamTwoScore = state.TeamTwoScore;
            var teamScore = state.GetScore(player.Player.TeamId);

            if (IsFall(state.PreviousPlay, player.Player.Id, playedCard))
            {
                var fallPoints = CardRankOrder.GetFallPoints(playedCard.Rank);
                teamScore = teamScore.Add(fallPoints);
                events.Add(new ScoreChangedEvent(player.Player.TeamId, fallPoints, teamScore, ScoreReason.Fall));
            }

            if (table.Count == 0 && !state.IsFinalDeal)
            {
                const int cleanTablePoints = 4;
                teamScore = teamScore.Add(cleanTablePoints);
                events.Add(new ScoreChangedEvent(
                    player.Player.TeamId,
                    cleanTablePoints,
                    teamScore,
                    ScoreReason.CleanTable));
            }

            SetScore(player.Player.TeamId, teamScore, ref teamOneScore, ref teamTwoScore);

            var opponentScore = player.Player.TeamId == TeamId.One ? teamTwoScore : teamOneScore;
            if (teamScore.Value >= state.Rules.VictoryTarget && teamScore.Value > opponentScore.Value)
            {
                var completed = state.With(
                    players,
                    state.CurrentSeat,
                    table.ToArray(),
                    teamOneScore,
                    teamTwoScore,
                    new PreviousPlay(player.Player.Id, playedCard, true),
                    MatchPhase.Completed,
                    player.Player.TeamId);
                events.Add(new MatchCompletedEvent(player.Player.TeamId));
                return RuleResult.Accepted(completed, events);
            }

            return CompleteAcceptedTurn(
                state,
                players,
                table.ToArray(),
                teamOneScore,
                teamTwoScore,
                new PreviousPlay(player.Player.Id, playedCard, true),
                events);
        }

        private static RuleResult CompleteAcceptedTurn(
            MatchState state,
            PlayerState[] players,
            Card[] table,
            Score teamOneScore,
            Score teamTwoScore,
            PreviousPlay previousPlay,
            List<DomainEvent> events)
        {
            var nextSeat = state.CurrentSeat == Seat.First ? Seat.Second : Seat.First;
            var next = state.With(
                players,
                nextSeat,
                table,
                teamOneScore,
                teamTwoScore,
                previousPlay,
                MatchPhase.Active,
                null);
            events.Add(new TurnChangedEvent(state.CurrentSeat, nextSeat));
            return RuleResult.Accepted(next, events);
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

        private static int FindPlayerIndex(IReadOnlyList<PlayerState> players, PlayerId playerId)
        {
            for (var index = 0; index < players.Count; index++)
            {
                if (players[index].Player.Id == playerId)
                {
                    return index;
                }
            }

            throw new InvalidOperationException($"Unknown player {playerId}.");
        }

        private static void SetScore(
            TeamId teamId,
            Score score,
            ref Score teamOneScore,
            ref Score teamTwoScore)
        {
            if (teamId == TeamId.One)
            {
                teamOneScore = score;
            }
            else
            {
                teamTwoScore = score;
            }
        }
    }
}
