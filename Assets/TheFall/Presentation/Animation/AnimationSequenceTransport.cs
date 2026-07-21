using System;
using System.Collections.Generic;

namespace TheFall.Presentation.Animation
{
    public readonly struct AnimationBeatTiming
    {
        public AnimationBeatTiming(float delaySeconds, float durationSeconds)
        {
            DelaySeconds = Math.Max(0f, delaySeconds);
            DurationSeconds = Math.Max(0f, durationSeconds);
        }

        public float DelaySeconds { get; }

        public float DurationSeconds { get; }

        public float TotalSeconds => DelaySeconds + DurationSeconds;
    }

    public readonly struct AnimationTransportPosition
    {
        public AnimationTransportPosition(int stepIndex, float progress, bool isDelaying)
        {
            StepIndex = stepIndex;
            Progress = progress;
            IsDelaying = isDelaying;
        }

        public int StepIndex { get; }

        public float Progress { get; }

        public bool IsDelaying { get; }
    }

    /// <summary>
    /// Deterministic, frame-source-independent transport for a composed presentation sequence.
    /// It owns elapsed presentation time only and never touches a MatchState or submits an intent.
    /// </summary>
    public sealed class AnimationSequenceTransport
    {
        private readonly IReadOnlyList<AnimationBeatTiming> _timings;
        private readonly float[] _stepEndTimes;

        public AnimationSequenceTransport(IReadOnlyList<AnimationBeatTiming> timings)
        {
            _timings = timings ?? throw new ArgumentNullException(nameof(timings));
            _stepEndTimes = new float[timings.Count];
            var elapsed = 0f;
            for (var index = 0; index < timings.Count; index++)
            {
                elapsed += timings[index].TotalSeconds;
                _stepEndTimes[index] = elapsed;
            }

            DurationSeconds = elapsed;
            PlaybackSpeed = 1f;
        }

        public float ElapsedSeconds { get; private set; }

        public float DurationSeconds { get; }

        public float NormalizedPosition => DurationSeconds <= 0f
            ? 1f
            : ElapsedSeconds / DurationSeconds;

        public float PlaybackSpeed { get; set; }

        public bool Loop { get; set; }

        public bool IsPlaying { get; private set; }

        public bool ReachedEnd => ElapsedSeconds >= DurationSeconds;

        public AnimationTransportPosition Position => Resolve(ElapsedSeconds);

        public void Play()
        {
            if (ReachedEnd && !Loop)
            {
                ElapsedSeconds = 0f;
            }

            IsPlaying = true;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Reset()
        {
            IsPlaying = false;
            ElapsedSeconds = 0f;
        }

        public void Restart()
        {
            ElapsedSeconds = 0f;
            IsPlaying = true;
        }

        public void SkipToEnd()
        {
            ElapsedSeconds = DurationSeconds;
            IsPlaying = false;
        }

        public void Seek(float elapsedSeconds)
        {
            ElapsedSeconds = Math.Max(0f, Math.Min(DurationSeconds, elapsedSeconds));
        }

        public void SeekNormalized(float normalizedPosition)
        {
            Seek(DurationSeconds * Math.Max(0f, Math.Min(1f, normalizedPosition)));
        }

        public void StepForward()
        {
            var position = Position;
            if (position.StepIndex >= _stepEndTimes.Length)
            {
                return;
            }

            IsPlaying = false;
            Seek(_stepEndTimes[position.StepIndex]);
        }

        public float GetStepStartSeconds(int stepIndex)
        {
            ValidateStepIndex(stepIndex);
            return stepIndex == 0 ? 0f : _stepEndTimes[stepIndex - 1];
        }

        public float GetStepMotionStartSeconds(int stepIndex)
        {
            ValidateStepIndex(stepIndex);
            return GetStepStartSeconds(stepIndex) + _timings[stepIndex].DelaySeconds;
        }

        public float GetStepEndSeconds(int stepIndex)
        {
            ValidateStepIndex(stepIndex);
            return _stepEndTimes[stepIndex];
        }

        public void SeekToStep(int stepIndex, float progress)
        {
            ValidateStepIndex(stepIndex);
            var timing = _timings[stepIndex];
            Seek(GetStepMotionStartSeconds(stepIndex) +
                timing.DurationSeconds * Math.Max(0f, Math.Min(1f, progress)));
        }

        public bool Tick(float unscaledDeltaSeconds)
        {
            if (!IsPlaying)
            {
                return false;
            }

            var previous = ElapsedSeconds;
            var advance = Math.Max(0f, unscaledDeltaSeconds) * Math.Max(0f, PlaybackSpeed);
            ElapsedSeconds += advance;
            if (ElapsedSeconds < DurationSeconds)
            {
                return previous != ElapsedSeconds;
            }

            if (Loop && DurationSeconds > 0f)
            {
                ElapsedSeconds %= DurationSeconds;
                return true;
            }

            ElapsedSeconds = DurationSeconds;
            IsPlaying = false;
            return previous != ElapsedSeconds;
        }

        private AnimationTransportPosition Resolve(float elapsedSeconds)
        {
            var cursor = 0f;
            for (var index = 0; index < _timings.Count; index++)
            {
                var timing = _timings[index];
                var delayEnd = cursor + timing.DelaySeconds;
                var stepEnd = delayEnd + timing.DurationSeconds;
                if (elapsedSeconds < delayEnd)
                {
                    return new AnimationTransportPosition(index, 0f, true);
                }

                if (elapsedSeconds < stepEnd ||
                    timing.DurationSeconds <= 0f && elapsedSeconds <= stepEnd)
                {
                    var progress = timing.DurationSeconds <= 0f
                        ? 1f
                        : (elapsedSeconds - delayEnd) / timing.DurationSeconds;
                    return new AnimationTransportPosition(index, Math.Max(0f, Math.Min(1f, progress)), false);
                }

                cursor = stepEnd;
            }

            return new AnimationTransportPosition(_timings.Count, 1f, false);
        }

        private void ValidateStepIndex(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _timings.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(stepIndex));
            }
        }
    }
}
