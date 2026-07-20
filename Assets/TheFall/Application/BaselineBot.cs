using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Application
{
    /// <summary>
    /// Information-safe snapshot supplied to the baseline bot. It intentionally omits the
    /// opponent hand, dealer-spread card identities, and hidden deck order.
    /// </summary>
    public sealed class BotTurnView
    {
        private readonly IReadOnlyList<Card> _ownHand;
        private readonly IReadOnlyList<Card> _table;
        private readonly IReadOnlyList<PlayerIntent> _legalIntents;

        internal BotTurnView(MatchState state, PlayerState bot, IReadOnlyList<PlayerIntent> legalIntents)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (legalIntents == null)
            {
                throw new ArgumentNullException(nameof(legalIntents));
            }

            PlayerId = bot.Player.Id;
            Phase = state.Phase;
            IsDealer = bot.Player.Seat == state.DealerSeat;
            TeamOneScore = state.TeamOneScore;
            TeamTwoScore = state.TeamTwoScore;
            RoundNumber = state.RoundNumber;
            DealNumber = state.DealNumber;
            IsFinalDeal = state.IsFinalDeal;
            IsTieExtension = state.IsTieExtension;
            PreviousPlay = state.PreviousPlay;
            Rules = state.Rules;
            _ownHand = Array.AsReadOnly(Copy(state.GetPlayer(bot.Player.Id).Hand));
            _table = Array.AsReadOnly(Copy(state.Table));
            _legalIntents = Array.AsReadOnly(Copy(legalIntents));
        }

        public PlayerId PlayerId { get; }

        public MatchPhase Phase { get; }

        public bool IsDealer { get; }

        public Score TeamOneScore { get; }

        public Score TeamTwoScore { get; }

        public int RoundNumber { get; }

        public int DealNumber { get; }

        public bool IsFinalDeal { get; }

        public bool IsTieExtension { get; }

        public PreviousPlay PreviousPlay { get; }

        public RuleConfiguration Rules { get; }

        public IReadOnlyList<Card> OwnHand => _ownHand;

        public IReadOnlyList<Card> Table => _table;

        internal IReadOnlyList<PlayerIntent> LegalIntents => _legalIntents;

        private static Card[] Copy(IReadOnlyList<Card> cards)
        {
            var copy = new Card[cards.Count];
            for (var index = 0; index < cards.Count; index++)
            {
                copy[index] = cards[index];
            }

            return copy;
        }

        private static PlayerIntent[] Copy(IReadOnlyList<PlayerIntent> intents)
        {
            var copy = new PlayerIntent[intents.Count];
            for (var index = 0; index < intents.Count; index++)
            {
                copy[index] = intents[index];
            }

            return copy;
        }
    }

    /// <summary>
    /// One deterministic first-playable policy. It announces a valid canto, prefers captures,
    /// and uses its injected seeded source only to break otherwise equivalent choices.
    /// </summary>
    public sealed class BaselineBot
    {
        private readonly IRandomSource _randomSource;

        public BaselineBot(IRandomSource randomSource)
        {
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        internal PlayerIntent SelectIntent(BotTurnView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (view.LegalIntents.Count == 0)
            {
                throw new InvalidOperationException("The baseline bot cannot act without a legal intent.");
            }

            if (view.Phase == MatchPhase.DealerSelection
                || view.Phase == MatchPhase.AwaitingDealerChoice)
            {
                return ChooseRandom(view.LegalIntents);
            }

            var canto = ChooseValidCanto(view);
            if (canto != null)
            {
                return canto;
            }

            return ChooseCardPlay(view);
        }

        private PlayerIntent ChooseValidCanto(BotTurnView view)
        {
            if (view.OwnHand.Count != 3)
            {
                return null;
            }

            var canto = CantoRules.Classify(view.OwnHand, view.Rules);
            if (canto == null)
            {
                return null;
            }

            for (var index = 0; index < view.LegalIntents.Count; index++)
            {
                if (view.LegalIntents[index] is AnnounceCantoIntent announcement
                    && announcement.ClaimedKind == canto.Kind)
                {
                    return announcement;
                }
            }

            return null;
        }

        private PlayerIntent ChooseCardPlay(BotTurnView view)
        {
            var candidates = new List<PlayCardIntent>();
            var bestValue = int.MinValue;

            for (var index = 0; index < view.LegalIntents.Count; index++)
            {
                if (!(view.LegalIntents[index] is PlayCardIntent play))
                {
                    continue;
                }

                var value = EvaluatePlay(view, play.Card);
                if (value > bestValue)
                {
                    candidates.Clear();
                    candidates.Add(play);
                    bestValue = value;
                }
                else if (value == bestValue)
                {
                    candidates.Add(play);
                }
            }

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("The active baseline bot has no legal card play.");
            }

            return candidates[_randomSource.NextInt(candidates.Count)];
        }

        private PlayerIntent ChooseRandom(IReadOnlyList<PlayerIntent> intents)
        {
            return intents[_randomSource.NextInt(intents.Count)];
        }

        private static int EvaluatePlay(BotTurnView view, Card card)
        {
            var matchingIndex = FindRank(view.Table, card.Rank);
            if (matchingIndex < 0)
            {
                return 0;
            }

            var capturedCount = 1;
            var rank = card.Rank;
            while (CardRankOrder.TryGetNext(rank, out var nextRank))
            {
                if (FindRank(view.Table, nextRank) < 0)
                {
                    break;
                }

                capturedCount++;
                rank = nextRank;
            }

            var value = capturedCount * 100;
            if (view.PreviousPlay != null
                && !view.PreviousPlay.WasCapture
                && view.PreviousPlay.PlayerId != view.PlayerId
                && view.PreviousPlay.Card.Rank == card.Rank)
            {
                value += CardRankOrder.GetFallPoints(card.Rank) * 10;
            }

            if (!view.IsFinalDeal && capturedCount == view.Table.Count)
            {
                value += 4;
            }

            return value;
        }

        private static int FindRank(IReadOnlyList<Card> cards, CardRank rank)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                if (cards[index].Rank == rank)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
