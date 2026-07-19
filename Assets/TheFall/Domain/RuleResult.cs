using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public sealed class RuleResult
    {
        private readonly IReadOnlyList<DomainEvent> _events;

        private RuleResult(bool isAccepted, RuleError error, MatchState state, IEnumerable<DomainEvent> events)
        {
            IsAccepted = isAccepted;
            Error = error;
            State = state ?? throw new ArgumentNullException(nameof(state));
            _events = Array.AsReadOnly(new List<DomainEvent>(events).ToArray());
        }

        public bool IsAccepted { get; }

        public RuleError Error { get; }

        public MatchState State { get; }

        public IReadOnlyList<DomainEvent> Events => _events;

        public static RuleResult Accepted(MatchState state, IEnumerable<DomainEvent> events)
        {
            return new RuleResult(true, RuleError.None, state, events);
        }

        public static RuleResult Rejected(MatchState unchangedState, RuleError error)
        {
            if (error == RuleError.None)
            {
                throw new ArgumentException("A rejected result needs an explicit error.", nameof(error));
            }

            return new RuleResult(false, error, unchangedState, Array.Empty<DomainEvent>());
        }
    }
}
