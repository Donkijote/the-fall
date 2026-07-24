using NUnit.Framework;
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
            Assert.That(collection.FlipDegrees, Is.GreaterThan(180f));
        }
    }
}
