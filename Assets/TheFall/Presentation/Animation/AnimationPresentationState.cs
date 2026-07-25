using System;
using System.Collections.Generic;
using System.Linq;
using TheFall.Domain;

namespace TheFall.Presentation.Animation
{
    public readonly struct AnimationCantoState
    {
        public AnimationCantoState(PlayerId playerId, CantoKind claimedKind)
        {
            PlayerId = playerId;
            ClaimedKind = claimedKind;
        }

        public PlayerId PlayerId { get; }

        public CantoKind ClaimedKind { get; }
    }

    /// <summary>
    /// Mutable rendered snapshot used only while explaining an authoritative match transition.
    /// Synchronization always copies the resolved domain state instead of deriving an outcome.
    /// </summary>
    public sealed class AnimationPresentationState
    {
        private readonly List<Player> _players = new List<Player>();
        private readonly Dictionary<PlayerId, List<Card>> _hands = new Dictionary<PlayerId, List<Card>>();
        private readonly Dictionary<PlayerId, Dictionary<Card, int>> _handLayoutIndices =
            new Dictionary<PlayerId, Dictionary<Card, int>>();
        private readonly Dictionary<PlayerId, List<Card>> _captured = new Dictionary<PlayerId, List<Card>>();
        private readonly Dictionary<TeamId, Score> _scores = new Dictionary<TeamId, Score>();
        private readonly List<Card> _table = new List<Card>();
        private readonly Dictionary<Card, int> _tableLayoutIndices = new Dictionary<Card, int>();
        private readonly List<Card> _dealerSelectionCards = new List<Card>();
        private readonly List<AnimationCantoState> _cantos = new List<AnimationCantoState>();

        public AnimationPresentationState(MatchState state)
        {
            Synchronize(state);
        }

        public IReadOnlyList<Player> Players => _players;

        public IReadOnlyList<Card> Table => _table;

        public IReadOnlyList<Card> DealerSelectionCards => _dealerSelectionCards;

        public IReadOnlyList<AnimationCantoState> Cantos => _cantos;

        public int DeckCount { get; private set; }

        public int DealerSpreadCount => Phase == MatchPhase.DealerSelection ? DeckCount : 0;

        public Seat DealerSeat { get; private set; }

        public Seat CurrentSeat { get; private set; }

        public MatchPhase Phase { get; private set; }

        public TeamId? WinnerTeam { get; private set; }

        public int RoundNumber { get; private set; }

        public int DealNumber { get; private set; }

        public bool IsFinalDeal { get; private set; }

        public bool IsTieExtension { get; private set; }

        public IReadOnlyList<Card> GetHand(PlayerId playerId)
        {
            return _hands[playerId];
        }

        public IReadOnlyList<Card> GetCaptured(PlayerId playerId)
        {
            return _captured[playerId];
        }

        public int GetHandLayoutIndex(PlayerId playerId, Card card)
        {
            return _handLayoutIndices[playerId][card];
        }

        public int GetHandLayoutSlotCount(PlayerId playerId)
        {
            var slots = 0;
            foreach (var index in _handLayoutIndices[playerId].Values)
            {
                slots = Math.Max(slots, index + 1);
            }

            return slots;
        }

        public int GetTableLayoutIndex(Card card)
        {
            return _tableLayoutIndices[card];
        }

        public Score GetScore(TeamId teamId)
        {
            return _scores[teamId];
        }

