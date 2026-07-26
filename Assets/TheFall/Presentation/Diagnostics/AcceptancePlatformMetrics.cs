using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TheFall.Presentation.Diagnostics
{
    public static class AcceptancePlatformMetrics
    {
        public const int ThermalStateUnavailable = -1;
        public const int ThermalStateNominal = 0;
        public const int ThermalStateFair = 1;
        public const int ThermalStateSerious = 2;
        public const int ThermalStateCritical = 3;

        public static double ProcessUptimeSeconds()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                var seconds = TheFallAcceptanceProcessUptimeSeconds();
                if (seconds >= 0d)
                {
                    return seconds;
                }
            }
            catch
            {
                // Fall through to the managed path.
            }
#endif

            try
            {
                return (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime())
                    .TotalSeconds;
            }
            catch
            {
                return -1d;
            }
        }

        public static long AppMemoryBytes()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                var bytes = TheFallAcceptancePhysicalFootprintBytes();
                if (bytes > 0)
                {
                    return checked((long)bytes);
                }
            }
            catch
            {
                // Fall through to the managed path.
            }
#endif

            try
            {
                return Process.GetCurrentProcess().WorkingSet64;
            }
            catch
            {
                return 0L;
            }
        }

        public static int ThermalState()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                return TheFallAcceptanceThermalState();
            }
            catch
            {
                return ThermalStateUnavailable;
            }
#else
            return ThermalStateUnavailable;
#endif
        }

        public static string ThermalStateName(int state)
        {
            switch (state)
            {
                case ThermalStateNominal:
                    return "nominal";
                case ThermalStateFair:
                    return "fair";
                case ThermalStateSerious:
                    return "serious";
                case ThermalStateCritical:
                    return "critical";
                default:
                    return "unavailable";
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern double TheFallAcceptanceProcessUptimeSeconds();

        [DllImport("__Internal")]
        private static extern ulong TheFallAcceptancePhysicalFootprintBytes();

        [DllImport("__Internal")]
        private static extern int TheFallAcceptanceThermalState();
#endif
    }
}
