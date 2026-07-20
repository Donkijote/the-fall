using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public enum MatchPhase
    {
        DealerSelection,
        AwaitingDealerChoice,
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

        internal PlayerState Deal(IEnumerable<Card> cards)
        {
            return new PlayerState(Player, cards, _capturedCards);
        }

        internal PlayerState Collect(IEnumerable<Card> cards)
        {
            var captured = new List<Card>(_capturedCards);
            captured.AddRange(cards);
            return new PlayerState(Player, _hand, captured);
        }

        internal PlayerState ResetRound()
        {
            return new PlayerState(Player, Array.Empty<Card>(), Array.Empty<Card>());
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

    public sealed class DealerCardSelection
    {
        public DealerCardSelection(PlayerId playerId, Card card)
        {
            PlayerId = playerId;
            Card = card;
        }

        public PlayerId PlayerId { get; }

        public Card Card { get; }
    }

    public sealed class MatchState
    {
        private readonly PlayerState[] _players;
        private readonly Card[] _table;
        private readonly Card[] _dealerSelectionCards;
        private readonly DealerCardSelection[] _currentDealerSelections;
        private readonly CantoAnnouncement[] _cantoAnnouncements;

        internal MatchState(
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
            TeamId? winnerTeam,
            int roundNumber,
            int dealNumber,
            bool isTieExtension,
            PlayerId? lastCapturer,
            Card[] dealerSelectionCards,
            DealerCardSelection[] currentDealerSelections,
            CantoAnnouncement[] cantoAnnouncements,
            bool? dealHandsBeforeTable,
            OpeningPattern? openingPattern)
        {
            ValidatePlayers(players);
            ValidateTable(table);

            if (roundNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(roundNumber));
            }

            if (dealNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dealNumber));
            }

            _players = (PlayerState[])players.Clone();
            _table = (Card[])table.Clone();
            _dealerSelectionCards = (Card[])dealerSelectionCards.Clone();
            _currentDealerSelections = (DealerCardSelection[])currentDealerSelections.Clone();
            _cantoAnnouncements = (CantoAnnouncement[])cantoAnnouncements.Clone();
            Players = Array.AsReadOnly(_players);
            Table = Array.AsReadOnly(_table);
            DealerSelectionCards = Array.AsReadOnly(_dealerSelectionCards);
            CurrentDealerSelections = Array.AsReadOnly(_currentDealerSelections);
            CantoAnnouncements = Array.AsReadOnly(_cantoAnnouncements);
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
            RoundNumber = roundNumber;
            DealNumber = dealNumber;
            IsTieExtension = isTieExtension;
            LastCapturer = lastCapturer;
            DealHandsBeforeTable = dealHandsBeforeTable;
            OpeningPattern = openingPattern;
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

        public int RoundNumber { get; }

        public int DealNumber { get; }

        public bool IsTieExtension { get; }

        public PlayerId? LastCapturer { get; }

        public IReadOnlyList<Card> DealerSelectionCards { get; }

        public IReadOnlyList<DealerCardSelection> CurrentDealerSelections { get; }

        public IReadOnlyList<CantoAnnouncement> CantoAnnouncements { get; }

        public bool? DealHandsBeforeTable { get; }

        public OpeningPattern? OpeningPattern { get; }

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
                null,
                1,
                1,
                false,
                null,
                Array.Empty<Card>(),
                Array.Empty<DealerCardSelection>(),
                Array.Empty<CantoAnnouncement>(),
                null,
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

            throw new ArgumentOutOfRangeException(nameof(teamId), "1v1 only scores teams one and two.");
        }

        internal PlayerState[] CopyPlayers()
        {
            return (PlayerState[])_players.Clone();
        }

        internal Card[] CopyDealerSelectionCards()
        {
            return (Card[])_dealerSelectionCards.Clone();
        }

        internal DealerCardSelection[] CopyCurrentDealerSelections()
        {
            return (DealerCardSelection[])_currentDealerSelections.Clone();
        }

        internal CantoAnnouncement[] CopyCantoAnnouncements()
        {
            return (CantoAnnouncement[])_cantoAnnouncements.Clone();
        }

        private static void ValidatePlayers(PlayerState[] players)
        {
            if (players == null || players.Length != 2 || players[0] == null || players[1] == null)
            {
                throw new ArgumentException("A 1v1 match requires exactly two players.", nameof(players));
            }

            if (players[0].Player.Seat == players[1].Player.Seat)
            {
                throw new ArgumentException("Players must occupy different seats.", nameof(players));
            }

            var hasFirstSeat = players[0].Player.Seat == Seat.First || players[1].Player.Seat == Seat.First;
            var hasSecondSeat = players[0].Player.Seat == Seat.Second || players[1].Player.Seat == Seat.Second;
            if (!hasFirstSeat || !hasSecondSeat)
            {
                throw new ArgumentException("A 1v1 match requires the first and second seats.", nameof(players));
            }

            var hasTeamOne = players[0].Player.TeamId == TeamId.One || players[1].Player.TeamId == TeamId.One;
            var hasTeamTwo = players[0].Player.TeamId == TeamId.Two || players[1].Player.TeamId == TeamId.Two;
            if (!hasTeamOne || !hasTeamTwo)
            {
                throw new ArgumentException("A 1v1 match requires teams one and two.", nameof(players));
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
