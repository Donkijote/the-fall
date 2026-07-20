using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Application
{
    /// <summary>
    /// Shared application entry point for intents produced by human input or bots.
    /// </summary>
    public sealed class MatchSession
    {
        private readonly IRandomSource _randomSource;

        public MatchSession(MatchState initialState)
            : this(initialState, null)
        {
        }

        public MatchSession(MatchState initialState, IRandomSource randomSource)
        {
            State = initialState ?? throw new ArgumentNullException(nameof(initialState));
            _randomSource = randomSource;
            StartupEvents = Array.Empty<DomainEvent>();
        }

        public MatchSession(
            Player first,
            Player second,
            RuleConfiguration rules,
            IRandomSource randomSource)
        {
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
            var result = OneVersusOneRules.StartMatch(first, second, rules, randomSource);
            State = result.State;
            StartupEvents = result.Events;
        }

        public MatchState State { get; private set; }

        public IReadOnlyList<DomainEvent> StartupEvents { get; }

        public IReadOnlyList<PlayerIntent> GetLegalIntents(PlayerId playerId)
        {
            return OneVersusOneRules.GetLegalIntents(State, playerId);
        }

        public RuleResult Submit(PlayerIntent intent)
        {
            var result = OneVersusOneRules.Resolve(State, intent, _randomSource);
            State = result.State;
            return result;
        }
    }
}
