using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public enum DomainEventKind
    {
        MatchStarted,
        DealerCardSelected,
        DealerSelectionTied,
        DealerSelected,
        DeckShuffled,
        DealerChoiceMade,
        DealStarted,
        CardDealt,
        OpeningCardRejected,
        OpeningCardPlaced,
        CardPlayed,
        CardPlacedOnTable,
        CardsCaptured,
        CantoAnnounced,
        CantoResolved,
        ScoreChanged,
        DealCompleted,
        LeftoversCollected,
        RoundCompleted,
        DealerRotated,
        TieExtensionStarted,
        TurnChanged,
        MatchCompleted,
    }

    public abstract class DomainEvent
    {
        protected DomainEvent(DomainEventKind kind)
        {
            Kind = kind;
        }

        public DomainEventKind Kind { get; }
    }

    public sealed class MatchStartedEvent : DomainEvent
    {
        public MatchStartedEvent(int dealerSpreadCardCount)
            : base(DomainEventKind.MatchStarted)
        {
            DealerSpreadCardCount = dealerSpreadCardCount;
        }

        public int DealerSpreadCardCount { get; }
    }

    public sealed class DealerCardSelectedEvent : DomainEvent
    {
        public DealerCardSelectedEvent(PlayerId playerId, Card card)
            : base(DomainEventKind.DealerCardSelected)
        {
            PlayerId = playerId;
            Card = card;
        }

        public PlayerId PlayerId { get; }

        public Card Card { get; }
    }

    public sealed class DealerSelectionTiedEvent : DomainEvent
    {
        public DealerSelectionTiedEvent(CardRank rank)
            : base(DomainEventKind.DealerSelectionTied)
        {
            Rank = rank;
        }

        public CardRank Rank { get; }
    }

    public sealed class DealerSelectedEvent : DomainEvent
    {
        public DealerSelectedEvent(PlayerId playerId, Seat dealerSeat)
            : base(DomainEventKind.DealerSelected)
        {
            PlayerId = playerId;
            DealerSeat = dealerSeat;
        }

        public PlayerId PlayerId { get; }

        public Seat DealerSeat { get; }
    }

    public sealed class DeckShuffledEvent : DomainEvent
    {
        public DeckShuffledEvent(int roundNumber, int cardCount)
            : base(DomainEventKind.DeckShuffled)
        {
            RoundNumber = roundNumber;
            CardCount = cardCount;
        }

        public int RoundNumber { get; }

        public int CardCount { get; }
    }

    public sealed class DealerChoiceMadeEvent : DomainEvent
    {
        public DealerChoiceMadeEvent(PlayerId dealerId, bool dealHandsBeforeTable, OpeningPattern openingPattern)
            : base(DomainEventKind.DealerChoiceMade)
        {
            DealerId = dealerId;
            DealHandsBeforeTable = dealHandsBeforeTable;
            OpeningPattern = openingPattern;
        }

        public PlayerId DealerId { get; }

        public bool DealHandsBeforeTable { get; }

        public OpeningPattern OpeningPattern { get; }
    }

    public sealed class DealStartedEvent : DomainEvent
    {
        public DealStartedEvent(int roundNumber, int dealNumber, bool isFinalDeal)
            : base(DomainEventKind.DealStarted)
        {
            RoundNumber = roundNumber;
            DealNumber = dealNumber;
            IsFinalDeal = isFinalDeal;
        }

        public int RoundNumber { get; }

        public int DealNumber { get; }

        public bool IsFinalDeal { get; }
    }

    public sealed class CardDealtEvent : DomainEvent
    {
        public CardDealtEvent(PlayerId playerId, Card card, int handPosition)
            : base(DomainEventKind.CardDealt)
        {
            PlayerId = playerId;
            Card = card;
            HandPosition = handPosition;
        }

        public PlayerId PlayerId { get; }

        public Card Card { get; }

        public int HandPosition { get; }
    }

    public sealed class OpeningCardRejectedEvent : DomainEvent
    {
        public OpeningCardRejectedEvent(Card card, int tablePosition, int reinsertedDeckIndex)
            : base(DomainEventKind.OpeningCardRejected)
        {
            Card = card;
            TablePosition = tablePosition;
            ReinsertedDeckIndex = reinsertedDeckIndex;
        }

        public Card Card { get; }

        public int TablePosition { get; }

        public int ReinsertedDeckIndex { get; }
    }

    public sealed class OpeningCardPlacedEvent : DomainEvent
    {
        public OpeningCardPlacedEvent(Card card, int tablePosition)
            : base(DomainEventKind.OpeningCardPlaced)
        {
            Card = card;
            TablePosition = tablePosition;
        }

        public Card Card { get; }

        public int TablePosition { get; }
    }

    public sealed class CardPlayedEvent : DomainEvent
    {
        public CardPlayedEvent(PlayerId playerId, Card card)
            : base(DomainEventKind.CardPlayed)
        {
            PlayerId = playerId;
            Card = card;
        }

        public PlayerId PlayerId { get; }

        public Card Card { get; }
    }

    public sealed class CardPlacedOnTableEvent : DomainEvent
    {
        public CardPlacedOnTableEvent(PlayerId playerId, Card card)
            : base(DomainEventKind.CardPlacedOnTable)
        {
            PlayerId = playerId;
            Card = card;
        }

        public PlayerId PlayerId { get; }

        public Card Card { get; }
    }

    public sealed class CardsCapturedEvent : DomainEvent
    {
        private readonly IReadOnlyList<Card> _cards;

        public CardsCapturedEvent(PlayerId playerId, IEnumerable<Card> cards)
            : base(DomainEventKind.CardsCaptured)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            PlayerId = playerId;
            _cards = Array.AsReadOnly(new List<Card>(cards).ToArray());
        }

        public PlayerId PlayerId { get; }

        public IReadOnlyList<Card> Cards => _cards;
    }

    public sealed class CantoAnnouncedEvent : DomainEvent
    {
        public CantoAnnouncedEvent(PlayerId playerId, CantoKind claimedKind)
            : base(DomainEventKind.CantoAnnounced)
        {
            PlayerId = playerId;
            ClaimedKind = claimedKind;
        }

        public PlayerId PlayerId { get; }

        public CantoKind ClaimedKind { get; }
    }

    public sealed class CantoResolvedEvent : DomainEvent
    {
        public CantoResolvedEvent(PlayerId playerId, CantoKind claimedKind, bool isValid, bool didScore)
            : base(DomainEventKind.CantoResolved)
        {
            PlayerId = playerId;
            ClaimedKind = claimedKind;
            IsValid = isValid;
            DidScore = didScore;
        }

        public PlayerId PlayerId { get; }

        public CantoKind ClaimedKind { get; }

        public bool IsValid { get; }

        public bool DidScore { get; }
    }

    public enum ScoreReason
    {
        OpeningPattern,
        Canto,
        FalseCantoPenalty,
        Fall,
        CleanTable,
        CapturedCards,
    }

    public sealed class ScoreChangedEvent : DomainEvent
    {
        public ScoreChangedEvent(TeamId teamId, int pointsAwarded, Score total, ScoreReason reason)
            : base(DomainEventKind.ScoreChanged)
        {
            TeamId = teamId;
            PointsAwarded = pointsAwarded;
            Total = total;
            Reason = reason;
        }

        public TeamId TeamId { get; }

        public int PointsAwarded { get; }

        public Score Total { get; }

        public ScoreReason Reason { get; }
    }

    public sealed class DealCompletedEvent : DomainEvent
    {
        public DealCompletedEvent(int roundNumber, int dealNumber)
            : base(DomainEventKind.DealCompleted)
        {
            RoundNumber = roundNumber;
            DealNumber = dealNumber;
        }

        public int RoundNumber { get; }

        public int DealNumber { get; }
    }

    public sealed class LeftoversCollectedEvent : DomainEvent
    {
        private readonly IReadOnlyList<Card> _cards;

        public LeftoversCollectedEvent(PlayerId playerId, IEnumerable<Card> cards)
            : base(DomainEventKind.LeftoversCollected)
        {
            PlayerId = playerId;
            _cards = Array.AsReadOnly(new List<Card>(cards).ToArray());
        }

        public PlayerId PlayerId { get; }

        public IReadOnlyList<Card> Cards => _cards;
    }

    public sealed class RoundCompletedEvent : DomainEvent
    {
        public RoundCompletedEvent(int roundNumber)
            : base(DomainEventKind.RoundCompleted)
        {
            RoundNumber = roundNumber;
        }

        public int RoundNumber { get; }
    }

    public sealed class DealerRotatedEvent : DomainEvent
    {
        public DealerRotatedEvent(Seat previousDealerSeat, Seat currentDealerSeat)
            : base(DomainEventKind.DealerRotated)
        {
            PreviousDealerSeat = previousDealerSeat;
            CurrentDealerSeat = currentDealerSeat;
        }

        public Seat PreviousDealerSeat { get; }

        public Seat CurrentDealerSeat { get; }
    }

    public sealed class TieExtensionStartedEvent : DomainEvent
    {
        public TieExtensionStartedEvent(int roundNumber, Score tiedScore)
            : base(DomainEventKind.TieExtensionStarted)
        {
            RoundNumber = roundNumber;
            TiedScore = tiedScore;
        }

        public int RoundNumber { get; }

        public Score TiedScore { get; }
    }

    public sealed class TurnChangedEvent : DomainEvent
    {
        public TurnChangedEvent(Seat previousSeat, Seat currentSeat)
            : base(DomainEventKind.TurnChanged)
        {
            PreviousSeat = previousSeat;
            CurrentSeat = currentSeat;
        }

        public Seat PreviousSeat { get; }

        public Seat CurrentSeat { get; }
    }

    public sealed class MatchCompletedEvent : DomainEvent
    {
        public MatchCompletedEvent(TeamId winnerTeam)
            : base(DomainEventKind.MatchCompleted)
        {
            WinnerTeam = winnerTeam;
        }

        public TeamId WinnerTeam { get; }
    }
}
