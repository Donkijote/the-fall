using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TheFall.Editor
{
    public static class TablePrototypeAssetSetup
    {
        private const string LegacyAssetRoot = "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/ENV-P-ROUND-TABLE";

        public const string AssetRoot = "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/RoundCardTable";
        public const string SourceRoot = AssetRoot + "/Source";
        public const string GeneratedRoot = AssetRoot + "/Generated";
        public const string ModelPath = SourceRoot + "/RoundCardTable.fbx";
        public const string AlbedoPath = SourceRoot + "/RoundCardTable_Albedo_2K.png";
        public const string NormalPath = SourceRoot + "/RoundCardTable_Normal_2K.png";
        public const string MetallicPath = SourceRoot + "/RoundCardTable_Metallic_2K.png";
        public const string RoughnessPath = SourceRoot + "/RoundCardTable_Roughness_2K.png";
        public const string EmissionPath = SourceRoot + "/RoundCardTable_Emission_2K.png";
        public const string MaskPath = GeneratedRoot + "/RoundCardTable_MetallicSmoothness_1K.png";
        public const string MaterialPath = GeneratedRoot + "/RoundCardTable.mat";
        public const string PrefabPath = GeneratedRoot + "/RoundCardTable.prefab";

        private const float TargetDiameterMetres = 1.45f;
        private const float TargetHeightMetres = 0.76f;
        private const int RuntimeTextureSize = 1024;
        private const int TriangleCeiling = 14000;

        [MenuItem("The Fall/Prototype Assets/Table/Generate")]
        public static void Generate()
        {
            MigrateReadableNames();
            RequireSourceFiles();
            Directory.CreateDirectory(GeneratedRoot);
            AssetDatabase.Refresh();

            ConfigureModelImporter();
            ConfigureTextureImporter(AlbedoPath, TextureImporterType.Default, true, RuntimeTextureSize, true);
            ConfigureTextureImporter(NormalPath, TextureImporterType.NormalMap, false, RuntimeTextureSize, true);
            ConfigureTextureImporter(MetallicPath, TextureImporterType.Default, false, RuntimeTextureSize, true);
            ConfigureTextureImporter(RoughnessPath, TextureImporterType.Default, false, RuntimeTextureSize, true);
            ConfigureTextureImporter(EmissionPath, TextureImporterType.Default, false, 256, false);

            CreatePackedMask();
            AssetDatabase.ImportAsset(MaskPath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureTextureImporter(MaskPath, TextureImporterType.Default, false, RuntimeTextureSize, true);
            CreateMaterial();
            CreatePrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The Fall V0 table asset generated and validated.");
        }

        [MenuItem("The Fall/Prototype Assets/Table/Migrate Readable Names")]
        public static void MigrateReadableNames()
        {
            if (AssetDatabase.IsValidFolder(LegacyAssetRoot) && !AssetDatabase.IsValidFolder(AssetRoot))
            {
                MoveAsset(LegacyAssetRoot, AssetRoot);
            }

            MoveAssetIfPresent(SourceRoot + "/ENV-P-ROUND-TABLE_SmartTopology.fbx", ModelPath);
            MoveAssetIfPresent(SourceRoot + "/ENV-P-ROUND-TABLE_Albedo_2K.png", AlbedoPath);
            MoveAssetIfPresent(SourceRoot + "/ENV-P-ROUND-TABLE_Normal_2K.png", NormalPath);
            MoveAssetIfPresent(SourceRoot + "/ENV-P-ROUND-TABLE_Metallic_2K.png", MetallicPath);
            MoveAssetIfPresent(SourceRoot + "/ENV-P-ROUND-TABLE_Roughness_2K.png", RoughnessPath);
            MoveAssetIfPresent(SourceRoot + "/ENV-P-ROUND-TABLE_Emission_2K.png", EmissionPath);
            MoveAssetIfPresent(GeneratedRoot + "/ENV-P-ROUND-TABLE_MetallicSmoothness_1K.png", MaskPath);
            MoveAssetIfPresent(GeneratedRoot + "/ENV-P-ROUND-TABLE_V0.mat", MaterialPath);
            MoveAssetIfPresent(GeneratedRoot + "/ENV-P-ROUND-TABLE_V0.prefab", PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("The Fall/Prototype Assets/Table/Validate")]
        public static void Validate()
        {
            RequireSourceFiles();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath);
            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(MaskPath);

            if (model == null || material == null || prefab == null || albedo == null || normal == null || mask == null)
            {
                throw new BuildFailedException("The V0 table import is incomplete. Run the table asset generator.");
            }

            var modelMeshFilters = model.GetComponentsInChildren<MeshFilter>(true);
            var modelRenderers = model.GetComponentsInChildren<Renderer>(true);
            var triangleCount = modelMeshFilters.Sum(filter => filter.sharedMesh.triangles.Length / 3);
            if (modelMeshFilters.Length != 1 || modelRenderers.Length != 1 || triangleCount > TriangleCeiling)
            {
                throw new BuildFailedException(
                    $"The table must remain one mesh/renderer and at most {TriangleCeiling:N0} triangles; found {modelMeshFilters.Length}, {modelRenderers.Length}, and {triangleCount:N0}.");
            }

            var mesh = modelMeshFilters[0].sharedMesh;
            if (mesh.uv == null || mesh.uv.Length != mesh.vertexCount)
            {
                throw new BuildFailedException("The table mesh requires complete UV0 coordinates.");
            }

            if (albedo.width != RuntimeTextureSize || normal.width != RuntimeTextureSize || mask.width != RuntimeTextureSize)
            {
                throw new BuildFailedException("The table runtime textures must import at 1024 pixels.");
            }

            if (material.shader == null || material.shader.name != "Universal Render Pipeline/Lit" ||
                material.GetTexture("_BaseMap") != albedo ||
                material.GetTexture("_BumpMap") != normal ||
                material.GetTexture("_MetallicGlossMap") != mask ||
                material.IsKeywordEnabled("_EMISSION"))
            {
                throw new BuildFailedException("The table material does not match the approved URP PBR mapping.");
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new BuildFailedException("The V0 table prefab could not be instantiated.");
            }

            try
            {
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                var bounds = CalculateBounds(renderers);
                var materialSlots = renderers.SelectMany(renderer => renderer.sharedMaterials).Distinct().ToArray();
                var colliders = instance.GetComponents<Collider>();

                if (!Approximately(bounds.size.x, TargetDiameterMetres, 0.01f) ||
                    !Approximately(bounds.size.z, TargetDiameterMetres, 0.01f) ||
                    !Approximately(bounds.size.y, TargetHeightMetres, 0.01f) ||
                    !Approximately(bounds.min.y, 0f, 0.01f))
                {
                    throw new BuildFailedException(
                        $"The table prefab bounds/pivot are invalid: size {bounds.size}, floor {bounds.min.y:F3}.");
                }

                if (materialSlots.Length != 1 || materialSlots[0] != material)
                {
                    throw new BuildFailedException("The table prefab must use exactly one approved material.");
                }

                if (colliders.Length != 2 || !colliders.Any(collider => collider is BoxCollider) ||
                    !colliders.Any(collider => collider is CapsuleCollider))
                {
                    throw new BuildFailedException("The table prefab requires one tabletop box and one pedestal capsule collider.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [MenuItem("The Fall/Prototype Assets/Table/Capture 1K-2K Comparison")]
        public static void CaptureTextureComparison()
        {
            TableCompositionSetup.CaptureStatistics twoKilopixel;
            try
            {
                ConfigureTextureImporter(AlbedoPath, TextureImporterType.Default, true, 2048, true);
                ConfigureTextureImporter(NormalPath, TextureImporterType.NormalMap, false, 2048, true);
                twoKilopixel = TableCompositionSetup.CaptureRepresentativeView("Logs/TablePrototype-2K.png");
            }
            finally
            {
                ConfigureTextureImporter(AlbedoPath, TextureImporterType.Default, true, RuntimeTextureSize, true);
                ConfigureTextureImporter(NormalPath, TextureImporterType.NormalMap, false, RuntimeTextureSize, true);
            }

            var oneKilopixel = TableCompositionSetup.CaptureRepresentativeView("Logs/TablePrototype-1K.png");
            Validate();

            Debug.Log(
                $"Table texture comparison captured. 2K scene: {twoKilopixel.Triangles:N0} triangles, {twoKilopixel.Vertices:N0} vertices, {twoKilopixel.RendererSubmissions:N0} renderer/material submissions. " +
                $"1K scene: {oneKilopixel.Triangles:N0} triangles, {oneKilopixel.Vertices:N0} vertices, {oneKilopixel.RendererSubmissions:N0} renderer/material submissions. Runtime import left at 1K.");
        }

        private static void RequireSourceFiles()
        {
            foreach (var path in new[] { ModelPath, AlbedoPath, NormalPath, MetallicPath, RoughnessPath, EmissionPath })
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("The approved Meshy table package is incomplete.", path);
                }
            }
        }

        private static void ConfigureModelImporter()
        {
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null)
            {
                throw new BuildFailedException("The approved table FBX does not have a ModelImporter.");
            }

            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
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

        private static void CreatePackedMask()
        {
            var metallic = LoadPng(MetallicPath);
            var roughness = LoadPng(RoughnessPath);

            try
            {
                if (metallic.width != roughness.width || metallic.height != roughness.height ||
                    metallic.width != RuntimeTextureSize * 2 || metallic.height != RuntimeTextureSize * 2)
                {
                    throw new BuildFailedException("The approved metallic and roughness sources must both be 2048 square.");
                }

                var metallicPixels = metallic.GetPixels32();
                var roughnessPixels = roughness.GetPixels32();
                var packedPixels = new Color32[RuntimeTextureSize * RuntimeTextureSize];

                for (var y = 0; y < RuntimeTextureSize; y++)
                {
                    for (var x = 0; x < RuntimeTextureSize; x++)
                    {
                        var sourceX = x * 2;
                        var sourceY = y * 2;
                        var lowerLeft = sourceY * metallic.width + sourceX;
                        var lowerRight = lowerLeft + 1;
                        var upperLeft = lowerLeft + metallic.width;
                        var upperRight = upperLeft + 1;
                        var metal = (
                            metallicPixels[lowerLeft].r +
                            metallicPixels[lowerRight].r +
                            metallicPixels[upperLeft].r +
                            metallicPixels[upperRight].r) / 4;
                        var rough = (
                            roughnessPixels[lowerLeft].r +
                            roughnessPixels[lowerRight].r +
                            roughnessPixels[upperLeft].r +
                            roughnessPixels[upperRight].r) / 4;
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
                    File.WriteAllBytes(MaskPath, packed.EncodeToPNG());
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

        private static void CreateMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new BuildFailedException("The URP Lit shader is unavailable.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "Round Card Table" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath));
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath));
            material.SetFloat("_BumpScale", 0.75f);
            material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(MaskPath));
            material.SetFloat("_Metallic", 1f);
            material.SetFloat("_Smoothness", 0.52f);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.SetTexture("_EmissionMap", null);
            material.SetColor("_EmissionColor", Color.black);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
        }

        private static void CreatePrefab()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (model == null || material == null)
            {
                throw new BuildFailedException("The table model and material must exist before prefab generation.");
            }

            var root = new GameObject("Round Card Table");
            try
            {
                var visualContainer = new GameObject("Visual");
                visualContainer.transform.SetParent(root.transform, false);

                var modelInstance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (modelInstance == null)
                {
                    throw new BuildFailedException("The table FBX could not be instantiated.");
                }

                modelInstance.name = "Smart Topology Source";
                modelInstance.transform.SetParent(visualContainer.transform, false);
                var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
                }

                var sourceBounds = CalculateBounds(renderers);
                if (sourceBounds.size.x <= 0f || sourceBounds.size.y <= 0f || sourceBounds.size.z <= 0f)
                {
                    throw new BuildFailedException("The table FBX has invalid bounds.");
                }

                visualContainer.transform.localScale = new Vector3(
                    TargetDiameterMetres / sourceBounds.size.x,
                    TargetHeightMetres / sourceBounds.size.y,
                    TargetDiameterMetres / sourceBounds.size.z);
                var scaledBounds = CalculateBounds(renderers);
                visualContainer.transform.localPosition = new Vector3(0f, -scaledBounds.min.y, 0f);

                var tabletop = root.AddComponent<BoxCollider>();
                tabletop.center = new Vector3(0f, 0.715f, 0f);
                tabletop.size = new Vector3(TargetDiameterMetres, 0.09f, TargetDiameterMetres);

                var pedestal = root.AddComponent<CapsuleCollider>();
                pedestal.direction = 1;
                pedestal.center = new Vector3(0f, 0.34f, 0f);
                pedestal.radius = 0.25f;
                pedestal.height = 0.68f;

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
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
                throw new BuildFailedException("The table does not contain a renderer.");
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static bool Approximately(float value, float expected, float tolerance)
        {
            return Mathf.Abs(value - expected) <= tolerance;
        }

        private static void MoveAssetIfPresent(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) != null &&
                AssetDatabase.LoadMainAssetAtPath(destinationPath) == null)
            {
                MoveAsset(sourcePath, destinationPath);
            }
        }

        private static void MoveAsset(string sourcePath, string destinationPath)
        {
            var error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new BuildFailedException(
                    $"Could not rename generated asset '{sourcePath}' to '{destinationPath}': {error}");
            }
        }
    }
}
