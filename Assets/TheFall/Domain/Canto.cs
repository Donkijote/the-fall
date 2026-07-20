using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public enum CantoKind
    {
        CasaGrande,
        CasaChica,
        Registro,
        Vigia,
        Patrulla,
        Trivilin,
        Ronda,
    }

    public sealed class CantoClassification
    {
        public CantoClassification(CantoKind kind, int points, CardRank strength, bool winsImmediately)
        {
            Kind = kind;
            Points = points;
            Strength = strength;
            WinsImmediately = winsImmediately;
        }

        public CantoKind Kind { get; }

        public int Points { get; }

        public CardRank Strength { get; }

        public bool WinsImmediately { get; }
    }

    public sealed class CantoAnnouncement
    {
        private readonly Card[] _hand;

        public CantoAnnouncement(PlayerId playerId, CantoKind claimedKind, IEnumerable<Card> hand)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            _hand = new List<Card>(hand).ToArray();
            if (_hand.Length != 3)
            {
                throw new ArgumentException("A canto announcement preserves exactly three cards.", nameof(hand));
            }

            PlayerId = playerId;
            ClaimedKind = claimedKind;
            Hand = Array.AsReadOnly(_hand);
        }

        public PlayerId PlayerId { get; }

        public CantoKind ClaimedKind { get; }

        public IReadOnlyList<Card> Hand { get; }
    }

    public static class CantoRules
    {
        public static CantoClassification Classify(IReadOnlyList<Card> hand, RuleConfiguration rules)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (hand.Count != 3)
            {
                throw new ArgumentException("Canto classification requires exactly three cards.", nameof(hand));
            }

            var ranks = new[] { hand[0].Rank, hand[1].Rank, hand[2].Rank };
            Array.Sort(ranks, CompareRanks);
            var counts = CountRanks(ranks);

            if (rules.CasaCantosEnabled && Matches(ranks, CardRank.One, CardRank.Twelve, CardRank.Twelve))
            {
                return new CantoClassification(CantoKind.CasaGrande, 12, CardRank.Twelve, false);
            }

            if (rules.CasaCantosEnabled && Matches(ranks, CardRank.One, CardRank.Eleven, CardRank.Eleven))
            {
                return new CantoClassification(CantoKind.CasaChica, 10, CardRank.Eleven, false);
            }

            if (Matches(ranks, CardRank.One, CardRank.Eleven, CardRank.Twelve))
            {
                return new CantoClassification(CantoKind.Registro, 8, CardRank.Twelve, false);
            }

            foreach (var pair in counts)
            {
                if (pair.Value == 3)
                {
                    return new CantoClassification(
                        CantoKind.Trivilin,
                        5,
                        pair.Key,
                        rules.TrivilinWinsImmediately);
                }
            }

            foreach (var pair in counts)
            {
                if (pair.Value != 2)
                {
                    continue;
                }

                var other = FindOtherRank(ranks, pair.Key);
                if (AreAdjacent(pair.Key, other))
                {
                    return new CantoClassification(CantoKind.Vigia, 7, pair.Key, false);
                }
            }

            if (AreConsecutive(ranks))
            {
                return new CantoClassification(CantoKind.Patrulla, 6, ranks[2], false);
            }

            foreach (var pair in counts)
            {
                if (pair.Value == 2)
                {
                    return new CantoClassification(
                        CantoKind.Ronda,
                        CardRankOrder.GetFallPoints(pair.Key),
                        pair.Key,
                        false);
                }
            }

            return null;
        }

        internal static int Compare(CantoClassification left, Seat leftSeat, CantoClassification right, Seat rightSeat, Seat dealerSeat)
        {
            var effectComparison = GetComparisonValue(left).CompareTo(GetComparisonValue(right));
            if (effectComparison != 0)
            {
                return effectComparison;
            }

            if (left.Kind == right.Kind)
            {
                var strengthComparison = CardRankOrder.GetIndex(left.Strength)
                    .CompareTo(CardRankOrder.GetIndex(right.Strength));
                if (strengthComparison != 0)
                {
                    return strengthComparison;
                }
            }

            var firstToAct = dealerSeat == Seat.First ? Seat.Second : Seat.First;
            if (leftSeat == rightSeat)
            {
                return 0;
            }

            return leftSeat == firstToAct ? 1 : -1;
        }

        private static int GetComparisonValue(CantoClassification canto)
        {
            return canto.WinsImmediately ? int.MaxValue : canto.Points;
        }

        private static Dictionary<CardRank, int> CountRanks(IEnumerable<CardRank> ranks)
        {
            var counts = new Dictionary<CardRank, int>();
            foreach (var rank in ranks)
            {
                counts.TryGetValue(rank, out var count);
                counts[rank] = count + 1;
            }

            return counts;
        }

        private static CardRank FindOtherRank(IEnumerable<CardRank> ranks, CardRank pairRank)
        {
            foreach (var rank in ranks)
            {
                if (rank != pairRank)
                {
                    return rank;
                }
            }

            return pairRank;
        }

        private static bool AreAdjacent(CardRank first, CardRank second)
        {
            return Math.Abs(CardRankOrder.GetIndex(first) - CardRankOrder.GetIndex(second)) == 1;
        }

        private static bool AreConsecutive(IReadOnlyList<CardRank> ranks)
        {
            return CardRankOrder.GetIndex(ranks[1]) == CardRankOrder.GetIndex(ranks[0]) + 1
                && CardRankOrder.GetIndex(ranks[2]) == CardRankOrder.GetIndex(ranks[1]) + 1;
        }

        private static bool Matches(IReadOnlyList<CardRank> ranks, CardRank first, CardRank second, CardRank third)
        {
            return ranks[0] == first && ranks[1] == second && ranks[2] == third;
        }

        private static int CompareRanks(CardRank left, CardRank right)
        {
            return CardRankOrder.GetIndex(left).CompareTo(CardRankOrder.GetIndex(right));
        }
    }
}
