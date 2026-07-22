using System;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Application.Animation;
using TheFall.Domain;
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
                ResolvedAnimationStepKind.NormalCapture,
                ResolvedAnimationStepKind.CascadeCapture,
                ResolvedAnimationStepKind.CascadeCapture,
                ResolvedAnimationStepKind.FallScore,
                ResolvedAnimationStepKind.CleanTableScore,
                ResolvedAnimationStepKind.TurnChanged,
                ResolvedAnimationStepKind.SynchronizeFinalState,
            }));
            Assert.That(sequence.Steps.Single(step => step.Kind == ResolvedAnimationStepKind.NormalCapture).Cards, Has.Count.EqualTo(2));
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
                (int)AnimationScenarioKind.FallCascadeAndCleanTable,
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
    }
}
