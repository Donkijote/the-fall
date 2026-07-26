using System.Linq;
using NUnit.Framework;
using TheFall.Presentation.Animation;
using TheFall.Presentation.Audio;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFall.Tests.EditMode
{
    public sealed class FirstPlayableAudioEditModeTests
    {
        private const string HomeScenePath = "Assets/TheFall/Presentation/Scenes/Home.unity";

        [Test]
        public void SemanticVocabularyMapsEveryRequiredResolvedBeatToADistinctDefinition()
        {
            AssertCue(ResolvedAnimationStepKind.Deal, PrototypeAudioCueKind.Deal);
            AssertCue(ResolvedAnimationStepKind.CardPlay, PrototypeAudioCueKind.Play);
            AssertCue(ResolvedAnimationStepKind.NormalCapture, PrototypeAudioCueKind.Capture);
            AssertCue(ResolvedAnimationStepKind.CascadeCapture, PrototypeAudioCueKind.Cascade);
            AssertCue(ResolvedAnimationStepKind.CaptureCollection, PrototypeAudioCueKind.Capture);
            AssertCue(ResolvedAnimationStepKind.FallScore, PrototypeAudioCueKind.Fall);
            AssertCue(ResolvedAnimationStepKind.CleanTableScore, PrototypeAudioCueKind.CleanTable);
            AssertCue(ResolvedAnimationStepKind.Canto, PrototypeAudioCueKind.Canto);
            AssertCue(ResolvedAnimationStepKind.Score, PrototypeAudioCueKind.Score);
            AssertCue(ResolvedAnimationStepKind.TurnChanged, PrototypeAudioCueKind.Transition);
            AssertCue(ResolvedAnimationStepKind.Round, PrototypeAudioCueKind.Transition);
            AssertCue(ResolvedAnimationStepKind.MatchCompleted, PrototypeAudioCueKind.Victory);

            var definitions = PrototypeAudioCueLibrary.Definitions;
            Assert.That(definitions, Has.Count.EqualTo(10));
            Assert.That(definitions.Select(item => item.Kind).Distinct().Count(), Is.EqualTo(10));
            Assert.That(
                definitions.Select(item =>
                        $"{item.PrimaryFrequencyHz:F0}:{item.SecondaryFrequencyHz:F0}:" +
                        $"{item.DurationSeconds:F3}:{item.PulseCount}:{item.NoiseBlend:F2}")
                    .Distinct()
                    .Count(),
                Is.EqualTo(10),
                "Every semantic cue must retain a distinct functional waveform fingerprint.");
            Assert.That(definitions.All(item => item.DurationSeconds <= 0.4f), Is.True);
        }

        [Test]
        public void HomeSceneOwnsOneSafeNonLoopingPrototypeEffectsSource()
        {
            var scene = EditorSceneManager.OpenScene(HomeScenePath);
            var presenter = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableAudioPresenter>(true))
                .Single();
            var source = presenter.GetComponent<AudioSource>();

            Assert.That(source, Is.Not.Null);
            Assert.That(source.playOnAwake, Is.False);
            Assert.That(source.loop, Is.False);
            Assert.That(source.spatialBlend, Is.Zero);
        }

        private static void AssertCue(
            ResolvedAnimationStepKind stepKind,
            PrototypeAudioCueKind expected)
        {
            Assert.That(PrototypeAudioCueLibrary.TryResolve(stepKind, out var actual), Is.True);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
