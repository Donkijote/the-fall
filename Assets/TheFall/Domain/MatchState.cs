using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public enum MatchPhase
    {
        Active,
        Completed,
    }

    public sealed class PlayerState
    {
        private readonly Card[] _hand;
        private readonly Card[] _capturedCards;

        public PlayerState(Player player, IEnumerable<Card> hand, IEnumerable<Card> capturedCards = null)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            _hand = CopyCards(hand, nameof(hand));
            _capturedCards = CopyCards(capturedCards ?? Array.Empty<Card>(), nameof(capturedCards));
            Hand = Array.AsReadOnly(_hand);
            CapturedCards = Array.AsReadOnly(_capturedCards);
        }

        public Player Player { get; }

        public IReadOnlyList<Card> Hand { get; }

        public IReadOnlyList<Card> CapturedCards { get; }

        internal PlayerState Play(Card card, IEnumerable<Card> newlyCaptured)
        {
            var hand = new List<Card>(_hand);
            if (!hand.Remove(card))
            {
                throw new InvalidOperationException($"{Player.Id} does not hold {card}.");
            }

            var captured = new List<Card>(_capturedCards);
            if (newlyCaptured != null)
            {
                captured.AddRange(newlyCaptured);
            }

            return new PlayerState(Player, hand, captured);
        }

        private static Card[] CopyCards(IEnumerable<Card> cards, string parameterName)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return new List<Card>(cards).ToArray();
        }
    }

    public sealed class PreviousPlay
    {
        public PreviousPlay(PlayerId playerId, Card card, bool wasCapture)
        {
            PlayerId = playerId;
            Card = card;
            WasCapture = wasCapture;
        }

        public PlayerId PlayerId { get; }

        public Card Card { get; }

        public bool WasCapture { get; }
    }

    public sealed class MatchState
    {
        private readonly PlayerState[] _players;
        private readonly Card[] _table;

        private MatchState(
            PlayerState[] players,
            Seat dealerSeat,
            Seat currentSeat,
            Card[] table,
            Deck deck,
            Score teamOneScore,
            Score teamTwoScore,
            RuleConfiguration rules,
            bool isFinalDeal,
            PreviousPlay previousPlay,
            MatchPhase phase,
            TeamId? winnerTeam)
        {
            ValidatePlayers(players);
            ValidateTable(table);

            _players = (PlayerState[])players.Clone();
            _table = (Card[])table.Clone();
            Players = Array.AsReadOnly(_players);
            Table = Array.AsReadOnly(_table);
            DealerSeat = dealerSeat;
            CurrentSeat = currentSeat;
            Deck = deck ?? throw new ArgumentNullException(nameof(deck));
            TeamOneScore = teamOneScore;
            TeamTwoScore = teamTwoScore;
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            IsFinalDeal = isFinalDeal;
            PreviousPlay = previousPlay;
            Phase = phase;
            WinnerTeam = winnerTeam;
        }

        public IReadOnlyList<PlayerState> Players { get; }

        public Seat DealerSeat { get; }

        public Seat CurrentSeat { get; }

        public IReadOnlyList<Card> Table { get; }

        public Deck Deck { get; }

        public Score TeamOneScore { get; }

        public Score TeamTwoScore { get; }

        public RuleConfiguration Rules { get; }

        public bool IsFinalDeal { get; }

        public PreviousPlay PreviousPlay { get; }

        public MatchPhase Phase { get; }

        public TeamId? WinnerTeam { get; }

        public static MatchState CreateOneVersusOne(
            PlayerState first,
            PlayerState second,
            Seat dealerSeat,
            Seat currentSeat,
            IEnumerable<Card> table,
            Deck deck,
            RuleConfiguration rules = null,
            bool isFinalDeal = false,
            Score teamOneScore = default,
            Score teamTwoScore = default,
            PreviousPlay previousPlay = null)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            return new MatchState(
                new[] { first, second },
                dealerSeat,
                currentSeat,
                new List<Card>(table).ToArray(),
                deck,
                teamOneScore,
                teamTwoScore,
                rules ?? RuleConfiguration.Standard,
                isFinalDeal,
                previousPlay,
                MatchPhase.Active,
                null);
        }

        public PlayerState GetPlayer(PlayerId playerId)
        {
            foreach (var player in _players)
            {
                if (player.Player.Id == playerId)
                {
                    return player;
                }
            }

            throw new ArgumentException($"Unknown player {playerId}.", nameof(playerId));
        }

        public PlayerState GetPlayerAt(Seat seat)
        {
            foreach (var player in _players)
            {
                if (player.Player.Seat == seat)
                {
                    return player;
                }
            }

            throw new InvalidOperationException($"No player occupies {seat}.");
        }

        public Score GetScore(TeamId teamId)
        {
            if (teamId == TeamId.One)
            {
                return TeamOneScore;
            }

            if (teamId == TeamId.Two)
            {
                return TeamTwoScore;
            }

            throw new ArgumentOutOfRangeException(nameof(teamId), "The 1v1 spike only scores teams one and two.");
        }

        internal MatchState With(
            PlayerState[] players,
            Seat currentSeat,
            Card[] table,
            Score teamOneScore,
            Score teamTwoScore,
            PreviousPlay previousPlay,
            MatchPhase phase,
            TeamId? winnerTeam)
        {
            return new MatchState(
                players,
                DealerSeat,
                currentSeat,
                table,
                Deck,
                teamOneScore,
                teamTwoScore,
                Rules,
                IsFinalDeal,
                previousPlay,
                phase,
                winnerTeam);
        }

        internal PlayerState[] CopyPlayers()
        {
            return (PlayerState[])_players.Clone();
        }

        private static void ValidatePlayers(PlayerState[] players)
        {
            if (players == null || players.Length != 2 || players[0] == null || players[1] == null)
            {
                throw new ArgumentException("The 1v1 spike requires exactly two players.", nameof(players));
            }

            if (players[0].Player.Seat == players[1].Player.Seat)
            {
                throw new ArgumentException("Players must occupy different seats.", nameof(players));
            }

            var hasFirstSeat = players[0].Player.Seat == Seat.First || players[1].Player.Seat == Seat.First;
            var hasSecondSeat = players[0].Player.Seat == Seat.Second || players[1].Player.Seat == Seat.Second;
            if (!hasFirstSeat || !hasSecondSeat)
            {
                throw new ArgumentException("The 1v1 spike requires the first and second seats.", nameof(players));
            }

            var hasTeamOne = players[0].Player.TeamId == TeamId.One || players[1].Player.TeamId == TeamId.One;
            var hasTeamTwo = players[0].Player.TeamId == TeamId.Two || players[1].Player.TeamId == TeamId.Two;
            if (!hasTeamOne || !hasTeamTwo)
            {
                throw new ArgumentException("The 1v1 spike requires teams one and two.", nameof(players));
            }
        }

        private static void ValidateTable(Card[] table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var ranks = new HashSet<CardRank>();
            foreach (var card in table)
            {
                if (!ranks.Add(card.Rank))
                {
                    throw new ArgumentException("The table cannot contain duplicate ranks.", nameof(table));
                }
            }
        }
    }
}
