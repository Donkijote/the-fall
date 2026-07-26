using System;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;
using TheFall.Presentation.Diagnostics;
using TheFall.Presentation.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFall.Presentation.Bootstrap
{
    /// <summary>
    /// Owns the application object graph. Dependencies are composed explicitly here as
    /// application and infrastructure services are introduced; no DI container is used.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InputIntentSource))]
    public sealed class CompositionRoot : MonoBehaviour
    {
        public const string DevelopmentSceneArgument = "--the-fall-scene";

        private static readonly PlayerId HumanId = new PlayerId("human");
        private static readonly PlayerId BotId = new PlayerId("baseline-bot");
        private static readonly string[] DevelopmentScenes =
        {
            "Home",
            "MatchPrototype",
            "AnimationLab",
        };

        private bool _loadHomeOnStart;

        public static CompositionRoot Instance { get; private set; }

        public bool IsComposed { get; private set; }

        public FirstPlayableFlow FirstPlayableFlow { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _loadHomeOnStart = gameObject.scene.name == "Bootstrap";
            Instance = this;
            Compose();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (_loadHomeOnStart)
            {
                var sceneOverride = ResolveDevelopmentSceneOverride(
                    Environment.GetCommandLineArgs(),
                    Debug.isDebugBuild);
                SceneManager.LoadSceneAsync(sceneOverride ?? "Home", LoadSceneMode.Single);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Compose()
        {
            if (IsComposed)
            {
                return;
            }

            GetComponent<InputIntentSource>().ValidateConfiguration();
            FirstPlayableFlow = new FirstPlayableFlow(CreateFirstPlayableMatch);
            FirstPlayableAcceptanceProbe.AttachWhenRequested(gameObject);
            IsComposed = true;
        }

        private static FirstPlayableMatchOrchestrator CreateFirstPlayableMatch(
            int seed,
            RuleConfiguration rules)
        {
            return FirstPlayableMatchFactory.Create(
                seed,
                new Player(HumanId, "Local Player", Seat.First, TeamId.One, PlayerControl.Human),
                new Player(BotId, "Baseline Bot", Seat.Second, TeamId.Two, PlayerControl.Bot),
                rules);
        }

        public static string ResolveDevelopmentSceneOverride(
            string[] arguments,
            bool isDevelopmentBuild)
        {
            if (!isDevelopmentBuild || arguments == null)
            {
                return null;
            }

            for (var index = 0; index < arguments.Length; index++)
            {
                if (arguments[index] != DevelopmentSceneArgument || index + 1 >= arguments.Length)
                {
                    continue;
                }

                var requestedScene = arguments[index + 1];
                foreach (var scene in DevelopmentScenes)
                {
                    if (requestedScene == scene)
                    {
                        return scene;
                    }
                }

                return null;
            }

            return null;
        }
    }
}
