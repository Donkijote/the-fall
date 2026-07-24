using TheFall.Domain;
using UnityEngine;

namespace TheFall.Presentation.Match
{
    public enum FirstPlayableCardZone
    {
        DealerSpread,
        DealerSelection,
        Deck,
        Table,
        LocalHand,
        OpponentHand,
        LocalCaptured,
        OpponentCaptured,
    }

    [DisallowMultipleComponent]
    public sealed class FirstPlayableRenderedCard : MonoBehaviour
    {
        public FirstPlayableCardZone Zone { get; private set; }

        public Card? Card { get; private set; }

        internal Card? PresentationCard { get; private set; }

        public bool IsFaceUp { get; private set; }

        public int InteractionIndex { get; private set; } = -1;

        public void Configure(
            FirstPlayableCardZone zone,
            bool isFaceUp,
            Card? card = null,
            int interactionIndex = -1)
        {
            Zone = zone;
            IsFaceUp = isFaceUp;
            Card = isFaceUp ? card : null;
            PresentationCard = card;
            InteractionIndex = interactionIndex;
        }

        internal void SetFaceUp(bool isFaceUp)
        {
            IsFaceUp = isFaceUp;
            Card = isFaceUp ? PresentationCard : null;
        }
    }
}
