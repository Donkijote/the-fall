using TheFall.Domain;

namespace TheFall.Application.Interaction
{
    public enum CardInteractionFeedback
    {
        Legal,
        Inspected,
        Selected,
        Confirmed,
        Cancelled,
        Rejected,
        TemporarilyBlocked,
    }

    public enum CardInteractionFeedbackReason
    {
        None,
        CardInspected,
        CardSelected,
        CardPlayed,
        SelectionCancelled,
        CardUnavailable,
        NoSelection,
        DifferentPlayer,
        PresentationBusy,
        DomainRejected,
    }

    /// <summary>
    /// Immutable interaction-only state. It is deliberately separate from deterministic match state,
    /// so view recomposition can retain a valid selection without replaying a gameplay intent.
    /// </summary>
    public sealed class CardInteractionState
    {
        internal CardInteractionState(
            Card? inspectedCard,
            Card? selectedCard,
            Card? feedbackCard,
            CardInteractionFeedback feedback,
            CardInteractionFeedbackReason feedbackReason,
            int revision)
        {
            InspectedCard = inspectedCard;
            SelectedCard = selectedCard;
            FeedbackCard = feedbackCard;
            Feedback = feedback;
            FeedbackReason = feedbackReason;
            Revision = revision;
        }

        public Card? InspectedCard { get; }

        public Card? SelectedCard { get; }

        public Card? FeedbackCard { get; }

        public CardInteractionFeedback Feedback { get; }

        public CardInteractionFeedbackReason FeedbackReason { get; }

        public int Revision { get; }

        public string FeedbackLocalizationKey => GetFeedbackLocalizationKey(Feedback, FeedbackReason);

        private static string GetFeedbackLocalizationKey(
            CardInteractionFeedback feedback,
            CardInteractionFeedbackReason reason)
        {
            if (feedback == CardInteractionFeedback.Rejected)
            {
                switch (reason)
                {
                    case CardInteractionFeedbackReason.NoSelection:
                        return "interaction.feedback.no-selection";
                    case CardInteractionFeedbackReason.DifferentPlayer:
                        return "interaction.feedback.different-player";
                    case CardInteractionFeedbackReason.DomainRejected:
                        return "interaction.feedback.domain-rejected";
                    default:
                        return "interaction.feedback.card-unavailable";
                }
            }

            switch (feedback)
            {
                case CardInteractionFeedback.Inspected:
                    return "interaction.feedback.inspected";
                case CardInteractionFeedback.Selected:
                    return "interaction.feedback.selected";
                case CardInteractionFeedback.Confirmed:
                    return "interaction.feedback.confirmed";
                case CardInteractionFeedback.Cancelled:
                    return "interaction.feedback.cancelled";
                case CardInteractionFeedback.TemporarilyBlocked:
                    return "interaction.feedback.temporarily-blocked";
                default:
                    return "interaction.feedback.legal";
            }
        }
    }
}
