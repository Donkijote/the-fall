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
        private const string IconDirectory = "Assets/TheFall/Content/UI/Icons";

        private static readonly string[] RequiredIconNames =
        {
            "audio",
            "bag",
            "canto",
            "clubs",
            "coins",
            "cups",
            "decks",
            "energy",
            "envelope",
            "gems",
            "home",
            "padlock",
            "quest",
            "rank",
            "replay",
            "send",
            "settings",
            "shield",
            "shop",
            "skip",
        };

        private static readonly EntryDefinition[] Entries =
        {
            Text("flow.login.eyebrow", "SEASON I · THE FIRST FALL"),
            Text("flow.login.title", "FORGE YOUR"),
            Text("flow.login.title-accent", "DESTINY"),
            Text("flow.login.description", "Enter a world where memory meets nerve. Master the old deck, choose your rules, and conquer the table."),
            Text("flow.login.proof", "40 cards · one table · first to 24"),
            Text("flow.login.panel-title", "GATEWAY"),
            Text("flow.login.panel-subtitle", "LOGIN TO ACCESS YOUR TABLE"),
            Text("flow.login.email", "EMAIL ADDRESS"),
            Text("flow.login.password", "PASSWORD"),
            Text("flow.login.enter", "ENTER THE REALM  →"),
            Text("flow.login.forgot", "Forgot Cipher?"),
            Text("flow.login.divider", "OR INVOKE"),
            Text("flow.login.google", "G"),
            Text("flow.login.apple", "A"),
            Text("flow.login.account-prefix", "New to the realm?"),
            Text("flow.login.create", "Create Account"),
            Text("flow.login.feedback.forgot", "Cipher recovery is not connected in this build."),
            Text("flow.login.feedback.google", "Google invocation is not connected in this build."),
            Text("flow.login.feedback.apple", "Apple invocation is not connected in this build."),
            Text("flow.login.feedback.create", "Account creation is not connected in this build."),
            Text("flow.home.profile-name", "THE WANDERER"),
            Text("flow.home.eyebrow", "LEVEL 1 · TABLE NOVICE"),
            Text("flow.home.xp-label", "XP"),
            Text("flow.home.level-value", "1"),
            Text("flow.home.subtitle", "The Baseline Bot waits at the offline table. Set your preferences once, begin the quest, and claim victory."),
            Text("flow.home.card-label", "CURRENT QUEST"),
            Text("flow.home.objective-title", "Defeat the Baseline Bot"),
            Text("flow.home.mode", "0 / 1 MATCH"),
            Text("flow.home.step.setup", "01 · Choose two optional rules"),
            Text("flow.home.step.match", "02 · MATCH"),
            Text("flow.home.step.result", "03 · RESULT"),
            Text("flow.home.start", "BEGIN QUEST"),
            Text("flow.home.prompt", "Begin Quest deals the match with your current Settings."),
            Text("flow.home.stat.mode-label", "COINS"),
            Text("flow.home.stat.mode-value", "14,200"),
            Text("flow.home.stat.target-label", "GEMS"),
            Text("flow.home.stat.target-value", "500"),
            Text("flow.home.stat.deck-label", "ENERGY"),
            Text("flow.home.stat.deck-value", "45/60"),
            Text("flow.home.status-ready", "READY"),
            Text("flow.home.status-detail", "LOCAL SESSION"),
            Text("flow.home.route-label", "MATCH ROUTE"),
            Text("flow.home.brief-label", "TABLE BRIEF"),
            Text("flow.home.brief.opponent-label", "OPPONENT"),
            Text("flow.home.brief.opponent-value", "Baseline Bot"),
            Text("flow.home.brief.rules-label", "DEFAULTS"),
            Text("flow.home.brief.rules-value", "Casas on · Trivilín 5"),
            Text("flow.home.brief.victory-label", "VICTORY"),
            Text("flow.home.brief.victory-value", "First to 24"),
            Text("flow.home.action-status", "Choose a hub destination."),
            Text("flow.home.mail", "MAIL"),
            Text("flow.home.settings", "SET"),
            Text("flow.home.nav.decks", "DECKS"),
            Text("flow.home.nav.bag", "BAG"),
            Text("flow.home.nav.shop", "SHOP"),
            Text("flow.home.nav.rank", "RANK"),
            Text("flow.home.chat.global", "Global"),
            Text("flow.home.chat.guild", "Guild"),
            Text("flow.home.chat.system", "System"),
            Text("flow.home.chat.send", "Send"),
            Text("flow.home.chat.input-label", "SCRIBE A LOCAL MESSAGE"),
            Text("flow.home.chat.date", "— TODAY —"),
            Text("flow.home.chat.global.one", "[HERALD] Welcome to the first season of The Fall."),
            Text("flow.home.chat.global.two", "[PLAYER] The Baseline Bot is waiting at the table."),
            Text("flow.home.chat.global.three", "[SCOUT] Casas are enabled by default."),
            Text("flow.home.chat.guild.one", "[GUILD] No guild is connected in this build."),
            Text("flow.home.chat.guild.two", "[GUILD] Social services remain local-only."),
            Text("flow.home.chat.guild.three", "[GUILD] Gather your table when guilds arrive."),
            Text("flow.home.chat.system.one", "[SYSTEM] Offline first-playable ready."),
            Text("flow.home.chat.system.two", "[SYSTEM] Target score: 24."),
            Text("flow.home.chat.system.three", "[SYSTEM] Deck integrity: 40 cards."),
            Smart("flow.home.chat.you", "[YOU] {0}"),
            Text("flow.home.chat.empty", "Write a message before sending."),
            Text("flow.home.chat.sent", "Local message added to Global."),
            Text("flow.home.modal.eyebrow", "HUB DESTINATION"),
            Text("flow.common.close", "Close"),
            Text("flow.home.mail.title", "Courier Mail"),
            Text("flow.home.mail.description", "Your courier inbox is ready for future connected messages. No remote mailbox is active in this offline build."),
            Text("flow.home.settings.title", "Settings"),
            Text("flow.home.settings.description", "Choose the rules, sound, and motion used when you begin your next quest. Changes remain active for this play session."),
            Text("flow.home.settings.rules-label", "MATCH RULES"),
            Text("flow.home.settings.rules-copy", "These choices are applied automatically when Begin Quest is selected."),
            Text("flow.home.settings.audio-label", "AUDIO"),
            Text("flow.home.settings.audio-copy", "Control the table mix independently or silence everything with Master audio."),
            Text("flow.home.settings.motion-label", "TABLE MOTION"),
            Text("flow.home.settings.motion-copy", "Set the pace and reduce movement before the table is dealt."),
            Text("flow.home.decks.title", "Decks"),
            Text("flow.home.decks.description", "Reviewing and collecting alternate decks is reserved for the progression layer. The complete 40-card first-playable deck is ready."),
            Text("flow.home.bag.title", "Bag"),
            Text("flow.home.bag.description", "Inventory presentation is in place; persistent rewards and owned items are not connected in this offline build."),
            Text("flow.home.shop.title", "Shop"),
            Text("flow.home.shop.description", "The storefront destination is staged for the final hub. Purchases and currency transactions are intentionally disabled."),
            Text("flow.home.rank.title", "Rank"),
            Text("flow.home.rank.description", "Competitive ranking will connect with online play. This build records no account or ladder progress."),
            Text("flow.home.status.decks", "Decks selected."),
            Text("flow.home.status.bag", "Bag selected."),
            Text("flow.home.status.shop", "Shop selected."),
            Text("flow.home.status.rank", "Rank selected."),
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
            Text("flow.animation.skip", "Skip animation"),
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

            ConfigureIconImports();
            ConfigureLocalization();
            ConfigureHomeScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The first-playable gateway, Home, setup, loading, match, and result flow was generated and validated.");
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
            foreach (var iconName in RequiredIconNames)
            {
                var iconPath = $"{IconDirectory}/{iconName}.png";
                var iconImporter = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                Require(
                    AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath) != null,
                    $"Required UI icon is missing: {iconPath}.",
                    errors);
                Require(iconImporter != null, $"Required UI icon has no texture importer: {iconPath}.", errors);
                if (iconImporter != null)
                {
                    Require(iconImporter.mipmapEnabled, $"UI icon must generate mipmaps for small UI treatments: {iconPath}.", errors);
                    Require(iconImporter.alphaIsTransparency, $"UI icon must preserve transparent edges: {iconPath}.", errors);
                    Require(iconImporter.wrapMode == TextureWrapMode.Clamp, $"UI icon must use clamp wrapping: {iconPath}.", errors);
                    Require(iconImporter.maxTextureSize == 256, $"UI icon must be capped at 256 px: {iconPath}.", errors);
                    Require(iconImporter.filterMode == FilterMode.Trilinear, $"UI icon must use trilinear filtering: {iconPath}.", errors);
                    Require(
                        iconImporter.textureCompression == TextureImporterCompression.Uncompressed,
                        $"UI icon must remain uncompressed: {iconPath}.",
                        errors);
                }
            }

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

        [MenuItem("The Fall/First Playable Flow/Configure UI Icons")]
        public static void ConfigureIconImports()
        {
            foreach (var iconName in RequiredIconNames)
            {
                var iconPath = $"{IconDirectory}/{iconName}.png";
                var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException($"Required UI icon has no texture importer: {iconPath}.");
                }

                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = true;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 256;
                importer.filterMode = FilterMode.Trilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
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
            purpose?.SetDescription("Localized first-playable gateway and flow with an authoritative fixed-camera 1v1 table presentation and resolved-beat prototype audio.");

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
