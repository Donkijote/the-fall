using System.Linq;
using NUnit.Framework;
using TheFall.Presentation.AssetReview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFall.Tests.EditMode
{
    public sealed class PrototypeAssetReviewEditModeTests
    {
        private const string ScenePath = "Assets/TheFall/Presentation/Scenes/AssetReview.unity";

        [Test]
        public void AssetReviewScene_ContainsApprovedTableAndInspectionRig()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var root = scene.GetRootGameObjects().Single(gameObject => gameObject.name == "AssetReview");
            var table = root.transform.Find("Round Card Table");
            var chair = root.transform.Find("Simple Chair");
            var character = root.transform.Find("Warm Challenger");
            var controller = root.GetComponentInChildren<PrototypeAssetReviewController>(true);

            Assert.That(table, Is.Not.Null);
            Assert.That(chair, Is.Not.Null);
            Assert.That(character, Is.Not.Null);
            Assert.That(table.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(1));
            Assert.That(chair.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(1));
            Assert.That(character.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(1));
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.ReviewCamera, Is.Not.Null);
            Assert.That(controller.FocusTarget, Is.Not.Null);
            Assert.That(controller.ReviewTargetCount, Is.EqualTo(3));
            Assert.That(root.GetComponentsInChildren<Light>(true), Has.Length.EqualTo(2));
            Assert.That(EditorBuildSettings.scenes.Any(candidate =>
                candidate.enabled && candidate.path == ScenePath), Is.True);
        }
    }
}
