using System;

namespace TheFall.Domain
{
    public enum OpeningPattern
    {
        Ascending,
        Descending,
    }

    public abstract class PlayerIntent
    {
        protected PlayerIntent(PlayerId playerId)
        {
            PlayerId = playerId;
        }

        public PlayerId PlayerId { get; }
    }

    public sealed class PlayCardIntent : PlayerIntent
    {
        public PlayCardIntent(PlayerId playerId, Card card)
            : base(playerId)
        {
            Card = card;
        }

        public Card Card { get; }

        public override string ToString()
        {
            return $"PlayCard({PlayerId}, {Card})";
        }
    }

    public sealed class SelectDealerCardIntent : PlayerIntent
    {
        public SelectDealerCardIntent(PlayerId playerId, Card card)
            : base(playerId)
        {
            Card = card;
        }

        public Card Card { get; }

        public override string ToString()
        {
            return $"SelectDealerCard({PlayerId}, {Card})";
        }
    }

    public sealed class ChooseDealOptionsIntent : PlayerIntent
    {
        public ChooseDealOptionsIntent(
            PlayerId playerId,
            bool dealHandsBeforeTable,
            OpeningPattern openingPattern)
            : base(playerId)
        {
            if (!Enum.IsDefined(typeof(OpeningPattern), openingPattern))
            {
                throw new ArgumentOutOfRangeException(nameof(openingPattern));
            }

            DealHandsBeforeTable = dealHandsBeforeTable;
            OpeningPattern = openingPattern;
        }

        public bool DealHandsBeforeTable { get; }

        public OpeningPattern OpeningPattern { get; }

        public override string ToString()
        {
            return $"ChooseDealOptions({PlayerId}, HandsFirst={DealHandsBeforeTable}, {OpeningPattern})";
        }
    }

    public sealed class AnnounceCantoIntent : PlayerIntent
    {
        public AnnounceCantoIntent(PlayerId playerId, CantoKind claimedKind)
            : base(playerId)
        {
            if (!Enum.IsDefined(typeof(CantoKind), claimedKind))
            {
                throw new ArgumentOutOfRangeException(nameof(claimedKind));
            }

            ClaimedKind = claimedKind;
        }

        public CantoKind ClaimedKind { get; }

        public override string ToString()
        {
            return $"AnnounceCanto({PlayerId}, {ClaimedKind})";
        }
    }

    public enum RuleError
    {
        None,
        UnsupportedIntent,
        MatchAlreadyCompleted,
        WrongPhase,
        UnknownPlayer,
        NotPlayersTurn,
        CardNotInHand,
        CardNotInDealerSpread,
        PlayerAlreadySelectedDealerCard,
        NotDealer,
        CantoOpportunityClosed,
        CantoAlreadyAnnounced,
        RandomSourceRequired,
    }
}
