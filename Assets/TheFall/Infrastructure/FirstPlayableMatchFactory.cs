using TheFall.Application;
using TheFall.Domain;

namespace TheFall.Infrastructure
{
    /// <summary>
    /// Composes independent deterministic streams for rules and baseline-bot tie breaking.
    /// </summary>
    public static class FirstPlayableMatchFactory
    {
        private const int BotSeedSalt = unchecked((int)0x9E3779B9u);

        public static FirstPlayableMatchOrchestrator Create(
            int seed,
            Player human,
            Player bot,
            RuleConfiguration rules = null)
        {
            return new FirstPlayableMatchOrchestrator(
                seed,
                human,
                bot,
                rules ?? RuleConfiguration.Standard,
                new SeededRandomSource(seed),
                new SeededRandomSource(unchecked(seed ^ BotSeedSalt)));
        }
    }
}
