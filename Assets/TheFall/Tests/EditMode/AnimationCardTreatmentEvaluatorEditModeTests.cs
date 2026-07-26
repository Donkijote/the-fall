using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheFall.Domain;
using TheFall.Presentation.Animation;
using UnityEngine;

namespace TheFall.Tests.EditMode
{
    public sealed class AnimationCardTreatmentEvaluatorEditModeTests
    {
        [Test]
        public void RevealMove_HasOneFaceTransitionAndNoPreAnimationReveal()
        {
            var previousFaceUp = false;
            var faceTransitions = 0;
            var previousDegrees = 0f;
            for (var sample = 0; sample <= 20; sample++)
            {
                var pose = AnimationCardTreatmentEvaluator.EvaluateRevealMove(
                    Vector3.zero,
                    Vector3.one,
                    sample / 20f,
                    AnimationBeatEasing.EaseInOut,
                    Vector3.up * 0.2f,
                    true);

                if (pose.FaceUp != previousFaceUp)
                {
                    faceTransitions++;
                }

                Assert.That(pose.FlipDegrees, Is.GreaterThanOrEqualTo(previousDegrees));
                previousFaceUp = pose.FaceUp;
                previousDegrees = pose.FlipDegrees;
            }

            var start = AnimationCardTreatmentEvaluator.EvaluateRevealMove(
                Vector3.zero,
                Vector3.one,
                0f,
                AnimationBeatEasing.EaseInOut,
                Vector3.up,
                true);
            Assert.That(start.Position, Is.EqualTo(Vector3.zero));
            Assert.That(start.FaceUp, Is.False);
            Assert.That(start.FlipDegrees, Is.Zero);
            Assert.That(faceTransitions, Is.EqualTo(1));
        }

        [Test]
        public void NormalCapture_PreservesWorkbenchContactAndCollectionPhases()
        {
            var start = new Vector3(-1f, 0f, 0f);
            var stack = Vector3.zero;
            var target = new Vector3(1f, 0f, 0f);
            var atContact = AnimationCardTreatmentEvaluator.EvaluateNormalCapture(
                start,
                stack,
                target,
                AnimationCardTreatmentEvaluator.CapturePickupStartProgress,
                AnimationBeatEasing.EaseInOut,
                Vector3.up,
                true,
                false,
                false);
            var collected = AnimationCardTreatmentEvaluator.EvaluateNormalCapture(
                start,
                stack,
                target,
                1f,
                AnimationBeatEasing.EaseInOut,
                Vector3.up,
                true,
                false,
                false);
            var settlingFaceDown = AnimationCardTreatmentEvaluator.EvaluateNormalCapture(
                start,
                stack,
                target,
                AnimationCardTreatmentEvaluator.CapturePickupStartProgress
                    + (1f - AnimationCardTreatmentEvaluator.CapturePickupStartProgress) * 0.75f,
                AnimationBeatEasing.EaseInOut,
                Vector3.up,
                true,
                false,
                false);
            var cascadeLeadIn = AnimationCardTreatmentEvaluator.EvaluateNormalCapture(
                start,
                stack,
                target,
                1f,
                AnimationBeatEasing.EaseInOut,
                Vector3.up,
                true,
                false,
                true);

            Assert.That(Vector3.Distance(atContact.Position, stack), Is.LessThan(0.0001f));
            Assert.That(atContact.FaceUp, Is.True);
            Assert.That(Vector3.Distance(collected.Position, target), Is.LessThan(0.0001f));
            Assert.That(collected.FaceUp, Is.False);
            Assert.That(collected.FlipDegrees, Is.EqualTo(360f).Within(0.001f));
            Assert.That(settlingFaceDown.FaceUp, Is.False);
            Assert.That(settlingFaceDown.FlipDegrees, Is.EqualTo(360f).Within(0.001f));
            Assert.That(Vector3.Distance(settlingFaceDown.Position, target), Is.GreaterThan(0.0001f));
            Assert.That(Vector3.Distance(cascadeLeadIn.Position, stack), Is.LessThan(0.0001f));
            Assert.That(cascadeLeadIn.FaceUp, Is.True);
            Assert.That(cascadeLeadIn.FlipDegrees, Is.EqualTo(180f).Within(0.001f));
        }

        [Test]
        public void CascadeCollection_FlipsOnlyTheTerminalTreatment()
        {
            var accumulation = AnimationCardTreatmentEvaluator.EvaluateCascade(
                Vector3.zero,
                Vector3.one,
                0.75f,
                AnimationBeatEasing.EaseInOut,
                Vector3.up,
                false,
                false);
            var collection = AnimationCardTreatmentEvaluator.EvaluateCascade(
                Vector3.zero,
                Vector3.one,
                0.75f,
                AnimationBeatEasing.EaseInOut,
                Vector3.up,
                false,
                true);

            Assert.That(accumulation.FaceUp, Is.True);
            Assert.That(accumulation.FlipDegrees, Is.EqualTo(180f));
            Assert.That(collection.FaceUp, Is.False);
            Assert.That(collection.FlipDegrees, Is.EqualTo(360f).Within(0.001f));
            Assert.That(Vector3.Distance(collection.Position, Vector3.one), Is.GreaterThan(0.0001f));
        }

