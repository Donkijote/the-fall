using System;

namespace TheFall.Domain
{
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

    public enum RuleError
    {
        None,
        UnsupportedIntent,
        MatchAlreadyCompleted,
        UnknownPlayer,
        NotPlayersTurn,
        CardNotInHand,
    }
}
