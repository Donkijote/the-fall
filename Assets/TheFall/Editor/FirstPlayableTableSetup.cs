using System;
using System.Collections.Generic;
using System.Linq;
using TheFall.Domain;
using TheFall.Presentation.Cards;
using TheFall.Presentation.Animation;
using TheFall.Presentation.Audio;
using TheFall.Presentation.Match;
using TheFall.Presentation.Scenes;
using TheFall.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFall.Editor
{
    public static class FirstPlayableTableSetup
    {
        private const string MatchScenePath = "Assets/TheFall/Presentation/Scenes/Match.unity";
        private const string TablePrefabPath = "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/RoundCardTable/Generated/RoundCardTable.prefab";
        private const string CardCatalogPath = "Assets/TheFall/Content/Cards/Generated/CardVisualCatalog.asset";
        private const string AnimationPresetPath = "Assets/TheFall/Content/Animation/AnimationSequenceConfiguration.asset";
        private const string AuthoringRoot = "Assets/TheFall/Presentation/Match/Authoring";

        private static readonly Color Lampblack = FromHex(0x241A14);
        private static readonly Color CharredWalnut = FromHex(0x3B291F);
        private static readonly Color Ochre = FromHex(0xA06F3C);
        private static readonly Color Moss = FromHex(0x6B7046);
        private static readonly Color Woad = FromHex(0x465C73);

        [MenuItem("The Fall/First Playable Table/Generate")]
        public static void Run()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            FirstPlayableFlowSetup.Run();
            var scene = EditorSceneManager.OpenScene(MatchScenePath, OpenSceneMode.Single);
            var controller = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableFlowController>(true))
                .SingleOrDefault()
                ?? throw new InvalidOperationException("The Match scene is missing its first-playable flow controller.");
            var camera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .SingleOrDefault(item => item.CompareTag("MainCamera"))
                ?? throw new InvalidOperationException("The Match scene is missing its main camera.");
            var table = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath)
                ?? throw new InvalidOperationException("RoundCardTable is missing.");
            var catalog = AssetDatabase.LoadAssetAtPath<CardVisualCatalog>(CardCatalogPath)
                ?? throw new InvalidOperationException("The complete card visual catalog is missing.");
            var animationPreset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(AnimationPresetPath)
                ?? throw new InvalidOperationException("The versioned first-playable animation preset is missing.");
            var layout = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableTableLayout>(true))
                .SingleOrDefault();
            var createdLayout = layout == null;
            layout ??= CreateAuthoredLayout(table, catalog);

            var presentation = controller.GetComponent<FirstPlayableTablePresentation>()
                ?? controller.gameObject.AddComponent<FirstPlayableTablePresentation>();
            presentation.Configure(camera, table, catalog, layout, animationPreset);
            var audioPresenter = controller.GetComponent<FirstPlayableAudioPresenter>()
                ?? controller.gameObject.AddComponent<FirstPlayableAudioPresenter>();
            var audioSource = controller.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            if (createdLayout)
            {
                camera.transform.position = FirstPlayableTablePresentation.CameraPosition;
                camera.transform.rotation = FirstPlayableTablePresentation.CameraRotation;
                camera.fieldOfView = FirstPlayableTablePresentation.CameraFieldOfView;
            }
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;

            var purpose = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ScenePurpose>(true))
                .FirstOrDefault();
            purpose?.SetDescription(
                "Localized first-playable flow with an authoritative fixed-camera 1v1 table presentation and resolved-beat prototype audio.");

            EditorUtility.SetDirty(presentation);
            EditorUtility.SetDirty(audioPresenter);
            EditorUtility.SetDirty(audioSource);
            EditorUtility.SetDirty(layout);
            EditorUtility.SetDirty(camera);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MatchScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            if (!Application.isBatchMode)
            {
                Selection.activeGameObject = layout.gameObject;
                SceneView.lastActiveSceneView?.FrameSelected();
            }

            Debug.Log("The persistent first-playable table authoring layout was generated and validated.");
        }

        [MenuItem("The Fall/First Playable Table/Open Authoring Layout")]
        public static void OpenAuthoringLayout()
        {
            var scene = EditorSceneManager.OpenScene(MatchScenePath, OpenSceneMode.Single);
            var layout = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableTableLayout>(true))
                .SingleOrDefault()
                ?? throw new InvalidOperationException("Generate the first-playable table authoring layout first.");
            Selection.activeGameObject = layout.gameObject;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        [MenuItem("The Fall/First Playable Table/Validate")]
        public static void Validate()
        {
            var errors = new List<string>();
            var scene = EditorSceneManager.OpenScene(MatchScenePath, OpenSceneMode.Single);
            var presentation = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableTablePresentation>(true))
                .SingleOrDefault();
            var layout = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableTableLayout>(true))
                .SingleOrDefault();
            var audioPresenter = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableAudioPresenter>(true))
                .SingleOrDefault();

            Require(presentation != null, "The Match scene has no integrated table presentation.", errors);
            Require(layout != null, "The Match scene has no persistent table authoring layout.", errors);
            Require(audioPresenter != null, "The Match scene has no first-playable audio presenter.", errors);
            Require(layout?.IsConfigured == true, "The persistent table authoring layout is incomplete.", errors);
            Require(
                audioPresenter != null
                && audioPresenter.TryGetComponent<AudioSource>(out var audioSource)
                && !audioSource.playOnAwake
                && !audioSource.loop
                && audioSource.spatialBlend == 0f,
                "The prototype effects source must be non-looping, non-spatial, and disabled on awake.",
                errors);
            if (presentation != null)
            {
                Require(presentation.GameplayCamera != null, "The integrated table has no camera.", errors);
                Require(presentation.TablePrototypePrefab != null, "The integrated table does not use RoundCardTable.", errors);
                Require(presentation.CardCatalog != null, "The integrated table has no card catalog.", errors);
                Require(presentation.AnimationPreset != null, "The integrated table has no animation preset.", errors);
                Require(
                    presentation.AnimationPreset?.PresetVersion == AnimationSequenceConfiguration.CurrentPresetVersion,
                    "The integrated table animation preset version is unsupported.",
                    errors);
                Require(presentation.AuthoredLayout == layout, "The integrated table does not use the Match scene authoring layout.", errors);
                Require(presentation.CardCatalog?.Entries.Count == 40, "The integrated table catalog must contain forty cards.", errors);
                Require(
                    presentation.GameplayCamera != null
                    && !presentation.GameplayCamera.orthographic
                    && presentation.GameplayCamera.fieldOfView > 1f
                    && presentation.GameplayCamera.fieldOfView < 179f,
                    "The integrated gameplay camera must retain a valid authored perspective.",
                    errors);
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "The integrated first-playable table validation failed:\n- " + string.Join("\n- ", errors));
            }
        }

        private static void Require(bool condition, string message, ICollection<string> errors)
        {
            if (!condition)
            {
                errors.Add(message);
            }
        }

        private static FirstPlayableTableLayout CreateAuthoredLayout(
            GameObject tablePrefab,
            CardVisualCatalog catalog)
        {
            EnsureFolder("Assets/TheFall/Presentation/Match");
            EnsureFolder(AuthoringRoot);
            var lampblack = EnsureMaterial("EnvironmentDark", Lampblack);
            var walnut = EnsureMaterial("EnvironmentWalnut", CharredWalnut);
            var localBody = EnsureMaterial("LocalSeat", Moss);
            var opponentBody = EnsureMaterial("OpponentSeat", Woad);
            var skin = EnsureMaterial("SeatSkin", Ochre);

            var root = new GameObject("First Playable Table Authoring");
            var layout = root.AddComponent<FirstPlayableTableLayout>();

            var environment = new GameObject("Environment");
            environment.transform.SetParent(root.transform, false);
            CreatePrimitive("Quiet Room Ground", PrimitiveType.Cube, environment.transform,
                new Vector3(0f, -0.08f, 0.2f), new Vector3(5.6f, 0.12f, 5.4f), Quaternion.identity, lampblack);
            CreatePrimitive("Warm Stage Pool", PrimitiveType.Cylinder, environment.transform,
                Vector3.zero, new Vector3(2.25f, 0.04f, 2.25f), Quaternion.identity, walnut);
            var lightObject = new GameObject("Warm Table Key", typeof(Light));
            lightObject.transform.SetParent(environment.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(55f, -28f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.78f, 0.55f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;

            var table = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab, root.transform);
            table.name = "RoundCardTable — Edit And Save";
            table.transform.localScale = new Vector3(1.45f, 1f, 1.45f);

            var localSeat = CreateSeat("Local Seat — Edit And Save", root.transform,
                new Vector3(0f, 0f, -1.38f), Quaternion.LookRotation(Vector3.forward), localBody, skin);
            var opponentSeat = CreateSeat("Opponent Seat — Edit And Save", root.transform,
                new Vector3(0f, 0f, 1.38f), Quaternion.LookRotation(Vector3.back), opponentBody, skin);

            var zones = new GameObject("Card Zone Anchors — Move Or Rotate");
            zones.transform.SetParent(root.transform, false);
            var dealerSpread = CreateAnchor("Dealer Spread", zones.transform, new Vector3(0f, 0.80f, 0f));
            var deck = CreateAnchor("Deck", zones.transform, new Vector3(0.72f, 0.80f, 0f));
            var tableCards = CreateAnchor("Table Cards", zones.transform, new Vector3(0f, 0.805f, -0.155f));
            var localHand = CreateAnchor("Local Hand", zones.transform, new Vector3(0f, 0.82f, -0.88f));
            var opponentHand = CreateAnchor("Opponent Hand", zones.transform, new Vector3(0f, 0.82f, 0.88f));
            var localCaptured = CreateAnchor("Local Captured", zones.transform, new Vector3(-0.88f, 0.80f, -0.55f));
            var opponentCaptured = CreateAnchor("Opponent Captured", zones.transform, new Vector3(0.88f, 0.80f, 0.55f));

            var previewCards = new List<Transform>();
            previewCards.Add(CreateCardPreview("Card Size Reference — Scale X Only", localHand, new Vector3(-0.29f, 0f, 0.025f), catalog,
                new Card(TheFall.Domain.CardSuit.Coins, TheFall.Domain.CardRank.Five), true));
            previewCards.Add(CreateCardPreview("Local 12", localHand, Vector3.zero, catalog,
                new Card(TheFall.Domain.CardSuit.Cups, TheFall.Domain.CardRank.Twelve), true));
            previewCards.Add(CreateCardPreview("Local 6", localHand, new Vector3(0.29f, 0f, 0.025f), catalog,
                new Card(TheFall.Domain.CardSuit.Swords, TheFall.Domain.CardRank.Six), true));
            previewCards.Add(CreateCardPreview("Table 3", tableCards, new Vector3(-0.115f, 0f, 0f), catalog,
                new Card(TheFall.Domain.CardSuit.Clubs, TheFall.Domain.CardRank.Three), true));
            previewCards.Add(CreateCardPreview("Table 7", tableCards, new Vector3(0.115f, 0f, 0f), catalog,
                new Card(TheFall.Domain.CardSuit.Coins, TheFall.Domain.CardRank.Seven), true));
            previewCards.Add(CreateCardPreview("Opponent Card 1", opponentHand, new Vector3(-0.125f, 0f, 0f), catalog,
                default, false));
            previewCards.Add(CreateCardPreview("Opponent Card 2", opponentHand, new Vector3(0.125f, 0f, 0f), catalog,
                default, false));
            previewCards.Add(CreateCardPreview("Deck", deck, Vector3.zero, catalog, default, false));
            previewCards.Add(CreateCardPreview("Local Capture", localCaptured, Vector3.zero, catalog, default, false));
            previewCards.Add(CreateCardPreview("Opponent Capture", opponentCaptured, Vector3.zero, catalog, default, false));

            layout.Configure(
                environment,
                table,
                localSeat,
                opponentSeat,
                zones.transform,
                dealerSpread,
                deck,
                tableCards,
                localHand,
                opponentHand,
                localCaptured,
                opponentCaptured,
                previewCards[0],
                previewCards.ToArray());
            return layout;
        }

        private static GameObject CreateSeat(
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material bodyMaterial,
            Material skinMaterial)
        {
            var seat = new GameObject(name);
            seat.transform.SetParent(parent, false);
            seat.transform.localPosition = position;
            seat.transform.localRotation = rotation;
            CreatePrimitive("Upper Body Placeholder", PrimitiveType.Capsule, seat.transform,
                new Vector3(0f, 0.48f, -0.08f), new Vector3(0.28f, 0.29f, 0.20f), Quaternion.identity, bodyMaterial);
            CreatePrimitive("Placeholder Head", PrimitiveType.Sphere, seat.transform,
                new Vector3(0f, 0.82f, -0.05f), new Vector3(0.24f, 0.26f, 0.24f), Quaternion.identity, skinMaterial);
            CreatePrimitive("Left Placeholder Hand", PrimitiveType.Sphere, seat.transform,
                new Vector3(-0.23f, 0.52f, 0.18f), new Vector3(0.09f, 0.06f, 0.12f), Quaternion.identity, skinMaterial);
            CreatePrimitive("Right Placeholder Hand", PrimitiveType.Sphere, seat.transform,
                new Vector3(0.23f, 0.52f, 0.18f), new Vector3(0.09f, 0.06f, 0.12f), Quaternion.identity, skinMaterial);
            return seat;
        }

        private static Transform CreateAnchor(string name, Transform parent, Vector3 position)
        {
            var anchor = new GameObject(name);
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = position;
            return anchor.transform;
        }

        private static Transform CreateCardPreview(
            string name,
            Transform parent,
            Vector3 position,
            CardVisualCatalog catalog,
            Card card,
            bool faceUp)
        {
            var cardObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cardObject.name = name;
            cardObject.transform.SetParent(parent, false);
            cardObject.transform.localPosition = position;
            cardObject.transform.localRotation = faceUp ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            cardObject.transform.localScale = new Vector3(0.19f, 0.012f, 0.19f * 88f / 63f);
            UnityEngine.Object.DestroyImmediate(cardObject.GetComponent<Collider>());
            cardObject.AddComponent<FirstPlayableCardAuthoringPreview>().Configure(catalog, card, faceUp);
            return cardObject.transform;
        }

        private static GameObject CreatePrimitive(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            Material material)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = position;
            primitive.transform.localScale = scale;
            primitive.transform.localRotation = rotation;
            UnityEngine.Object.DestroyImmediate(primitive.GetComponent<Collider>());
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            return primitive;
        }

        private static Material EnsureMaterial(string name, Color color)
        {
            var path = $"{AuthoringRoot}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader)
            {
                name = name,
                color = color,
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = path.Substring(0, path.LastIndexOf('/'));
            var name = path.Substring(path.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Color FromHex(int rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }
    }
}
