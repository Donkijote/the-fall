using System;
using System.IO;
using System.Linq;
using TheFall.Domain;
using TheFall.Presentation.Animation;
using TheFall.Presentation.Cards;
using TheFall.Presentation.Scenes;
using TheFall.Presentation.Table;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFall.Editor
{
    public static class AnimationLabSetup
    {
        private const string ScenePath = "Assets/TheFall/Presentation/Scenes/AnimationLab.unity";
        private const string ConfigurationFolder = "Assets/TheFall/Content/Animation";
        private const string ConfigurationPath = ConfigurationFolder + "/AnimationSequenceConfiguration.asset";
        private const string IterationPresetPath = ConfigurationFolder + "/AnimationFastIterationPreset.asset";
        private const string TablePrefabPath = "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/RoundCardTable/Generated/RoundCardTable.prefab";
        private const string CardCatalogPath = "Assets/TheFall/Content/Cards/Generated/CardVisualCatalog.asset";

        [MenuItem("The Fall/Animation Laboratory/Generate")]
        public static void Run()
        {
            EnsureFolder(ConfigurationFolder);
            var configuration = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(ConfigurationPath);
            if (configuration == null)
            {
                configuration = ScriptableObject.CreateInstance<AnimationSequenceConfiguration>();
                AssetDatabase.CreateAsset(configuration, ConfigurationPath);
            }

            configuration.SetPresetIdentity("Workbench Default");
            configuration.EnsureDefaults();
            EditorUtility.SetDirty(configuration);

            var iterationPreset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(IterationPresetPath);
            if (iterationPreset == null)
            {
                iterationPreset = UnityEngine.Object.Instantiate(configuration);
                iterationPreset.name = "AnimationFastIterationPreset";
                iterationPreset.SetPresetIdentity("Fast Iteration");
                iterationPreset.SetTransport(2f, true);
                AssetDatabase.CreateAsset(iterationPreset, IterationPresetPath);
            }

            iterationPreset.EnsureDefaults();
            EditorUtility.SetDirty(iterationPreset);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var sceneRoot = scene.GetRootGameObjects().Single(root => root.name == "AnimationLab");
            var camera = sceneRoot.GetComponentInChildren<Camera>(true);
            if (camera == null)
            {
                throw new InvalidOperationException("AnimationLab requires its project-owned gameplay camera.");
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

            var controller = experimentRoot.GetComponent<AnimationLabController>();
            if (controller == null)
            {
                controller = experimentRoot.gameObject.AddComponent<AnimationLabController>();
            }

            controller.Configure(
                configuration,
                new[] { configuration, iterationPreset },
                camera,
                AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath),
                AssetDatabase.LoadAssetAtPath<CardVisualCatalog>(CardCatalogPath));
            sceneRoot.GetComponent<ScenePurpose>()?.SetDescription(
                "Real-time resolved-event sequence workbench for reusable beat composition, live presentation tuning, named versioned presets, deterministic transport, 1v1 seat and profile comparison, diagnosis, and authoritative state synchronization.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The Fall animation laboratory generated and validated.");
        }

        [MenuItem("The Fall/Animation Laboratory/Validate")]
        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controller = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AnimationLabController>(true))
                .SingleOrDefault();

            if (controller == null ||
                controller.Configuration == null ||
                controller.Presets == null ||
                controller.Presets.Count < 2 ||
                controller.GameplayCamera == null ||
                controller.TablePrototypePrefab == null ||
                controller.CardVisualCatalog == null)
            {
                throw new BuildFailedException(
                    "AnimationLab is missing its controller or required presentation references.");
            }

            if (controller.GameplayCamera.transform.localPosition != TableCompositionPrototype.CameraPosition ||
                Quaternion.Angle(
                    controller.GameplayCamera.transform.localRotation,
                    TableCompositionPrototype.CameraRotation) > 0.01f ||
                !Mathf.Approximately(controller.GameplayCamera.fieldOfView, 44f))
            {
                throw new BuildFailedException(
                    "AnimationLab must retain the stationary gameplay-camera parameters.");
            }

            var configuration = controller.Configuration;
            configuration.EnsureDefaults();
            if (configuration.CardPlaySeconds < 0f ||
                configuration.NormalCaptureSeconds < 0f ||
                configuration.CascadeStepSeconds < 0f ||
                configuration.ScoreBeatSeconds < 0f ||
                configuration.TurnChangeSeconds < 0f ||
                configuration.FastForwardMultiplier < 1f)
            {
                throw new BuildFailedException(
                    "Animation timing must remain valid presentation configuration.");
            }


            if (configuration.PresetVersion != AnimationSequenceConfiguration.CurrentPresetVersion ||
                configuration.Beats.Count == 0 ||
                controller.Presets.Any(preset =>
                    preset == null ||
                    preset.PresetVersion != AnimationSequenceConfiguration.CurrentPresetVersion ||
                    preset.Beats.Count == 0))
            {
                throw new BuildFailedException(
                    "AnimationLab requires named, versioned presentation presets with reusable beats.");
            }
        }

        [MenuItem("The Fall/Animation Laboratory/Capture Validation Set")]
        public static void CaptureValidationSet()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controller = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AnimationLabController>(true))
                .Single();
            Directory.CreateDirectory("Logs");

            Capture(
                controller,
                Seat.First,
                new Vector2Int(844, 390),
                "Logs/AnimationLab-SeatOne-Landscape.png");
            Capture(
                controller,
                Seat.First,
                new Vector2Int(390, 844),
                "Logs/AnimationLab-SeatOne-Portrait.png");
            Capture(
                controller,
                Seat.Second,
                new Vector2Int(844, 390),
                "Logs/AnimationLab-SeatTwo-Landscape.png");
            Capture(
                controller,
                Seat.Second,
                new Vector2Int(390, 844),
                "Logs/AnimationLab-SeatTwo-Portrait.png");

            controller.ClearEditorPreview();
            controller.GameplayCamera.targetTexture = null;
            Debug.Log("The Fall animation laboratory validation captures written to Logs.");
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        private static void Capture(
            AnimationLabController controller,
            Seat actingSeat,
            Vector2Int viewport,
            string outputPath)
        {
            var renderTexture = new RenderTexture(viewport.x, viewport.y, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(viewport.x, viewport.y, TextureFormat.RGB24, false);
            var camera = controller.GameplayCamera;
            var previousActive = RenderTexture.active;

            try
            {
                camera.targetTexture = renderTexture;
                camera.aspect = (float)viewport.x / viewport.y;
                camera.ResetProjectionMatrix();
                controller.BuildEditorPreview(actingSeat, viewport, true);
                camera.Render();
                camera.Render();
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0f, 0f, viewport.x, viewport.y), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
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
    }
}
