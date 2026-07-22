using System.Collections;
using System.Linq;
using NUnit.Framework;
using TheFall.Domain;
using TheFall.Presentation.Animation;
using TheFall.Presentation.Table;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheFall.Tests.PlayMode
{
    public sealed class AnimationLabPlayModeTests
    {
        [UnityTest]
        public IEnumerator RecordedSequence_CompletesAtTheResolvedStateWithoutMovingTheCamera()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            Assert.That(controller, Is.Not.Null);
            var cameraPosition = controller.GameplayCamera.transform.position;
            var cameraRotation = controller.GameplayCamera.transform.rotation;
            yield return new WaitUntil(() => !controller.IsPlaying);

            Assert.That(controller.ResolvedEvents, Is.Not.Empty);
            Assert.That(controller.CompletionReason, Is.EqualTo(AnimationSequenceCompletionReason.Completed));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
            Assert.That(controller.RenderedState.Table, Is.Empty);
            Assert.That(controller.CardViewCount, Is.EqualTo(6));
            Assert.That(controller.GameplayCamera.transform.position, Is.EqualTo(cameraPosition));
            Assert.That(controller.GameplayCamera.transform.rotation, Is.EqualTo(cameraRotation));
        }

        [UnityTest]
        public IEnumerator SkipInterruptAndCancel_AllSynchronizeTheResolvedEndState()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.ResetForTests(Seat.First, new Vector2Int(844, 390), true);
            yield return null;
            controller.SkipToEnd();
            Assert.That(controller.CompletionReason, Is.EqualTo(AnimationSequenceCompletionReason.Skipped));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);

            controller.ResetForTests(Seat.First, new Vector2Int(844, 390), true);
            yield return null;
            controller.InterruptAndSynchronize();
            Assert.That(controller.CompletionReason, Is.EqualTo(AnimationSequenceCompletionReason.Interrupted));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);

            controller.ResetForTests(Seat.First, new Vector2Int(844, 390), true);
            yield return null;
            controller.CancelAndSynchronize();
            Assert.That(controller.CompletionReason, Is.EqualTo(AnimationSequenceCompletionReason.Cancelled));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator BothSeatsAndOrientations_ProduceTheSameResolvedOutcomeShape()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.ResetForTests(Seat.First, new Vector2Int(390, 844));
            controller.CompleteImmediatelyForTests();
            var firstActor = controller.FinalState.GetPlayerAt(Seat.First);

            Assert.That(controller.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.Portrait));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
            Assert.That(controller.RenderedState.GetCaptured(firstActor.Player.Id), Has.Count.EqualTo(4));

            controller.ResetForTests(Seat.Second, new Vector2Int(844, 390));
            controller.CompleteImmediatelyForTests();
            var secondActor = controller.FinalState.GetPlayerAt(Seat.Second);

            Assert.That(controller.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.WideLandscape));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
            Assert.That(controller.RenderedState.GetCaptured(secondActor.Player.Id), Has.Count.EqualTo(4));
            Assert.That(controller.RenderedState.GetScore(secondActor.Player.TeamId).Value, Is.EqualTo(12));
        }

        [UnityTest]
        public IEnumerator FastForward_ProfilesTheRepresentativeSequenceWithoutChangingItsOutcome()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.ResetForTests(Seat.First, new Vector2Int(1920, 1080));
            controller.SetFastForward(true);
            controller.PlayRepresentativeSequence();
            yield return new WaitUntil(() => !controller.IsPlaying);

            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
            Assert.That(controller.LastSequenceElapsedSeconds, Is.GreaterThan(0f));
            Assert.That(controller.LastSequenceFrameCount, Is.GreaterThan(0));
            Assert.That(controller.LastSequenceCpuMilliseconds, Is.GreaterThan(0f));
            Assert.That(controller.LastSequencePeakUpdateCpuMilliseconds, Is.GreaterThan(0f));
            Debug.Log(
                $"AnimationLab profile: fast-forward sequence completed in {controller.LastSequenceElapsedSeconds * 1000f:F1} ms, used {controller.LastSequenceCpuMilliseconds:F2} ms measured presentation CPU with a {controller.LastSequencePeakUpdateCpuMilliseconds:F3} ms peak update, and rendered {controller.CardViewCount} card views across {controller.LastSequenceFrameCount} batch-runner updates.");
        }

        [UnityTest]
        public IEnumerator WorkbenchTransport_PausesStepsSeeksResetsAndResumesWithoutMutatingTheRecording()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.ResetForTests(Seat.First, new Vector2Int(1920, 1080), true);
            var authoritativeState = controller.FinalState;
            var sourceEvents = controller.ResolvedEvents;
            yield return null;

            controller.Pause();
            var pausedAt = controller.ElapsedSeconds;
            yield return null;
            Assert.That(controller.ElapsedSeconds, Is.EqualTo(pausedAt));

            controller.SingleStep();
            Assert.That(controller.ElapsedSeconds, Is.GreaterThan(pausedAt));
            controller.SeekNormalized(0.5f);
            Assert.That(controller.NormalizedPosition, Is.EqualTo(0.5f).Within(0.01f));
            controller.ResetToStart();
            Assert.That(controller.ElapsedSeconds, Is.Zero);
            Assert.That(controller.FinalState, Is.SameAs(authoritativeState));
            Assert.That(controller.ResolvedEvents, Is.SameAs(sourceEvents));

            controller.Resume();
            yield return new WaitUntil(() => !controller.IsPlaying);
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator ScenarioSeatProfileAndLivePresetChanges_RecomposeTheNextPreview()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.Pause();
            controller.LoadPreset(1);
            Assert.That(controller.WorkingConfiguration.PresetName, Is.EqualTo("Fast Iteration"));
            Assert.That(controller.WorkingConfiguration.PlaybackSpeed, Is.EqualTo(2f));
            controller.WorkingConfiguration.SetTransport(2f, false);
            controller.SetScenario(TheFall.Application.Animation.AnimationScenarioKind.NonCapturingPlacement);
            controller.SetActingSeat(Seat.Second);
            controller.SetPreviewProfile(AnimationPreviewProfile.Portrait);
            var playBeat = controller.WorkingConfiguration.GetBeat(ResolvedAnimationStepKind.CardPlay);
            playBeat.SetTiming(0f, 0f);
            controller.RestartSequence();
            yield return new WaitUntil(() => !controller.IsPlaying);

            Assert.That(controller.ScenarioKind,
                Is.EqualTo(TheFall.Application.Animation.AnimationScenarioKind.NonCapturingPlacement));
            Assert.That(controller.ActingSeat, Is.EqualTo(Seat.Second));
            Assert.That(controller.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.Portrait));
            Assert.That(controller.Sequence.Steps.Select(step => step.Kind), Is.EqualTo(new[]
            {
                ResolvedAnimationStepKind.CardPlay,
                ResolvedAnimationStepKind.TablePlacement,
                ResolvedAnimationStepKind.TurnChanged,
                ResolvedAnimationStepKind.SynchronizeFinalState,
            }));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }
    }
}
