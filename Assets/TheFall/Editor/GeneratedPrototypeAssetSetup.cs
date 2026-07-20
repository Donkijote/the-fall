using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TheFall.Editor
{
    public static class GeneratedPrototypeAssetSetup
    {
        public const string ChairRoot =
            "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/SimpleChair";
        public const string ChairPrefabPath = ChairRoot + "/Generated/SimpleChair.prefab";
        public const string CharacterRoot =
            "Assets/TheFall/Content/PrototypeAssets/Models/Characters/WarmChallenger";
        public const string CharacterPrefabPath = CharacterRoot + "/Generated/WarmChallenger.prefab";

        private const int RuntimeTextureSize = 1024;

        private static readonly AssetSpec Chair = new AssetSpec(
            "SimpleChair",
            "Simple Chair",
            ChairRoot,
            1f,
            12336,
            ModelImporterMeshCompression.Medium,
            0.75f,
            0.5f,
            ColliderKind.Box);

        private static readonly AssetSpec Character = new AssetSpec(
            "WarmChallenger",
            "Warm Challenger",
            CharacterRoot,
            1.78f,
            366508,
            ModelImporterMeshCompression.Off,
            0.65f,
            0.42f,
            ColliderKind.Capsule);

        [MenuItem("The Fall/Prototype Assets/Generate Chair and Character")]
        public static void GenerateAll()
        {
            Generate(Chair);
            Generate(Character);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateAll();
            Debug.Log("The Fall generated and validated SimpleChair and WarmChallenger.");
        }

        [MenuItem("The Fall/Prototype Assets/Validate Chair and Character")]
        public static void ValidateAll()
        {
            Validate(Chair);
            Validate(Character);
        }

        private static void Generate(AssetSpec spec)
        {
            RequireSourceFiles(spec);
            Directory.CreateDirectory(spec.GeneratedRoot);
            AssetDatabase.Refresh();

            ConfigureModelImporter(spec);
            ConfigureTextureImporter(spec.AlbedoPath, TextureImporterType.Default, true, RuntimeTextureSize, true);
            ConfigureTextureImporter(spec.NormalPath, TextureImporterType.NormalMap, false, RuntimeTextureSize, true);
            ConfigureTextureImporter(spec.MetallicPath, TextureImporterType.Default, false, RuntimeTextureSize, true);
            ConfigureTextureImporter(spec.RoughnessPath, TextureImporterType.Default, false, RuntimeTextureSize, true);
            ConfigureTextureImporter(spec.EmissionPath, TextureImporterType.Default, false, 256, false);

            CreatePackedMask(spec);
            AssetDatabase.ImportAsset(spec.MaskPath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureTextureImporter(spec.MaskPath, TextureImporterType.Default, false, RuntimeTextureSize, true);
            CreateMaterial(spec);
            CreatePrefab(spec);
        }

        private static void Validate(AssetSpec spec)
        {
            RequireSourceFiles(spec);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(spec.ModelPath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(spec.MaterialPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(spec.AlbedoPath);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(spec.NormalPath);
            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(spec.MaskPath);

            if (model == null || material == null || prefab == null || albedo == null || normal == null || mask == null)
            {
                throw new BuildFailedException($"{spec.DisplayName} import is incomplete.");
            }

            if (model.name.Contains("Meshy", StringComparison.OrdinalIgnoreCase) ||
                spec.ModelPath.Contains("Meshy", StringComparison.OrdinalIgnoreCase))
            {
                throw new BuildFailedException($"{spec.DisplayName} still exposes a vendor-generated asset name.");
            }

            var meshFilters = model.GetComponentsInChildren<MeshFilter>(true);
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            var skinnedRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (meshFilters.Length != 1 || renderers.Length != 1 || skinnedRenderers.Length != 0)
            {
                throw new BuildFailedException(
                    $"{spec.DisplayName} must remain one static mesh/renderer; found {meshFilters.Length}, {renderers.Length}, and {skinnedRenderers.Length} skinned renderers.");
            }

            var mesh = meshFilters[0].sharedMesh;
            var triangleCount = 0;
            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                if (mesh.GetTopology(submesh) == MeshTopology.Triangles)
                {
                    triangleCount += (int)mesh.GetIndexCount(submesh) / 3;
                }
            }

            if (triangleCount != spec.ExpectedTriangles ||
                mesh.uv.Length != mesh.vertexCount ||
                mesh.normals.Length != mesh.vertexCount ||
                mesh.tangents.Length != mesh.vertexCount)
            {
                throw new BuildFailedException(
                    $"{spec.DisplayName} geometry changed: {triangleCount:N0} triangles, {mesh.vertexCount:N0} vertices, UV0 {mesh.uv.Length:N0}.");
            }

            var importer = AssetImporter.GetAtPath(spec.ModelPath) as ModelImporter;
            if (importer == null || importer.importAnimation || importer.isReadable)
            {
                throw new BuildFailedException($"{spec.DisplayName} must import as a static, CPU-unreadable prototype.");
            }

            if (albedo.width != RuntimeTextureSize || normal.width != RuntimeTextureSize || mask.width != RuntimeTextureSize)
            {
                throw new BuildFailedException($"{spec.DisplayName} runtime textures must import at 1024 pixels.");
            }

            if (material.shader == null || material.shader.name != "Universal Render Pipeline/Lit" ||
                material.GetTexture("_BaseMap") != albedo ||
                material.GetTexture("_BumpMap") != normal ||
                material.GetTexture("_MetallicGlossMap") != mask ||
                material.IsKeywordEnabled("_EMISSION"))
            {
                throw new BuildFailedException($"{spec.DisplayName} material does not match the approved URP PBR mapping.");
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new BuildFailedException($"{spec.DisplayName} prefab could not be instantiated.");
            }

            try
            {
                var prefabRenderers = instance.GetComponentsInChildren<Renderer>(true);
                var bounds = CalculateBounds(prefabRenderers);
                var materials = prefabRenderers.SelectMany(renderer => renderer.sharedMaterials).Distinct().ToArray();
                if (Mathf.Abs(bounds.size.y - spec.TargetHeightMetres) > 0.01f ||
                    Mathf.Abs(bounds.min.y) > 0.01f ||
                    materials.Length != 1 || materials[0] != material ||
                    instance.GetComponents<Collider>().Length != 1)
                {
                    throw new BuildFailedException(
                        $"{spec.DisplayName} prefab scale, pivot, material, or collider is invalid: size {bounds.size}, floor {bounds.min.y:F3}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void RequireSourceFiles(AssetSpec spec)
        {
            foreach (var path in new[]
            {
                spec.ModelPath,
                spec.AlbedoPath,
                spec.NormalPath,
                spec.MetallicPath,
                spec.RoughnessPath,
                spec.EmissionPath,
            })
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"{spec.DisplayName} source package is incomplete.", path);
                }
            }
        }

        private static void ConfigureModelImporter(AssetSpec spec)
        {
            AssetDatabase.ImportAsset(spec.ModelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(spec.ModelPath) as ModelImporter;
            if (importer == null)
            {
                throw new BuildFailedException($"{spec.DisplayName} does not have a ModelImporter.");
            }

            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.isReadable = false;
            importer.meshCompression = spec.MeshCompression;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importBlendShapes = false;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }

        private static void ConfigureTextureImporter(
            string path,
            TextureImporterType type,
            bool sRgb,
            int maxSize,
            bool mipmaps)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new BuildFailedException($"{path} does not have a TextureImporter.");
            }

            importer.textureType = type;
            importer.sRGBTexture = sRgb;
            importer.maxTextureSize = maxSize;
            importer.mipmapEnabled = mipmaps;
            importer.anisoLevel = mipmaps ? 4 : 1;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.SaveAndReimport();
        }

        private static void CreatePackedMask(AssetSpec spec)
        {
            var metallic = LoadPng(spec.MetallicPath);
            var roughness = LoadPng(spec.RoughnessPath);

            try
            {
                if (metallic.width != 2048 || metallic.height != 2048 ||
                    roughness.width != 2048 || roughness.height != 2048)
                {
                    throw new BuildFailedException($"{spec.DisplayName} PBR sources must be 2048 square.");
                }

                var metallicPixels = metallic.GetPixels32();
                var roughnessPixels = roughness.GetPixels32();
                var packedPixels = new Color32[RuntimeTextureSize * RuntimeTextureSize];
                for (var y = 0; y < RuntimeTextureSize; y++)
                {
                    for (var x = 0; x < RuntimeTextureSize; x++)
                    {
                        var lowerLeft = y * 2 * metallic.width + x * 2;
                        var lowerRight = lowerLeft + 1;
                        var upperLeft = lowerLeft + metallic.width;
                        var upperRight = upperLeft + 1;
                        var metal = (
                            metallicPixels[lowerLeft].r + metallicPixels[lowerRight].r +
                            metallicPixels[upperLeft].r + metallicPixels[upperRight].r) / 4;
                        var rough = (
                            roughnessPixels[lowerLeft].r + roughnessPixels[lowerRight].r +
                            roughnessPixels[upperLeft].r + roughnessPixels[upperRight].r) / 4;
                        packedPixels[y * RuntimeTextureSize + x] = new Color32(
                            (byte)metal,
                            (byte)metal,
                            (byte)metal,
                            (byte)(255 - rough));
                    }
                }

                var packed = new Texture2D(RuntimeTextureSize, RuntimeTextureSize, TextureFormat.RGBA32, false, true);
                try
                {
                    packed.SetPixels32(packedPixels);
                    packed.Apply(false, false);
                    File.WriteAllBytes(spec.MaskPath, packed.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(packed);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(metallic);
                UnityEngine.Object.DestroyImmediate(roughness);
            }
        }

        private static Texture2D LoadPng(string path)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            if (!texture.LoadImage(File.ReadAllBytes(path), false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new BuildFailedException($"Could not decode {path}.");
            }

            return texture;
        }

        private static void CreateMaterial(AssetSpec spec)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new BuildFailedException("The URP Lit shader is unavailable.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(spec.MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = spec.DisplayName };
                AssetDatabase.CreateAsset(material, spec.MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(spec.AlbedoPath));
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(spec.NormalPath));
            material.SetFloat("_BumpScale", spec.NormalStrength);
            material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(spec.MaskPath));
            material.SetFloat("_Metallic", 1f);
            material.SetFloat("_Smoothness", spec.Smoothness);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.SetTexture("_EmissionMap", null);
            material.SetColor("_EmissionColor", Color.black);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
        }

        private static void CreatePrefab(AssetSpec spec)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(spec.ModelPath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(spec.MaterialPath);
            if (model == null || material == null)
            {
                throw new BuildFailedException($"{spec.DisplayName} model and material must exist before prefab generation.");
            }

            var root = new GameObject(spec.DisplayName);
            try
            {
                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                var modelInstance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (modelInstance == null)
                {
                    throw new BuildFailedException($"{spec.DisplayName} FBX could not be instantiated.");
                }

                modelInstance.name = spec.DisplayName + " Source";
                modelInstance.transform.SetParent(visual.transform, false);
                var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
                }

                var sourceBounds = CalculateBounds(renderers);
                var uniformScale = spec.TargetHeightMetres / sourceBounds.size.y;
                visual.transform.localScale = Vector3.one * uniformScale;
                var scaledBounds = CalculateBounds(renderers);
                visual.transform.localPosition = new Vector3(
                    -scaledBounds.center.x,
                    -scaledBounds.min.y,
                    -scaledBounds.center.z);

                if (spec.Collider == ColliderKind.Box)
                {
                    var collider = root.AddComponent<BoxCollider>();
                    collider.center = new Vector3(0f, spec.TargetHeightMetres * 0.5f, 0f);
                    collider.size = new Vector3(
                        scaledBounds.size.x,
                        spec.TargetHeightMetres,
                        scaledBounds.size.z);
                }
                else
                {
                    var collider = root.AddComponent<CapsuleCollider>();
                    collider.direction = 1;
                    collider.center = new Vector3(0f, spec.TargetHeightMetres * 0.5f, 0f);
                    collider.radius = 0.28f;
                    collider.height = spec.TargetHeightMetres;
                }

                PrefabUtility.SaveAsPrefabAsset(root, spec.PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Bounds CalculateBounds(Renderer[] renderers)
        {
            if (renderers.Length == 0)
            {
                throw new BuildFailedException("Generated prototype does not contain a renderer.");
            }

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        private enum ColliderKind
        {
            Box,
            Capsule,
        }

        private sealed class AssetSpec
        {
            public AssetSpec(
                string fileName,
                string displayName,
                string root,
                float targetHeightMetres,
                int expectedTriangles,
                ModelImporterMeshCompression meshCompression,
                float normalStrength,
                float smoothness,
                ColliderKind collider)
            {
                FileName = fileName;
                DisplayName = displayName;
                Root = root;
                TargetHeightMetres = targetHeightMetres;
                ExpectedTriangles = expectedTriangles;
                MeshCompression = meshCompression;
                NormalStrength = normalStrength;
                Smoothness = smoothness;
                Collider = collider;
            }

            public string FileName { get; }

            public string DisplayName { get; }

            public string Root { get; }

            public float TargetHeightMetres { get; }

            public int ExpectedTriangles { get; }

            public ModelImporterMeshCompression MeshCompression { get; }

            public float NormalStrength { get; }

            public float Smoothness { get; }

            public ColliderKind Collider { get; }

            public string SourceRoot => Root + "/Source";

            public string GeneratedRoot => Root + "/Generated";

            public string ModelPath => SourceRoot + "/" + FileName + ".fbx";

            public string AlbedoPath => SourceRoot + "/" + FileName + "_Albedo_2K.png";

            public string NormalPath => SourceRoot + "/" + FileName + "_Normal_2K.png";

            public string MetallicPath => SourceRoot + "/" + FileName + "_Metallic_2K.png";

            public string RoughnessPath => SourceRoot + "/" + FileName + "_Roughness_2K.png";

            public string EmissionPath => SourceRoot + "/" + FileName + "_Emission_2K.png";

            public string MaskPath => GeneratedRoot + "/" + FileName + "_MetallicSmoothness_1K.png";

            public string MaterialPath => GeneratedRoot + "/" + FileName + ".mat";

            public string PrefabPath => GeneratedRoot + "/" + FileName + ".prefab";
        }
    }
}
