using TheFall.Domain;

namespace TheFall.Application.Interaction
{
    public sealed class CardInteractionResult
    {
        internal CardInteractionResult(
            bool isAccepted,
            CardInteractionState state,
            RuleResult ruleResult = null)
        {
            IsAccepted = isAccepted;
            State = state;
            RuleResult = ruleResult;
        }

        public bool IsAccepted { get; }

        public CardInteractionState State { get; }

        public RuleResult RuleResult { get; }
    }
}
