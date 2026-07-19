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
        public MatchSession(MatchState initialState)
        {
            State = initialState ?? throw new ArgumentNullException(nameof(initialState));
        }

        public MatchState State { get; private set; }

        public IReadOnlyList<PlayerIntent> GetLegalIntents(PlayerId playerId)
        {
            return OneVersusOneRules.GetLegalIntents(State, playerId);
        }

        public RuleResult Submit(PlayerIntent intent)
        {
            var result = OneVersusOneRules.Resolve(State, intent);
            State = result.State;
            return result;
        }
    }
}
