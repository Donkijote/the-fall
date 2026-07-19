using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public enum DomainEventKind
    {
        CardPlayed,
        CardPlacedOnTable,
        CardsCaptured,
        ScoreChanged,
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

    public enum ScoreReason
    {
        Fall,
        CleanTable,
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
