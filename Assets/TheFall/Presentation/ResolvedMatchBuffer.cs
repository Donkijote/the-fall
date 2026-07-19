using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Presentation
{
    /// <summary>
    /// Holds facts already resolved by the domain for presentation sequencing.
    /// It deliberately contains no rule evaluation.
    /// </summary>
    public sealed class ResolvedMatchBuffer
    {
        private IReadOnlyList<DomainEvent> _events = Array.Empty<DomainEvent>();

        public MatchState State { get; private set; }

        public IReadOnlyList<DomainEvent> Events => _events;

        public void Consume(RuleResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            State = result.State;
            _events = result.Events;
        }
    }
}
