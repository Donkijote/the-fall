using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TheFall.Tests.EditMode
{
    public sealed class TablePrototypeAssetEditModeTests
    {
        private const string AssetRoot = "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/RoundCardTable";
        private const string PrefabPath = AssetRoot + "/Generated/RoundCardTable.prefab";
        private const string MaterialPath = AssetRoot + "/Generated/RoundCardTable.mat";

        [Test]
        public void V0TablePrefab_HasApprovedDimensionsPivotMaterialAndColliders()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(material, Is.Not.Null);
            Assert.That(prefab.GetComponents<Collider>(), Has.Length.EqualTo(2));
            Assert.That(prefab.GetComponent<BoxCollider>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<CapsuleCollider>(), Is.Not.Null);

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
                Assert.That(renderers[0].sharedMaterials, Is.EqualTo(new[] { material }));
                Assert.That(bounds.size.x, Is.EqualTo(1.45f).Within(0.01f));
                Assert.That(bounds.size.y, Is.EqualTo(0.76f).Within(0.01f));
                Assert.That(bounds.size.z, Is.EqualTo(1.45f).Within(0.01f));
                Assert.That(bounds.min.y, Is.EqualTo(0f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void V0TableSource_StaysInsideDocumentedExceptionCeiling()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(AssetRoot + "/Source/RoundCardTable.fbx");
            var meshFilters = source.GetComponentsInChildren<MeshFilter>(true);
            var triangleCount = meshFilters.Sum(filter => filter.sharedMesh.triangles.Length / 3);

            Assert.That(meshFilters, Has.Length.EqualTo(1));
            Assert.That(triangleCount, Is.EqualTo(13253));
            Assert.That(triangleCount, Is.LessThanOrEqualTo(14000));
            Assert.That(meshFilters[0].sharedMesh.uv, Has.Length.EqualTo(meshFilters[0].sharedMesh.vertexCount));
        }

        [Test]
        public void V0TableMaterial_UsesOneKilopixelUrpPbrTexturesWithoutEmission()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            var albedo = material.GetTexture("_BaseMap") as Texture2D;
            var normal = material.GetTexture("_BumpMap") as Texture2D;
            var mask = material.GetTexture("_MetallicGlossMap") as Texture2D;

            Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
            Assert.That(albedo, Is.Not.Null);
            Assert.That(normal, Is.Not.Null);
            Assert.That(mask, Is.Not.Null);
            Assert.That(albedo.width, Is.EqualTo(1024));
            Assert.That(normal.width, Is.EqualTo(1024));
            Assert.That(mask.width, Is.EqualTo(1024));
            Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.True);
            Assert.That(material.IsKeywordEnabled("_METALLICSPECGLOSSMAP"), Is.True);
            Assert.That(material.IsKeywordEnabled("_EMISSION"), Is.False);
        }
    }
}
