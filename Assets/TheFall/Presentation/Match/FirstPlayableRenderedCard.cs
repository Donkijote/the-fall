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

        public int LayoutIndex { get; private set; } = -1;

        internal float RestingYawDegrees { get; private set; }

        public void Configure(
            FirstPlayableCardZone zone,
            bool isFaceUp,
            Card? card = null,
            int interactionIndex = -1,
            int layoutIndex = -1,
            float restingYawDegrees = 0f)
        {
            Zone = zone;
            IsFaceUp = isFaceUp;
            Card = isFaceUp ? card : null;
            PresentationCard = card;
            InteractionIndex = interactionIndex;
            LayoutIndex = layoutIndex;
            RestingYawDegrees = restingYawDegrees;
        }

        internal void SetFaceUp(bool isFaceUp)
        {
            IsFaceUp = isFaceUp;
            Card = isFaceUp ? PresentationCard : null;
        }

        internal void SetRestingYawDegrees(float restingYawDegrees)
        {
            RestingYawDegrees = restingYawDegrees;
        }
    }
}