        [Test]
        public void HandLayout_UsesAThreeCardFanWithAnUprightForwardCenter()
        {
            var left = AnimationHandCardLayoutEvaluator.Resolve(0, 3);
            var center = AnimationHandCardLayoutEvaluator.Resolve(1, 3);
            var right = AnimationHandCardLayoutEvaluator.Resolve(2, 3);

            Assert.That(left.LateralOffset, Is.EqualTo(-0.235f).Within(0.0001f));
            Assert.That(center.LateralOffset, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(right.LateralOffset, Is.EqualTo(0.235f).Within(0.0001f));
            Assert.That(left.OutwardOffset, Is.EqualTo(0.04f).Within(0.0001f));
            Assert.That(center.OutwardOffset, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(right.OutwardOffset, Is.EqualTo(0.04f).Within(0.0001f));
            Assert.That(left.FanYawDegrees, Is.EqualTo(-10f).Within(0.0001f));
            Assert.That(center.FanYawDegrees, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(right.FanYawDegrees, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(left.HeightOffset, Is.LessThan(center.HeightOffset));
            Assert.That(center.HeightOffset, Is.LessThan(right.HeightOffset));
        }

        [Test]
        public void TableLayout_UsesDeterministicScatteredSlotsWithinTheBoundedField()
        {
            var ranks = new[]
            {
                CardRank.One,
                CardRank.Two,
                CardRank.Three,
                CardRank.Four,
                CardRank.Five,
                CardRank.Six,
                CardRank.Seven,
                CardRank.Ten,
                CardRank.Eleven,
                CardRank.Twelve,
            };
            var firstRun = new List<int>();
            var secondRun = new List<int>();
            var positions = new List<Vector3>();
            foreach (var rank in ranks)
            {
                var card = new Card(CardSuit.Coins, rank);
                var firstSlot = AnimationTableCardLayoutEvaluator.ResolveAvailableIndex(
                    card,
                    firstRun);
                var secondSlot = AnimationTableCardLayoutEvaluator.ResolveAvailableIndex(
                    card,
                    secondRun);
                firstRun.Add(firstSlot);
                secondRun.Add(secondSlot);
                positions.Add(AnimationTableCardLayoutEvaluator.ResolveLocalPosition(
                    firstSlot,
                    card));
            }

            Assert.That(firstRun, Is.EqualTo(secondRun));
            Assert.That(firstRun.Distinct().Count(), Is.EqualTo(ranks.Length));
            Assert.That(firstRun, Is.Not.EqualTo(Enumerable.Range(0, ranks.Length)));
            Assert.That(positions.Select(position => position.z).Distinct().Count(), Is.GreaterThan(2));
            Assert.That(positions, Has.All.Matches<Vector3>(
                position => Mathf.Abs(position.x) < 0.49f
                    && Mathf.Abs(position.z) < 0.40f));
            for (var first = 0; first < positions.Count; first++)
            {
                for (var second = first + 1; second < positions.Count; second++)
                {
                    var delta = positions[first] - positions[second];
                    Assert.That(
                        Mathf.Abs(delta.x) >= 0.21f
                            || Mathf.Abs(delta.z) >= 0.25f,
                        Is.True,
                        $"Table anchors {first} and {second} do not leave card clearance.");
                }
            }
        }

        [Test]
        public void TableLayout_UsesCenteredTwoByTwoGridOnlyForOpeningCards()
        {
            var openingSlots = new List<int>();
            var openingPositions = new List<Vector3>();
            foreach (var rank in new[]
            {
                CardRank.One,
                CardRank.Two,
                CardRank.Three,
                CardRank.Four,
            })
            {
                var card = new Card(CardSuit.Coins, rank);
                var slot = AnimationTableCardLayoutEvaluator.ResolveAvailableIndex(
                    card,
                    openingSlots,
                    true);
                openingSlots.Add(slot);
                openingPositions.Add(
                    AnimationTableCardLayoutEvaluator.ResolveLocalPosition(slot, card));
                Assert.That(
                    AnimationTableCardLayoutEvaluator.ResolveYaw(card, slot),
                    Is.Zero);
            }

            Assert.That(openingSlots, Is.EqualTo(new[] { 0, 1, 2, 3 }));
            Assert.That(
                openingPositions.Select(position => position.x).Distinct().Count(),
                Is.EqualTo(2));
            Assert.That(
                openingPositions.Select(position => position.z).Distinct().Count(),
                Is.EqualTo(2));

            var playedCard = new Card(CardSuit.Cups, CardRank.Five);
            var playedSlot = AnimationTableCardLayoutEvaluator.ResolveAvailableIndex(
                playedCard,
                openingSlots);
            var playedPosition = AnimationTableCardLayoutEvaluator.ResolveLocalPosition(
                playedSlot,
                playedCard);
            Assert.That(playedSlot, Is.InRange(4, 9));
            Assert.That(Mathf.Abs(playedPosition.z), Is.LessThan(0.29f));
        }
    }
}
