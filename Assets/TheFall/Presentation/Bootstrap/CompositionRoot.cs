using TheFall.Application;
using TheFall.Domain;
using TheFall.Infrastructure;
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
        private static readonly PlayerId HumanId = new PlayerId("human");
        private static readonly PlayerId BotId = new PlayerId("baseline-bot");

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
                SceneManager.LoadSceneAsync("Home", LoadSceneMode.Single);
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
    }
}
