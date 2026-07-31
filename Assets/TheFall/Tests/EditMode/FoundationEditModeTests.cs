using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using TheFall.Application.Input;
using TheFall.Domain;
using TheFall.Presentation.Bootstrap;
using TheFall.Presentation.Match;
using TheFall.Presentation.Scenes;
using TheFall.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace TheFall.Tests.EditMode
{
    public sealed class FoundationEditModeTests
    {
        private const string UiTableGuid = "5366e2894cb2a41e782277c1311dfc07";

        private static string ScreenAssetPath(string screenAssetName)
        {
            var screenName = screenAssetName.EndsWith("Screen")
                ? screenAssetName.Substring(0, screenAssetName.Length - "Screen".Length)
                : screenAssetName;
            return $"Assets/TheFall/Presentation/UI/Screen/{screenName}/UI/{screenAssetName}.uxml";
        }

        [Test]
        public void DomainAssembly_DoesNotReferenceUnityEngine()
        {
            var references = typeof(DomainAssembly).Assembly.GetReferencedAssemblies();

            Assert.That(references.Any(reference => reference.Name.StartsWith("UnityEngine")), Is.False);
        }

        [Test]
        public void ProjectInput_ContainsEverySharedIntentAction()
        {
            Assert.That(InputSystem.actions, Is.Not.Null);

            foreach (PlayerIntentKind intent in System.Enum.GetValues(typeof(PlayerIntentKind)))
            {
                Assert.That(InputSystem.actions.FindAction($"Gameplay/{intent}"), Is.Not.Null);
            }
        }

        [Test]
        public void ProjectInput_MapsTouchMouseAndKeyboardWithoutConflictingConfirmOrCancelBindings()
        {
            var inspect = InputSystem.actions.FindAction("Gameplay/Inspect");
            var select = InputSystem.actions.FindAction("Gameplay/Select");
            var confirm = InputSystem.actions.FindAction("Gameplay/Confirm");
            var cancel = InputSystem.actions.FindAction("Gameplay/Cancel");

            Assert.That(inspect.bindings.Any(binding =>
                binding.path == "<Touchscreen>/primaryTouch/press" &&
                binding.interactions.Contains("Hold")), Is.True);
            Assert.That(select.bindings.Any(binding =>
                binding.path == "<Touchscreen>/primaryTouch/press" &&
                binding.interactions.Contains("Tap")), Is.True);
            Assert.That(confirm.bindings.Any(binding => binding.path.Contains("Touchscreen")), Is.False);

            Assert.That(inspect.bindings.Any(binding => binding.path == "<Mouse>/rightButton"), Is.True);
            Assert.That(select.bindings.Any(binding => binding.path == "<Mouse>/leftButton"), Is.True);
            Assert.That(cancel.bindings.Any(binding => binding.path == "<Mouse>/rightButton"), Is.False);

            Assert.That(inspect.bindings.Any(binding => binding.path == "<Keyboard>/i"), Is.True);
            Assert.That(select.bindings.Any(binding => binding.path == "<Keyboard>/e"), Is.True);
            Assert.That(confirm.bindings.Any(binding => binding.path == "<Keyboard>/enter"), Is.True);
            Assert.That(cancel.bindings.Any(binding => binding.path == "<Keyboard>/escape"), Is.True);
        }

        [Test]
        public void MobilePlayerSettings_AllowOnlyBothLandscapeDirections()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.AutoRotation));
            Assert.That(PlayerSettings.allowedAutorotateToPortrait, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToPortraitUpsideDown, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeLeft, Is.True);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeRight, Is.True);
        }

        [TestCase("Login", FirstPlayableSceneKind.Login, AdaptiveUiProfile.PhoneLandscape, false)]
        [TestCase("Hub", FirstPlayableSceneKind.Hub, AdaptiveUiProfile.PhoneLandscape, false)]
        [TestCase("Match", FirstPlayableSceneKind.Match, AdaptiveUiProfile.PhoneLandscape, true)]
        public void FirstPlayableScenes_OwnOnlyTheirPresentationLifecycle(
            string sceneName,
            FirstPlayableSceneKind expectedKind,
            AdaptiveUiProfile expectedPreviewProfile,
            bool expectsTable)
        {
            var scene = EditorSceneManager.OpenScene(
                $"Assets/TheFall/Presentation/Scenes/{sceneName}.unity",
                OpenSceneMode.Single);
            var controller = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<FirstPlayableFlowController>(true))
                .Single();
            var hasTable = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<FirstPlayableTablePresentation>(true))
                .Any();
            var document = controller.GetComponent<UIDocument>();
            var documentRoot = document.rootVisualElement;
            var previewRoot = documentRoot.Q<AdaptiveUiPreviewRoot>();
            var expectedScreen = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                ScreenAssetPath($"{sceneName}Screen"));

            Assert.That(controller.SceneKind, Is.EqualTo(expectedKind));
            Assert.That(controller.enabled, Is.True);
            Assert.That(controller.HasConfiguredScreenAssets, Is.True);
            Assert.That(document.visualTreeAsset, Is.SameAs(expectedScreen));
            Assert.That(previewRoot, Is.Not.Null);
            Assert.That(previewRoot.PreviewProfile, Is.EqualTo(expectedPreviewProfile));
            Assert.That(previewRoot.ClassListContains("authoring-preview-root"), Is.False);
            Assert.That(previewRoot.ClassListContains("screen-root"), Is.False);
            Assert.That(documentRoot.ClassListContains("screen-root"), Is.True);
            Assert.That(
                documentRoot.ClassListContains(
                    AdaptiveUiFoundation.ProfileClass(expectedPreviewProfile)),
                Is.True);
            Assert.That(
                documentRoot.ClassListContains("profile-mobile-landscape"),
                Is.EqualTo(expectedPreviewProfile != AdaptiveUiProfile.Desktop));
            Assert.That(hasTable, Is.EqualTo(expectsTable));
        }

        [TestCase("LoginScreen")]
        [TestCase("HubScreen")]
        [TestCase("SetupScreen")]
        [TestCase("LoadingScreen")]
        [TestCase("MatchScreen")]
        [TestCase("ResultScreen")]
        public void ScreenAssets_DoNotExposePlayerScrolling(string screenAssetName)
        {
            var assetPath = ScreenAssetPath(screenAssetName);
            var document = XDocument.Load(Path.GetFullPath(assetPath));

            Assert.That(
                document.Descendants().Any(element => element.Name.LocalName == "ScrollView"),
                Is.False,
                $"{screenAssetName} must reflow inside its viewport instead of exposing scrolling.");
        }

        [TestCase("LoginScreen")]
        [TestCase("HubScreen")]
        [TestCase("SetupScreen")]
        [TestCase("LoadingScreen")]
        [TestCase("MatchScreen")]
        [TestCase("ResultScreen")]
        public void ScreenAssets_DelegateInteractiveInsetsToOneSafeAreaElement(string screenAssetName)
        {
            var assetPath = ScreenAssetPath(screenAssetName);
            var document = XDocument.Load(Path.GetFullPath(assetPath));
            var safeArea = document
                .Descendants()
                .Single(element => element.Name.LocalName == "Bitbebop.SafeArea");
            var interactiveControls = document
                .Descendants()
                .Where(element =>
                    element.Name.LocalName == "Button"
                    || element.Name.LocalName == "Toggle"
                    || element.Name.LocalName == "TextField")
                .ToArray();

            Assert.That(interactiveControls, Is.Not.Empty);
            foreach (var control in interactiveControls)
            {
                Assert.That(
                    control.Ancestors().Contains(safeArea),
                    Is.True,
                    $"{screenAssetName}/{control.Attribute("name")?.Value} must remain inside its screen-owned SafeArea.");
            }
        }

        [TestCase("LoginScreen", AdaptiveUiProfile.PhoneLandscape)]
        [TestCase("HubScreen", AdaptiveUiProfile.PhoneLandscape)]
        [TestCase("SetupScreen", AdaptiveUiProfile.PhoneLandscape)]
        [TestCase("LoadingScreen", AdaptiveUiProfile.PhoneLandscape)]
        [TestCase("MatchScreen", AdaptiveUiProfile.PhoneLandscape)]
        [TestCase("ResultScreen", AdaptiveUiProfile.PhoneLandscape)]
        public void ScreenAssets_ExposeSwitchableAuthoringPreviewRoot(
            string screenAssetName,
            AdaptiveUiProfile expectedPreviewProfile)
        {
            var assetPath = ScreenAssetPath(screenAssetName);
            var document = XDocument.Load(Path.GetFullPath(assetPath));
            var previewRoot = document
                .Descendants()
                .Single(element => element.Name.LocalName.EndsWith("AdaptiveUiPreviewRoot"));
            var safeArea = document
                .Descendants()
                .Single(element => element.Name.LocalName == "Bitbebop.SafeArea");

            Assert.That(
                previewRoot.Attribute("preview-profile")?.Value,
                Is.EqualTo(expectedPreviewProfile.ToString()));
            Assert.That(safeArea.Ancestors().Contains(previewRoot), Is.True);
        }

        [Test]
        public void HubStyles_KeepProfileCascadeSplitAndExplicit()
        {
            var document = XDocument.Load(Path.GetFullPath(
                ScreenAssetPath("HubScreen")));
            var hubStyleSources = document.Root?
                .Elements()
                .Where(element => element.Name.LocalName == "Style")
                .Select(element => element.Attribute("src")?.Value)
                .Where(source => source != null && source.Contains("HubScreen."))
                .ToArray();

            Assert.That(
                hubStyleSources,
                Has.Length.EqualTo(4));
            var expectedHubStyles = new[]
            {
                "HubScreen.Base.uss",
                "HubScreen.Desktop.uss",
                "HubScreen.PhoneLandscape.uss",
                "HubScreen.TabletLandscape.uss",
            };
            for (var index = 0; index < expectedHubStyles.Length; index++)
            {
                Assert.That(
                    hubStyleSources[index],
                    Does.Contain(expectedHubStyles[index]));
            }

            var baseStyles = File.ReadAllText(Path.GetFullPath(
                "Assets/TheFall/Presentation/UI/Screen/Hub/Styles/HubScreen.Base.uss"));

            Assert.That(baseStyles, Does.Not.Contain(".screen-root.profile-"));
            Assert.That(File.Exists(Path.GetFullPath(
                "Assets/TheFall/Presentation/UI/Screen/Hub/Styles/HubScreen.HandheldLandscape.uss")), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(
                "Assets/TheFall/Presentation/UI/Screen/Hub/Styles/HubScreen.uss")), Is.False);
        }

        [Test]
        public void SharedStyles_DoNotOwnProfileOrSceneSpecificRules()
        {
            var sharedStyles = File.ReadAllText(Path.GetFullPath(
                "Assets/TheFall/Presentation/UI/Screen/Shared/Styles/FlowShared.uss"));

            Assert.That(
                sharedStyles,
                Does.Not.Contain(".profile-"));
            var sceneOwnedSelectors = new[]
            {
                ".stage-login",
                ".suit-token",
                ".default-note",
                ".default-badge",
                ".fixed-summary",
                ".stage-match",
                ".match-home-floating",
                ".event-line",
                ".outcome-fall",
                ".presentation-toggle",
            };
            foreach (var selector in sceneOwnedSelectors)
            {
                Assert.That(sharedStyles, Does.Not.Contain(selector), selector);
            }

            var hubBaseStyles = File.ReadAllText(Path.GetFullPath(
                "Assets/TheFall/Presentation/UI/Screen/Hub/Styles/HubScreen.Base.uss"));
            Assert.That(hubBaseStyles, Does.Contain(".presentation-toggle"));

            var responsiveScreenStyles = new[]
            {
                "Login/Styles/LoginScreen.uss",
                "Setup/Styles/SetupScreen.uss",
                "Loading/Styles/LoadingScreen.uss",
                "Match/Styles/MatchScreen.uss",
                "Result/Styles/ResultScreen.uss",
            };
            foreach (var relativePath in responsiveScreenStyles)
            {
                var styles = File.ReadAllText(Path.GetFullPath(
                    $"Assets/TheFall/Presentation/UI/Screen/{relativePath}"));
                Assert.That(
                    styles,
                    Does.Contain(".screen-root.profile-mobile-landscape"),
                    relativePath);
            }
        }

        [TestCase("LoginScreen")]
        [TestCase("HubScreen")]
        [TestCase("SetupScreen")]
        [TestCase("LoadingScreen")]
        [TestCase("MatchScreen")]
        [TestCase("ResultScreen")]
        public void ScreenAssets_ExposeAuthoringCopyOrLocalizedIconTooltipsInUiBuilder(string screenAssetName)
        {
            var assetPath = ScreenAssetPath(screenAssetName);
            var document = XDocument.Load(Path.GetFullPath(assetPath));
            var textControls = document
                .Descendants()
                .Where(element =>
                    element.Name.LocalName == "Label" ||
                    element.Name.LocalName == "Button" ||
                    element.Name.LocalName == "Toggle")
                .ToArray();

            Assert.That(textControls, Is.Not.Empty);
            foreach (var control in textControls)
            {
                var isIconOnly = (control.Attribute("class")?.Value ?? string.Empty)
                    .Split(' ')
                    .Contains("icon-only-button");
                var localizedProperty = isIconOnly ? "tooltip" : "text";
                var previewText = control.Attribute("text")?.Value;
                var localizationBinding = control
                    .Descendants()
                    .SingleOrDefault(element =>
                        element.Name.LocalName.EndsWith("LocalizedString") &&
                        element.Attribute("property")?.Value == localizedProperty);
                var controlName = control.Attribute("name")?.Value ?? control.Name.LocalName;

                Assert.That(
                    !string.IsNullOrWhiteSpace(previewText) || localizationBinding != null,
                    Is.True,
                    $"{screenAssetName}/{controlName} needs localized copy or an icon-only tooltip.");

                if (localizationBinding == null)
                {
                    continue;
                }

                Assert.That(
                    localizationBinding.Attribute("table")?.Value,
                    Is.EqualTo($"GUID:{UiTableGuid}"),
                    $"{screenAssetName}/{controlName} must bind to the authoritative UI table.");
                Assert.That(
                    localizationBinding.Attribute("entry")?.Value,
                    Is.Not.Null.And.Not.Empty,
                    $"{screenAssetName}/{controlName} has no localization key.");

                if (isIconOnly)
                {
                    Assert.That(
                        control
                            .Descendants()
                            .Any(element =>
                                element.Name.LocalName.EndsWith("LocalizedString") &&
                                element.Attribute("property")?.Value == "text"),
                        Is.False,
                        $"{screenAssetName}/{controlName} must not render localized text below its icon.");
                }
            }
        }

        [TestCase("Login")]
        [TestCase("Hub")]
        [TestCase("Match")]
        [TestCase("MatchPrototype")]
        [TestCase("AnimationLab")]
        public void DevelopmentSceneOverride_AcceptsRetainedLaunchScenes(string scene)
        {
            var resolved = CompositionRoot.ResolveDevelopmentSceneOverride(
                new[] { "TheFall", CompositionRoot.DevelopmentSceneArgument, scene },
                true);

            Assert.That(resolved, Is.EqualTo(scene));
        }

        [Test]
        public void DevelopmentSceneOverride_IsIgnoredOutsideDevelopmentBuilds()
        {
            var resolved = CompositionRoot.ResolveDevelopmentSceneOverride(
                new[]
                {
                    "TheFall",
                    CompositionRoot.DevelopmentSceneArgument,
                    "MatchPrototype",
                },
                false);

            Assert.That(resolved, Is.Null);
        }

        [TestCase("AssetReview")]
        [TestCase("MissingScene")]
        public void DevelopmentSceneOverride_RejectsScenesOutsideTheDeviceChecklist(string scene)
        {
            var resolved = CompositionRoot.ResolveDevelopmentSceneOverride(
                new[] { "TheFall", CompositionRoot.DevelopmentSceneArgument, scene },
                true);

            Assert.That(resolved, Is.Null);
        }
    }
}
