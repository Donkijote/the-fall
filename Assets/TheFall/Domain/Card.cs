using System;

namespace TheFall.Domain
{
    public enum CardSuit
    {
        Coins,
        Cups,
        Swords,
        Clubs,
    }

    public enum CardRank
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Ten = 10,
        Eleven = 11,
        Twelve = 12,
    }

    public readonly struct Card : IEquatable<Card>
    {
        public Card(CardSuit suit, CardRank rank)
        {
            if (!Enum.IsDefined(typeof(CardSuit), suit))
            {
                throw new ArgumentOutOfRangeException(nameof(suit));
            }

            if (!Enum.IsDefined(typeof(CardRank), rank))
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }

            Suit = suit;
            Rank = rank;
        }

        public CardSuit Suit { get; }

        public CardRank Rank { get; }

        public bool Equals(Card other)
        {
            return Suit == other.Suit && Rank == other.Rank;
        }

        public override bool Equals(object obj)
        {
            return obj is Card other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ((int)Suit * 397) ^ (int)Rank;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }

        public static bool operator ==(Card left, Card right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Card left, Card right)
        {
            return !left.Equals(right);
        }
    }

    public static class CardRankOrder
    {
        private static readonly CardRank[] OrderedRanks =
        {
            CardRank.One,
            CardRank.Two,
            CardRank.Three,
            CardRank.Four,
            CardRank.Five,
            CardRank.Six,
            CardRank.Seven,
            CardRank.Ten,
            CardRank.Eleven,
            CardRank.Twelve,
        };

        public static bool TryGetNext(CardRank rank, out CardRank next)
        {
            for (var index = 0; index < OrderedRanks.Length - 1; index++)
            {
                if (OrderedRanks[index] != rank)
                {
                    continue;
                }

                next = OrderedRanks[index + 1];
                return true;
            }

            next = default;
            return false;
        }

        public static int GetFallPoints(CardRank rank)
        {
            switch (rank)
            {
                case CardRank.Ten:
                    return 2;
                case CardRank.Eleven:
                    return 3;
                case CardRank.Twelve:
                    return 4;
                default:
                    return 1;
            }
        }
    }
}
