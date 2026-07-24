using System;
using System.Collections.Generic;
using TheFall.Presentation.Animation;
using UnityEngine;

namespace TheFall.Presentation.Audio
{
    public enum PrototypeAudioCueKind
    {
        Deal,
        Play,
        Capture,
        Cascade,
        Fall,
        CleanTable,
        Canto,
        Score,
        Transition,
        Victory,
    }

    public sealed class PrototypeAudioCueDefinition
    {
        internal PrototypeAudioCueDefinition(
            PrototypeAudioCueKind kind,
            float primaryFrequencyHz,
            float secondaryFrequencyHz,
            float durationSeconds,
            int pulseCount,
            float noiseBlend,
            float gain)
        {
            Kind = kind;
            PrimaryFrequencyHz = primaryFrequencyHz;
            SecondaryFrequencyHz = secondaryFrequencyHz;
            DurationSeconds = durationSeconds;
            PulseCount = pulseCount;
            NoiseBlend = noiseBlend;
            Gain = gain;
        }

        public PrototypeAudioCueKind Kind { get; }

        public float PrimaryFrequencyHz { get; }

        public float SecondaryFrequencyHz { get; }

        public float DurationSeconds { get; }

        public int PulseCount { get; }

        public float NoiseBlend { get; }

        public float Gain { get; }
    }

    /// <summary>
    /// Project-authored functional cue definitions. The generated waveforms are presentation
    /// assets only and never influence animation timing, accepted intents, or rule state.
    /// </summary>
    public static class PrototypeAudioCueLibrary
    {
        public const int SampleRate = 44100;

        private static readonly IReadOnlyList<PrototypeAudioCueDefinition> CueDefinitions =
            Array.AsReadOnly(new[]
            {
                Cue(PrototypeAudioCueKind.Deal, 620f, 930f, 0.055f, 1, 0.08f, 0.22f),
                Cue(PrototypeAudioCueKind.Play, 260f, 170f, 0.080f, 1, 0.18f, 0.28f),
                Cue(PrototypeAudioCueKind.Capture, 440f, 660f, 0.120f, 2, 0.10f, 0.28f),
                Cue(PrototypeAudioCueKind.Cascade, 540f, 810f, 0.090f, 2, 0.06f, 0.24f),
                Cue(PrototypeAudioCueKind.Fall, 220f, 880f, 0.220f, 3, 0.04f, 0.30f),
                Cue(PrototypeAudioCueKind.CleanTable, 740f, 1110f, 0.240f, 3, 0.02f, 0.27f),
                Cue(PrototypeAudioCueKind.Canto, 392f, 784f, 0.260f, 2, 0.01f, 0.25f),
                Cue(PrototypeAudioCueKind.Score, 660f, 990f, 0.140f, 2, 0.03f, 0.23f),
                Cue(PrototypeAudioCueKind.Transition, 330f, 495f, 0.100f, 1, 0.04f, 0.18f),
                Cue(PrototypeAudioCueKind.Victory, 523f, 1046f, 0.380f, 4, 0.01f, 0.28f),
            });

        public static IReadOnlyList<PrototypeAudioCueDefinition> Definitions => CueDefinitions;

        public static PrototypeAudioCueDefinition Get(PrototypeAudioCueKind kind)
        {
            for (var index = 0; index < CueDefinitions.Count; index++)
            {
                if (CueDefinitions[index].Kind == kind)
                {
                    return CueDefinitions[index];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown prototype audio cue.");
        }

        public static bool TryResolve(
            ResolvedAnimationStepKind stepKind,
            out PrototypeAudioCueKind cueKind)
        {
            switch (stepKind)
            {
                case ResolvedAnimationStepKind.Deal:
                    cueKind = PrototypeAudioCueKind.Deal;
                    return true;
                case ResolvedAnimationStepKind.CardPlay:
                    cueKind = PrototypeAudioCueKind.Play;
                    return true;
                case ResolvedAnimationStepKind.NormalCapture:
                    cueKind = PrototypeAudioCueKind.Capture;
                    return true;
                case ResolvedAnimationStepKind.CascadeCapture:
                    cueKind = PrototypeAudioCueKind.Cascade;
                    return true;
                case ResolvedAnimationStepKind.FallScore:
                    cueKind = PrototypeAudioCueKind.Fall;
                    return true;
                case ResolvedAnimationStepKind.CleanTableScore:
                    cueKind = PrototypeAudioCueKind.CleanTable;
                    return true;
                case ResolvedAnimationStepKind.Canto:
                    cueKind = PrototypeAudioCueKind.Canto;
                    return true;
                case ResolvedAnimationStepKind.Score:
                    cueKind = PrototypeAudioCueKind.Score;
                    return true;
                case ResolvedAnimationStepKind.MatchStarted:
                case ResolvedAnimationStepKind.DealCompleted:
                case ResolvedAnimationStepKind.Round:
                case ResolvedAnimationStepKind.DealerRotation:
                case ResolvedAnimationStepKind.TieExtension:
                case ResolvedAnimationStepKind.TurnChanged:
                    cueKind = PrototypeAudioCueKind.Transition;
                    return true;
                case ResolvedAnimationStepKind.MatchCompleted:
                    cueKind = PrototypeAudioCueKind.Victory;
                    return true;
                default:
                    cueKind = default;
                    return false;
            }
        }

        public static AudioClip CreateClip(PrototypeAudioCueKind kind)
        {
            var definition = Get(kind);
            var sampleCount = Mathf.CeilToInt(definition.DurationSeconds * SampleRate);
            var samples = new float[sampleCount];
            for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                var time = sampleIndex / (float)SampleRate;
                var normalized = sampleIndex / (float)Math.Max(1, sampleCount - 1);
                var pulsePosition = Mathf.Repeat(normalized * definition.PulseCount, 1f);
                var pulseEnvelope = Mathf.Sin(Mathf.PI * pulsePosition);
                var overallEnvelope = Mathf.Sin(Mathf.PI * normalized);
                var tonal = Mathf.Sin(2f * Mathf.PI * definition.PrimaryFrequencyHz * time) * 0.72f
                    + Mathf.Sin(2f * Mathf.PI * definition.SecondaryFrequencyHz * time) * 0.28f;
                var noiseSeed = unchecked(sampleIndex * 1103515245 + ((int)kind + 1) * 12345);
                var noise = ((noiseSeed >> 16) & 0x7fff) / 16383.5f - 1f;
                samples[sampleIndex] = definition.Gain
                    * overallEnvelope
                    * pulseEnvelope
                    * Mathf.Lerp(tonal, noise, definition.NoiseBlend);
            }

            var clip = AudioClip.Create(
                $"Project-Owned Prototype {kind}",
                sampleCount,
                1,
                SampleRate,
                false);
            clip.hideFlags = HideFlags.HideAndDontSave;
            clip.SetData(samples, 0);
            return clip;
        }

        private static PrototypeAudioCueDefinition Cue(
            PrototypeAudioCueKind kind,
            float primaryFrequencyHz,
            float secondaryFrequencyHz,
            float durationSeconds,
            int pulseCount,
            float noiseBlend,
            float gain)
        {
            return new PrototypeAudioCueDefinition(
                kind,
                primaryFrequencyHz,
                secondaryFrequencyHz,
                durationSeconds,
                pulseCount,
                noiseBlend,
                gain);
        }
    }
}
