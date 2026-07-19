using System;

namespace TheFall.Domain
{
    public sealed class RuleConfiguration
    {
        public RuleConfiguration(
            int victoryTarget = 24,
            bool casaCantosEnabled = true,
            bool trivilinWinsImmediately = false)
        {
            if (victoryTarget <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(victoryTarget));
            }

            VictoryTarget = victoryTarget;
            CasaCantosEnabled = casaCantosEnabled;
            TrivilinWinsImmediately = trivilinWinsImmediately;
        }

        public int VictoryTarget { get; }

        public bool CasaCantosEnabled { get; }

        public bool TrivilinWinsImmediately { get; }

        public static RuleConfiguration Standard { get; } = new RuleConfiguration();
    }
}
