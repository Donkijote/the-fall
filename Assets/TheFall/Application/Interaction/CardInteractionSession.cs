using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Application.Interaction
{
    /// <summary>
    /// Coordinates reversible card interaction before submitting a confirmed play to MatchSession.
    /// Platform adapters share this surface and never evaluate card-game rules themselves.
    /// </summary>
    public sealed class CardInteractionSession
    {
        private readonly Func<MatchState> _getMatchState;
        private readonly Func<PlayerId, IReadOnlyList<PlayerIntent>> _getLegalIntents;
        private readonly Func<PlayCardIntent, RuleResult> _submitPlayIntent;
        private readonly List<CardInteractionIntent> _intentHistory = new List<CardInteractionIntent>();
        private readonly IReadOnlyList<CardInteractionIntent> _intentHistoryView;
        private bool _isTemporarilyBlocked;

        public CardInteractionSession(MatchSession matchSession, PlayerId localPlayerId)
            : this(
                localPlayerId,
                () => matchSession?.State,
                playerId => matchSession?.GetLegalIntents(playerId),
                intent => matchSession?.Submit(intent))
        {
            if (matchSession == null)
            {
                throw new ArgumentNullException(nameof(matchSession));
            }
        }

        public CardInteractionSession(
            PlayerId localPlayerId,
            Func<MatchState> getMatchState,
            Func<PlayerId, IReadOnlyList<PlayerIntent>> getLegalIntents,
            Func<PlayCardIntent, RuleResult> submitPlayIntent)
        {
            _getMatchState = getMatchState ?? throw new ArgumentNullException(nameof(getMatchState));
            _getLegalIntents = getLegalIntents ?? throw new ArgumentNullException(nameof(getLegalIntents));
            _submitPlayIntent = submitPlayIntent ?? throw new ArgumentNullException(nameof(submitPlayIntent));
            LocalPlayerId = localPlayerId;
            _intentHistoryView = _intentHistory.AsReadOnly();
            State = new CardInteractionState(
                null,
                null,
                null,
                CardInteractionFeedback.Legal,
                CardInteractionFeedbackReason.None,
                1);
        }

        public PlayerId LocalPlayerId { get; }

        public MatchState MatchState => _getMatchState();

        public CardInteractionState State { get; private set; }

        public IReadOnlyList<CardInteractionIntent> IntentHistory => _intentHistoryView;

        public bool IsTemporarilyBlocked => _isTemporarilyBlocked;

        public bool IsCardLegal(Card card)
        {
            foreach (var legalIntent in _getLegalIntents(LocalPlayerId))
            {
                if (legalIntent is PlayCardIntent playCard && playCard.Card == card)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetTemporarilyBlocked(bool isBlocked)
        {
            if (_isTemporarilyBlocked == isBlocked)
            {
                return;
            }

            _isTemporarilyBlocked = isBlocked;
            var feedback = isBlocked
                ? CardInteractionFeedback.TemporarilyBlocked
                : State.SelectedCard.HasValue
                    ? CardInteractionFeedback.Selected
                    : CardInteractionFeedback.Legal;
            var reason = isBlocked
                ? CardInteractionFeedbackReason.PresentationBusy
                : State.SelectedCard.HasValue
                    ? CardInteractionFeedbackReason.CardSelected
                    : CardInteractionFeedbackReason.None;
            SetState(
                State.InspectedCard,
                State.SelectedCard,
                State.SelectedCard,
                feedback,
                reason);
        }

        public CardInteractionResult Submit(CardInteractionIntent intent)
        {
            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            _intentHistory.Add(intent);

            if (intent.PlayerId != LocalPlayerId)
            {
                return Reject(null, CardInteractionFeedbackReason.DifferentPlayer);
            }

            if (intent is CancelCardInteractionIntent)
            {
                SetState(
                    null,
                    null,
                    null,
                    CardInteractionFeedback.Cancelled,
                    CardInteractionFeedbackReason.SelectionCancelled);
                return new CardInteractionResult(true, State);
            }

            if (_isTemporarilyBlocked)
            {
                var blockedCard = (intent as CardTargetInteractionIntent)?.Card ?? State.SelectedCard;
                SetState(
                    State.InspectedCard,
                    State.SelectedCard,
                    blockedCard,
                    CardInteractionFeedback.TemporarilyBlocked,
                    CardInteractionFeedbackReason.PresentationBusy);
                return new CardInteractionResult(false, State);
            }

            if (intent is InspectCardInteractionIntent inspect)
            {
                if (!IsCardLegal(inspect.Card))
                {
                    return Reject(inspect.Card, CardInteractionFeedbackReason.CardUnavailable);
                }

                SetState(
                    inspect.Card,
                    State.SelectedCard,
                    inspect.Card,
                    State.SelectedCard == inspect.Card
                        ? CardInteractionFeedback.Selected
                        : CardInteractionFeedback.Inspected,
                    CardInteractionFeedbackReason.CardInspected);
                return new CardInteractionResult(true, State);
            }

            if (intent is SelectCardInteractionIntent select)
            {
                if (!IsCardLegal(select.Card))
                {
                    return Reject(select.Card, CardInteractionFeedbackReason.CardUnavailable);
                }

                SetState(
                    State.InspectedCard,
                    select.Card,
                    select.Card,
                    CardInteractionFeedback.Selected,
                    CardInteractionFeedbackReason.CardSelected);
                return new CardInteractionResult(true, State);
            }

            if (intent is ConfirmCardInteractionIntent)
            {
                if (!State.SelectedCard.HasValue)
                {
                    return Reject(null, CardInteractionFeedbackReason.NoSelection);
                }

                var confirmedPlay = new PlayCardInteractionIntent(LocalPlayerId, State.SelectedCard.Value);
                _intentHistory.Add(confirmedPlay);
                return SubmitPlay(confirmedPlay);
            }

            if (intent is PlayCardInteractionIntent play)
            {
                return SubmitPlay(play);
            }

            throw new ArgumentOutOfRangeException(nameof(intent), intent.GetType().Name, null);
        }

        private CardInteractionResult SubmitPlay(PlayCardInteractionIntent intent)
        {
            if (!State.SelectedCard.HasValue || State.SelectedCard.Value != intent.Card)
            {
                return Reject(intent.Card, CardInteractionFeedbackReason.CardUnavailable);
            }

            var ruleResult = _submitPlayIntent(new PlayCardIntent(intent.PlayerId, intent.Card));
            if (!ruleResult.IsAccepted)
            {
                SetState(
                    State.InspectedCard,
                    State.SelectedCard,
                    intent.Card,
                    CardInteractionFeedback.Rejected,
                    CardInteractionFeedbackReason.DomainRejected);
                return new CardInteractionResult(false, State, ruleResult);
            }

            SetState(
                null,
                null,
                intent.Card,
                CardInteractionFeedback.Confirmed,
                CardInteractionFeedbackReason.CardPlayed);
            return new CardInteractionResult(true, State, ruleResult);
        }

        private CardInteractionResult Reject(Card? card, CardInteractionFeedbackReason reason)
        {
            SetState(
                State.InspectedCard,
                State.SelectedCard,
                card,
                CardInteractionFeedback.Rejected,
                reason);
            return new CardInteractionResult(false, State);
        }

        private void SetState(
            Card? inspectedCard,
            Card? selectedCard,
            Card? feedbackCard,
            CardInteractionFeedback feedback,
            CardInteractionFeedbackReason feedbackReason)
        {
            State = new CardInteractionState(
                inspectedCard,
                selectedCard,
                feedbackCard,
                feedback,
                feedbackReason,
                State.Revision + 1);
        }
    }
}
