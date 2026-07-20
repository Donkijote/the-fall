using System;
using System.Collections.Generic;
using TheFall.Application.Interaction;
using TheFall.Domain;

namespace TheFall.Presentation.Input
{
    /// <summary>
    /// Maps platform gestures and controls into the same application interaction intents.
    /// Touch confirms by tapping an already-selected card; desktop can confirm with the keyboard.
    /// </summary>
    public sealed class CardInteractionInputAdapter
    {
        private readonly CardInteractionSession _session;
        private Card[] _cards = Array.Empty<Card>();
        private int _focusedIndex;

        public CardInteractionInputAdapter(CardInteractionSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public event Action<CardInteractionResult> ResultProduced;

        public Card? FocusedCard => _cards.Length == 0 ? (Card?)null : _cards[_focusedIndex];

        public void SetCards(IReadOnlyList<Card> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            _cards = new Card[cards.Count];
            for (var index = 0; index < cards.Count; index++)
            {
                _cards[index] = cards[index];
            }

            _focusedIndex = _cards.Length == 0 ? 0 : Math.Min(_focusedIndex, _cards.Length - 1);
        }

        public Card? Navigate(int direction)
        {
            if (_cards.Length == 0 || direction == 0)
            {
                return FocusedCard;
            }

            _focusedIndex = (_focusedIndex + Math.Sign(direction) + _cards.Length) % _cards.Length;
            return FocusedCard;
        }

        public CardInteractionResult TouchInspect(Card card)
        {
            Focus(card);
            return Submit(new InspectCardInteractionIntent(_session.LocalPlayerId, card));
        }

        public CardInteractionResult TouchTap(Card card)
        {
            Focus(card);
            return _session.State.SelectedCard == card
                ? Submit(new ConfirmCardInteractionIntent(_session.LocalPlayerId))
                : Submit(new SelectCardInteractionIntent(_session.LocalPlayerId, card));
        }

        public CardInteractionResult MouseInspect(Card card)
        {
            Focus(card);
            return Submit(new InspectCardInteractionIntent(_session.LocalPlayerId, card));
        }

        public CardInteractionResult MouseSelect(Card card)
        {
            Focus(card);
            return Submit(new SelectCardInteractionIntent(_session.LocalPlayerId, card));
        }

        public CardInteractionResult KeyboardInspect()
        {
            return FocusedCard.HasValue
                ? Submit(new InspectCardInteractionIntent(_session.LocalPlayerId, FocusedCard.Value))
                : Submit(new ConfirmCardInteractionIntent(_session.LocalPlayerId));
        }

        public CardInteractionResult KeyboardSelect()
        {
            return FocusedCard.HasValue
                ? Submit(new SelectCardInteractionIntent(_session.LocalPlayerId, FocusedCard.Value))
                : Submit(new ConfirmCardInteractionIntent(_session.LocalPlayerId));
        }

        public CardInteractionResult KeyboardConfirm()
        {
            return Submit(new ConfirmCardInteractionIntent(_session.LocalPlayerId));
        }

        public CardInteractionResult Cancel()
        {
            return Submit(new CancelCardInteractionIntent(_session.LocalPlayerId));
        }

        private void Focus(Card card)
        {
            for (var index = 0; index < _cards.Length; index++)
            {
                if (_cards[index] != card)
                {
                    continue;
                }

                _focusedIndex = index;
                return;
            }
        }

        private CardInteractionResult Submit(CardInteractionIntent intent)
        {
            var result = _session.Submit(intent);
            ResultProduced?.Invoke(result);
            return result;
        }
    }
}
