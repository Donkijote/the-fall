using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using TheFall.Presentation.Bootstrap;
using TheFall.Presentation.Input;
using TheFall.Presentation.Scenes;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Build;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TheFall.Editor
{
    public static class FoundationSetup
    {
        private const string Root = "Assets/TheFall";
        private const string SettingsDirectory = Root + "/Content/Settings";
        private const string LocalizationDirectory = Root + "/Content/Localization";
        private const string LocalizationTablesDirectory = LocalizationDirectory + "/Tables";
        private const string SceneDirectory = Root + "/Presentation/Scenes";
        private const string ScreenUiDirectory = Root + "/Presentation/UI/Screen";
        private const string WorldUiDirectory = Root + "/Presentation/UI/WorldSpace";
        private const string InputActionsPath = Root + "/Presentation/Input/TheFallInput.inputactions";
        private const string PanelSettingsPath = ScreenUiDirectory + "/ScreenPanelSettings.asset";
        private const string WorldSpaceLabelPath = WorldUiDirectory + "/WorldSpaceLabel.prefab";

        [MenuItem("The Fall/Foundation/Generate")]
        public static void Run()
        {
            EnsureFolders();
            MoveVendorAssets();
            MoveGeneratedRenderAssets();
            ConfigureRenderPipeline();
            ConfigurePlayerSettings();
            ConfigureInput();
            ConfigureLocalization();
            ConfigureUi();
            MoveGeneratedUiAssets();
            ConfigureScenes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The Fall foundation generated and validated.");
        }

        [MenuItem("The Fall/Foundation/Validate")]
        public static void Validate()
        {
            var errors = new List<string>();

            Require(PlayerSettings.companyName == "Donkijote", "Company name is not Donkijote.", errors);
            Require(PlayerSettings.productName == "The Fall", "Product name is not The Fall.", errors);
            Require(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Standalone) == "com.donkijote.thefall", "Standalone application identifier is incorrect.", errors);
            Require(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) == "com.donkijote.thefall", "Android application identifier is incorrect.", errors);
            Require(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS) == "com.donkijote.thefall", "iOS application identifier is incorrect.", errors);
            Require(PlayerSettings.defaultInterfaceOrientation == UIOrientation.AutoRotation, "Mobile orientation is not Auto Rotation.", errors);
            Require(!PlayerSettings.allowedAutorotateToPortrait, "Mobile portrait orientation must be disabled.", errors);
            Require(!PlayerSettings.allowedAutorotateToPortraitUpsideDown, "Mobile upside-down portrait orientation must be disabled.", errors);
            Require(PlayerSettings.allowedAutorotateToLandscapeLeft, "Mobile landscape-left orientation must be enabled.", errors);
            Require(PlayerSettings.allowedAutorotateToLandscapeRight, "Mobile landscape-right orientation must be enabled.", errors);
            Require(PlayerSettings.resizableWindow, "Desktop windows are not resizable.", errors);
            Require(GraphicsSettings.defaultRenderPipeline != null, "The URP asset is not configured.", errors);

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            Require(actions != null, "The project input actions asset is missing.", errors);
            if (actions != null)
            {
                foreach (var actionName in new[] { "Point", "Navigate", "Inspect", "Select", "Confirm", "Cancel" })
                {
                    Require(actions.FindAction($"Gameplay/{actionName}") != null, $"Gameplay/{actionName} is missing.", errors);
                }
            }

            Require(LocalizationEditorSettings.ActiveLocalizationSettings != null, "Localization Settings are not active.", errors);
            Require(LocalizationEditorSettings.GetLocale("en") != null, "English source locale is missing.", errors);
            Require(LocalizationEditorSettings.GetPseudoLocales().Any(locale => locale.Identifier.Code == "qps-ploc"), "Pseudo locale is missing.", errors);

            var expectedScenes = new[]
            {
                "Bootstrap",
                FirstPlayableSceneContract.LoginSceneName,
                FirstPlayableSceneContract.HubSceneName,
                FirstPlayableSceneContract.MatchSceneName,
                "MatchPrototype",
                "AnimationLab",
                "AssetReview",
            };
            var buildScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => Path.GetFileNameWithoutExtension(scene.path)).ToArray();
            Require(buildScenes.SequenceEqual(expectedScenes), "Build scenes are not configured in foundation order.", errors);

            foreach (var sceneName in expectedScenes)
            {
                Require(File.Exists($"{SceneDirectory}/{sceneName}.unity"), $"{sceneName} scene is missing.", errors);
            }

            Require(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ScreenUiDirectory + "/LoginScreen.uxml") != null, "Login UI Toolkit screen is missing.", errors);
            Require(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ScreenUiDirectory + "/HubScreen.uxml") != null, "Hub UI Toolkit screen is missing.", errors);
            Require(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ScreenUiDirectory + "/MatchScreen.uxml") != null, "Match UI Toolkit screen is missing.", errors);
            Require(AssetDatabase.LoadAssetAtPath<GameObject>(WorldSpaceLabelPath) != null, "World-space TextMeshPro foundation is missing.", errors);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException("The Fall foundation validation failed:\n- " + string.Join("\n- ", errors));
            }

            FirstPlayableFlowSetup.Validate();
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Donkijote";
            PlayerSettings.productName = "The Fall";
            PlayerSettings.bundleVersion = "0.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.donkijote.thefall");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.donkijote.thefall");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.donkijote.thefall");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SetPreloadedAssets(Array.Empty<Object>());
            EditorSettings.projectGenerationRootNamespace = "TheFall";
        }

        private static void ConfigureInput()
        {
            AssetDatabase.ImportAsset(InputActionsPath, ImportAssetOptions.ForceUpdate);
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions == null)
            {
                throw new InvalidOperationException($"Unable to load {InputActionsPath}.");
            }

            InputSystem.actions = actions;
        }

        private static void ConfigureLocalization()
        {
            var settingsPath = LocalizationDirectory + "/LocalizationSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(settingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "The Fall Localization Settings";
                AssetDatabase.CreateAsset(settings, settingsPath);
            }

            LocalizationEditorSettings.ActiveLocalizationSettings = settings;

            var english = LocalizationEditorSettings.GetLocale("en");
            if (english == null)
            {
                english = Locale.CreateLocale(SystemLanguage.English);
                english.name = "English (en)";
                AssetDatabase.CreateAsset(english, LocalizationDirectory + "/English (en).asset");
                LocalizationEditorSettings.AddLocale(english);
            }

            var pseudo = LocalizationEditorSettings.GetPseudoLocales().FirstOrDefault(locale => locale.Identifier.Code == "qps-ploc");
            if (pseudo == null)
            {
                pseudo = PseudoLocale.CreatePseudoLocale();
                pseudo.Identifier = new LocaleIdentifier("qps-ploc");
                pseudo.LocaleName = "Pseudo (qps-ploc)";
                pseudo.name = "Pseudo (qps-ploc)";
                AssetDatabase.CreateAsset(pseudo, LocalizationDirectory + "/Pseudo (qps-ploc).asset");
                LocalizationEditorSettings.AddLocale(pseudo);
            }

            LocalizationSettings.ProjectLocale = english;
            EditorUtility.SetDirty(settings);

            var uiCollection = LocalizationEditorSettings.GetStringTableCollection("UI");
            if (uiCollection == null)
            {
                uiCollection = LocalizationEditorSettings.CreateStringTableCollection("UI", LocalizationTablesDirectory);
            }

            var englishTable = uiCollection.GetTable(english.Identifier) as StringTable;
            if (englishTable != null && englishTable.GetEntry("app.title") == null)
            {
                englishTable.AddEntry("app.title", "The Fall");
                EditorUtility.SetDirty(englishTable);
                EditorUtility.SetDirty(englishTable.SharedData);
            }

            if (englishTable != null)
            {
                LocalizationEditorSettings.SetPreloadTableFlag(englishTable, true);
            }

            var addressablesSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (addressablesSettings == null)
            {
                throw new InvalidOperationException("Localization did not create Addressables settings.");
            }

            addressablesSettings.ContentStateBuildPath = "Library/TheFall/AddressablesContentState";
            EditorUtility.SetDirty(addressablesSettings);

            AssetDatabase.SaveAssets();

            MoveGeneratedAddressablesAssets();
        }

        private static void ConfigureRenderPipeline()
        {
            var rendererPath = SettingsDirectory + "/TheFallRenderer.asset";
            var pipelinePath = SettingsDirectory + "/TheFallRenderPipeline.asset";
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);

            if (pipelineAsset == null)
            {
                var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                rendererData.name = "The Fall Renderer";
                AssetDatabase.CreateAsset(rendererData, rendererPath);

                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                pipelineAsset.name = "The Fall Render Pipeline";
                AssetDatabase.CreateAsset(pipelineAsset, pipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
        }

        private static void ConfigureUi()
        {
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.name = "Screen Panel Settings";
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(WorldSpaceLabelPath) != null)
            {
                return;
            }

            var root = new GameObject("WorldSpaceLabel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            try
            {
                var rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(400f, 100f);

                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;

                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(root.transform, false);
                var labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                var label = labelObject.GetComponent<TextMeshProUGUI>();
                label.text = string.Empty;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.Normal;

                PrefabUtility.SaveAsPrefabAsset(root, WorldSpaceLabelPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void ConfigureScenes()
        {
            CreateBootstrapScene();
            CreateFirstPlayableScene(
                FirstPlayableSceneContract.LoginSceneName,
                "Full-bleed localized gateway and account-entry presentation.");
            CreateFirstPlayableScene(
                FirstPlayableSceneContract.HubSceneName,
                "Localized player hub, settings, and pre-match presentation.");
            CreateFirstPlayableScene(
                FirstPlayableSceneContract.MatchSceneName,
                "Authoritative fixed-camera 1v1 table, loading transition, match HUD, and result presentation.");
            CreatePrototypeScene("MatchPrototype", "Foundation for the first 1v1 deterministic rules and table-composition prototype.", true);
            CreatePrototypeScene("AnimationLab", "Isolated card and character presentation experiments driven by resolved events.", false);

            EditorBuildSettings.scenes = new[]
            {
                BuildScene("Bootstrap"),
                BuildScene(FirstPlayableSceneContract.LoginSceneName),
                BuildScene(FirstPlayableSceneContract.HubSceneName),
                BuildScene(FirstPlayableSceneContract.MatchSceneName),
                BuildScene("MatchPrototype"),
                BuildScene("AnimationLab"),
                BuildScene("AssetReview"),
            };
        }

        private static void CreateBootstrapScene()
        {
            if (SceneExists("Bootstrap"))
            {
                return;
            }

            var scene = NewScene();
            var root = new GameObject("Bootstrap");
            root.AddComponent<InputIntentSource>();
            root.AddComponent<CompositionRoot>();
            AddPurpose(root, "Application startup and persistent manual dependency composition.");
            SaveScene(scene, "Bootstrap");
        }

        private static void CreateFirstPlayableScene(string sceneName, string purpose)
        {
            if (SceneExists(sceneName))
            {
                return;
            }

            var scene = NewScene();
            var root = new GameObject(sceneName);
            AddPurpose(root, purpose);

            var ui = new GameObject("Screen UI", typeof(UIDocument));
            ui.transform.SetParent(root.transform, false);
            var document = ui.GetComponent<UIDocument>();
            document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                ScreenUiDirectory + "/" + sceneName + "Screen.uxml");

            SaveScene(scene, sceneName);
        }

        private static void CreatePrototypeScene(string sceneName, string purpose, bool includeWorldSpaceUi)
        {
            if (SceneExists(sceneName))
            {
                return;
            }

            var scene = NewScene();
            var root = new GameObject(sceneName);
            AddPurpose(root, purpose);
            CreateCamera(root.transform);
            CreateLight(root.transform);

            var experimentRoot = new GameObject("Experiment Root");
            experimentRoot.transform.SetParent(root.transform, false);

            if (includeWorldSpaceUi)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorldSpaceLabelPath);
                var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
                if (instance != null)
                {
                    instance.name = "World-space UI Foundation";
                    instance.transform.SetParent(root.transform, false);
                }
            }

            SaveScene(scene, sceneName);
        }

        private static Camera CreateCamera(Transform parent)
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 8f, -6f);
            cameraObject.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.035f, 0.045f);
            return camera;
        }

        private static void CreateLight(Transform parent)
        {
            var lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
        }

        private static void AddPurpose(GameObject root, string description)
        {
            var scenePurpose = root.AddComponent<ScenePurpose>();
            scenePurpose.SetDescription(description);
        }

        private static Scene NewScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void SaveScene(Scene scene, string sceneName)
        {
            EditorSceneManager.SaveScene(scene, $"{SceneDirectory}/{sceneName}.unity");
        }

        private static bool SceneExists(string sceneName)
        {
            return File.Exists($"{SceneDirectory}/{sceneName}.unity");
        }

        private static EditorBuildSettingsScene BuildScene(string sceneName)
        {
            return new EditorBuildSettingsScene($"{SceneDirectory}/{sceneName}.unity", true);
        }

        private static void MoveGeneratedRenderAssets()
        {
            MoveAssetIfPresent("Assets/DefaultVolumeProfile.asset", SettingsDirectory + "/DefaultVolumeProfile.asset");
            MoveAssetIfPresent("Assets/UniversalRenderPipelineGlobalSettings.asset", SettingsDirectory + "/UniversalRenderPipelineGlobalSettings.asset");
        }

        private static void MoveGeneratedUiAssets()
        {
            const string generatedUiPath = "Assets/UI Toolkit";
            var projectUiPath = Root + "/Content/UI Toolkit";
            if (AssetDatabase.IsValidFolder(generatedUiPath) && !AssetDatabase.IsValidFolder(projectUiPath))
            {
                var error = AssetDatabase.MoveAsset(generatedUiPath, projectUiPath);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new InvalidOperationException(error);
                }
            }
        }

        private static void MoveVendorAssets()
        {
            const string generatedTextMeshProPath = "Assets/TextMesh Pro";
            var projectTextMeshProPath = Root + "/Content/Vendor/TextMesh Pro";
            if (AssetDatabase.IsValidFolder(generatedTextMeshProPath) && !AssetDatabase.IsValidFolder(projectTextMeshProPath))
            {
                var error = AssetDatabase.MoveAsset(generatedTextMeshProPath, projectTextMeshProPath);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new InvalidOperationException(error);
                }
            }
        }

        private static void MoveGeneratedAddressablesAssets()
        {
            const string generatedAddressablesPath = "Assets/AddressableAssetsData";
            var projectAddressablesPath = LocalizationDirectory + "/Addressables";

            if (!AssetDatabase.IsValidFolder(generatedAddressablesPath))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(projectAddressablesPath))
            {
                var error = AssetDatabase.MoveAsset(generatedAddressablesPath, projectAddressablesPath);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new InvalidOperationException(error);
                }

                return;
            }

            var retainedProfilePath = projectAddressablesPath + "/ProfileDataSourceSettings.asset";
            if (AssetDatabase.LoadMainAssetAtPath(retainedProfilePath) != null)
            {
                AssetDatabase.DeleteAsset(retainedProfilePath);
            }

            var oldContentStatePath = projectAddressablesPath + "/OSX/addressables_content_state.bin";
            if (AssetDatabase.LoadMainAssetAtPath(oldContentStatePath) != null)
            {
                AssetDatabase.DeleteAsset(oldContentStatePath);
                AssetDatabase.DeleteAsset(projectAddressablesPath + "/OSX");
            }
        }

        private static void MoveAssetIfPresent(string source, string destination)
        {
            if (AssetDatabase.LoadMainAssetAtPath(source) == null || AssetDatabase.LoadMainAssetAtPath(destination) != null)
            {
                return;
            }

            var error = AssetDatabase.MoveAsset(source, destination);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(error);
            }
        }

        private static void EnsureFolders()
        {
            foreach (var folder in new[]
            {
                Root + "/Content",
                Root + "/Content/Vendor",
                SettingsDirectory,
                LocalizationDirectory,
                LocalizationTablesDirectory,
                WorldUiDirectory,
            })
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"Invalid asset folder: {path}");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void Require(bool condition, string message, ICollection<string> errors)
        {
            if (!condition)
            {
                errors.Add(message);
            }
        }
    }
}
