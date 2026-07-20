using System.Collections;
using System.Linq;
using NUnit.Framework;
using TheFall.Presentation.Table;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheFall.Tests.PlayMode
{
    public sealed class TableCompositionPlayModeTests
    {
        [UnityTest]
        public IEnumerator MatchPrototype_RepresentsEverySupportedModeWithAStationaryCamera()
        {
            yield return SceneManager.LoadSceneAsync("MatchPrototype", LoadSceneMode.Single);

            var prototype = Object.FindAnyObjectByType<TableCompositionPrototype>();
            Assert.That(prototype, Is.Not.Null);

            var camera = prototype.GameplayCamera;
            var cameraPosition = camera.transform.position;
            var cameraRotation = camera.transform.rotation;
            var cameraFieldOfView = camera.fieldOfView;

            foreach (var mode in new[]
            {
                TableSeatingMode.OneVersusOne,
                TableSeatingMode.ThreePlayer,
                TableSeatingMode.TwoVersusTwo,
            })
            {
                prototype.SetSeatingMode(mode);
                var expectedSeats = TableCompositionLayout.GetSeats(mode);

                Assert.That(prototype.SeatViews, Has.Count.EqualTo(expectedSeats.Count));
                Assert.That(prototype.SeatViews.Single(view => view.IsLocal).LogicalIndex, Is.Zero);
                Assert.That(prototype.SeatViews.Where(view => !view.IsLocal).All(view => view.IsHandPrivate), Is.True);
                Assert.That(prototype.SeatViews.All(view => view.CapturedPileAnchor != null), Is.True);

                prototype.ApplyViewportForTests(new Vector2Int(844, 390), new Rect(36f, 0f, 772f, 390f));
                Assert.That(prototype.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.WideLandscape));
                Assert.That(prototype.SeatViews, Has.Count.EqualTo(expectedSeats.Count));

                prototype.ApplyViewportForTests(new Vector2Int(390, 844), new Rect(0f, 34f, 390f, 776f));
                Assert.That(prototype.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.Portrait));
                Assert.That(prototype.SeatViews, Has.Count.EqualTo(expectedSeats.Count));
            }

            yield return null;

            Assert.That(camera.transform.position, Is.EqualTo(cameraPosition));
            Assert.That(camera.transform.rotation, Is.EqualTo(cameraRotation));
            Assert.That(camera.fieldOfView, Is.EqualTo(cameraFieldOfView));
        }

        [UnityTest]
        public IEnumerator PortraitRecomposition_PreservesActivePresentationStateAndCameraPose()
        {
            yield return SceneManager.LoadSceneAsync("MatchPrototype", LoadSceneMode.Single);

            var prototype = Object.FindAnyObjectByType<TableCompositionPrototype>();
            prototype.SetSeatingMode(TableSeatingMode.TwoVersusTwo);
            prototype.SetActiveLogicalSeat(2);

            var presentationVersion = prototype.PresentationStateVersion;
            var cameraPosition = prototype.GameplayCamera.transform.position;
            var cameraRotation = prototype.GameplayCamera.transform.rotation;

            prototype.ApplyViewportForTests(new Vector2Int(844, 390), new Rect(36f, 0f, 772f, 390f));
            var landscapeRevision = prototype.LayoutRevision;
            prototype.ApplyViewportForTests(new Vector2Int(390, 844), new Rect(0f, 34f, 390f, 776f));

            Assert.That(prototype.CurrentProfile.Kind, Is.EqualTo(TableCompositionProfileKind.Portrait));
            Assert.That(prototype.LayoutRevision, Is.GreaterThan(landscapeRevision));
            Assert.That(prototype.SeatingMode, Is.EqualTo(TableSeatingMode.TwoVersusTwo));
            Assert.That(prototype.ActiveLogicalSeat, Is.EqualTo(2));
            Assert.That(prototype.PresentationStateVersion, Is.EqualTo(presentationVersion));
            Assert.That(prototype.SeatViews.Single(view => view.IsActive).LogicalIndex, Is.EqualTo(2));
            Assert.That(prototype.GameplayCamera.transform.position, Is.EqualTo(cameraPosition));
            Assert.That(prototype.GameplayCamera.transform.rotation, Is.EqualTo(cameraRotation));
        }
    }
}
