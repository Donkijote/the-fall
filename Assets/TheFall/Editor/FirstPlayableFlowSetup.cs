using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheFall.Presentation.Scenes;
using TheFall.Presentation.UI;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace TheFall.Editor
{
    public static class FirstPlayableFlowSetup
    {
        private const string HomeScenePath = "Assets/TheFall/Presentation/Scenes/Home.unity";
        private const string UxmlPath = "Assets/TheFall/Presentation/UI/Screen/HomeScreen.uxml";

        private static readonly EntryDefinition[] Entries =
        {
            Text("flow.home.eyebrow", "FIRST PLAYABLE"),
            Text("flow.home.subtitle", "A complete, deterministic table match. Configure two traditional rules, face one baseline bot, and race to 24."),
            Text("flow.home.mode", "Offline · 1 player vs 1 baseline bot · First to 24"),
            Text("flow.home.start", "Start first playable"),
            Text("flow.home.prompt", "Mouse: click · Keyboard: Tab, Enter, or Space"),
            Text("flow.setup.eyebrow", "MATCH SETUP"),
            Text("flow.setup.title", "Choose the table rules"),
            Text("flow.setup.subtitle", "Only the two documented first-playable options can be changed. Everything else remains fixed."),
            Text("flow.setup.casas", "Enable Casa Grande and Casa Chica"),
            Text("flow.setup.casas-description", "On by default. When off, those hands score as their corresponding Ronda."),
            Text("flow.setup.trivilin", "Trivilín wins immediately"),
            Text("flow.setup.trivilin-description", "Off by default, so Trivilín awards five points. Turn this on for immediate victory."),
            Text("flow.setup.fixed", "Fixed match: human vs baseline bot · 24-point target · complete 1v1 rules"),
            Text("flow.setup.start", "Start match"),
            Text("flow.setup.prompt", "Use Tab to move, Space to toggle, and Enter to activate."),
            Text("flow.common.back", "Back"),
            Text("flow.common.return-home", "Return Home"),
            Text("flow.loading.eyebrow", "LOADING"),
            Text("flow.loading.title", "Preparing the table"),
            Text("flow.loading.message", "Creating a fresh deterministic match session…"),
            Text("flow.match.eyebrow", "OFFLINE · 1V1"),
            Text("flow.match.title", "First playable match"),
            Text("flow.match.actions-title", "AVAILABLE ACTIONS"),
            Text("flow.match.prompt", "Choose with the mouse, or use Tab and Enter/Space."),
            Text("flow.match.phase.dealer-selection", "Choose a face-down dealer card"),
            Text("flow.match.phase.dealer-choice", "Choose the dealer setup"),
            Text("flow.match.phase.active", "Play your turn"),
            Text("flow.match.phase.completed", "Match complete"),
            Smart("flow.match.score", "You {0} · Bot {1} · Target {2}"),
            Smart("flow.match.progress", "Round {0} · Deal {1} · {2}"),
            Smart("flow.match.turn", "Dealer: {0} · Active: {1}"),
            Text("flow.match.tie-extension", "Tie extension"),
            Text("flow.match.standard-round", "Standard round"),
            Text("flow.player.you", "You"),
            Text("flow.player.bot", "Baseline Bot"),
            Smart("flow.action.dealer-card", "Choose face-down card {0}"),
            Smart("flow.action.deal-options", "{0} · {1}"),
            Text("flow.action.hands-first", "Hands first"),
            Text("flow.action.table-first", "Table first"),
            Text("flow.action.ascending", "Ascending 1–2–3–4"),
            Text("flow.action.descending", "Descending 4–3–2–1"),
            Smart("flow.action.announce-canto", "Announce {0}"),
            Smart("flow.action.play-card", "Play {0} of {1}"),
            Text("flow.action.unavailable", "Unavailable action"),
            Text("canto.casagrande", "Casa Grande"),
            Text("canto.casachica", "Casa Chica"),
            Text("canto.registro", "Registro"),
            Text("canto.vigia", "Vigía"),
            Text("canto.patrulla", "Patrulla"),
            Text("canto.trivilin", "Trivilín"),
            Text("canto.ronda", "Ronda"),
            Text("card.suit.coins", "Coins"),
            Text("card.suit.cups", "Cups"),
            Text("card.suit.swords", "Swords"),
            Text("card.suit.clubs", "Clubs"),
            Text("flow.result.eyebrow", "RESULT"),
            Text("flow.result.title", "The table is settled"),
            Text("flow.result.victory", "You won"),
            Text("flow.result.defeat", "Baseline Bot won"),
            Smart("flow.result.score", "Final score {0}–{1} · Completed in round {2}"),
            Text("flow.result.replay", "Replay"),
            Text("flow.result.prompt", "Replay starts a fresh session with the same two rule choices."),
        };

        [MenuItem("The Fall/First Playable Flow/Generate")]
        public static void Run()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            ConfigureLocalization();
            ConfigureHomeScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The first-playable Home, setup, loading, match, and result flow was generated and validated.");
        }

        [MenuItem("The Fall/First Playable Flow/Validate")]
        public static void Validate()
        {
            var errors = new List<string>();
            var collection = LocalizationEditorSettings.GetStringTableCollection("UI");
            var english = LocalizationEditorSettings.GetLocale("en");
            var englishTable = collection?.GetTable(english?.Identifier ?? default) as StringTable;

            Require(englishTable != null, "The English UI string table is missing.", errors);
            if (englishTable != null)
            {
                foreach (var definition in Entries)
                {
                    var entry = englishTable.GetEntry(definition.Key);
                    Require(entry != null, $"Localization key {definition.Key} is missing.", errors);
                    if (entry != null)
                    {
                        Require(entry.IsSmart == definition.IsSmart, $"Localization key {definition.Key} has the wrong Smart String setting.", errors);
                    }
                }
            }

            Require(File.Exists(HomeScenePath), "The Home scene is missing.", errors);
            Require(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath) != null, "The first-playable UXML is missing.", errors);

            if (File.Exists(HomeScenePath))
            {
                var scene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
                Require(
                    scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<FirstPlayableFlowController>(true)).Any(),
                    "The Home scene has no FirstPlayableFlowController.",
                    errors);
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException("The first-playable flow validation failed:\n- " + string.Join("\n- ", errors));
            }
        }

        private static void ConfigureLocalization()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("UI")
                ?? throw new InvalidOperationException("The UI string table collection is missing.");
            var english = LocalizationEditorSettings.GetLocale("en")
                ?? throw new InvalidOperationException("The English locale is missing.");
            var table = collection.GetTable(english.Identifier) as StringTable
                ?? throw new InvalidOperationException("The English UI string table is missing.");

            foreach (var definition in Entries)
            {
                var entry = table.GetEntry(definition.Key) ?? table.AddEntry(definition.Key, definition.Value);
                entry.Value = definition.Value;
                entry.IsSmart = definition.IsSmart;
            }

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);
            LocalizationEditorSettings.SetPreloadTableFlag(table, true);
        }

        private static void ConfigureHomeScene()
        {
            var scene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
            var document = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<UIDocument>(true))
                .SingleOrDefault();
            if (document == null)
            {
                throw new InvalidOperationException("The Home scene UI Document is missing.");
            }

            if (document.GetComponent<FirstPlayableFlowController>() == null)
            {
                document.gameObject.AddComponent<FirstPlayableFlowController>();
            }

            var purpose = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ScenePurpose>(true))
                .FirstOrDefault();
            purpose?.SetDescription("Localized first-playable Home, setup, loading, match, result, replay, and return flow.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, HomeScenePath);
        }

        private static EntryDefinition Text(string key, string value)
        {
            return new EntryDefinition(key, value, false);
        }

        private static EntryDefinition Smart(string key, string value)
        {
            return new EntryDefinition(key, value, true);
        }

        private static void Require(bool condition, string message, ICollection<string> errors)
        {
            if (!condition)
            {
                errors.Add(message);
            }
        }

        private sealed class EntryDefinition
        {
            public EntryDefinition(string key, string value, bool isSmart)
            {
                Key = key;
                Value = value;
                IsSmart = isSmart;
            }

            public string Key { get; }

            public string Value { get; }

            public bool IsSmart { get; }
        }
    }
}
