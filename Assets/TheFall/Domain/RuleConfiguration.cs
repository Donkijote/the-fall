namespace TheFall.Domain
{
    public sealed class RuleConfiguration
    {
        public const int StandardVictoryTarget = 24;

        public RuleConfiguration(
            bool casaCantosEnabled = true,
            bool trivilinWinsImmediately = false)
        {
            VictoryTarget = StandardVictoryTarget;
            CasaCantosEnabled = casaCantosEnabled;
            TrivilinWinsImmediately = trivilinWinsImmediately;
        }

        public int VictoryTarget { get; }

        public bool CasaCantosEnabled { get; }

        public bool TrivilinWinsImmediately { get; }

        public static RuleConfiguration Standard { get; } = new RuleConfiguration();
    }
}
