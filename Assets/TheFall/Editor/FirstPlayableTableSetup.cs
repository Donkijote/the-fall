using System;
using System.Collections.Generic;
using System.Linq;
using TheFall.Presentation.Cards;
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
        private const string HomeScenePath = "Assets/TheFall/Presentation/Scenes/Home.unity";
        private const string TablePrefabPath = "Assets/TheFall/Content/PrototypeAssets/Models/Furniture/RoundCardTable/Generated/RoundCardTable.prefab";
        private const string CardCatalogPath = "Assets/TheFall/Content/Cards/Generated/CardVisualCatalog.asset";

        [MenuItem("The Fall/First Playable Table/Generate")]
        public static void Run()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            FirstPlayableFlowSetup.Run();
            var scene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
            var controller = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableFlowController>(true))
                .SingleOrDefault()
                ?? throw new InvalidOperationException("The Home scene is missing its first-playable flow controller.");
            var camera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .SingleOrDefault(item => item.CompareTag("MainCamera"))
                ?? throw new InvalidOperationException("The Home scene is missing its main camera.");
            var table = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath)
                ?? throw new InvalidOperationException("RoundCardTable is missing.");
            var catalog = AssetDatabase.LoadAssetAtPath<CardVisualCatalog>(CardCatalogPath)
                ?? throw new InvalidOperationException("The complete card visual catalog is missing.");

            var presentation = controller.GetComponent<FirstPlayableTablePresentation>()
                ?? controller.gameObject.AddComponent<FirstPlayableTablePresentation>();
            presentation.Configure(camera, table, catalog);
            camera.transform.position = FirstPlayableTablePresentation.CameraPosition;
            camera.transform.rotation = FirstPlayableTablePresentation.CameraRotation;
            camera.fieldOfView = FirstPlayableTablePresentation.CameraFieldOfView;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;

            var purpose = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ScenePurpose>(true))
                .FirstOrDefault();
            purpose?.SetDescription(
                "Localized first-playable flow with an authoritative fixed-camera 1v1 table presentation.");

            EditorUtility.SetDirty(presentation);
            EditorUtility.SetDirty(camera);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, HomeScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The integrated first-playable 1v1 table was generated and validated.");
        }

        [MenuItem("The Fall/First Playable Table/Validate")]
        public static void Validate()
        {
            var errors = new List<string>();
            var scene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
            var presentation = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FirstPlayableTablePresentation>(true))
                .SingleOrDefault();

            Require(presentation != null, "The Home scene has no integrated table presentation.", errors);
            if (presentation != null)
            {
                Require(presentation.GameplayCamera != null, "The integrated table has no camera.", errors);
                Require(presentation.TablePrototypePrefab != null, "The integrated table does not use RoundCardTable.", errors);
                Require(presentation.CardCatalog != null, "The integrated table has no card catalog.", errors);
                Require(presentation.CardCatalog?.Entries.Count == 40, "The integrated table catalog must contain forty cards.", errors);
                Require(
                    presentation.GameplayCamera != null
                    && presentation.GameplayCamera.transform.position == FirstPlayableTablePresentation.CameraPosition
                    && presentation.GameplayCamera.transform.rotation == FirstPlayableTablePresentation.CameraRotation
                    && Mathf.Approximately(
                        presentation.GameplayCamera.fieldOfView,
                        FirstPlayableTablePresentation.CameraFieldOfView),
                    "The integrated gameplay camera is not stationary at the approved prototype pose.",
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
    }
}
