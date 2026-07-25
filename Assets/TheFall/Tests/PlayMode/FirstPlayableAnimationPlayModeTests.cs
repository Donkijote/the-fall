using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Presentation.Animation;
using TheFall.Presentation.Audio;
using TheFall.Presentation.Bootstrap;
using TheFall.Presentation.Match;
using TheFall.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TheFall.Tests.PlayMode
{
    [SetUpFixture]
    public sealed class PlayModeAudioMuteFixture
    {
        private float _previousVolume;

        [OneTimeSetUp]
        public void MuteAudio()
        {
            _previousVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }

        [OneTimeTearDown]
        public void RestoreAudio()
        {
            AudioListener.volume = _previousVolume;
        }
    }

    public sealed class FirstPlayableAnimationPlayModeTests
    {
        [UnityTest]
        public IEnumerator RuntimePresentation_BlocksDuplicateInputAndEveryExitConverges()
        {
            yield return LoadMatchWithoutSettlingPresentation();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            var audio = table.AudioPresenter;
            var ui = controller.GetComponent<UIDocument>().rootVisualElement;

            Assert.That(table.AnimationPreset, Is.Not.Null);
            Assert.That(table.AnimationPreset.PresetVersion, Is.EqualTo(AnimationSequenceConfiguration.CurrentPresetVersion));
            Assert.That(table.IsPresentationBusy, Is.True);
            Assert.That(controller.IsPresentationBusy, Is.True);
            Assert.That(ui.Q<Toggle>("animation-fast-toggle"), Is.Not.Null);
            Assert.That(ui.Q<Toggle>("animation-reduced-toggle"), Is.Not.Null);
            Assert.That(ui.Q<Button>("animation-skip-button"), Is.Not.Null);
            Assert.That(ui.Q<Toggle>("audio-master-toggle"), Is.Not.Null);
            Assert.That(ui.Q<Toggle>("audio-effects-toggle"), Is.Not.Null);
            Assert.That(ui.Q<Toggle>("audio-music-toggle"), Is.Not.Null);
            Assert.That(audio.MasterEnabled, Is.True);
            Assert.That(audio.EffectsEnabled, Is.True);
            Assert.That(audio.MusicEnabled, Is.False);

            var blockedIntent = ChooseHumanIntent(
                controller.Flow.Match.State,
                controller.Flow.Match.GetHumanLegalIntents());
            var initialTraceCount = controller.Flow.Match.Trace.IntentHistory.Count;
            Assert.That(controller.SubmitHumanIntent(blockedIntent), Is.False);
            Assert.That(controller.Flow.Match.Trace.IntentHistory, Has.Count.EqualTo(initialTraceCount));

            yield return WaitForPresentation(table);
            Assert.That(table.AnimationCompletionReason, Is.EqualTo(AnimationSequenceCompletionReason.Completed));
            Assert.That(table.AnimationPlayer.IsRenderedStateSynchronized, Is.True);
            Assert.That(table.RenderedState, Is.SameAs(controller.Flow.Match.State));

            var masterToggle = ui.Q<Toggle>("audio-master-toggle");
            var effectsToggle = ui.Q<Toggle>("audio-effects-toggle");
            var musicToggle = ui.Q<Toggle>("audio-music-toggle");
            masterToggle.value = false;
            var playedBeforeMutedBatch = audio.PlayedCueCount;
            var unchangedState = controller.Flow.Match.State;
            var unchangedTraceCount = controller.Flow.Match.Trace.IntentHistory.Count;
            foreach (var viewport in new[]
            {
                new Vector2Int(1280, 720),
                new Vector2Int(1440, 900),
                new Vector2Int(1920, 1080),
                new Vector2Int(2560, 1440),
            })
            {
                table.ApplyViewportForTests(viewport, new Rect(0f, 0f, viewport.x, viewport.y));
                Assert.That(controller.Flow.Match.State, Is.SameAs(unchangedState));
                Assert.That(controller.Flow.Match.Trace.IntentHistory, Has.Count.EqualTo(unchangedTraceCount));
            }

            var firstIntent = ChooseHumanIntent(
                controller.Flow.Match.State,
                controller.Flow.Match.GetHumanLegalIntents());
            Assert.That(controller.SubmitHumanIntent(firstIntent), Is.True);
            Assert.That(table.IsPresentationBusy, Is.True);
            var acceptedTraceCount = controller.Flow.Match.Trace.IntentHistory.Count;
            Assert.That(controller.SubmitHumanIntent(firstIntent), Is.False);
            Assert.That(controller.Flow.Match.Trace.IntentHistory, Has.Count.EqualTo(acceptedTraceCount));

            table.SetFastForward(true);
            table.SetReducedMotion(true);
            yield return WaitForPresentation(table);
            Assert.That(audio.PlayedCueCount, Is.EqualTo(playedBeforeMutedBatch));
            masterToggle.value = true;
            effectsToggle.value = false;
            Assert.That(audio.EffectsAudible, Is.False);
            musicToggle.value = true;
            Assert.That(audio.MusicEnabled, Is.True);
            musicToggle.value = false;
            Assert.That(table.AnimationPlayer.FastForward, Is.True);
            Assert.That(table.AnimationPlayer.ReducedMotion, Is.True);
            Assert.That(table.AnimationPlayer.IsRenderedStateSynchronized, Is.True);

            SubmitNext(controller);
            table.InterruptPresentation();
            AssertSynchronized(table, controller, AnimationSequenceCompletionReason.Interrupted);
            Assert.That(audio.PlayedCueCount, Is.EqualTo(playedBeforeMutedBatch));
            Assert.That(audio.ActiveCue, Is.Null);
            effectsToggle.value = true;
            Assert.That(audio.EffectsAudible, Is.True);

            SubmitNext(controller);
            table.CancelPresentation();
            AssertSynchronized(table, controller, AnimationSequenceCompletionReason.Cancelled);
            Assert.That(audio.ActiveCue, Is.Null);

            SubmitNext(controller);
            table.SkipPresentation();
            AssertSynchronized(table, controller, AnimationSequenceCompletionReason.Skipped);
            Assert.That(audio.ActiveCue, Is.Null);

            SubmitNext(controller);
            Assert.That(controller.ReturnHome(), Is.True);
            Assert.That(table.AnimationCompletionReason, Is.EqualTo(AnimationSequenceCompletionReason.Interrupted));
            Assert.That(controller.Flow.Match, Is.Null);
            Assert.That(controller.IsPresentationBusy, Is.False);
            Assert.That(table.Snapshot, Is.Null);
            Assert.That(audio.ActiveCue, Is.Null);
        }

        [UnityTest]
        public IEnumerator LocalDeal_RecompositionKeepsOneContinuousFaceDownToFaceUpFlip()
        {
            yield return LoadMatchWithoutSettlingPresentation();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            table.SkipPresentation();

            var deadline = Time.realtimeSinceStartup + 20f;
            ResolvedAnimationStep dealStep = null;
            while (Time.realtimeSinceStartup < deadline)
            {
                var active = table.AnimationPlayer.ActiveStep;
                if (active?.Kind == ResolvedAnimationStepKind.Deal
                    && active.PlayerId == table.Snapshot.LocalPlayerId)
                {
                    dealStep = active;
                    break;
                }

                if (!table.IsPresentationBusy)
                {
                    var legal = controller.Flow.Match.GetHumanLegalIntents();
                    Assert.That(
                        controller.SubmitHumanIntent(
                            ChooseHumanIntent(controller.Flow.Match.State, legal)),
                        Is.True);
                }

                yield return null;
            }

            Assert.That(dealStep, Is.Not.Null);
            while (table.AnimationPlayer.ActiveStepProgress < 0.15f
                && ReferenceEquals(table.AnimationPlayer.ActiveStep, dealStep)
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            var movingCardName = $"Local Hand {dealStep.Cards[0]}";
            var movingCard = table.RenderedCards.Single(card =>
                card.Zone == FirstPlayableCardZone.LocalHand
                && card.name == movingCardName);
            Assert.That(movingCard.IsFaceUp, Is.False);
            Assert.That(movingCard.Card, Is.Null);

            table.ApplyViewportForTests(
                new Vector2Int(1440, 900),
                new Rect(0f, 0f, 1440f, 900f));
            movingCard = table.RenderedCards.Single(card =>
                card.Zone == FirstPlayableCardZone.LocalHand
                && card.name == movingCardName);
            Assert.That(movingCard.IsFaceUp, Is.False);
            Assert.That(movingCard.Card, Is.Null);

            var previousFaceUp = false;
            var faceTransitions = 0;
            while (ReferenceEquals(table.AnimationPlayer.ActiveStep, dealStep)
                && table.IsPresentationBusy
                && Time.realtimeSinceStartup < deadline)
            {
                movingCard = table.RenderedCards.Single(card =>
                    card.Zone == FirstPlayableCardZone.LocalHand
                    && card.name == movingCardName);
                if (movingCard.IsFaceUp != previousFaceUp)
                {
                    faceTransitions++;
                    previousFaceUp = movingCard.IsFaceUp;
                }

                if (faceTransitions > 0)
                {
                    Assert.That(movingCard.IsFaceUp, Is.True);
                }

                yield return null;
            }

            Assert.That(faceTransitions, Is.EqualTo(1));
            table.SkipPresentation();
        }

        [UnityTest]
        public IEnumerator FastForwardCompleteMatch_ProfilesIntegratedRenderingWithoutPooling()
        {
            yield return LoadMatchWithoutSettlingPresentation();
            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            var table = Object.FindAnyObjectByType<FirstPlayableTablePresentation>();
            var audio = table.AudioPresenter;
            table.SetFastForward(true);
            var startedAt = Time.realtimeSinceStartup;
            var observedSpatialBeats = new HashSet<ResolvedAnimationStepKind>();
            var spatialFrameCounts = new Dictionary<ResolvedAnimationStepKind, int>();
            var observedParallelReflowBeats = new HashSet<ResolvedAnimationStepKind>();
            var observedRevealFlip = false;
            var observedCollectionFlip = false;
            yield return ObservePresentation(
                table,
                observedSpatialBeats,
                spatialFrameCounts,
                observedParallelReflowBeats,
                value => observedRevealFlip |= value,
                value => observedCollectionFlip |= value);

            var humanIntents = 0;
            while (controller.Flow.Stage == FirstPlayableFlowStage.Match && humanIntents++ < 5000)
            {
                var legal = controller.Flow.Match.GetHumanLegalIntents();
                Assert.That(
                    controller.SubmitHumanIntent(ChooseHumanIntent(controller.Flow.Match.State, legal)),
                    Is.True);
                yield return ObservePresentation(
                    table,
                    observedSpatialBeats,
                    spatialFrameCounts,
                    observedParallelReflowBeats,
                    value => observedRevealFlip |= value,
                    value => observedCollectionFlip |= value);
            }

            Assert.That(humanIntents, Is.LessThan(5000));
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Result));
            Assert.That(table.AnimationPlayer.IsRenderedStateSynchronized, Is.True);
            Assert.That(table.RenderedState, Is.SameAs(controller.Flow.Match.State));
            Assert.That(table.AnimationPlayer.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.Deal));
            Assert.That(table.AnimationPlayer.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.CardPlay));
            Assert.That(table.AnimationPlayer.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.NormalCapture));
            Assert.That(table.AnimationPlayer.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.MatchCompleted));
            Assert.That(observedSpatialBeats, Does.Contain(ResolvedAnimationStepKind.DealerSelection));
            Assert.That(observedSpatialBeats, Does.Contain(ResolvedAnimationStepKind.Deal));
            Assert.That(observedSpatialBeats, Does.Contain(ResolvedAnimationStepKind.OpeningPlacement));
            Assert.That(observedSpatialBeats, Does.Contain(ResolvedAnimationStepKind.CardPlay));
            Assert.That(observedSpatialBeats, Does.Contain(ResolvedAnimationStepKind.NormalCapture));
            Assert.That(observedSpatialBeats, Does.Contain(ResolvedAnimationStepKind.CascadeCapture));
            Assert.That(observedSpatialBeats, Does.Contain(ResolvedAnimationStepKind.Leftovers));
            Assert.That(
                table.AnimationPlayer.PresentedSteps,
                Has.None.EqualTo(ResolvedAnimationStepKind.HandReflow));
            Assert.That(
                table.AnimationPlayer.PresentedSteps,
                Has.None.EqualTo(ResolvedAnimationStepKind.TablePlacement));
            Assert.That(observedParallelReflowBeats, Does.Contain(ResolvedAnimationStepKind.Deal));
            Assert.That(observedParallelReflowBeats, Does.Contain(ResolvedAnimationStepKind.CardPlay));
            Assert.That(observedParallelReflowBeats, Does.Contain(ResolvedAnimationStepKind.NormalCapture));
            if (table.AnimationPlayer.PresentedSteps.Contains(
                    ResolvedAnimationStepKind.CaptureCollection))
            {
                Assert.That(
                    observedSpatialBeats,
                    Does.Contain(ResolvedAnimationStepKind.CaptureCollection));
            }
            if (table.AnimationPlayer.PresentedSteps.Contains(
                    ResolvedAnimationStepKind.OpeningRejection))
            {
                Assert.That(observedSpatialBeats,
                    Does.Contain(ResolvedAnimationStepKind.OpeningRejection));
            }

            Assert.That(spatialFrameCounts[ResolvedAnimationStepKind.Deal], Is.GreaterThan(1));
            Assert.That(spatialFrameCounts[ResolvedAnimationStepKind.OpeningPlacement], Is.GreaterThan(1));
            Assert.That(spatialFrameCounts[ResolvedAnimationStepKind.NormalCapture], Is.GreaterThan(1));
            Assert.That(observedRevealFlip, Is.True);
            Assert.That(observedCollectionFlip, Is.True);
            var expectedAudio = table.AnimationPlayer.PresentedSteps
                .Where(step => PrototypeAudioCueLibrary.TryResolve(step, out _))
                .Select(step =>
                {
                    PrototypeAudioCueLibrary.TryResolve(step, out var cue);
                    return cue;
                })
                .ToArray();
            Assert.That(audio.CueHistory, Is.EqualTo(expectedAudio));
            Assert.That(audio.PlayedCueCount, Is.EqualTo(expectedAudio.Length));
            Assert.That(audio.CueHistory, Does.Contain(PrototypeAudioCueKind.Deal));
            Assert.That(audio.CueHistory, Does.Contain(PrototypeAudioCueKind.Play));
            Assert.That(audio.CueHistory, Does.Contain(PrototypeAudioCueKind.Capture));
            Assert.That(audio.CueHistory, Does.Contain(PrototypeAudioCueKind.Victory));
            Assert.That(
                controller.Flow.Match.Trace.Events.OfType<CardPlayedEvent>()
                    .Select(item => item.PlayerId)
                    .Distinct()
                    .ToArray(),
                Has.Length.EqualTo(2));

            TestContext.WriteLine(
                $"Integrated complete-match animation profile: " +
                $"{controller.Flow.Match.Trace.IntentHistory.Count} intent records, " +
                $"{controller.Flow.Match.Trace.Events.Count} source events, " +
                $"{table.AnimationPlayer.PresentedSteps.Count} visible beats, " +
                $"{table.AnimationPlayer.FrameCount} rendered updates, " +
                $"{table.AnimationPresentationCpuMilliseconds:F2} ms integrated presentation CPU, " +
                $"{table.AnimationPresentationPeakUpdateCpuMilliseconds:F3} ms peak update, " +
                $"{(Time.realtimeSinceStartup - startedAt) * 1000f:F1} ms batch wall time.");
        }

        private static void SubmitNext(FirstPlayableFlowController controller)
        {
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            var legal = controller.Flow.Match.GetHumanLegalIntents();
            Assert.That(controller.SubmitHumanIntent(ChooseHumanIntent(controller.Flow.Match.State, legal)), Is.True);
            Assert.That(controller.IsPresentationBusy, Is.True);
        }

        private static void AssertSynchronized(
            FirstPlayableTablePresentation table,
            FirstPlayableFlowController controller,
            AnimationSequenceCompletionReason reason)
        {
            Assert.That(table.AnimationCompletionReason, Is.EqualTo(reason));
            Assert.That(table.AnimationPlayer.IsRenderedStateSynchronized, Is.True);
            Assert.That(table.RenderedState, Is.SameAs(controller.Flow.Match.State));
            Assert.That(controller.IsPresentationBusy, Is.False);
        }

        private static IEnumerator WaitForPresentation(FirstPlayableTablePresentation table)
        {
            var deadline = Time.realtimeSinceStartup + 10f;
            while (table.IsPresentationBusy && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(table.IsPresentationBusy, Is.False);
        }

        private static IEnumerator ObservePresentation(
            FirstPlayableTablePresentation table,
            ISet<ResolvedAnimationStepKind> observedSpatialBeats,
            IDictionary<ResolvedAnimationStepKind, int> spatialFrameCounts,
            ISet<ResolvedAnimationStepKind> observedParallelReflowBeats,
            System.Action<bool> observeRevealFlip,
            System.Action<bool> observeCollectionFlip)
        {
            var deadline = Time.realtimeSinceStartup + 10f;
            ResolvedAnimationStep trackedCaptureStep = null;
            var stableTablePositions = new Dictionary<Card, Vector3>();
            while (table.IsPresentationBusy && Time.realtimeSinceStartup < deadline)
            {
                var step = table.AnimationPlayer.ActiveStep;
                var progress = table.AnimationPlayer.ActiveStepProgress;
                if (!ReferenceEquals(step, trackedCaptureStep))
                {
                    if (CompletesCollection(trackedCaptureStep))
                    {
                        Assert.That(
                            table.RenderedCards
                                .Where(card =>
                                    card.Zone == FirstPlayableCardZone.LocalCaptured
                                    || card.Zone == FirstPlayableCardZone.OpponentCaptured)
                                .All(card => !card.IsFaceUp && !card.Card.HasValue),
                            Is.True,
                            $"{trackedCaptureStep.Kind} left a captured card face-up at its step boundary.");
                    }

                    trackedCaptureStep = step;
                    stableTablePositions.Clear();
                }

                if (step != null
                    && IsCaptureTreatment(step.Kind)
                    && step.SourceEvent is CardsCapturedEvent captured)
                {
                    foreach (var rendered in table.RenderedCards)
                    {
                        if (rendered.Zone != FirstPlayableCardZone.Table
                            || !rendered.Card.HasValue
                            || captured.Cards.Contains(rendered.Card.Value))
                        {
                            continue;
                        }

                        if (stableTablePositions.TryGetValue(
                                rendered.Card.Value,
                                out var stablePosition))
                        {
                            Assert.That(
                                Vector3.Distance(
                                    rendered.transform.position,
                                    stablePosition),
                                Is.LessThan(0.0001f),
                                $"{rendered.Card.Value} moved during {step.Kind}.");
                        }
                        else
                        {
                            stableTablePositions[rendered.Card.Value] =
                                rendered.transform.position;
                        }
                    }
                }

                if (step != null
                    && table.ActiveSpatialMotionKind == step.Kind
                    && table.ActiveSpatialMotionCount > 0
                    && progress > 0.05f
                    && progress < 0.95f)
                {
                    observedSpatialBeats.Add(step.Kind);
                    spatialFrameCounts.TryGetValue(step.Kind, out var count);
                    spatialFrameCounts[step.Kind] = count + 1;
                    if (table.ActiveParallelHandReflowMotionCount > 0)
                    {
                        observedParallelReflowBeats.Add(step.Kind);
                    }

                    observeRevealFlip(
                        (step.Kind == ResolvedAnimationStepKind.Deal
                            || step.Kind == ResolvedAnimationStepKind.OpeningPlacement)
                        && table.ActiveCardFlipDegrees > 0f
                        && table.ActiveCardFlipDegrees < 180f);
                    observeCollectionFlip(
                        (step.Kind == ResolvedAnimationStepKind.NormalCapture
                            || step.Kind == ResolvedAnimationStepKind.CascadeCapture
                            || step.Kind == ResolvedAnimationStepKind.CaptureCollection
                            || step.Kind == ResolvedAnimationStepKind.Leftovers)
                        && table.ActiveCardFlipDegrees > 180f
                        && table.ActiveCardFlipDegrees < 360f);
                }

                yield return null;
            }

            Assert.That(table.IsPresentationBusy, Is.False);
        }

        private static bool IsCaptureTreatment(ResolvedAnimationStepKind kind)
        {
            return kind == ResolvedAnimationStepKind.NormalCapture
                || kind == ResolvedAnimationStepKind.CascadeCapture
                || kind == ResolvedAnimationStepKind.CaptureCollection;
        }

        private static bool CompletesCollection(ResolvedAnimationStep step)
        {
            if (step == null)
            {
                return false;
            }

            if (step.Kind == ResolvedAnimationStepKind.CaptureCollection
                || step.Kind == ResolvedAnimationStepKind.Leftovers)
            {
                return true;
            }

            return step.Kind == ResolvedAnimationStepKind.NormalCapture
                && step.SourceEvent is CardsCapturedEvent captured
                && captured.Cards.Count <= 2;
        }

        private static IEnumerator LoadMatchWithoutSettlingPresentation()
        {
            if (CompositionRoot.Instance != null)
            {
                Object.Destroy(CompositionRoot.Instance.gameObject);
                yield return null;
            }

            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
            var deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != "Home" && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            var controller = Object.FindAnyObjectByType<FirstPlayableFlowController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.OpenSetup(), Is.True);
            Assert.That(controller.StartMatch(), Is.True);
            yield return null;
            Assert.That(controller.Flow.Stage, Is.EqualTo(FirstPlayableFlowStage.Match));
            Assert.That(Object.FindAnyObjectByType<FirstPlayableTablePresentation>(), Is.Not.Null);
        }

        private static PlayerIntent ChooseHumanIntent(MatchState state, IReadOnlyList<PlayerIntent> legal)
        {
            if (state.Phase == MatchPhase.AwaitingDealerChoice)
            {
                return legal.OfType<ChooseDealOptionsIntent>()
                    .Single(item => item.DealHandsBeforeTable && item.OpeningPattern == OpeningPattern.Ascending);
            }

            return legal.OfType<PlayCardIntent>().FirstOrDefault() ?? legal[0];
        }
    }
}
