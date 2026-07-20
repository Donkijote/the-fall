using System;
using System.Linq;
using NUnit.Framework;
using TheFall.Application;
using TheFall.Application.Animation;
using TheFall.Domain;
using TheFall.Presentation.Animation;
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
    }
}
