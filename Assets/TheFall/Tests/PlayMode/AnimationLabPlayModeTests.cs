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
            Assert.That(controller.AnimatableStepCount, Is.EqualTo(1));
            Assert.That(controller.Sequence.Steps[0].Kind, Is.EqualTo(ResolvedAnimationStepKind.CardPlay));
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
            controller.SetScenario(TheFall.Application.Animation.AnimationScenarioKind.NormalCapture);
            controller.ResetForTests(Seat.First, new Vector2Int(390, 844));
            controller.CompleteImmediatelyForTests();
            var firstActor = controller.FinalState.GetPlayerAt(Seat.First);

            Assert.That(controller.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.Portrait));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
            Assert.That(controller.RenderedState.GetCaptured(firstActor.Player.Id), Has.Count.EqualTo(2));

            controller.ResetForTests(Seat.Second, new Vector2Int(844, 390));
            controller.CompleteImmediatelyForTests();
            var secondActor = controller.FinalState.GetPlayerAt(Seat.Second);

            Assert.That(controller.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.WideLandscape));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
            Assert.That(controller.RenderedState.GetCaptured(secondActor.Player.Id), Has.Count.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CaptureMatchingPair_PlaysOntoTheMatchThenFlipsTheStackIntoTheCapturedPile()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.SetScenario(
                TheFall.Application.Animation.AnimationScenarioKind.NormalCapture);

            Assert.That(controller.AnimatableStepCount, Is.EqualTo(1));
            Assert.That(controller.Sequence.Steps[0].Kind, Is.EqualTo(ResolvedAnimationStepKind.NormalCapture));

            controller.SeekToStep(0, 0.01f);
            var played = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Captured Pair Card Two of Coins");
            var matching = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Captured Pair Card Two of Cups");
            var matchingStart = matching.localPosition;
            Assert.That(controller.CapturePairViewCount, Is.EqualTo(2));
            Assert.That(controller.FaceDownCapturePairViewCount, Is.Zero);
            Assert.That(controller.CapturePairFlipDegrees, Is.EqualTo(180f).Within(0.1f));
            Assert.That(controller.TryGetPrimaryMotion(out _), Is.True);

            controller.SeekToStep(0, 0.3f);
            matching = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Captured Pair Card Two of Cups");
            Assert.That(
                Vector3.Distance(matching.localPosition, matchingStart),
                Is.LessThan(0.0001f));

            controller.SeekToStep(0, 0.42f);
            played = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Captured Pair Card Two of Coins");
            matching = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Captured Pair Card Two of Cups");
            Assert.That(
                Vector2.Distance(
                    new Vector2(played.localPosition.x, played.localPosition.z),
                    new Vector2(matching.localPosition.x, matching.localPosition.z)),
                Is.LessThan(0.0001f));
            Assert.That(played.localPosition.y, Is.GreaterThan(matching.localPosition.y));
            Assert.That(controller.FaceDownCapturePairViewCount, Is.Zero);
            Assert.That(controller.CapturePairFlipDegrees, Is.EqualTo(180f).Within(0.1f));

            controller.SeekToStep(0, 0.75f);
            Assert.That(controller.FaceDownCapturePairViewCount, Is.EqualTo(2));
            Assert.That(controller.CapturePairFlipDegrees, Is.GreaterThan(270f));

            controller.CompleteImmediatelyForTests();
            Assert.That(controller.CapturePairViewCount, Is.Zero);
            Assert.That(controller.CapturedPileViewCount, Is.EqualTo(2));
            Assert.That(controller.RenderedState.Table, Has.Count.EqualTo(1));
            Assert.That(controller.RenderedState.GetCaptured(
                controller.FinalState.GetPlayerAt(controller.ActingSeat).Player.Id),
                Has.Count.EqualTo(2));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);

            var firstSeatPile = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .First(item => item.name == "Face-down Seat One Captured Card 1");
            Assert.That(firstSeatPile.localPosition.x, Is.LessThan(-0.4f));

            controller.SetActingSeat(Seat.Second);
            controller.CompleteImmediatelyForTests();
            var secondSeatPile = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .First(item => item.name == "Face-down Seat Two Captured Card 1");
            Assert.That(secondSeatPile.localPosition.x, Is.GreaterThan(0.4f));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator CascadeCapture_GrowsTheStackAtEachCardThenFlipsItOntoTheCapturedPile()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.SetScenario(
                TheFall.Application.Animation.AnimationScenarioKind.CascadeCapture);

            Assert.That(controller.AnimatableStepCount, Is.EqualTo(5));
            Assert.That(
                controller.Sequence.Steps.Take(5).Select(step => step.Kind),
                Is.EqualTo(new[]
                {
                    ResolvedAnimationStepKind.NormalCapture,
                    ResolvedAnimationStepKind.CascadeCapture,
                    ResolvedAnimationStepKind.CascadeCapture,
                    ResolvedAnimationStepKind.CascadeCapture,
                    ResolvedAnimationStepKind.CascadeCapture,
                }));

            controller.SeekToStep(0, 0.99f);
            var played = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Captured Pair Card Two of Coins");
            var matching = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Captured Pair Card Two of Cups");
            Assert.That(
                Vector2.Distance(
                    new Vector2(played.localPosition.x, played.localPosition.z),
                    new Vector2(matching.localPosition.x, matching.localPosition.z)),
                Is.LessThan(0.0001f));
            Assert.That(played.localPosition.y, Is.GreaterThan(matching.localPosition.y));
            Assert.That(controller.FaceDownCapturePairViewCount, Is.Zero);

            controller.SeekToStep(1, 0.5f);
            Assert.That(controller.CascadeStackViewCount, Is.EqualTo(3));
            Assert.That(controller.FaceDownCascadeStackViewCount, Is.Zero);
            Assert.That(controller.CascadeStackFlipDegrees, Is.EqualTo(180f).Within(0.1f));

            controller.SeekToStep(2, 0.5f);
            Assert.That(controller.CascadeStackViewCount, Is.EqualTo(4));
            Assert.That(controller.FaceDownCascadeStackViewCount, Is.Zero);
            Assert.That(controller.CascadeStackFlipDegrees, Is.EqualTo(180f).Within(0.1f));

            controller.SeekToStep(3, 0.75f);
            Assert.That(controller.CascadeStackViewCount, Is.EqualTo(5));
            Assert.That(controller.FaceDownCascadeStackViewCount, Is.Zero);
            Assert.That(controller.CascadeStackFlipDegrees, Is.EqualTo(180f).Within(0.1f));

            controller.SeekToStep(4, 0.75f);
            Assert.That(controller.CascadeStackViewCount, Is.EqualTo(5));
            Assert.That(controller.FaceDownCascadeStackViewCount, Is.EqualTo(5));
            Assert.That(controller.CascadeStackFlipDegrees, Is.GreaterThan(270f));

            controller.CompleteImmediatelyForTests();
            Assert.That(controller.CascadeStackViewCount, Is.Zero);
            Assert.That(controller.CapturedPileViewCount, Is.EqualTo(5));
            Assert.That(controller.RenderedState.Table, Has.Count.EqualTo(1));
            Assert.That(controller.RenderedState.GetCaptured(
                controller.FinalState.GetPlayerAt(controller.ActingSeat).Player.Id),
                Has.Count.EqualTo(5));
            var pile = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .First(item => item.name == "Face-down Seat One Captured Card 1");
            Assert.That(pile.localPosition.x, Is.LessThan(-0.4f));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator DealerSelection_ShowsTheWholeFaceDownSpreadAndFlipsTheSelectedCard()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.SetScenario(
                TheFall.Application.Animation.AnimationScenarioKind.DealerCardSelection);

            controller.SeekToStep(0, 0f);
            Assert.That(controller.DealerSpreadViewCount, Is.EqualTo(40));
            Assert.That(controller.RevealedDealerCardViewCount, Is.Zero);
            Assert.That(controller.DealerCardFlipDegrees, Is.Zero);

            controller.SeekToStep(0, 0.5f);
            Assert.That(controller.DealerSpreadViewCount, Is.EqualTo(40));
            Assert.That(controller.DealerCardFlipDegrees, Is.EqualTo(90f).Within(0.1f));

            controller.SeekToStep(0, 0.75f);
            Assert.That(controller.DealerSpreadViewCount, Is.EqualTo(40));
            Assert.That(controller.RevealedDealerCardViewCount, Is.EqualTo(1));
            Assert.That(controller.DealerCardFlipDegrees, Is.GreaterThan(90f));

            controller.CompleteImmediatelyForTests();
            Assert.That(controller.DealerSpreadViewCount, Is.EqualTo(40));
            Assert.That(controller.RevealedDealerCardViewCount, Is.EqualTo(1));
            Assert.That(controller.RevealedDealerCardClearance, Is.GreaterThan(0f));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator DealOneCard_ShowsTheDeckDealsFaceUpThenFaceDownAndKeepsTheHandStill()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.SetScenario(
                TheFall.Application.Animation.AnimationScenarioKind.DealCard);

            controller.SeekToStep(0, 0.01f);
            Assert.That(controller.AnimatableStepCount, Is.EqualTo(2));
            Assert.That(controller.DeckViewCount, Is.EqualTo(36));
            Assert.That(controller.OpponentHandViewCount, Is.EqualTo(2));
            Assert.That(controller.ActiveDealCardIsFaceUp, Is.False);
            Assert.That(controller.TryGetPrimaryMotion(out _), Is.True);
            var handOne = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Seven of Clubs");
            var handTwo = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Twelve of Swords");
            var handOneStart = handOne.localPosition;
            var handTwoStart = handTwo.localPosition;

            controller.SeekToStep(0, 0.75f);
            handOne = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Seven of Clubs");
            handTwo = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Twelve of Swords");
            Assert.That(Vector3.Distance(handOne.localPosition, handOneStart), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(handTwo.localPosition, handTwoStart), Is.LessThan(0.0001f));
            Assert.That(controller.ActiveDealCardIsFaceUp, Is.True);
            Assert.That(controller.DealCardFlipDegrees, Is.GreaterThan(90f));

            controller.SeekToStep(1, 0.75f);
            handOne = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Seven of Clubs");
            handTwo = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Twelve of Swords");
            Assert.That(Vector3.Distance(handOne.localPosition, handOneStart), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(handTwo.localPosition, handTwoStart), Is.LessThan(0.0001f));
            Assert.That(controller.DeckViewCount, Is.EqualTo(35));
            Assert.That(controller.OpponentHandViewCount, Is.EqualTo(2));
            Assert.That(controller.ActiveDealCardIsFaceUp, Is.False);
            Assert.That(controller.DealCardFlipDegrees, Is.Zero);

            controller.CompleteImmediatelyForTests();
            Assert.That(controller.DeckViewCount, Is.EqualTo(34));
            Assert.That(controller.OpponentHandViewCount, Is.EqualTo(3));
            Assert.That(controller.RenderedState.GetHand(
                controller.FinalState.GetPlayerAt(controller.ActingSeat).Player.Id), Has.Count.EqualTo(3));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator OpeningPlacement_PicksTheFaceDownDeckTopAndFlipsItOntoTheTable()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.SetScenario(
                TheFall.Application.Animation.AnimationScenarioKind.OpeningPlacement);

            controller.SeekToStep(0, 0.01f);
            Assert.That(controller.DeckViewCount, Is.EqualTo(39));
            Assert.That(controller.ActiveDeckCardIsFaceUp, Is.False);
            Assert.That(controller.DeckCardFlipDegrees, Is.LessThan(1f));
            Assert.That(controller.TryGetPrimaryMotion(out _), Is.True);
            var existingTableCard = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Ten of Swords");
            var existingTablePosition = existingTableCard.localPosition;

            controller.SeekToStep(0, 0.75f);
            existingTableCard = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Ten of Swords");
            Assert.That(
                Vector3.Distance(existingTableCard.localPosition, existingTablePosition),
                Is.LessThan(0.0001f));
            Assert.That(controller.DeckViewCount, Is.EqualTo(39));
            Assert.That(controller.ActiveDeckCardIsFaceUp, Is.True);
            Assert.That(controller.DeckCardFlipDegrees, Is.GreaterThan(90f));

            controller.CompleteImmediatelyForTests();
            Assert.That(controller.DeckViewCount, Is.EqualTo(38));
            Assert.That(controller.RenderedState.Table, Has.Count.EqualTo(2));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator OpeningRejection_FlipsTheTableCardDownAndInsertsItIntoTheDeckMiddle()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.SetScenario(
                TheFall.Application.Animation.AnimationScenarioKind.OpeningRejection);

            controller.SeekToStep(0, 0.01f);
            Assert.That(controller.DeckViewCount, Is.EqualTo(38));
            Assert.That(controller.ActiveRejectedCardIsFaceDown, Is.False);
            Assert.That(controller.RejectedCardFlipDegrees, Is.EqualTo(180f).Within(1f));
            Assert.That(controller.TryGetPrimaryMotion(out _), Is.True);
            var acceptedTableCard = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Ten of Swords");
            var acceptedTablePosition = acceptedTableCard.localPosition;

            controller.SeekToStep(0, 0.5f);
            Assert.That(controller.RejectionDeckGap, Is.GreaterThan(0.9f));
            Assert.That(controller.RejectedCardFlipDegrees, Is.EqualTo(270f).Within(0.1f));

            controller.SeekToStep(0, 0.75f);
            acceptedTableCard = controller.PreviewRoot
                .GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Ten of Swords");
            Assert.That(
                Vector3.Distance(acceptedTableCard.localPosition, acceptedTablePosition),
                Is.LessThan(0.0001f));
            Assert.That(controller.ActiveRejectedCardIsFaceDown, Is.True);
            Assert.That(controller.RejectedCardFlipDegrees, Is.GreaterThan(270f));

            controller.CompleteImmediatelyForTests();
            Assert.That(controller.DeckViewCount, Is.EqualTo(39));
            Assert.That(controller.RenderedState.Table, Has.Count.EqualTo(1));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator FastForward_ProfilesTheRepresentativeSequenceWithoutChangingItsOutcome()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            controller.ResetForTests(Seat.First, new Vector2Int(1920, 1080));
            controller.SetFastForward(true);
            controller.PlayAnimation();
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
            controller.SetScenario(TheFall.Application.Animation.AnimationScenarioKind.TablePlacement);
            controller.SetActingSeat(Seat.Second);
            controller.SetPreviewProfile(AnimationPreviewProfile.Portrait);
            var playBeat = controller.WorkingConfiguration.GetBeat(ResolvedAnimationStepKind.CardPlay);
            playBeat.SetTiming(0f, 0f);
            controller.RestartSequence();
            yield return new WaitUntil(() => !controller.IsPlaying);

            Assert.That(controller.ScenarioKind,
                Is.EqualTo(TheFall.Application.Animation.AnimationScenarioKind.TablePlacement));
            Assert.That(controller.ActingSeat, Is.EqualTo(Seat.Second));
            Assert.That(controller.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.Portrait));
            Assert.That(controller.Sequence.Steps.Select(step => step.Kind), Is.EqualTo(new[]
            {
                ResolvedAnimationStepKind.TablePlacement,
                ResolvedAnimationStepKind.SynchronizeFinalState,
            }));
            Assert.That(controller.IsRenderedStateSynchronized, Is.True);
        }

        [UnityTest]
        public IEnumerator EveryRecordedAnimation_PreviewsExpectedBeatsForBothSeats()
        {
            yield return SceneManager.LoadSceneAsync("AnimationLab", LoadSceneMode.Single);

            var controller = Object.FindAnyObjectByType<AnimationLabController>();
            var scenarios = (TheFall.Application.Animation.AnimationScenarioKind[])
                System.Enum.GetValues(typeof(TheFall.Application.Animation.AnimationScenarioKind));
            foreach (var scenario in scenarios)
            {
                controller.SetScenario(scenario);
                foreach (var seat in new[] { Seat.First, Seat.Second })
                {
                    controller.SetActingSeat(seat);
                    controller.SeekToStep(0, 0.5f);
                    var recording = TheFall.Application.Animation.AnimationScenarioRecording.Create(
                        scenario,
                        seat);
                    var expectedSequence = ResolvedAnimationSequence.Create(
                        recording.Result.Events,
                        recording.Result.State,
                        recording.PreviewBeats
                            .Select(beat => (ResolvedAnimationStepKind)(int)beat)
                            .ToArray());
                    var expectedStepCount = expectedSequence.Steps.Count - 1;
                    Assert.That(
                        controller.AnimatableStepCount,
                        Is.EqualTo(expectedStepCount),
                        scenario.ToString());
                    Assert.That(controller.ActiveStep, Is.Not.Null, scenario.ToString());
                    Assert.That(
                        controller.ActiveStep.Kind,
                        Is.EqualTo((ResolvedAnimationStepKind)(int)recording.PreviewBeats[0]),
                        scenario.ToString());
                    if (scenario == TheFall.Application.Animation.AnimationScenarioKind.DealCard
                        || scenario == TheFall.Application.Animation.AnimationScenarioKind.PlayCard
                        || scenario == TheFall.Application.Animation.AnimationScenarioKind.HandReflow)
                    {
                        Assert.That(
                            controller.TryGetPrimaryMotion(out _),
                            Is.True,
                            $"{scenario} must expose its own adjustable motion path.");
                    }

                    controller.CompleteImmediatelyForTests();
                    Assert.That(controller.IsRenderedStateSynchronized, Is.True, $"{scenario} · {seat}");
                }
            }
        }
    }
}
