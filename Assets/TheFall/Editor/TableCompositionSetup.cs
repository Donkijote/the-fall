using System;
using System.IO;
using System.Linq;
using TheFall.Presentation.Scenes;
using TheFall.Presentation.Table;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFall.Editor
{
    public static class TableCompositionSetup
    {
        private const string ScenePath = "Assets/TheFall/Presentation/Scenes/MatchPrototype.unity";
        private const string TablePrefabPath = "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/RoundCardTable/Generated/RoundCardTable.prefab";

        [MenuItem("The Fall/Table Composition/Generate")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            var sceneRoot = roots.Single(root => root.name == "MatchPrototype");
            var camera = sceneRoot.GetComponentInChildren<Camera>(true);
            if (camera == null)
            {
                throw new InvalidOperationException("MatchPrototype requires its project-owned gameplay camera.");
            }

            camera.transform.localPosition = TableCompositionPrototype.CameraPosition;
            camera.transform.localRotation = TableCompositionPrototype.CameraRotation;
            camera.fieldOfView = 44f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.025f, 0.02f);

            var experimentRoot = sceneRoot.transform.Find("Experiment Root");
            if (experimentRoot == null)
            {
                var experimentObject = new GameObject("Experiment Root");
                experimentObject.transform.SetParent(sceneRoot.transform, false);
                experimentRoot = experimentObject.transform;
            }

            var prototype = experimentRoot.GetComponent<TableCompositionPrototype>();
            if (prototype == null)
            {
                prototype = experimentRoot.gameObject.AddComponent<TableCompositionPrototype>();
            }

            prototype.ConfigureCamera(camera);
            prototype.ConfigureTablePrototype(AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath));

            var purpose = sceneRoot.GetComponent<ScenePurpose>();
            if (purpose != null)
            {
                purpose.SetDescription("Stationary-camera table and cross-platform card-interaction prototype for shared touch, mouse, and keyboard application intents across 1v1, three-player, and opposite-teammate 2v2 safe-area-aware layouts.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The Fall table composition generated and validated.");
        }

        [MenuItem("The Fall/Table Composition/Validate")]
        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var prototype = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<TableCompositionPrototype>(true))
                .SingleOrDefault();

            if (prototype == null)
            {
                throw new BuildFailedException("MatchPrototype does not contain a TableCompositionPrototype.");
            }

            var camera = prototype.GameplayCamera;
            if (camera == null ||
                camera.transform.localPosition != TableCompositionPrototype.CameraPosition ||
                Quaternion.Angle(camera.transform.localRotation, TableCompositionPrototype.CameraRotation) > 0.01f ||
                !Mathf.Approximately(camera.fieldOfView, 44f))
            {
                throw new BuildFailedException("The MatchPrototype gameplay camera does not match the stationary prototype parameters.");
            }

            if (prototype.TablePrototypePrefab == null)
            {
                throw new BuildFailedException("MatchPrototype does not reference the approved V0 table prefab.");
            }
        }

        [MenuItem("The Fall/Table Composition/Capture Validation Set")]
        public static void CaptureValidationSet()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var prototype = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<TableCompositionPrototype>(true))
                .Single();
            var camera = prototype.GameplayCamera;
            Directory.CreateDirectory("Logs");

            foreach (TableSeatingMode mode in Enum.GetValues(typeof(TableSeatingMode)))
            {
                Capture(
                    prototype,
                    camera,
                    mode,
                    new Vector2Int(844, 390),
                    new Rect(36f, 0f, 772f, 390f),
                    $"Logs/TableComposition-{mode}-Landscape.png");
                Capture(
                    prototype,
                    camera,
                    mode,
                    new Vector2Int(1024, 768),
                    new Rect(0f, 24f, 1024f, 720f),
                    $"Logs/TableComposition-{mode}-TabletLandscape.png");
            }

            prototype.ClearEditorPreview();
            camera.targetTexture = null;
            Debug.Log("The Fall table composition validation captures written to Logs.");
        }

        [MenuItem("The Fall/Table Composition/Capture Representative View")]
        public static void CaptureRepresentativeView()
        {
            var statistics = CaptureRepresentativeView("Logs/TableComposition-Representative.png");
            Debug.Log($"Representative table capture: {statistics.Triangles:N0} triangles, {statistics.Vertices:N0} vertices, {statistics.RendererSubmissions:N0} renderer/material submissions.");
        }

        internal static CaptureStatistics CaptureRepresentativeView(string outputPath)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var prototype = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<TableCompositionPrototype>(true))
                .Single();
            var camera = prototype.GameplayCamera;
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "Logs");

            var statistics = Capture(
                prototype,
                camera,
                TableSeatingMode.TwoVersusTwo,
                new Vector2Int(1920, 1080),
                new Rect(0f, 0f, 1920f, 1080f),
                outputPath);
            prototype.ClearEditorPreview();
            camera.targetTexture = null;
            return statistics;
        }

        private static CaptureStatistics Capture(
            TableCompositionPrototype prototype,
            Camera camera,
            TableSeatingMode mode,
            Vector2Int viewport,
            Rect safeArea,
            string outputPath)
        {
            var renderTexture = new RenderTexture(viewport.x, viewport.y, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(viewport.x, viewport.y, TextureFormat.RGB24, false);
            var previousActive = RenderTexture.active;

            try
            {
                camera.targetTexture = renderTexture;
                camera.aspect = (float)viewport.x / viewport.y;
                camera.ResetProjectionMatrix();
                prototype.BuildEditorPreview(mode, viewport, safeArea);
                camera.Render();
                camera.Render();
                var statistics = CalculateCaptureStatistics(prototype);
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0f, 0f, viewport.x, viewport.y), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                return statistics;
            }
            finally
            {
                camera.targetTexture = null;
                camera.ResetAspect();
                camera.ResetProjectionMatrix();
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static CaptureStatistics CalculateCaptureStatistics(TableCompositionPrototype prototype)
        {
            var meshFilters = prototype.GetComponentsInChildren<MeshFilter>(false)
                .Where(filter =>
                {
                    var renderer = filter.GetComponent<Renderer>();
                    return renderer != null && renderer.enabled && filter.sharedMesh != null;
                })
                .ToArray();
            var renderers = prototype.GetComponentsInChildren<Renderer>(false)
                .Where(renderer => renderer.enabled)
                .ToArray();

            return new CaptureStatistics(
                meshFilters.Sum(filter => filter.sharedMesh.triangles.Length / 3),
                meshFilters.Sum(filter => filter.sharedMesh.vertexCount),
                renderers.Sum(renderer => renderer.sharedMaterials.Length));
        }

        internal readonly struct CaptureStatistics
        {
            public CaptureStatistics(int triangles, int vertices, int rendererSubmissions)
            {
                Triangles = triangles;
                Vertices = vertices;
                RendererSubmissions = rendererSubmissions;
            }

            public int Triangles { get; }

            public int Vertices { get; }

            public int RendererSubmissions { get; }
        }
    }
}
