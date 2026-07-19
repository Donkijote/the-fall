using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public sealed class Deck
    {
        private readonly Card[] _cards;
        private readonly IReadOnlyList<Card> _readOnlyCards;

        public Deck(IEnumerable<Card> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            _cards = new List<Card>(cards).ToArray();
            _readOnlyCards = Array.AsReadOnly(_cards);
        }

        public int Count => _cards.Length;

        public IReadOnlyList<Card> Cards => _readOnlyCards;

        public static Deck CreateSpanishDeck()
        {
            var cards = new List<Card>(40);

            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                {
                    cards.Add(new Card(suit, rank));
                }
            }

            return new Deck(cards);
        }

        public Deck Shuffle(IRandomSource randomSource)
        {
            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            var shuffled = (Card[])_cards.Clone();

            for (var index = shuffled.Length - 1; index > 0; index--)
            {
                var swapIndex = randomSource.NextInt(index + 1);
                var card = shuffled[index];
                shuffled[index] = shuffled[swapIndex];
                shuffled[swapIndex] = card;
            }

            return new Deck(shuffled);
        }

        public Deck Remove(IEnumerable<Card> cardsToRemove)
        {
            if (cardsToRemove == null)
            {
                throw new ArgumentNullException(nameof(cardsToRemove));
            }

            var remaining = new List<Card>(_cards);

            foreach (var card in cardsToRemove)
            {
                if (!remaining.Remove(card))
                {
                    throw new ArgumentException($"The deck does not contain {card}.", nameof(cardsToRemove));
                }
            }

            return new Deck(remaining);
        }
    }
}
