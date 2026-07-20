using System.Linq;
using NUnit.Framework;
using TheFall.Presentation.Table;
using UnityEngine;

namespace TheFall.Tests.EditMode
{
    public sealed class TableCompositionEditModeTests
    {
        [TestCase(TableSeatingMode.OneVersusOne, 2)]
        [TestCase(TableSeatingMode.ThreePlayer, 3)]
        [TestCase(TableSeatingMode.TwoVersusTwo, 4)]
        public void EveryMode_AnchorsLocalSeatAtBottomAndAdvancesCounterClockwise(
            TableSeatingMode mode,
            int expectedCount)
        {
            var seats = TableCompositionLayout.GetSeats(mode);

            Assert.That(seats, Has.Count.EqualTo(expectedCount));
            Assert.That(seats[0].IsLocal, Is.True);
            Assert.That(seats[0].AnchorAngleDegrees, Is.EqualTo(0f));
            Assert.That(seats.Select(seat => seat.LogicalIndex), Is.EqualTo(Enumerable.Range(0, expectedCount)));
            Assert.That(seats.Skip(1).Select(seat => seat.AnchorAngleDegrees), Is.Ordered.Ascending);
        }

        [Test]
        public void TwoVersusTwo_PlacesTeammatesOppositeAndKeepsEveryRemoteHandPrivate()
        {
            var seats = TableCompositionLayout.GetSeats(TableSeatingMode.TwoVersusTwo);

            Assert.That(seats[0].TeamIndex, Is.EqualTo(seats[2].TeamIndex));
            Assert.That(Mathf.DeltaAngle(seats[0].AnchorAngleDegrees, seats[2].AnchorAngleDegrees), Is.EqualTo(180f));
            Assert.That(seats[1].TeamIndex, Is.EqualTo(seats[3].TeamIndex));
            Assert.That(seats.Select(seat => seat.TeamIndex), Is.EqualTo(new[] { 0, 1, 0, 1 }));
            Assert.That(seats[0].IsHandPrivate, Is.False);
            Assert.That(seats.Skip(1).All(seat => seat.IsHandPrivate), Is.True);
        }

        [Test]
        public void ViewportProfiles_AuthorPortraitStandardAndWideCompositions()
        {
            Assert.That(
                TableCompositionLayout.ResolveProfile(new Vector2Int(390, 844)).Kind,
                Is.EqualTo(TableCompositionProfileKind.Portrait));
            Assert.That(
                TableCompositionLayout.ResolveProfile(new Vector2Int(1440, 1080)).Kind,
                Is.EqualTo(TableCompositionProfileKind.StandardLandscape));
            Assert.That(
                TableCompositionLayout.ResolveProfile(new Vector2Int(1920, 1080)).Kind,
                Is.EqualTo(TableCompositionProfileKind.WideLandscape));
        }

        [Test]
        public void SafeArea_IsNormalizedAndClampedToTheViewport()
        {
            var normalized = TableCompositionLayout.NormalizeSafeArea(
                new Vector2Int(390, 844),
                Rect.MinMaxRect(-10f, 24f, 400f, 810f));

            Assert.That(normalized.xMin, Is.EqualTo(0f));
            Assert.That(normalized.yMin, Is.EqualTo(24f / 844f).Within(0.0001f));
            Assert.That(normalized.xMax, Is.EqualTo(1f));
            Assert.That(normalized.yMax, Is.EqualTo(810f / 844f).Within(0.0001f));
        }
    }
}
