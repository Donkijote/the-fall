using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TheFall.Tests.EditMode
{
    public sealed class GeneratedPrototypeAssetEditModeTests
    {
        private const string ChairRoot =
            "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/SimpleChair";
        private const string CharacterRoot =
            "Assets/TheFall/Content/PrototypeAssets/Models/Characters/WarmChallenger";

        [Test]
        public void SimpleChair_UsesReadableIdentityAndValidatedStaticGeometry()
        {
            AssertStaticSource(ChairRoot, "SimpleChair", 12336, 9320);
            AssertPrefab(ChairRoot, "SimpleChair", 1f, typeof(BoxCollider));
        }

        [Test]
        public void WarmChallenger_UsesReadableIdentityAndRecordsSelectedHighResolutionGeometry()
        {
            AssertStaticSource(CharacterRoot, "WarmChallenger", 366508, 192160);
            AssertPrefab(CharacterRoot, "WarmChallenger", 1.78f, typeof(CapsuleCollider));
        }

        [Test]
        public void GeneratedPrototypeMaterials_UseOneKilopixelUrpPbrMapsWithoutEmission()
        {
            AssertMaterial(ChairRoot, "SimpleChair");
            AssertMaterial(CharacterRoot, "WarmChallenger");
        }

        [Test]
        public void RoundCardTable_NoLongerUsesLegacyTechnicalFileNames()
        {
            const string readablePath =
                "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/RoundCardTable/Generated/RoundCardTable.prefab";
            const string legacyPath =
                "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/ENV-P-ROUND-TABLE/Generated/ENV-P-ROUND-TABLE_V0.prefab";

            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(readablePath), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(legacyPath), Is.Null);
        }

        private static void AssertStaticSource(
            string root,
            string assetName,
            int expectedTriangles,
            int expectedVertices)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>($"{root}/Source/{assetName}.fbx");
            Assert.That(source, Is.Not.Null);
            Assert.That(source.name, Does.Not.Contain("Meshy"));
            Assert.That(source.GetComponentsInChildren<SkinnedMeshRenderer>(true), Is.Empty);

            var meshFilters = source.GetComponentsInChildren<MeshFilter>(true);
            Assert.That(meshFilters, Has.Length.EqualTo(1));
            var mesh = meshFilters[0].sharedMesh;
            var triangleCount = Enumerable.Range(0, mesh.subMeshCount)
                .Where(submesh => mesh.GetTopology(submesh) == MeshTopology.Triangles)
                .Sum(submesh => (int)mesh.GetIndexCount(submesh) / 3);

            Assert.That(triangleCount, Is.EqualTo(expectedTriangles));
            Assert.That(mesh.vertexCount, Is.EqualTo(expectedVertices));
            Assert.That(mesh.uv, Has.Length.EqualTo(mesh.vertexCount));
            Assert.That(mesh.normals, Has.Length.EqualTo(mesh.vertexCount));
            Assert.That(mesh.tangents, Has.Length.EqualTo(mesh.vertexCount));
        }

        private static void AssertPrefab(
            string root,
            string assetName,
            float targetHeight,
            System.Type colliderType)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{root}/Generated/{assetName}.prefab");
            var instance = Object.Instantiate(prefab);
            try
            {
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers.Skip(1))
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                Assert.That(renderers, Has.Length.EqualTo(1));
                Assert.That(bounds.size.y, Is.EqualTo(targetHeight).Within(0.01f));
                Assert.That(bounds.min.y, Is.EqualTo(0f).Within(0.01f));
                Assert.That(instance.GetComponents<Collider>(), Has.Length.EqualTo(1));
                Assert.That(instance.GetComponent(colliderType), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void AssertMaterial(string root, string assetName)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{root}/Generated/{assetName}.mat");
            var albedo = material.GetTexture("_BaseMap") as Texture2D;
            var normal = material.GetTexture("_BumpMap") as Texture2D;
            var mask = material.GetTexture("_MetallicGlossMap") as Texture2D;

            Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
            Assert.That(albedo.width, Is.EqualTo(1024));
            Assert.That(normal.width, Is.EqualTo(1024));
            Assert.That(mask.width, Is.EqualTo(1024));
            Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.True);
            Assert.That(material.IsKeywordEnabled("_METALLICSPECGLOSSMAP"), Is.True);
            Assert.That(material.IsKeywordEnabled("_EMISSION"), Is.False);
        }
    }
}
