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
            var table = GameObject.Find("Table Under Review");

            Assert.That(controller, Is.Not.Null);
            Assert.That(table, Is.Not.Null);
            Assert.That(controller.ReviewCamera, Is.Not.Null);
            Assert.That(controller.FocusTarget, Is.Not.Null);
            Assert.That(controller.DistanceMetres, Is.EqualTo(3.2f).Within(0.001f));
            Assert.That(controller.ReviewCamera.transform.position, Is.Not.EqualTo(Vector3.zero));
            Assert.That(table.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(1));
        }
    }
}
