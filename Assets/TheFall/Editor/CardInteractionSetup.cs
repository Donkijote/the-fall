using System;
using System.Linq;
using TheFall.Presentation.Interaction;
using TheFall.Presentation.Scenes;
using TheFall.Presentation.Table;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;

namespace TheFall.Editor
{
    public static class CardInteractionSetup
    {
        private const string ScenePath = "Assets/TheFall/Presentation/Scenes/MatchPrototype.unity";

        [MenuItem("The Fall/Card Interaction/Generate")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var tableComposition = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<TableCompositionPrototype>(true))
                .SingleOrDefault();
            if (tableComposition == null)
            {
                throw new InvalidOperationException("Generate the table composition before card interaction.");
            }

            var interaction = tableComposition.GetComponent<CardInteractionPrototype>();
            if (interaction == null)
            {
                interaction = tableComposition.gameObject.AddComponent<CardInteractionPrototype>();
            }

            interaction.ConfigureTableComposition(tableComposition);

            var purpose = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ScenePurpose>(true))
                .SingleOrDefault();
            purpose?.SetDescription(
                "Stationary-camera table and cross-platform card-interaction prototype for shared touch, mouse, and keyboard application intents across safe-area-aware mobile-landscape and desktop profiles.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            UnityEngine.Debug.Log("The Fall card interaction prototype generated and validated.");
        }

        [MenuItem("The Fall/Card Interaction/Validate")]
        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var interaction = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<CardInteractionPrototype>(true))
                .SingleOrDefault();

            if (interaction == null || interaction.TableComposition == null)
            {
                throw new BuildFailedException(
                    "MatchPrototype does not contain a card interaction prototype bound to the table composition.");
            }
        }
    }
}
