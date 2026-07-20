using System;
using System.Collections.Generic;
using System.Linq;
using TheFall.Domain;

namespace TheFall.Presentation.Animation
{
    /// <summary>
    /// Mutable rendered snapshot used only while explaining an authoritative match transition.
    /// Synchronization always copies the resolved domain state instead of deriving an outcome.
    /// </summary>
    public sealed class AnimationPresentationState
    {
        private readonly List<Player> _players = new List<Player>();
        private readonly Dictionary<PlayerId, List<Card>> _hands = new Dictionary<PlayerId, List<Card>>();
        private readonly Dictionary<PlayerId, List<Card>> _captured = new Dictionary<PlayerId, List<Card>>();
        private readonly Dictionary<TeamId, Score> _scores = new Dictionary<TeamId, Score>();
        private readonly List<Card> _table = new List<Card>();

        public AnimationPresentationState(MatchState state)
        {
            Synchronize(state);
        }

        public IReadOnlyList<Player> Players => _players;

        public IReadOnlyList<Card> Table => _table;

        public Seat CurrentSeat { get; private set; }

        public MatchPhase Phase { get; private set; }

        public TeamId? WinnerTeam { get; private set; }

        public IReadOnlyList<Card> GetHand(PlayerId playerId)
        {
            return _hands[playerId];
        }

        public IReadOnlyList<Card> GetCaptured(PlayerId playerId)
        {
            return _captured[playerId];
        }

        public Score GetScore(TeamId teamId)
        {
            return _scores[teamId];
        }

        public void Apply(ResolvedAnimationStep step, MatchState finalState)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            switch (step.Kind)
            {
                case ResolvedAnimationStepKind.CardPlay:
                    MovePlayedCardToTable(step.PlayerId, step.Cards[0]);
                    break;
                case ResolvedAnimationStepKind.TablePlacement:
                    AddUnique(_table, step.Cards[0]);
                    break;
                case ResolvedAnimationStepKind.NormalCapture:
                case ResolvedAnimationStepKind.CascadeCapture:
                    MoveCapturedCards(step.PlayerId, step.Cards);
                    break;
                case ResolvedAnimationStepKind.FallScore:
                case ResolvedAnimationStepKind.CleanTableScore:
                    _scores[step.TeamId] = step.Total;
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
            _captured.Clear();
            foreach (var playerState in state.Players)
            {
                _players.Add(playerState.Player);
                _hands[playerState.Player.Id] = new List<Card>(playerState.Hand);
                _captured[playerState.Player.Id] = new List<Card>(playerState.CapturedCards);
            }

            _table.Clear();
            _table.AddRange(state.Table);
            _scores.Clear();
            _scores[TeamId.One] = state.TeamOneScore;
            _scores[TeamId.Two] = state.TeamTwoScore;
            CurrentSeat = state.CurrentSeat;
            Phase = state.Phase;
            WinnerTeam = state.WinnerTeam;
        }

        public bool IsSynchronizedWith(MatchState state)
        {
            if (state == null ||
                CurrentSeat != state.CurrentSeat ||
                Phase != state.Phase ||
                WinnerTeam != state.WinnerTeam ||
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

            return true;
        }

        private void MovePlayedCardToTable(PlayerId playerId, Card card)
        {
            if (_hands.TryGetValue(playerId, out var hand))
            {
                hand.Remove(card);
            }

            AddUnique(_table, card);
        }

        private void MoveCapturedCards(PlayerId playerId, IReadOnlyList<Card> cards)
        {
            var captured = _captured[playerId];
            foreach (var card in cards)
            {
                _table.Remove(card);
                AddUnique(captured, card);
            }
        }

        private static void AddUnique(List<Card> cards, Card card)
        {
            if (!cards.Contains(card))
            {
                cards.Add(card);
            }
        }
    }
}
