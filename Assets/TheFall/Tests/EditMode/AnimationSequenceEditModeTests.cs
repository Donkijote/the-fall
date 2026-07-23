using System;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Application.Animation;
using TheFall.Domain;
using TheFall.Infrastructure;
using TheFall.Presentation.Animation;
using TheFall.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFall.Tests.EditMode
{
    public sealed class AnimationSequenceEditModeTests
    {
        [Test]
        public void RepresentativeRecording_ComesFromOrderedResolvedDomainEvents()
        {
            var recording = RepresentativeAnimationTurn.Create(Seat.First);

            Assert.That(recording.Result.IsAccepted, Is.True);
            Assert.That(recording.Result.Events.Select(resolvedEvent => resolvedEvent.Kind), Is.EqualTo(new[]
            {
                DomainEventKind.CardPlayed,
                DomainEventKind.CardsCaptured,
                DomainEventKind.ScoreChanged,
                DomainEventKind.ScoreChanged,
                DomainEventKind.TurnChanged,
            }));
            Assert.That(recording.Result.Events.OfType<CardsCapturedEvent>().Single().Cards, Has.Count.EqualTo(4));
            Assert.That(recording.Result.Events.OfType<ScoreChangedEvent>().Select(score => score.Reason), Is.EqualTo(new[]
            {
                ScoreReason.Fall,
                ScoreReason.CleanTable,
            }));
        }

        [Test]
        public void Sequence_MapsResolvedCaptureIntoCountablePresentationBeats()
        {
            var recording = RepresentativeAnimationTurn.Create(Seat.First);
            var sequence = ResolvedAnimationSequence.Create(
                recording.Result.Events,
                recording.Result.State);

            Assert.That(sequence.Steps.Select(step => step.Kind), Is.EqualTo(new[]
            {
                ResolvedAnimationStepKind.CardPlay,
                ResolvedAnimationStepKind.HandReflow,
                ResolvedAnimationStepKind.NormalCapture,
                ResolvedAnimationStepKind.CascadeCapture,
                ResolvedAnimationStepKind.CascadeCapture,
                ResolvedAnimationStepKind.CascadeCapture,
                ResolvedAnimationStepKind.FallScore,
                ResolvedAnimationStepKind.CleanTableScore,
                ResolvedAnimationStepKind.TurnChanged,
                ResolvedAnimationStepKind.SynchronizeFinalState,
            }));
            Assert.That(
                sequence.Steps.Single(step => step.Kind == ResolvedAnimationStepKind.CardPlay).Cards,
                Has.Count.EqualTo(2));
            Assert.That(sequence.Steps.Single(step => step.Kind == ResolvedAnimationStepKind.NormalCapture).Cards, Has.Count.EqualTo(2));
            Assert.That(
                sequence.Steps.Last(step => step.Kind == ResolvedAnimationStepKind.CascadeCapture).Cards,
                Has.Count.EqualTo(4));
        }

        [Test]
        public void NonCapturingPlay_MapsCardPlayAndTablePlacementWithoutPresentationRules()
        {
            var firstId = new PlayerId("placement-first");
            var secondId = new PlayerId("placement-second");
            var playedCard = new Card(CardSuit.Coins, CardRank.Five);
            var state = MatchState.CreateOneVersusOne(
                new PlayerState(
                    new Player(firstId, "First", Seat.First, TeamId.One, PlayerControl.Human),
                    new[] { playedCard }),
                new PlayerState(
                    new Player(secondId, "Second", Seat.Second, TeamId.Two, PlayerControl.Bot),
                    new[] { new Card(CardSuit.Cups, CardRank.Seven) }),
                Seat.Second,
                Seat.First,
                new[] { new Card(CardSuit.Clubs, CardRank.Two) },
                new Deck(Array.Empty<Card>()));
            var result = new MatchSession(state).Submit(new PlayCardIntent(firstId, playedCard));
            var sequence = ResolvedAnimationSequence.Create(result.Events, result.State);
            var renderedState = new AnimationPresentationState(state);

            foreach (var step in sequence.Steps)
            {
                renderedState.Apply(step, sequence.FinalState);
            }

            Assert.That(sequence.Steps.Select(step => step.Kind), Is.EqualTo(new[]
            {
                ResolvedAnimationStepKind.CardPlay,
                ResolvedAnimationStepKind.HandReflow,
                ResolvedAnimationStepKind.TablePlacement,
                ResolvedAnimationStepKind.TurnChanged,
                ResolvedAnimationStepKind.SynchronizeFinalState,
            }));
            Assert.That(renderedState.IsSynchronizedWith(result.State), Is.True);
            Assert.That(renderedState.Table, Does.Contain(playedCard));
        }

        [TestCase(Seat.First)]
        [TestCase(Seat.Second)]
        public void EveryRelevantSeat_EndsAtTheAuthoritativeRenderedState(Seat actingSeat)
        {
            var recording = RepresentativeAnimationTurn.Create(actingSeat);
            var sequence = ResolvedAnimationSequence.Create(
                recording.Result.Events,
                recording.Result.State);
            var renderedState = new AnimationPresentationState(recording.InitialState);

            foreach (var step in sequence.Steps)
            {
                renderedState.Apply(step, sequence.FinalState);
            }

            var actor = recording.Result.State.GetPlayerAt(actingSeat);
            Assert.That(renderedState.IsSynchronizedWith(recording.Result.State), Is.True);
            Assert.That(renderedState.GetCaptured(actor.Player.Id), Has.Count.EqualTo(4));
            Assert.That(renderedState.Table, Is.Empty);
            Assert.That(renderedState.GetScore(actor.Player.TeamId).Value, Is.EqualTo(12));
        }

        [Test]
        public void Timing_IsPresentationConfigurationWithFastForwardAndReducedMotionVariants()
        {
            var configuration = ScriptableObject.CreateInstance<AnimationSequenceConfiguration>();
            try
            {
                var normal = configuration.GetDuration(
                    ResolvedAnimationStepKind.NormalCapture,
                    false,
                    false);
                var fast = configuration.GetDuration(
                    ResolvedAnimationStepKind.NormalCapture,
                    true,
                    false);
                var reduced = configuration.GetDuration(
                    ResolvedAnimationStepKind.NormalCapture,
                    false,
                    true);

                Assert.That(normal, Is.EqualTo(configuration.NormalCaptureSeconds));
                Assert.That(fast, Is.LessThan(normal));
                Assert.That(reduced, Is.LessThanOrEqualTo(normal));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(configuration);
            }
        }

        [Test]
        public void PresetAsset_IsNamedVersionedAndContainsReusableSerializedBeats()
        {
            const string presetPath = "Assets/TheFall/Content/Animation/AnimationSequenceConfiguration.asset";
            var preset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(presetPath);

            Assert.That(preset, Is.Not.Null);
            Assert.That(preset.PresetName, Is.Not.Empty);
            Assert.That(preset.PresetVersion, Is.EqualTo(AnimationSequenceConfiguration.CurrentPresetVersion));
            Assert.That(preset.Beats.Select(beat => beat.Kind), Does.Contain(ResolvedAnimationStepKind.CardPlay));
            Assert.That(preset.Beats.Select(beat => beat.Kind), Does.Contain(ResolvedAnimationStepKind.HandReflow));
            Assert.That(preset.Beats.Select(beat => beat.Kind), Does.Contain(ResolvedAnimationStepKind.Canto));
            Assert.That(preset.Beats.Select(beat => beat.Kind), Does.Contain(ResolvedAnimationStepKind.MatchCompleted));
        }

        [Test]
        public void Composition_UsesPresetOrderWithoutChangingSourceEventsOrFinalState()
        {
            var recording = RepresentativeAnimationTurn.Create(Seat.First);
            var sequence = ResolvedAnimationSequence.Create(
                recording.Result.Events,
                recording.Result.State,
                new[]
                {
                    ResolvedAnimationStepKind.FallScore,
                    ResolvedAnimationStepKind.CardPlay,
                    ResolvedAnimationStepKind.NormalCapture,
                    ResolvedAnimationStepKind.CascadeCapture,
                    ResolvedAnimationStepKind.CleanTableScore,
                    ResolvedAnimationStepKind.TurnChanged,
                });
            var rendered = new AnimationPresentationState(recording.InitialState);

            foreach (var step in sequence.Steps)
            {
                rendered.Apply(step, sequence.FinalState);
            }

            Assert.That(sequence.Steps.Take(3).Select(step => step.Kind), Is.EqualTo(new[]
            {
                ResolvedAnimationStepKind.FallScore,
                ResolvedAnimationStepKind.CardPlay,
                ResolvedAnimationStepKind.NormalCapture,
            }));
            Assert.That(sequence.SourceEvents, Is.EqualTo(recording.Result.Events));
            Assert.That(rendered.IsSynchronizedWith(recording.Result.State), Is.True);
        }

        [Test]
        public void Transport_PauseStepSeekLoopSkipAndResetRemainDeterministic()
        {
            var transport = new AnimationSequenceTransport(new[]
            {
                new AnimationBeatTiming(0.1f, 0.2f),
                new AnimationBeatTiming(0f, 0.3f),
            });

            transport.Play();
            transport.Tick(0.15f);
            transport.Pause();
            var pausedAt = transport.ElapsedSeconds;
            transport.Tick(1f);
            Assert.That(transport.ElapsedSeconds, Is.EqualTo(pausedAt));

            transport.StepForward();
            Assert.That(transport.ElapsedSeconds, Is.EqualTo(0.3f).Within(0.0001f));
            transport.SeekNormalized(0.5f);
            Assert.That(transport.NormalizedPosition, Is.EqualTo(0.5f).Within(0.0001f));

            transport.Loop = true;
            transport.Play();
            transport.Tick(0.5f);
            Assert.That(transport.IsPlaying, Is.True);
            Assert.That(transport.ElapsedSeconds, Is.LessThan(transport.DurationSeconds));

            transport.SkipToEnd();
            Assert.That(transport.ReachedEnd, Is.True);
            transport.Reset();
            Assert.That(transport.ElapsedSeconds, Is.Zero);
            Assert.That(transport.IsPlaying, Is.False);
        }

        [Test]
        public void BeatEvaluator_UsesTheSameWireframePathForAuthoringAndPlayback()
        {
            var start = new Vector3(-1f, 0f, 0f);
            var target = new Vector3(1f, 0f, 0f);
            var trajectory = new Vector3(0f, 0.5f, 0.25f);

            var midpoint = AnimationBeatEvaluator.EvaluatePosition(
                start,
                target,
                0.5f,
                AnimationBeatEasing.Linear,
                trajectory);

            Assert.That(midpoint, Is.EqualTo(new Vector3(0f, 0.5f, 0.25f)));
            Assert.That(AnimationBeatEvaluator.EvaluatePosition(
                start,
                target,
                0f,
                AnimationBeatEasing.EaseInOut,
                trajectory), Is.EqualTo(start));
            Assert.That(Vector3.Distance(
                AnimationBeatEvaluator.EvaluatePosition(
                    start,
                    target,
                    1f,
                    AnimationBeatEasing.EaseInOut,
                    trajectory),
                target), Is.LessThan(0.00001f));
        }

        [Test]
        public void EditModeWorkbench_PreviewsAndSeeksIndividualBeatsWithoutPlayMode()
        {
            EditorSceneManager.OpenScene(
                "Assets/TheFall/Presentation/Scenes/AnimationLab.unity",
                OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindAnyObjectByType<AnimationLabController>();
            var preset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(
                "Assets/TheFall/Content/Animation/AnimationSequenceConfiguration.asset");

            Assert.That(UnityEngine.Application.isPlaying, Is.False);
            Assert.That(controller, Is.Not.Null);
            controller.BeginEditorWorkbenchPreview(
                (int)AnimationScenarioKind.PlayCard,
                Seat.First,
                AnimationPreviewProfile.Desktop,
                preset);

            try
            {
                var cardPlayIndex = controller.Sequence.Steps
                    .Take(controller.AnimatableStepCount)
                    .Select((step, index) => new { step, index })
                    .Single(entry => entry.step.Kind == ResolvedAnimationStepKind.CardPlay)
                    .index;
                controller.SeekToStep(cardPlayIndex, 0.5f);

                Assert.That(controller.PreviewRoot, Is.Not.Null);
                Assert.That(controller.gameObject.scene.isDirty, Is.False);
                Assert.That(controller.CurrentStepIndex, Is.EqualTo(cardPlayIndex));
                Assert.That(controller.ActiveStep.Kind, Is.EqualTo(ResolvedAnimationStepKind.CardPlay));
                Assert.That(controller.ActiveStepProgress, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(controller.TryGetPrimaryMotion(out var motion), Is.True);
                Assert.That(motion.StartWorld, Is.Not.EqualTo(motion.TargetWorld));

                var before = controller.ElapsedSeconds;
                controller.Resume();
                Assert.That(controller.TickEditorPreview(0.05f), Is.True);
                Assert.That(controller.ElapsedSeconds, Is.GreaterThan(before));
                Assert.That(UnityEngine.Application.isPlaying, Is.False);
            }
            finally
            {
                controller.ClearEditorPreview();
            }
        }

        [Test]
        public void EditModeWorkbench_PlayOnceIgnoresPresetLoopAndRestsAtTheFinalState()
        {
            EditorSceneManager.OpenScene(
                "Assets/TheFall/Presentation/Scenes/AnimationLab.unity",
                OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindAnyObjectByType<AnimationLabController>();
            var loopingPreset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(
                "Assets/TheFall/Content/Animation/AnimationFastIterationPreset.asset");

            Assert.That(controller, Is.Not.Null);
            Assert.That(loopingPreset.Loop, Is.True);
            controller.BeginEditorWorkbenchPreview(
                (int)AnimationScenarioKind.DealerCardSelection,
                Seat.First,
                AnimationPreviewProfile.Desktop,
                loopingPreset);

            try
            {
                controller.PlayOnce();
                for (var tick = 0; tick < 100 && controller.IsPlaying; tick++)
                {
                    controller.TickEditorPreview(0.05f);
                }

                Assert.That(controller.IsPlaying, Is.False);
                Assert.That(controller.NormalizedPosition, Is.EqualTo(1f));
                Assert.That(
                    controller.CompletionReason,
                    Is.EqualTo(AnimationSequenceCompletionReason.Completed));
                Assert.That(controller.IsRenderedStateSynchronized, Is.True);
                Assert.That(controller.RevealedDealerCardViewCount, Is.EqualTo(1));
                Assert.That(controller.RevealedDealerCardClearance, Is.GreaterThan(0f));

                var completedRoot = controller.PreviewRoot;
                Assert.That(controller.TickEditorPreview(1f), Is.False);
                Assert.That(controller.NormalizedPosition, Is.EqualTo(1f));
                Assert.That(controller.PreviewRoot, Is.SameAs(completedRoot));
                Assert.That(controller.IsRenderedStateSynchronized, Is.True);
            }
            finally
            {
                controller.ClearEditorPreview();
            }
        }

        [Test]
        public void AnimationWorkbenchWindow_IsAvailableAsAnEditModeAuthoringSurface()
        {
            var window = ScriptableObject.CreateInstance<AnimationWorkbenchWindow>();
            try
            {
                Assert.That(window, Is.Not.Null);
                Assert.That(window.titleContent.text, Is.EqualTo("Animation Workbench"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void CompleteDomainEventVocabulary_HasAReusablePresentationBeatInSourceOrder()
        {
            var playerId = new PlayerId("event-player");
            var card = new Card(CardSuit.Coins, CardRank.Two);
            var cascade = new Card(CardSuit.Cups, CardRank.Three);
            var events = new DomainEvent[]
            {
                new MatchStartedEvent(40),
                new DealerCardSelectedEvent(playerId, card),
                new DealerSelectionTiedEvent(CardRank.Two),
                new DealerSelectedEvent(playerId, Seat.First),
                new DeckShuffledEvent(1, 40),
                new DealerChoiceMadeEvent(playerId, true, OpeningPattern.Ascending),
                new DealStartedEvent(1, 1, false),
                new CardDealtEvent(playerId, card, 0),
                new OpeningCardRejectedEvent(card, 0, 10),
                new OpeningCardPlacedEvent(card, 0),
                new CardPlayedEvent(playerId, card),
                new CardPlacedOnTableEvent(playerId, card),
                new CardsCapturedEvent(playerId, new[] { card, card, cascade }),
                new CantoAnnouncedEvent(playerId, CantoKind.Ronda),
                new CantoResolvedEvent(playerId, CantoKind.Ronda, true, true),
                new ScoreChangedEvent(TeamId.One, 1, new Score(1), ScoreReason.Canto),
                new DealCompletedEvent(1, 1),
                new LeftoversCollectedEvent(playerId, new[] { card }),
                new RoundCompletedEvent(1),
                new DealerRotatedEvent(Seat.Second, Seat.First),
                new TieExtensionStartedEvent(2, new Score(24)),
                new TurnChangedEvent(Seat.Second, Seat.First),
                new MatchCompletedEvent(TeamId.One),
            };
            var sequence = ResolvedAnimationSequence.Create(
                events,
                RepresentativeAnimationTurn.Create(Seat.First).Result.State);

            Assert.That(sequence.Steps.Select(step => step.Kind), Is.EqualTo(new[]
            {
                ResolvedAnimationStepKind.MatchStarted,
                ResolvedAnimationStepKind.DealerSelection,
                ResolvedAnimationStepKind.DealerSelection,
                ResolvedAnimationStepKind.DealerSelection,
                ResolvedAnimationStepKind.DealerSelection,
                ResolvedAnimationStepKind.DealerChoice,
                ResolvedAnimationStepKind.Deal,
                ResolvedAnimationStepKind.Deal,
                ResolvedAnimationStepKind.OpeningRejection,
                ResolvedAnimationStepKind.OpeningPlacement,
                ResolvedAnimationStepKind.CardPlay,
                ResolvedAnimationStepKind.HandReflow,
                ResolvedAnimationStepKind.TablePlacement,
                ResolvedAnimationStepKind.NormalCapture,
                ResolvedAnimationStepKind.CascadeCapture,
                ResolvedAnimationStepKind.CascadeCapture,
                ResolvedAnimationStepKind.Canto,
                ResolvedAnimationStepKind.Canto,
                ResolvedAnimationStepKind.Score,
                ResolvedAnimationStepKind.DealCompleted,
                ResolvedAnimationStepKind.Leftovers,
                ResolvedAnimationStepKind.Round,
                ResolvedAnimationStepKind.DealerRotation,
                ResolvedAnimationStepKind.TieExtension,
                ResolvedAnimationStepKind.TurnChanged,
                ResolvedAnimationStepKind.MatchCompleted,
                ResolvedAnimationStepKind.SynchronizeFinalState,
            }));
            Assert.That(sequence.SourceEvents, Is.EqualTo(events));
        }

        [Test]
        public void WorkbenchLibrary_ProvidesTheExpectedTunableBeatsPerRecordedAnimation()
        {
            var scenarios = (AnimationScenarioKind[])Enum.GetValues(typeof(AnimationScenarioKind));
            Assert.That(AnimationScenarioRecording.DisplayNames, Has.Count.EqualTo(scenarios.Length));
            Assert.That(
                AnimationScenarioRecording.DisplayNames.Distinct().ToArray(),
                Has.Length.EqualTo(scenarios.Length));

            foreach (var scenario in scenarios)
            {
                var recording = AnimationScenarioRecording.Create(scenario, Seat.First);
                var expectedBeats = recording.PreviewBeats
                    .Select(beat => (ResolvedAnimationStepKind)(int)beat)
                    .ToArray();
                var sequence = ResolvedAnimationSequence.Create(
                    recording.Result.Events,
                    recording.Result.State,
                    expectedBeats);

                Assert.That(recording.Result.IsAccepted, Is.True, scenario.ToString());
                var expectedTunableCount = sequence.Steps.Count - 1;
                Assert.That(
                    sequence.Steps,
                    Has.Count.EqualTo(expectedTunableCount + 1),
                    scenario.ToString());
                Assert.That(
                    sequence.Steps
                        .Take(expectedTunableCount)
                        .All(step => expectedBeats.Contains(step.Kind)),
                    Is.True,
                    scenario.ToString());
                Assert.That(
                    sequence.Steps[expectedTunableCount].Kind,
                    Is.EqualTo(ResolvedAnimationStepKind.SynchronizeFinalState),
                    scenario.ToString());
            }
        }

        [Test]
        public void FirstPlayableRuntimePlayer_ProfilesACompleteMatchAndConvergesAfterEveryBatch()
        {
            var preset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(
                "Assets/TheFall/Content/Animation/AnimationSequenceConfiguration.asset");
            var match = CreateFirstPlayableMatch(2400);
            var player = new FirstPlayableAnimationPlayer(preset);

            player.PlayInitialTrace(match.Trace);
            Drain(player);
            var safety = 0;
            while (match.State.Phase != MatchPhase.Completed && safety++ < 5000)
            {
                var legal = match.GetHumanLegalIntents();
                var advance = match.SubmitHumanIntent(ChooseHumanIntent(match.State, legal));
                player.PlayAdvance(advance);
                Drain(player);
                Assert.That(player.IsRenderedStateSynchronized, Is.True);
                Assert.That(player.RenderedState.IsSynchronizedWith(match.State), Is.True);
            }

            Assert.That(safety, Is.LessThan(5000));
            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Completed));
            Assert.That(player.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.DealerSelection));
            Assert.That(player.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.Deal));
            Assert.That(player.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.CardPlay));
            Assert.That(player.PresentedSteps, Does.Contain(ResolvedAnimationStepKind.MatchCompleted));
            Assert.That(
                match.Trace.Events.OfType<CardPlayedEvent>().Select(item => item.PlayerId).Distinct().ToArray(),
                Has.Length.EqualTo(2));
            Assert.That(player.FrameCount, Is.GreaterThan(0));
            Assert.That(player.CpuMilliseconds, Is.GreaterThanOrEqualTo(0d));
            TestContext.WriteLine(
                $"First-playable animation profile: {match.Trace.IntentHistory.Count} accepted intent records, " +
                $"{match.Trace.Events.Count} source events, {player.PresentedSteps.Count} beats, " +
                $"{player.FrameCount} deterministic transport ticks, {player.CpuMilliseconds:F2} ms presentation CPU, " +
                $"{player.PeakTickCpuMilliseconds:F3} ms peak tick.");
        }

        [Test]
        public void FirstPlayableRuntimePlayer_TimingVariantsCannotMutateAcceptedRuleState()
        {
            var preset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(
                "Assets/TheFall/Content/Animation/AnimationSequenceConfiguration.asset");
            var match = CreateFirstPlayableMatch(2400);
            var accepted = match.SubmitHumanIntent(ChooseHumanIntent(
                match.State,
                match.GetHumanLegalIntents()));
            var acceptedState = match.State;
            var acceptedTrace = match.Trace.IntentHistory.ToArray();

            var normal = new FirstPlayableAnimationPlayer(preset);
            normal.PlayAdvance(accepted);
            Drain(normal);

            var timingVariant = new FirstPlayableAnimationPlayer(preset);
            timingVariant.PlayAdvance(accepted);
            timingVariant.Tick(0.01f);
            var activeSourceEvent = timingVariant.ActiveStep.SourceEvent;
            timingVariant.SetFastForward(true);
            timingVariant.SetReducedMotion(true);
            Assert.That(timingVariant.ActiveStep.SourceEvent, Is.SameAs(activeSourceEvent));
            Drain(timingVariant);

            Assert.That(match.State, Is.SameAs(acceptedState));
            Assert.That(match.Trace.IntentHistory, Is.EqualTo(acceptedTrace));
            Assert.That(normal.RenderedState.IsSynchronizedWith(acceptedState), Is.True);
            Assert.That(timingVariant.RenderedState.IsSynchronizedWith(acceptedState), Is.True);
        }

        [TestCase(AnimationSequenceCompletionReason.Skipped)]
        [TestCase(AnimationSequenceCompletionReason.Interrupted)]
        [TestCase(AnimationSequenceCompletionReason.Cancelled)]
        public void FirstPlayableRuntimePlayer_EarlyExitAlwaysSynchronizesAcceptedState(
            AnimationSequenceCompletionReason reason)
        {
            var preset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(
                "Assets/TheFall/Content/Animation/AnimationSequenceConfiguration.asset");
            var match = CreateFirstPlayableMatch(2400);
            var player = new FirstPlayableAnimationPlayer(preset);
            player.PlayInitialTrace(match.Trace);

            if (reason == AnimationSequenceCompletionReason.Skipped)
            {
                player.SkipAndSynchronize();
            }
            else if (reason == AnimationSequenceCompletionReason.Interrupted)
            {
                player.InterruptAndSynchronize();
            }
            else
            {
                player.CancelAndSynchronize();
            }

            Assert.That(player.CompletionReason, Is.EqualTo(reason));
            Assert.That(player.IsRenderedStateSynchronized, Is.True);
            Assert.That(player.RenderedState.IsSynchronizedWith(match.State), Is.True);
        }

        private static FirstPlayableMatchOrchestrator CreateFirstPlayableMatch(int seed)
        {
            return FirstPlayableMatchFactory.Create(
                seed,
                new Player(new PlayerId("runtime-human"), "Human", Seat.First, TeamId.One, PlayerControl.Human),
                new Player(new PlayerId("runtime-bot"), "Bot", Seat.Second, TeamId.Two, PlayerControl.Bot));
        }

        private static PlayerIntent ChooseHumanIntent(MatchState state, System.Collections.Generic.IReadOnlyList<PlayerIntent> legal)
        {
            if (state.Phase == MatchPhase.AwaitingDealerChoice)
            {
                return legal.OfType<ChooseDealOptionsIntent>()
                    .Single(item => item.DealHandsBeforeTable && item.OpeningPattern == OpeningPattern.Ascending);
            }

            return legal.OfType<PlayCardIntent>().FirstOrDefault() ?? legal[0];
        }

        private static void Drain(FirstPlayableAnimationPlayer player)
        {
            var safety = 0;
            while (player.IsBusy && safety++ < 100000)
            {
                player.Tick(0.02f);
            }

            Assert.That(safety, Is.LessThan(100000));
        }
    }
}
