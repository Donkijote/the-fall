using System;
using System.IO;
using System.Linq;
using TheFall.Presentation.AssetReview;
using TheFall.Presentation.Scenes;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace TheFall.Editor
{
    public static class PrototypeAssetReviewSetup
    {
        public const string ScenePath = "Assets/TheFall/Presentation/Scenes/AssetReview.unity";

        private const string TablePrefabPath =
            "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/ENV-P-ROUND-TABLE/Generated/ENV-P-ROUND-TABLE_V0.prefab";
        private const string FloorMaterialPath =
            "Assets/TheFall/Presentation/AssetReview/AssetReviewFloor.mat";

        [MenuItem("The Fall/Prototype Assets/Open Asset Review Scene")]
        public static void Open()
        {
            if (!File.Exists(ScenePath))
            {
                Generate();
                return;
            }

            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("The Fall/Prototype Assets/Generate Asset Review Scene")]
        public static void Generate()
        {
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
            if (tablePrefab == null)
            {
                throw new BuildFailedException("Generate the approved V0 table prefab before the asset review scene.");
            }

            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder("Assets/TheFall/Presentation/AssetReview");
            var floorMaterial = CreateFloorMaterial();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "AssetReview";

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.29f, 0.24f, 0.2f);
            RenderSettings.ambientEquatorColor = new Color(0.14f, 0.16f, 0.18f);
            RenderSettings.ambientGroundColor = new Color(0.055f, 0.045f, 0.04f);
            RenderSettings.ambientIntensity = 0.85f;

            var root = new GameObject("AssetReview");
            var purpose = root.AddComponent<ScenePurpose>();
            purpose.SetDescription(
                "Isolated Play-mode inspection scene for approved generated prototype assets; currently presents ENV-P-ROUND-TABLE with neutral orbit controls and review lighting.");

            var table = PrefabUtility.InstantiatePrefab(tablePrefab, scene) as GameObject;
            if (table == null)
            {
                throw new BuildFailedException("The approved V0 table prefab could not be instantiated.");
            }

            table.name = "Table Under Review";
            table.transform.SetParent(root.transform, false);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Neutral Review Floor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            floor.transform.localScale = new Vector3(2f, 1f, 2f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

            var focusTarget = new GameObject("Camera Focus - Table Centre");
            focusTarget.transform.SetParent(root.transform, false);
            focusTarget.transform.localPosition = new Vector3(0f, 0.38f, 0f);

            var cameraObject = new GameObject(
                "Review Camera",
                typeof(Camera),
                typeof(AudioListener),
                typeof(PrototypeAssetReviewController));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 36f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.038f, 0.031f, 0.028f);
            cameraObject.GetComponent<PrototypeAssetReviewController>().Configure(camera, focusTarget.transform);

            CreateDirectionalLight(
                "Warm Key Light",
                root.transform,
                Quaternion.Euler(46f, -32f, 0f),
                new Color(1f, 0.86f, 0.69f),
                1.35f,
                LightShadows.Soft);
            CreateDirectionalLight(
                "Cool Fill Light",
                root.transform,
                Quaternion.Euler(32f, 145f, 0f),
                new Color(0.55f, 0.68f, 0.84f),
                0.42f,
                LightShadows.None);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The Fall generated and opened the AssetReview scene.");
        }

        [MenuItem("The Fall/Prototype Assets/Validate Asset Review Scene")]
        public static void Validate()
        {
            if (!File.Exists(ScenePath))
            {
                throw new BuildFailedException("AssetReview scene is missing.");
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            var root = roots.SingleOrDefault(candidate => candidate.name == "AssetReview");
            var controller = root == null
                ? null
                : root.GetComponentInChildren<PrototypeAssetReviewController>(true);
            var table = root == null
                ? null
                : root.transform.Find("Table Under Review");
            var floor = root == null
                ? null
                : root.transform.Find("Neutral Review Floor");

            if (root == null || controller == null || controller.ReviewCamera == null ||
                controller.FocusTarget == null || table == null || floor == null)
            {
                throw new BuildFailedException("AssetReview is missing its table, floor, camera, or orbit controller.");
            }

            if (table.GetComponentsInChildren<Renderer>(true).Length != 1 ||
                root.GetComponentsInChildren<Light>(true).Length != 2)
            {
                throw new BuildFailedException("AssetReview must contain one table renderer and two review lights.");
            }

            var buildScene = EditorBuildSettings.scenes.SingleOrDefault(candidate => candidate.path == ScenePath);
            if (string.IsNullOrEmpty(buildScene.path) || !buildScene.enabled)
            {
                throw new BuildFailedException("AssetReview must be enabled in the build scene list.");
            }
        }

        [MenuItem("The Fall/Prototype Assets/Capture Asset Review Scene")]
        public static void Capture()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controller = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeAssetReviewController>(true))
                .Single();
            var camera = controller.ReviewCamera;
            var renderTexture = new RenderTexture(1440, 900, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(1440, 900, TextureFormat.RGB24, false);
            var previousActive = RenderTexture.active;
            Directory.CreateDirectory("Logs");

            try
            {
                controller.ResetView();
                camera.targetTexture = renderTexture;
                camera.aspect = 1440f / 900f;
                camera.ResetProjectionMatrix();
                camera.Render();
                camera.Render();
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0f, 0f, 1440f, 900f), 0, 0);
                texture.Apply();
                File.WriteAllBytes("Logs/AssetReview-Table.png", texture.EncodeToPNG());
                Debug.Log("The Fall AssetReview capture written to Logs/AssetReview-Table.png.");
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

        private static Material CreateFloorMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new BuildFailedException("The URP Lit shader is unavailable.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(FloorMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "Asset Review Floor" };
                AssetDatabase.CreateAsset(material, FloorMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", new Color(0.16f, 0.135f, 0.12f));
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.12f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateDirectionalLight(
            string name,
            Transform parent,
            Quaternion rotation,
            Color color,
            float intensity,
            LightShadows shadows)
        {
            var lightObject = new GameObject(name, typeof(Light));
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localRotation = rotation;
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = shadows;
        }

        private static void EnsureBuildScene()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            var existingIndex = scenes.FindIndex(scene => scene.path == ScenePath);
            if (existingIndex >= 0)
            {
                scenes[existingIndex] = new EditorBuildSettingsScene(ScenePath, true);
            }
            else
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }
    }
}
