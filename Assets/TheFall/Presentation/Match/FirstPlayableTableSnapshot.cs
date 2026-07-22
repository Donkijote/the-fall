using System;
using System.Collections.Generic;
using TheFall.Domain;
using TheFall.Presentation.Animation;

namespace TheFall.Presentation.Match
{
    public readonly struct FirstPlayableCantoView
    {
        public FirstPlayableCantoView(PlayerId playerId, CantoKind claimedKind)
        {
            PlayerId = playerId;
            ClaimedKind = claimedKind;
        }

        public PlayerId PlayerId { get; }

        public CantoKind ClaimedKind { get; }
    }

    /// <summary>
    /// Read-only projection of authoritative match state for the 1v1 table. Hidden opponent
    /// cards and dealer-spread identities are reduced to counts before presentation sees them.
    /// </summary>
    public sealed class FirstPlayableTableSnapshot
    {
        private readonly IReadOnlyList<Card> _localHand;
        private readonly IReadOnlyList<Card> _tableCards;
        private readonly IReadOnlyList<Card> _localCapturedCards;
        private readonly IReadOnlyList<Card> _opponentCapturedCards;
        private readonly IReadOnlyList<FirstPlayableCantoView> _cantos;

        private FirstPlayableTableSnapshot(MatchState state)
        {
            AuthoritativeState = state ?? throw new ArgumentNullException(nameof(state));
            var local = state.GetPlayerAt(Seat.First);
            var opponent = state.GetPlayerAt(Seat.Second);

            LocalPlayerId = local.Player.Id;
            OpponentPlayerId = opponent.Player.Id;
            LocalPlayerName = local.Player.DisplayName;
            OpponentPlayerName = opponent.Player.DisplayName;
            _localHand = Copy(local.Hand);
            OpponentHandCount = opponent.Hand.Count;
            _tableCards = Copy(state.Table);
            _localCapturedCards = Copy(local.CapturedCards);
            _opponentCapturedCards = Copy(opponent.CapturedCards);
            DealerSpreadCount = state.Phase == MatchPhase.DealerSelection ? state.Deck.Count : 0;
            DeckCount = state.Deck.Count;
            DealerSeat = state.DealerSeat;
            ActiveSeat = state.CurrentSeat;
            LocalScore = state.TeamOneScore.Value;
            OpponentScore = state.TeamTwoScore.Value;
            RoundNumber = state.RoundNumber;
            DealNumber = state.DealNumber;
            IsFinalDeal = state.IsFinalDeal;
            IsTieExtension = state.IsTieExtension;
            Phase = state.Phase;
            WinnerTeam = state.WinnerTeam;

            var cantos = new FirstPlayableCantoView[state.CantoAnnouncements.Count];
            for (var index = 0; index < state.CantoAnnouncements.Count; index++)
            {
                var announcement = state.CantoAnnouncements[index];
                cantos[index] = new FirstPlayableCantoView(announcement.PlayerId, announcement.ClaimedKind);
            }

            _cantos = Array.AsReadOnly(cantos);
        }

        private FirstPlayableTableSnapshot(
            AnimationPresentationState state,
            MatchState referenceState)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            AuthoritativeState = referenceState ?? throw new ArgumentNullException(nameof(referenceState));
            var local = FindPlayer(state.Players, Seat.First);
            var opponent = FindPlayer(state.Players, Seat.Second);

            LocalPlayerId = local.Id;
            OpponentPlayerId = opponent.Id;
            LocalPlayerName = local.DisplayName;
            OpponentPlayerName = opponent.DisplayName;
            _localHand = Copy(state.GetHand(local.Id));
            OpponentHandCount = state.GetHand(opponent.Id).Count;
            _tableCards = Copy(state.Table);
            _localCapturedCards = Copy(state.GetCaptured(local.Id));
            _opponentCapturedCards = Copy(state.GetCaptured(opponent.Id));
            DealerSpreadCount = state.DealerSpreadCount;
            DeckCount = state.DeckCount;
            DealerSeat = state.DealerSeat;
            ActiveSeat = state.CurrentSeat;
            LocalScore = state.GetScore(TeamId.One).Value;
            OpponentScore = state.GetScore(TeamId.Two).Value;
            RoundNumber = state.RoundNumber;
            DealNumber = state.DealNumber;
            IsFinalDeal = state.IsFinalDeal;
            IsTieExtension = state.IsTieExtension;
            Phase = state.Phase;
            WinnerTeam = state.WinnerTeam;

            var cantos = new FirstPlayableCantoView[state.Cantos.Count];
            for (var index = 0; index < state.Cantos.Count; index++)
            {
                var canto = state.Cantos[index];
                cantos[index] = new FirstPlayableCantoView(canto.PlayerId, canto.ClaimedKind);
            }

            _cantos = Array.AsReadOnly(cantos);
        }

        public MatchState AuthoritativeState { get; }

        public PlayerId LocalPlayerId { get; }

        public PlayerId OpponentPlayerId { get; }

        public string LocalPlayerName { get; }

        public string OpponentPlayerName { get; }

        public IReadOnlyList<Card> LocalHand => _localHand;

        public int OpponentHandCount { get; }

        public IReadOnlyList<Card> TableCards => _tableCards;

        public IReadOnlyList<Card> LocalCapturedCards => _localCapturedCards;

        public IReadOnlyList<Card> OpponentCapturedCards => _opponentCapturedCards;

        public int DealerSpreadCount { get; }

        public int DeckCount { get; }

        public Seat DealerSeat { get; }

        public Seat ActiveSeat { get; }

        public int LocalScore { get; }

        public int OpponentScore { get; }

        public int RoundNumber { get; }

        public int DealNumber { get; }

        public bool IsFinalDeal { get; }

        public bool IsTieExtension { get; }

        public MatchPhase Phase { get; }

        public TeamId? WinnerTeam { get; }

        public IReadOnlyList<FirstPlayableCantoView> Cantos => _cantos;

        public static FirstPlayableTableSnapshot Create(MatchState state)
        {
            return new FirstPlayableTableSnapshot(state);
        }

        public static FirstPlayableTableSnapshot Create(
            AnimationPresentationState state,
            MatchState referenceState)
        {
            return new FirstPlayableTableSnapshot(state, referenceState);
        }

        private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> cards)
        {
            var copy = new Card[cards.Count];
            for (var index = 0; index < cards.Count; index++)
            {
                copy[index] = cards[index];
            }

            return Array.AsReadOnly(copy);
        }

        private static Player FindPlayer(IReadOnlyList<Player> players, Seat seat)
        {
            for (var index = 0; index < players.Count; index++)
            {
                if (players[index].Seat == seat)
                {
                    return players[index];
                }
            }

            throw new InvalidOperationException($"The presentation state has no player at {seat}.");
        }
    }
}
