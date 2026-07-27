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
            Text("flow.home.eyebrow", "OFFLINE TABLE MATCH"),
            Text("flow.home.subtitle", "Reach 24 points before the Baseline Bot in one complete match of The Fall."),
            Text("flow.home.card-label", "YOUR NEXT MATCH"),
            Text("flow.home.mode", "1 PLAYER · BASELINE BOT · FIRST TO 24"),
            Text("flow.home.step.setup", "01 · Choose two optional rules"),
            Text("flow.home.step.match", "02 · Play a complete 1v1 match"),
            Text("flow.home.step.result", "03 · See the winner and final score"),
            Text("flow.home.start", "Set up match"),
            Text("flow.home.prompt", "Review the two rule options before the match begins."),
            Text("flow.setup.eyebrow", "NEW MATCH"),
            Text("flow.setup.title", "Choose your rules"),
            Text("flow.setup.subtitle", "These are the only player choices before the match. Recommended defaults are already selected."),
            Text("flow.setup.default-note", "You can change either option now. Returning Home restores both defaults."),
            Text("flow.setup.casas", "Casa Grande and Casa Chica"),
            Text("flow.setup.casas-default", "DEFAULT: ON"),
            Text("flow.setup.casas-description", "When on, Casa Grande scores 12 points and Casa Chica scores 10. When off, each scores as its Ronda."),
            Text("flow.setup.casas-state.enabled", "ON · Special canto scoring"),
            Text("flow.setup.casas-state.disabled", "OFF · Ronda scoring"),
            Text("flow.setup.trivilin", "Trivilín effect"),
            Text("flow.setup.trivilin-default", "DEFAULT: 5 POINTS"),
            Text("flow.setup.trivilin-description", "Choose whether Trivilín awards five points or wins the match immediately."),
            Text("flow.setup.trivilin-state.points", "FIVE POINTS · Match continues"),
            Text("flow.setup.trivilin-state.immediate", "IMMEDIATE VICTORY · Match ends"),
            Text("flow.setup.fixed-label", "MATCH BASICS"),
            Text("flow.setup.fixed", "Offline 1v1 · You vs Baseline Bot · First to 24 · Complete 1v1 rules"),
            Text("flow.setup.start", "Start match"),
            Text("flow.setup.prompt", "Choose any changes, then start the match."),
            Text("flow.common.back", "Back"),
            Text("flow.common.return-home", "Return to Home"),
            Text("flow.loading.eyebrow", "MATCH IN PROGRESS"),
            Text("flow.loading.title", "Setting the table"),
            Text("flow.loading.message", "Creating one fresh match with your selected rules."),
            Smart("flow.loading.session", "Session {0} · {1}"),
            Text("flow.loading.status", "Please wait. Match controls unlock when the table is ready."),
            Text("flow.loading.cancel", "Cancel and return Home"),
            Smart("flow.rules.summary", "{0} · {1}"),
            Text("flow.rules.casas.enabled", "Casas on"),
            Text("flow.rules.casas.disabled", "Casas off"),
            Text("flow.rules.trivilin.points", "Trivilín: five points"),
            Text("flow.rules.trivilin.immediate", "Trivilín: immediate victory"),
            Text("flow.match.eyebrow", "OFFLINE · 1V1"),
            Text("flow.match.title", "First playable match"),
            Text("flow.match.prompt", "Click a card to select it; click it again or press Enter/Space to play."),
            Text("flow.match.phase.dealer-selection", "Choose a face-down dealer card"),
            Text("flow.match.phase.dealer-choice", "Choose the dealer setup"),
            Text("flow.match.phase.active", "Play your turn"),
            Text("flow.match.phase.completed", "Match complete"),
            Smart("flow.match.score", "You {0} · Bot {1} · Target {2}"),
            Smart("flow.match.progress", "Round {0} · Deal {1} · {2}"),
            Smart("flow.match.turn", "Dealer: {0} · Active: {1}"),
            Smart("flow.match.turn.dealer-pending", "Dealer: pending · Active: {0}"),
            Text("flow.match.canto.none", "Cantos: none announced this deal"),
            Smart("flow.match.canto.announcement", "{0}: {1}"),
            Smart("flow.match.canto.summary", "Cantos: {0}"),
            Text("flow.match.event.ready", "The table is ready."),
            Smart("flow.match.event.match-started", "Dealer selection began with {0} face-down cards."),
            Smart("flow.match.event.dealer-selected", "{0} is the dealer."),
            Smart("flow.match.event.deck-shuffled", "Round {0} deck shuffled."),
            Smart("flow.match.event.deal-started", "Round {0}, deal {1} began."),
            Smart("flow.match.event.card-played", "{0} played {1} of {2}."),
            Smart("flow.match.event.cards-captured", "{0} captured {1} cards."),
            Smart("flow.match.event.canto-announced", "{0} announced {1}."),
            Smart("flow.match.event.score-changed", "{0}: {1:+#;-#;0} · total {2}"),
            Smart("flow.match.event.round-completed", "Round {0} is complete."),
            Smart("flow.match.event.tie-extension", "Tie extension at {0} points."),
            Smart("flow.match.event.turn-changed", "Turn: {0}."),
            Smart("flow.match.event.match-completed", "Match complete. Winner: {0}."),
            Text("flow.match.event.resolved", "The authoritative table state was updated."),
            Text("flow.match.score-reason.openingpattern", "Opening pattern"),
            Text("flow.match.score-reason.canto", "Canto"),
            Text("flow.match.score-reason.falsecantopenalty", "False canto"),
            Text("flow.match.score-reason.fall", "Fall"),
            Text("flow.match.score-reason.cleantable", "Clean table"),
            Text("flow.match.score-reason.capturedcards", "Captured cards"),
            Text("flow.match.tie-extension", "Tie extension"),
            Text("flow.match.standard-round", "Standard round"),
            Text("flow.animation.fast-forward", "Fast forward"),
            Text("flow.animation.reduced-motion", "Reduced motion"),
            Text("flow.animation.skip", "Skip"),
            Text("flow.audio.master", "Master audio"),
            Text("flow.audio.effects", "Effects"),
            Text("flow.audio.music", "Music"),
            Text("flow.player.you", "You"),
            Text("flow.player.bot", "Baseline Bot"),
            Text("flow.context.dealer-icon", "DEAL"),
            Text("flow.context.dealer-title", "Choose how to deal"),
            Text("flow.context.dealer-tooltip", "Required dealer setup"),
            Text("flow.context.dealer-required", "Dealer choice required. Choose the deal order and opening pattern."),
            Text("flow.context.dealer-card-prompt", "Choose one of the face-down cards spread across the table."),
            Text("flow.context.canto-icon", "!"),
            Text("flow.context.canto-title", "Announce a canto"),
            Text("flow.context.canto-tooltip", "Optional canto announcement"),
            Smart("flow.action.dealer-card", "Choose face-down card {0}"),
            Smart("flow.action.deal-options", "{0} · {1}"),
            Text("flow.action.hands-first", "Hands first"),
            Text("flow.action.table-first", "Table first"),
            Text("flow.action.ascending", "Ascending 1–2–3–4"),
            Text("flow.action.descending", "Descending 4–3–2–1"),
            Smart("flow.action.announce-canto", "Announce {0}"),
            Smart("flow.action.play-card", "Play {0} of {1}"),
            Text("flow.action.unavailable", "Unavailable action"),
            Text("interaction.feedback.legal", "Choose a legal card to inspect or select."),
            Text("interaction.feedback.selected", "Card selected. Click it again or confirm to play; cancel to release it."),
            Text("interaction.feedback.confirmed", "✓ Card play accepted by the match."),
            Text("interaction.feedback.temporarily-blocked", "Ⅱ Presentation is busy; selection retained."),
            Text("interaction.feedback.no-selection", "× Select a card before confirming."),
            Text("interaction.feedback.different-player", "× That card belongs to a different player."),
            Text("interaction.feedback.domain-rejected", "× The authoritative match rejected that play."),
            Text("interaction.feedback.card-unavailable", "× That card is not currently available."),
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
            Text("flow.result.eyebrow", "FINAL RESULT"),
            Text("flow.result.title", "Match complete"),
            Text("flow.result.winner-label", "WINNER"),
            Text("flow.result.victory", "You win"),
            Text("flow.result.defeat", "Baseline Bot wins"),
            Smart("flow.result.score", "Final score · You {0} · Bot {1} · Round {2}"),
            Smart("flow.result.rules", "Played with {0}"),
            Text("flow.result.next", "Choose what happens next"),
            Text("flow.result.replay", "Play again"),
            Text("flow.result.prompt", "Play again keeps these rules. Returning Home clears the session and restores the recommended defaults."),
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
