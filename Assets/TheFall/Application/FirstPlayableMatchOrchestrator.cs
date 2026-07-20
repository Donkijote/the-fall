using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Application
{
    public enum IntentActor
    {
        Human,
        Bot,
    }

    public sealed class IntentResolutionRecord
    {
        private readonly IReadOnlyList<DomainEvent> _events;

        internal IntentResolutionRecord(
            int sequence,
            IntentActor actor,
            MatchState priorState,
            PlayerIntent intent,
            RuleResult result)
        {
            Sequence = sequence;
            Actor = actor;
            PriorState = priorState ?? throw new ArgumentNullException(nameof(priorState));
            Intent = intent ?? throw new ArgumentNullException(nameof(intent));
            ResultingState = result?.State ?? throw new ArgumentNullException(nameof(result));
            IsAccepted = result.IsAccepted;
            Error = result.Error;
            var events = new DomainEvent[result.Events.Count];
            for (var index = 0; index < result.Events.Count; index++)
            {
                events[index] = result.Events[index];
            }

            _events = Array.AsReadOnly(events);
        }

        public int Sequence { get; }

        public IntentActor Actor { get; }

        public MatchState PriorState { get; }

        public PlayerIntent Intent { get; }

        public bool IsAccepted { get; }

        public RuleError Error { get; }

        public MatchState ResultingState { get; }

        public IReadOnlyList<DomainEvent> Events => _events;
    }

    public sealed class MatchTrace
    {
        private readonly List<IntentResolutionRecord> _intentHistory = new List<IntentResolutionRecord>();
        private readonly List<DomainEvent> _events;
        private readonly IReadOnlyList<IntentResolutionRecord> _readOnlyIntentHistory;
        private readonly IReadOnlyList<DomainEvent> _readOnlyEvents;

        internal MatchTrace(int seed, MatchState initialState, IReadOnlyList<DomainEvent> startupEvents)
        {
            Seed = seed;
            InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            FinalState = initialState;
            _events = new List<DomainEvent>(startupEvents ?? throw new ArgumentNullException(nameof(startupEvents)));
            _readOnlyIntentHistory = _intentHistory.AsReadOnly();
            _readOnlyEvents = _events.AsReadOnly();
        }

        public int Seed { get; }

        public MatchState InitialState { get; }

        public MatchState FinalState { get; private set; }

        public IReadOnlyList<IntentResolutionRecord> IntentHistory => _readOnlyIntentHistory;

        public IReadOnlyList<DomainEvent> Events => _readOnlyEvents;

        internal void Append(IntentResolutionRecord record)
        {
            _intentHistory.Add(record);
            for (var index = 0; index < record.Events.Count; index++)
            {
                _events.Add(record.Events[index]);
            }

            FinalState = record.ResultingState;
        }
    }

    public sealed class MatchAdvanceResult
    {
        private readonly IReadOnlyList<IntentResolutionRecord> _resolutions;

        internal MatchAdvanceResult(RuleResult humanResult, IReadOnlyList<IntentResolutionRecord> resolutions)
        {
            HumanResult = humanResult ?? throw new ArgumentNullException(nameof(humanResult));
            var copy = new IntentResolutionRecord[resolutions.Count];
            for (var index = 0; index < resolutions.Count; index++)
            {
                copy[index] = resolutions[index];
            }

            _resolutions = Array.AsReadOnly(copy);
        }

        public RuleResult HumanResult { get; }

        public IReadOnlyList<IntentResolutionRecord> Resolutions => _resolutions;
    }

    public sealed class MatchOrchestrationException : InvalidOperationException
    {
        internal MatchOrchestrationException(string message, MatchTrace trace)
            : base(message)
        {
            Trace = trace ?? throw new ArgumentNullException(nameof(trace));
        }

        public MatchTrace Trace { get; }
    }

    /// <summary>
    /// Human-facing application session that resolves human intents and automatically supplies
    /// every opponent choice through the same domain intent surface.
    /// </summary>
    public sealed class FirstPlayableMatchOrchestrator
    {
        private const int MaximumAutomaticBotIntents = 16;

        private readonly MatchSession _session;
        private readonly BaselineBot _bot;
        private readonly PlayerId _humanPlayerId;
        private readonly PlayerId _botPlayerId;

        public FirstPlayableMatchOrchestrator(
            int seed,
            Player human,
            Player bot,
            RuleConfiguration rules,
            IRandomSource matchRandomSource,
            IRandomSource botRandomSource)
        {
            ValidatePlayers(human, bot);
            _humanPlayerId = human.Id;
            _botPlayerId = bot.Id;
            _bot = new BaselineBot(botRandomSource);
            _session = new MatchSession(human, bot, rules ?? RuleConfiguration.Standard, matchRandomSource);
            Trace = new MatchTrace(seed, _session.State, _session.StartupEvents);
            AdvanceBotTurns();
        }

        public MatchState State => _session.State;

        public MatchTrace Trace { get; }

        public IReadOnlyList<PlayerIntent> GetHumanLegalIntents()
        {
            return _session.GetLegalIntents(_humanPlayerId);
        }

        public MatchAdvanceResult SubmitHumanIntent(PlayerIntent intent)
        {
            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            if (intent.PlayerId != _humanPlayerId)
            {
                throw new ArgumentException("The human adapter may submit intents only for the human player.", nameof(intent));
            }

            var resolutions = new List<IntentResolutionRecord>();
            var humanResult = SubmitRecorded(IntentActor.Human, intent, resolutions);
            if (humanResult.IsAccepted)
            {
                AdvanceBotTurns(resolutions);
            }

            return new MatchAdvanceResult(humanResult, resolutions);
        }

        private void AdvanceBotTurns(ICollection<IntentResolutionRecord> resolutions = null)
        {
            var automaticIntentCount = 0;
            while (_session.State.Phase != MatchPhase.Completed)
            {
                var activePlayer = _session.State.GetPlayerAt(_session.State.CurrentSeat).Player;
                if (activePlayer.Id == _humanPlayerId)
                {
                    return;
                }

                if (activePlayer.Id != _botPlayerId || activePlayer.Control != PlayerControl.Bot)
                {
                    throw CreateOrchestrationFailure("The active non-human seat is not owned by the configured bot.");
                }

                if (++automaticIntentCount > MaximumAutomaticBotIntents)
                {
                    throw CreateOrchestrationFailure("The bot exceeded the automatic-intent safety limit.");
                }

                var legalIntents = _session.GetLegalIntents(_botPlayerId);
                if (legalIntents.Count == 0)
                {
                    throw CreateOrchestrationFailure("The bot turn has no legal intent.");
                }

                var botState = _session.State.GetPlayer(_botPlayerId);
                var intent = _bot.SelectIntent(new BotTurnView(_session.State, botState, legalIntents));
                if (!ContainsReference(legalIntents, intent))
                {
                    throw CreateOrchestrationFailure("The bot selected an intent outside the shared legal-intent surface.");
                }

                var result = SubmitRecorded(IntentActor.Bot, intent, resolutions);
                if (!result.IsAccepted)
                {
                    throw CreateOrchestrationFailure($"The bot submitted an invalid intent: {result.Error}.");
                }
            }
        }

        private RuleResult SubmitRecorded(
            IntentActor actor,
            PlayerIntent intent,
            ICollection<IntentResolutionRecord> resolutions)
        {
            var priorState = _session.State;
            var result = _session.Submit(intent);
            var record = new IntentResolutionRecord(
                Trace.IntentHistory.Count + 1,
                actor,
                priorState,
                intent,
                result);
            Trace.Append(record);
            resolutions?.Add(record);
            return result;
        }

        private MatchOrchestrationException CreateOrchestrationFailure(string message)
        {
            return new MatchOrchestrationException(
                $"Seed {Trace.Seed}, intent count {Trace.IntentHistory.Count}, phase {_session.State.Phase}: {message}",
                Trace);
        }

        private static bool ContainsReference(IReadOnlyList<PlayerIntent> intents, PlayerIntent expected)
        {
            for (var index = 0; index < intents.Count; index++)
            {
                if (ReferenceEquals(intents[index], expected))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidatePlayers(Player human, Player bot)
        {
            if (human == null)
            {
                throw new ArgumentNullException(nameof(human));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (human.Control != PlayerControl.Human)
            {
                throw new ArgumentException("The human participant must use Human control.", nameof(human));
            }

            if (bot.Control != PlayerControl.Bot)
            {
                throw new ArgumentException("The bot participant must use Bot control.", nameof(bot));
            }

            if (human.Id == bot.Id)
            {
                throw new ArgumentException("The human and bot need distinct player identifiers.", nameof(bot));
            }
        }
    }
}
