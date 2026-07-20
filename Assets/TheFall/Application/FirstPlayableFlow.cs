using System;
using TheFall.Domain;

namespace TheFall.Application
{
    public enum FirstPlayableFlowStage
    {
        Home,
        Setup,
        Loading,
        Match,
        Result,
    }

    public sealed class FirstPlayableSetup
    {
        public FirstPlayableSetup(
            bool casaCantosEnabled = true,
            bool trivilinWinsImmediately = false)
        {
            CasaCantosEnabled = casaCantosEnabled;
            TrivilinWinsImmediately = trivilinWinsImmediately;
        }

        public bool CasaCantosEnabled { get; }

        public bool TrivilinWinsImmediately { get; }

        public RuleConfiguration ToRuleConfiguration()
        {
            return new RuleConfiguration(CasaCantosEnabled, TrivilinWinsImmediately);
        }

        public static FirstPlayableSetup Default { get; } = new FirstPlayableSetup();
    }

    /// <summary>
    /// Owns the navigation boundary around one first-playable match. Invalid or repeated
    /// navigation is ignored, and every replay receives a new orchestration session.
    /// </summary>
    public sealed class FirstPlayableFlow
    {
        private readonly Func<int, RuleConfiguration, FirstPlayableMatchOrchestrator> _matchFactory;
        private readonly int _initialSeed;

        public FirstPlayableFlow(
            Func<int, RuleConfiguration, FirstPlayableMatchOrchestrator> matchFactory,
            int initialSeed = 2400)
        {
            _matchFactory = matchFactory ?? throw new ArgumentNullException(nameof(matchFactory));
            _initialSeed = initialSeed;
            Setup = FirstPlayableSetup.Default;
        }

        public FirstPlayableFlowStage Stage { get; private set; } = FirstPlayableFlowStage.Home;

        public FirstPlayableSetup Setup { get; private set; }

        public FirstPlayableMatchOrchestrator Match { get; private set; }

        public int SessionNumber { get; private set; }

        public bool TryOpenSetup()
        {
            if (Stage != FirstPlayableFlowStage.Home)
            {
                return false;
            }

            Setup = FirstPlayableSetup.Default;
            Stage = FirstPlayableFlowStage.Setup;
            return true;
        }

        public bool TryConfigure(bool casaCantosEnabled, bool trivilinWinsImmediately)
        {
            if (Stage != FirstPlayableFlowStage.Setup)
            {
                return false;
            }

            Setup = new FirstPlayableSetup(casaCantosEnabled, trivilinWinsImmediately);
            return true;
        }

        public bool TryStartMatch()
        {
            if (Stage != FirstPlayableFlowStage.Setup)
            {
                return false;
            }

            return CreateLoadingSession();
        }

        public bool TryFinishLoading(int expectedSessionNumber)
        {
            if (Stage != FirstPlayableFlowStage.Loading
                || Match == null
                || expectedSessionNumber != SessionNumber)
            {
                return false;
            }

            Stage = Match.State.Phase == MatchPhase.Completed
                ? FirstPlayableFlowStage.Result
                : FirstPlayableFlowStage.Match;
            return true;
        }

        public bool TrySubmitHumanIntent(PlayerIntent intent, out MatchAdvanceResult result)
        {
            result = null;
            if (Stage != FirstPlayableFlowStage.Match || Match == null || intent == null)
            {
                return false;
            }

            result = Match.SubmitHumanIntent(intent);
            if (Match.State.Phase == MatchPhase.Completed)
            {
                Stage = FirstPlayableFlowStage.Result;
            }

            return true;
        }

        public bool TryReplay()
        {
            if (Stage != FirstPlayableFlowStage.Result || Match == null)
            {
                return false;
            }

            return CreateLoadingSession();
        }

        public bool TryReturnHome()
        {
            if (Stage == FirstPlayableFlowStage.Home)
            {
                return false;
            }

            Match = null;
            Setup = FirstPlayableSetup.Default;
            Stage = FirstPlayableFlowStage.Home;
            return true;
        }

        private bool CreateLoadingSession()
        {
            var nextSessionNumber = checked(SessionNumber + 1);
            var seed = unchecked(_initialSeed + nextSessionNumber - 1);
            var match = _matchFactory(seed, Setup.ToRuleConfiguration());

            Match = match ?? throw new InvalidOperationException("The first-playable match factory returned no session.");
            SessionNumber = nextSessionNumber;
            Stage = FirstPlayableFlowStage.Loading;
            return true;
        }
    }
}