        public void Apply(
            ResolvedAnimationStep step,
            MatchState finalState,
            bool deferHandReflow = false)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            switch (step.Kind)
            {
                case ResolvedAnimationStepKind.MatchStarted:
                    if (step.SourceEvent is MatchStartedEvent started)
                    {
                        Phase = MatchPhase.DealerSelection;
                        DeckCount = started.DealerSpreadCardCount;
                    }

                    break;
                case ResolvedAnimationStepKind.DealerSelection:
                    ApplyDealerSelection(step.SourceEvent);
                    break;
                case ResolvedAnimationStepKind.DealerChoice:
                    Phase = MatchPhase.Active;
                    break;
                case ResolvedAnimationStepKind.OpeningRejection:
                    // Drawing and reinserting a rejected opening card has no net state effect.
                    // The presentation treatment owns its temporary face-up table pose and deck gap.
                    break;
                case ResolvedAnimationStepKind.Deal:
                    if (step.SourceEvent is DealStartedEvent dealStarted)
                    {
                        RoundNumber = dealStarted.RoundNumber;
                        DealNumber = dealStarted.DealNumber;
                        IsFinalDeal = dealStarted.IsFinalDeal;
                        _cantos.Clear();
                    }
                    else if (step.SourceEvent is CardDealtEvent)
                    {
                        AddUnique(_hands[step.PlayerId], step.Cards[0]);
                        ReindexHand(step.PlayerId);
                        DeckCount = Math.Max(0, DeckCount - 1);
                    }

                    break;
                case ResolvedAnimationStepKind.OpeningPlacement:
                    AddTableCard(step.Cards[0], true);
                    DeckCount = Math.Max(0, DeckCount - 1);
                    break;
                case ResolvedAnimationStepKind.CardPlay:
                    MovePlayedCardToTable(
                        step.PlayerId,
                        step.Cards[0],
                        !deferHandReflow);
                    break;
                case ResolvedAnimationStepKind.HandReflow:
                    ReindexHand(step.PlayerId);
                    break;
                case ResolvedAnimationStepKind.TablePlacement:
                    AddTableCard(step.Cards[0], false);
                    break;
                case ResolvedAnimationStepKind.NormalCapture:
                case ResolvedAnimationStepKind.CascadeCapture:
                    MoveCapturedCards(step.PlayerId, step.Cards);
                    break;
                case ResolvedAnimationStepKind.CaptureCollection:
                    break;
                case ResolvedAnimationStepKind.FallScore:
                case ResolvedAnimationStepKind.CleanTableScore:
                case ResolvedAnimationStepKind.Score:
                    _scores[step.TeamId] = step.Total;
                    break;
                case ResolvedAnimationStepKind.Canto:
                    if (step.SourceEvent is CantoAnnouncedEvent canto
                        && !_cantos.Exists(item => item.PlayerId == canto.PlayerId))
                    {
                        _cantos.Add(new AnimationCantoState(canto.PlayerId, canto.ClaimedKind));
                    }

                    break;
                case ResolvedAnimationStepKind.DealCompleted:
                    break;
                case ResolvedAnimationStepKind.Leftovers:
                    MoveCapturedCards(step.PlayerId, step.Cards);
                    break;
                case ResolvedAnimationStepKind.Round:
                    if (step.SourceEvent is RoundCompletedEvent completedRound)
                    {
                        RoundNumber = completedRound.RoundNumber;
                    }

                    break;
                case ResolvedAnimationStepKind.DealerRotation:
                    DealerSeat = step.CurrentSeat;
                    break;
                case ResolvedAnimationStepKind.TieExtension:
                    IsTieExtension = true;
                    if (step.SourceEvent is TieExtensionStartedEvent extension)
                    {
                        RoundNumber = extension.RoundNumber;
                    }

                    break;
                case ResolvedAnimationStepKind.TurnChanged:
                    CurrentSeat = step.CurrentSeat;
                    break;
                case ResolvedAnimationStepKind.MatchCompleted:
                    Phase = MatchPhase.Completed;
                    WinnerTeam = step.TeamId;
                    break;
                case ResolvedAnimationStepKind.SynchronizeFinalState:
                    Synchronize(finalState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(step), step.Kind, null);
            }
        }

        public void Synchronize(MatchState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _players.Clear();
            _hands.Clear();
            _handLayoutIndices.Clear();
            _captured.Clear();
            foreach (var playerState in state.Players)
            {
                _players.Add(playerState.Player);
                _hands[playerState.Player.Id] = new List<Card>(playerState.Hand);
                _handLayoutIndices[playerState.Player.Id] = new Dictionary<Card, int>();
                _captured[playerState.Player.Id] = new List<Card>(playerState.CapturedCards);
                ReindexHand(playerState.Player.Id);
            }

            var previousTableLayoutIndices =
                new Dictionary<Card, int>(_tableLayoutIndices);
            _table.Clear();
            _table.AddRange(state.Table);
            _tableLayoutIndices.Clear();
            var retainedTableLayoutIndices = new HashSet<int>();
            foreach (var card in _table)
            {
                if (previousTableLayoutIndices.TryGetValue(card, out var previousIndex)
                    && previousIndex >= 0
                    && previousIndex < AnimationTableCardLayoutEvaluator.Capacity
                    && retainedTableLayoutIndices.Add(previousIndex))
                {
                    _tableLayoutIndices[card] = previousIndex;
                }
            }

            foreach (var card in _table)
            {
                if (_tableLayoutIndices.ContainsKey(card))
                {
                    continue;
                }

                _tableLayoutIndices[card] =
                    AnimationTableCardLayoutEvaluator.ResolveAvailableIndex(
                        card,
                        _tableLayoutIndices.Values,
                        _table.Count <= 4);
            }
            _dealerSelectionCards.Clear();
            _dealerSelectionCards.AddRange(state.DealerSelectionCards);
            _scores.Clear();
            _scores[TeamId.One] = state.TeamOneScore;
            _scores[TeamId.Two] = state.TeamTwoScore;
            _cantos.Clear();
            foreach (var canto in state.CantoAnnouncements)
            {
                _cantos.Add(new AnimationCantoState(canto.PlayerId, canto.ClaimedKind));
            }

            DeckCount = state.Deck.Count;
            DealerSeat = state.DealerSeat;
            CurrentSeat = state.CurrentSeat;
            Phase = state.Phase;
            WinnerTeam = state.WinnerTeam;
            RoundNumber = state.RoundNumber;
            DealNumber = state.DealNumber;
            IsFinalDeal = state.IsFinalDeal;
            IsTieExtension = state.IsTieExtension;
        }

