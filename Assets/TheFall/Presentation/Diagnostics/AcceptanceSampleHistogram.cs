using System;

namespace TheFall.Presentation.Diagnostics
{
    /// <summary>
    /// Fixed-memory histogram for long-running acceptance samples. Percentiles use the
    /// upper edge of the selected bucket so reported values remain conservative.
    /// </summary>
    public sealed class AcceptanceSampleHistogram
    {
        private const double BucketWidthMilliseconds = 0.05d;
        private const int BucketCount = 40000;

        private readonly long[] _buckets = new long[BucketCount];
        private double _sumMilliseconds;

        public long Count { get; private set; }

        public double MaximumMilliseconds { get; private set; }

        public long OverOneHundredMillisecondsCount { get; private set; }

        public double MeanMilliseconds =>
            Count == 0 ? 0d : _sumMilliseconds / Count;

        public void Add(double milliseconds)
        {
            if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) || milliseconds < 0d)
            {
                return;
            }

            var bucket = Math.Min(
                BucketCount - 1,
                (int)Math.Floor(milliseconds / BucketWidthMilliseconds));
            _buckets[bucket]++;
            Count++;
            _sumMilliseconds += milliseconds;
            MaximumMilliseconds = Math.Max(MaximumMilliseconds, milliseconds);
            if (milliseconds > 100d)
            {
                OverOneHundredMillisecondsCount++;
            }
        }

        public double Percentile(double percentile)
        {
            if (Count == 0)
            {
                return 0d;
            }

            var clamped = Math.Max(0d, Math.Min(1d, percentile));
            var target = Math.Max(1L, (long)Math.Ceiling(Count * clamped));
            long observed = 0;
            for (var index = 0; index < _buckets.Length; index++)
            {
                observed += _buckets[index];
                if (observed >= target)
                {
                    return Math.Min(
                        MaximumMilliseconds,
                        (index + 1) * BucketWidthMilliseconds);
                }
            }

            return MaximumMilliseconds;
        }
    }
}
