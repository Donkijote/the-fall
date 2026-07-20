using TheFall.Domain;

namespace TheFall.Application.Interaction
{
    public enum CardInteractionIntentKind
    {
        Inspect,
        Select,
        Confirm,
        Play,
        Cancel,
    }

    /// <summary>
    /// Platform-neutral application intent produced by touch, mouse, keyboard, or bots.
    /// Only Play crosses into the deterministic domain boundary.
    /// </summary>
    public abstract class CardInteractionIntent
    {
        protected CardInteractionIntent(CardInteractionIntentKind kind, PlayerId playerId)
        {
            Kind = kind;
            PlayerId = playerId;
        }

        public CardInteractionIntentKind Kind { get; }

        public PlayerId PlayerId { get; }
    }

    public abstract class CardTargetInteractionIntent : CardInteractionIntent
    {
        protected CardTargetInteractionIntent(
            CardInteractionIntentKind kind,
            PlayerId playerId,
            Card card)
            : base(kind, playerId)
        {
            Card = card;
        }

        public Card Card { get; }
    }

    public sealed class InspectCardInteractionIntent : CardTargetInteractionIntent
    {
        public InspectCardInteractionIntent(PlayerId playerId, Card card)
            : base(CardInteractionIntentKind.Inspect, playerId, card)
        {
        }
    }

    public sealed class SelectCardInteractionIntent : CardTargetInteractionIntent
    {
        public SelectCardInteractionIntent(PlayerId playerId, Card card)
            : base(CardInteractionIntentKind.Select, playerId, card)
        {
        }
    }

    public sealed class ConfirmCardInteractionIntent : CardInteractionIntent
    {
        public ConfirmCardInteractionIntent(PlayerId playerId)
            : base(CardInteractionIntentKind.Confirm, playerId)
        {
        }
    }

    public sealed class PlayCardInteractionIntent : CardTargetInteractionIntent
    {
        public PlayCardInteractionIntent(PlayerId playerId, Card card)
            : base(CardInteractionIntentKind.Play, playerId, card)
        {
        }
    }

    public sealed class CancelCardInteractionIntent : CardInteractionIntent
    {
        public CancelCardInteractionIntent(PlayerId playerId)
            : base(CardInteractionIntentKind.Cancel, playerId)
        {
        }
    }
}