        public bool IsSynchronizedWith(MatchState state)
        {
            if (state == null ||
                CurrentSeat != state.CurrentSeat ||
                Phase != state.Phase ||
                WinnerTeam != state.WinnerTeam ||
                DeckCount != state.Deck.Count ||
                DealerSeat != state.DealerSeat ||
                RoundNumber != state.RoundNumber ||
                DealNumber != state.DealNumber ||
                IsFinalDeal != state.IsFinalDeal ||
                IsTieExtension != state.IsTieExtension ||
                _players.Count != state.Players.Count ||
                _cantos.Count != state.CantoAnnouncements.Count ||
                !_dealerSelectionCards.SequenceEqual(state.DealerSelectionCards) ||
                !GetScore(TeamId.One).Equals(state.TeamOneScore) ||
                !GetScore(TeamId.Two).Equals(state.TeamTwoScore) ||
                !_table.SequenceEqual(state.Table))
            {
                return false;
            }

            foreach (var playerState in state.Players)
            {
                if (!_hands.TryGetValue(playerState.Player.Id, out var hand) ||
                    !_captured.TryGetValue(playerState.Player.Id, out var captured) ||
                    !hand.SequenceEqual(playerState.Hand) ||
                    !captured.SequenceEqual(playerState.CapturedCards))
                {
                    return false;
                }
            }

            for (var index = 0; index < _cantos.Count; index++)
            {
                var presentedCanto = _cantos[index];
                var authoritativeCanto = state.CantoAnnouncements[index];
                if (presentedCanto.PlayerId != authoritativeCanto.PlayerId ||
                    presentedCanto.ClaimedKind != authoritativeCanto.ClaimedKind)
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyDealerSelection(DomainEvent resolvedEvent)
        {
            if (resolvedEvent is DealerCardSelectedEvent selectedCard)
            {
                DeckCount = Math.Max(0, DeckCount - 1);
                AddUnique(_dealerSelectionCards, selectedCard.Card);
            }
            else if (resolvedEvent is DealerSelectedEvent selected)
            {
                DealerSeat = selected.DealerSeat;
                Phase = MatchPhase.AwaitingDealerChoice;
            }
            else if (resolvedEvent is DeckShuffledEvent shuffled)
            {
                if (shuffled.RoundNumber > RoundNumber)
                {
                    foreach (var hand in _hands.Values)
                    {
                        hand.Clear();
                    }

                    foreach (var player in _players)
                    {
                        ReindexHand(player.Id);
                    }

                    foreach (var captured in _captured.Values)
                    {
                        captured.Clear();
                    }

                    _table.Clear();
                    _cantos.Clear();
                    DealNumber = 0;
                    IsFinalDeal = false;
                }

                RoundNumber = shuffled.RoundNumber;
                DeckCount = shuffled.CardCount;
                _dealerSelectionCards.Clear();
            }
        }

        private void MovePlayedCardToTable(
            PlayerId playerId,
            Card card,
            bool reindexHand)
        {
            if (_hands.TryGetValue(playerId, out var hand))
            {
                hand.Remove(card);
                if (reindexHand)
                {
                    ReindexHand(playerId);
                }
            }

            AddTableCard(card, false);
        }

        private void ReindexHand(PlayerId playerId)
        {
            var indices = _handLayoutIndices[playerId];
            indices.Clear();
            var hand = _hands[playerId];
            for (var index = 0; index < hand.Count; index++)
            {
                indices[hand[index]] = index;
            }
        }

        private void MoveCapturedCards(PlayerId playerId, IReadOnlyList<Card> cards)
        {
            var captured = _captured[playerId];
            var removedFromHand = false;
            foreach (var card in cards)
            {
                removedFromHand |= _hands[playerId].Remove(card);
                RemoveTableCard(card);
                AddUnique(captured, card);
            }

            if (removedFromHand)
            {
                ReindexHand(playerId);
            }
        }

        private static void AddUnique(List<Card> cards, Card card)
        {
            if (!cards.Contains(card))
            {
                cards.Add(card);
            }
        }

        private void AddTableCard(Card card, bool preferOpeningGrid)
        {
            if (_table.Contains(card))
            {
                return;
            }

            var occupiedIndices = new List<int>();
            foreach (var tableCard in _table)
            {
                if (_tableLayoutIndices.TryGetValue(tableCard, out var occupiedIndex))
                {
                    occupiedIndices.Add(occupiedIndex);
                }
            }

            _table.Add(card);
            if (!_tableLayoutIndices.TryGetValue(card, out var retainedIndex)
                || occupiedIndices.Contains(retainedIndex))
            {
                _tableLayoutIndices[card] =
                    AnimationTableCardLayoutEvaluator.ResolveAvailableIndex(
                        card,
                        occupiedIndices,
                        preferOpeningGrid);
            }
        }

        private void RemoveTableCard(Card card)
        {
            _table.Remove(card);
        }
    }
}
