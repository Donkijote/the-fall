using System.Collections;
using NUnit.Framework;
using TheFall.Presentation.AssetReview;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheFall.Tests.PlayMode
{
    public sealed class PrototypeAssetReviewPlayModeTests
    {
        [UnityTest]
        public IEnumerator AssetReview_PlayModeFramesApprovedTableFromOrbitCamera()
        {
            yield return SceneManager.LoadSceneAsync("AssetReview", LoadSceneMode.Single);
            yield return null;

            var controller = Object.FindAnyObjectByType<PrototypeAssetReviewController>();
            var table = GameObject.Find("Round Card Table");
            var chair = GameObject.Find("Simple Chair");
            var character = GameObject.Find("Warm Challenger");

            Assert.That(controller, Is.Not.Null);
            Assert.That(table, Is.Not.Null);
            Assert.That(chair, Is.Not.Null);
            Assert.That(character, Is.Not.Null);
            Assert.That(controller.ReviewCamera, Is.Not.Null);
            Assert.That(controller.FocusTarget, Is.Not.Null);
            Assert.That(controller.DistanceMetres, Is.EqualTo(3.2f).Within(0.001f));
            Assert.That(controller.ReviewTargetCount, Is.EqualTo(3));
            controller.SelectTarget(2);
            Assert.That(controller.SelectedTargetLabel, Is.EqualTo("Warm Challenger"));
            Assert.That(controller.ReviewCamera.transform.position, Is.Not.EqualTo(Vector3.zero));
            Assert.That(table.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(1));
        }
    }
}
