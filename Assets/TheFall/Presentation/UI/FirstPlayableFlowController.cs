using System;
using System.Collections;
using System.Collections.Generic;
using TheFall.Application;
using TheFall.Application.Interaction;
using TheFall.Domain;
using TheFall.Presentation.Bootstrap;
using TheFall.Presentation.Scenes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using DeviceApplication = UnityEngine.Device.Application;
using DeviceScreen = UnityEngine.Device.Screen;

namespace TheFall.Presentation.UI
{
    public enum FirstPlayableScreenKind
    {
        Login,
        Hub,
        Setup,
        Loading,
        Match,
        Result,
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class FirstPlayableFlowController : MonoBehaviour
    {
        private const string TableName = "UI";

        private static readonly string[] MatchOutcomeClasses =
        {
            "outcome-capture",
            "outcome-fall",
            "outcome-clean-table",
            "outcome-canto",
            "outcome-score",
            "outcome-tie",
            "outcome-victory",
        };

        private static readonly IReadOnlyDictionary<string, string> LabelLocalizationKeys =
            new Dictionary<string, string>
            {
                { "login-eyebrow", "flow.login.eyebrow" },
                { "login-title", "flow.login.title" },
                { "login-title-accent", "flow.login.title-accent" },
                { "login-description", "flow.login.description" },
                { "login-proof", "flow.login.proof" },
                { "login-panel-title", "flow.login.panel-title" },
                { "login-panel-subtitle", "flow.login.panel-subtitle" },
                { "login-email-label", "flow.login.email" },
                { "login-password-label", "flow.login.password" },
                { "login-enter-label", "flow.login.enter" },
                { "login-divider-label", "flow.login.divider" },
                { "login-account-prefix", "flow.login.account-prefix" },
                { "home-eyebrow", "flow.home.eyebrow" },
                { "home-title", "flow.home.profile-name" },
                { "home-xp-label", "flow.home.xp-label" },
                { "home-level-value", "flow.home.level-value" },
                { "home-subtitle", "flow.home.subtitle" },
                { "home-card-label", "flow.home.card-label" },
                { "home-objective-title", "flow.home.objective-title" },
                { "home-mode", "flow.home.mode" },
                { "home-stat-mode-value", "flow.home.stat.mode-value" },
                { "home-stat-target-value", "flow.home.stat.target-value" },
                { "home-stat-deck-value", "flow.home.stat.deck-value" },
                { "home-action-status", "flow.home.action-status" },
                { "home-chat-input-label", "flow.home.chat.input-label" },
                { "hub-modal-eyebrow", "flow.home.modal.eyebrow" },
                { "home-settings-rules-label", "flow.home.settings.rules-label" },
                { "home-settings-rules-copy", "flow.home.settings.rules-copy" },
                { "home-settings-casas-description", "flow.setup.casas-description" },
                { "home-settings-trivilin-description", "flow.setup.trivilin-description" },
                { "home-settings-audio-label", "flow.home.settings.audio-label" },
                { "home-settings-audio-copy", "flow.home.settings.audio-copy" },
                { "home-settings-motion-label", "flow.home.settings.motion-label" },
                { "home-settings-motion-copy", "flow.home.settings.motion-copy" },
                { "setup-eyebrow", "flow.setup.eyebrow" },
                { "setup-title", "flow.setup.title" },
                { "setup-subtitle", "flow.setup.subtitle" },
                { "setup-default-note", "flow.setup.default-note" },
                { "casas-default", "flow.setup.casas-default" },
                { "casas-description", "flow.setup.casas-description" },
                { "trivilin-default", "flow.setup.trivilin-default" },
                { "trivilin-description", "flow.setup.trivilin-description" },
                { "setup-fixed-label", "flow.setup.fixed-label" },
                { "setup-fixed", "flow.setup.fixed" },
                { "setup-prompt", "flow.setup.prompt" },
                { "loading-eyebrow", "flow.loading.eyebrow" },
                { "loading-title", "flow.loading.title" },
                { "loading-message", "flow.loading.message" },
                { "loading-status", "flow.loading.status" },
                { "dealer-options-title", "flow.context.dealer-title" },
                { "canto-options-title", "flow.context.canto-title" },
                { "match-event-label", "flow.match.event-label" },
                { "match-feedback-label", "flow.match.feedback-label" },
                { "match-score-objective", "flow.match.score-objective" },
                { "result-eyebrow", "flow.result.eyebrow" },
                { "result-title", "flow.result.title" },
                { "result-winner-label", "flow.result.winner-label" },
                { "result-next", "flow.result.next" },
                { "result-prompt", "flow.result.prompt" },
            };

        private static readonly IReadOnlyDictionary<string, string> ButtonLocalizationKeys =
            new Dictionary<string, string>
            {
                { "login-forgot-button", "flow.login.forgot" },
                { "login-google-button", "flow.login.google" },
                { "login-apple-button", "flow.login.apple" },
                { "login-create-button", "flow.login.create" },
                { "home-start-button", "flow.home.start" },
                { "home-mail-button", "flow.home.mail" },
                { "home-settings-button", "flow.home.settings" },
                { "home-decks-button", "flow.home.nav.decks" },
                { "home-bag-button", "flow.home.nav.bag" },
                { "home-shop-button", "flow.home.nav.shop" },
                { "home-rank-button", "flow.home.nav.rank" },
                { "home-chat-global-button", "flow.home.chat.global" },
                { "home-chat-guild-button", "flow.home.chat.guild" },
                { "home-chat-system-button", "flow.home.chat.system" },
                { "home-chat-send-button", "flow.home.chat.send" },
                { "hub-modal-close-button", "flow.common.close" },
                { "setup-start-button", "flow.setup.start" },
                { "setup-back-button", "flow.common.back" },
                { "loading-home-button", "flow.loading.cancel" },
                { "match-home-button", "flow.common.return-home" },
                { "dealer-options-button", "flow.context.dealer-icon" },
                { "canto-options-button", "flow.context.canto-icon" },
                { "result-replay-button", "flow.result.replay" },
                { "result-home-button", "flow.common.return-home" },
                { "animation-skip-button", "flow.animation.skip" },
            };

        [SerializeField] private VisualTreeAsset _loginScreenAsset;
        [SerializeField] private VisualTreeAsset _hubScreenAsset;
        [SerializeField] private VisualTreeAsset _setupScreenAsset;
        [SerializeField] private VisualTreeAsset _loadingScreenAsset;
        [SerializeField] private VisualTreeAsset _matchScreenAsset;
        [SerializeField] private VisualTreeAsset _resultScreenAsset;
        [SerializeField] private FirstPlayableSceneKind _sceneKind;

        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _screen;
        private VisualElement _mountedStage;
        private FirstPlayableScreenKind? _mountedScreenKind;
        private Toggle _casasToggle;
        private Toggle _trivilinToggle;
        private Label _casasState;
        private Label _trivilinState;
        private Label _loadingSession;
        private Label _matchPhase;
        private Label _matchScore;
        private Label _matchProgress;
        private Label _matchTurn;
        private Label _matchCanto;
        private Label _matchEvent;
        private VisualElement _matchEventCallout;
        private Label _matchFeedback;
        private Label _matchFeedbackSymbol;
        private VisualElement _matchFeedbackCallout;
        private Label _resultOutcome;
        private Label _resultScore;
        private Label _resultRules;
        private VisualElement _dealerContext;
        private VisualElement _dealerOptionsMenu;
        private VisualElement _dealerOptions;
        private Button _dealerOptionsButton;
        private VisualElement _cantoContext;
        private VisualElement _cantoOptionsMenu;
        private VisualElement _cantoOptions;
        private Button _cantoOptionsButton;
        private Button _animationSkipButton;
        private Label _loginFeedback;
        private Label _homeActionStatus;
        private Label _homeChatDate;
        private Label _homeChatMessageOne;
        private Label _homeChatMessageTwo;
        private Label _homeChatMessageThree;
        private Label _homeChatUserMessage;
        private TextField _homeChatInput;
        private Button _homeChatGlobalButton;
        private Button _homeChatGuildButton;
        private Button _homeChatSystemButton;
        private VisualElement _hubModal;
        private Label _hubModalTitle;
        private Label _hubModalDescription;
        private VisualElement _hubSettingsContent;
        private Toggle _homeCasasToggle;
        private Toggle _homeTrivilinToggle;
        private Toggle _homeAnimationFastToggle;
        private Toggle _homeAnimationReducedToggle;
        private Toggle _homeAudioMasterToggle;
        private Toggle _homeAudioEffectsToggle;
        private Toggle _homeAudioMusicToggle;
        private Coroutine _loadingCoroutine;
        private bool _isDocumentRootBound;
        private bool _hasEnteredGateway;
        private bool _isDealerMenuOpen;
        private bool _isCantoMenuOpen;
        private MatchState _contextState;
        private Vector2Int _adaptiveViewport;
        private Rect _adaptiveSafeArea;
        private bool _hasAdaptiveViewportOverride;
        private string _homeChatChannel = "global";
        private string _homeChatUserMessageText;
        private bool _homeCasasEnabled = true;
        private bool _homeTrivilinImmediate;
        private bool _animationFastForwardEnabled;
        private bool _animationReducedMotionEnabled;
        private bool _audioMasterEnabled = true;
        private bool _audioEffectsEnabled = true;
        private bool _audioMusicEnabled;
        private FirstPlayablePresentationState _presentationState;
        private bool _sceneTransitionPending;
        private AdaptiveUiProfile _authoringPreviewProfile = AdaptiveUiProfile.PhoneLandscape;

        public FirstPlayableFlow Flow { get; private set; }

        public event Action PresentationChanged;

        public event Action<MatchAdvanceResult> MatchAdvanced;

        public event Action<bool> AnimationFastForwardChanged;

        public event Action<bool> AnimationReducedMotionChanged;

        public event Action AnimationSkipRequested;

        public event Action<bool> AudioMasterChanged;

        public event Action<bool> AudioEffectsChanged;

        public event Action<bool> AudioMusicChanged;

        public bool IsPresentationBusy { get; private set; }

        public bool AudioMasterEnabled => _audioMasterEnabled;

        public bool AudioEffectsEnabled => _audioEffectsEnabled;

        public bool AudioMusicEnabled => _audioMusicEnabled;

        public bool HasEnteredGateway => _hasEnteredGateway;

        public FirstPlayableSceneKind SceneKind => _sceneKind;

        public FirstPlayableScreenKind? CurrentScreenKind => _mountedScreenKind;

        public int MountedScreenCount => _root?.childCount ?? 0;

        public bool HasConfiguredScreenAssets =>
            _sceneKind == FirstPlayableSceneKind.Login
                ? _loginScreenAsset != null
                : _sceneKind == FirstPlayableSceneKind.Hub
                    ? _hubScreenAsset != null && _setupScreenAsset != null
                    : _loadingScreenAsset != null
                        && _matchScreenAsset != null
                        && _resultScreenAsset != null;

        public void SetAudioMasterEnabled(bool enabled)
        {
            _audioMasterEnabled = enabled;
            if (_presentationState != null)
            {
                _presentationState.AudioMasterEnabled = enabled;
            }
            _homeAudioMasterToggle?.SetValueWithoutNotify(enabled);
            AudioMasterChanged?.Invoke(enabled);
        }

        public void SetAudioEffectsEnabled(bool enabled)
        {
            _audioEffectsEnabled = enabled;
            if (_presentationState != null)
            {
                _presentationState.AudioEffectsEnabled = enabled;
            }
            _homeAudioEffectsToggle?.SetValueWithoutNotify(enabled);
            AudioEffectsChanged?.Invoke(enabled);
        }

        public void SetAudioMusicEnabled(bool enabled)
        {
            _audioMusicEnabled = enabled;
            if (_presentationState != null)
            {
                _presentationState.AudioMusicEnabled = enabled;
            }
            _homeAudioMusicToggle?.SetValueWithoutNotify(enabled);
            AudioMusicChanged?.Invoke(enabled);
        }

        public AdaptiveUiLayout CurrentAdaptiveLayout { get; private set; }

        public void ConfigureScene(
            FirstPlayableSceneKind sceneKind,
            VisualTreeAsset login,
            VisualTreeAsset hub,
            VisualTreeAsset setup,
            VisualTreeAsset loading,
            VisualTreeAsset match,
            VisualTreeAsset result)
        {
            _sceneKind = sceneKind;
            _loginScreenAsset = login;
            _hubScreenAsset = hub;
            _setupScreenAsset = setup;
            _loadingScreenAsset = loading;
            _matchScreenAsset = match;
            _resultScreenAsset = result;
        }

        private void OnEnable()
        {
            _document = GetComponent<UIDocument>();
            _root = _document.rootVisualElement;
            _authoringPreviewProfile = ResolveAuthoringPreviewProfile();
            UseSceneLayoutForPreviewRoots();
            BindDocumentRoot();
            ApplyAdaptiveLayout(
                RuntimeViewport(),
                RuntimeSafeArea(),
                DeviceApplication.isMobilePlatform);

            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            var compositionRoot = CompositionRoot.Instance != null
                ? CompositionRoot.Instance
                : FindAnyObjectByType<CompositionRoot>();
            if (compositionRoot == null || compositionRoot.FirstPlayableFlow == null)
            {
                Debug.LogError("The first-playable UI requires the Bootstrap composition root.", this);
                enabled = false;
                return;
            }

            Flow = compositionRoot.FirstPlayableFlow;
            _presentationState = compositionRoot.FirstPlayablePresentationState;
            PrepareDirectSceneState();
            RestorePresentationState();
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
            Render();
        }

        private IEnumerator Start()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                yield break;
            }

            yield return LocalizationSettings.InitializationOperation;
            Render();
            if (Flow != null
                && Flow.Stage == FirstPlayableFlowStage.Loading
                && !_sceneTransitionPending
                && _loadingCoroutine == null)
            {
                BeginLoadingTransition();
            }
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
            _screen?.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            ClearMountedScreenReferences();
            _mountedScreenKind = null;
            _screen = null;
            _root = null;
            _isDocumentRootBound = false;
            _presentationState = null;
            _sceneTransitionPending = false;
        }

        private void Update()
        {
            if (_hasAdaptiveViewportOverride || _screen == null)
            {
                return;
            }

            var viewport = RuntimeViewport();
            var safeArea = RuntimeSafeArea();
            if (viewport != _adaptiveViewport
                || safeArea != _adaptiveSafeArea)
            {
                ApplyAdaptiveLayout(viewport, safeArea, DeviceApplication.isMobilePlatform);
            }
        }

        public bool EnterGateway()
        {
            if (_hasEnteredGateway || Flow.Stage != FirstPlayableFlowStage.Home)
            {
                return false;
            }

            Require<TextField>("login-email").SetValueWithoutNotify(string.Empty);
            Require<TextField>("login-password").SetValueWithoutNotify(string.Empty);
            _hasEnteredGateway = true;
            _presentationState.HasEnteredGateway = true;
            Render();
            return true;
        }

        public bool OpenSetup()
        {
            if (!_hasEnteredGateway || !Flow.TryOpenSetup())
            {
                return false;
            }

            Render();
            return true;
        }

        public bool BeginQuest()
        {
            if (!_hasEnteredGateway
                || Flow.Stage != FirstPlayableFlowStage.Home
                || !Flow.TryOpenSetup())
            {
                return false;
            }

            Flow.TryConfigure(_homeCasasEnabled, _homeTrivilinImmediate);
            if (Flow.TryStartMatch())
            {
                BeginLoadingTransition();
                return true;
            }

            Flow.TryReturnHome();
            Render();
            return false;
        }

        public bool StartMatch()
        {
            Flow.TryConfigure(_casasToggle.value, _trivilinToggle.value);
            if (!Flow.TryStartMatch())
            {
                return false;
            }

            BeginLoadingTransition();
            return true;
        }

        public bool SubmitHumanIntent(PlayerIntent intent)
        {
            return TrySubmitHumanIntent(intent, out _);
        }

        public bool TrySubmitHumanIntent(PlayerIntent intent, out MatchAdvanceResult result)
        {
            if (IsPresentationBusy)
            {
                result = null;
                return false;
            }

            if (!Flow.TrySubmitHumanIntent(intent, out result))
            {
                return false;
            }

            MatchAdvanced?.Invoke(result);
            Render();
            return true;
        }

        public bool Replay()
        {
            if (!Flow.TryReplay())
            {
                return false;
            }

            BeginLoadingTransition();
            return true;
        }

        public bool ReturnHome()
        {
            if (!Flow.TryReturnHome())
            {
                return false;
            }

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }

            SetVisible(_hubModal, false);
            PresentationChanged?.Invoke();
            Render();
            return true;
        }

        public void RenderInteractionFeedback(CardInteractionState state)
        {
            if (state == null)
            {
                return;
            }

            SetMatchFeedback(
                state.FeedbackLocalizationKey,
                InteractionSemanticState(state.Feedback),
                InteractionSymbol(state.Feedback));
        }

        public void RenderPresentationEvent(DomainEvent resolvedEvent)
        {
            if (_matchEvent != null && resolvedEvent != null)
            {
                _matchEvent.text = EventSummary(resolvedEvent);
                ApplyOutcomeClass(resolvedEvent);
            }
        }

        public void SetPresentationBusy(bool isBusy)
        {
            if (IsPresentationBusy == isBusy)
            {
                return;
            }

            IsPresentationBusy = isBusy;
            if (!isBusy)
            {
                _contextState = null;
            }

            UpdatePresentationAvailability();
            if (!isBusy
                && _root != null
                && Flow != null
                && LocalizationSettings.SelectedLocale != null)
            {
                Render();
            }
        }

        public void Render()
        {
            if (_root == null || Flow == null)
            {
                return;
            }

            if (!_hasEnteredGateway)
            {
                if (!EnsurePresentationScene(FirstPlayableScreenKind.Login))
                {
                    return;
                }

                EnsureScreenMounted(FirstPlayableScreenKind.Login);
                RefreshLocalizedStaticText();
                _screen.EnableInClassList("show-login", true);
                _screen.EnableInClassList("show-hub", false);
                _screen.EnableInClassList("show-table", false);
                Focus("login-enter-button");
                UpdatePresentationAvailability();
                PresentationChanged?.Invoke();
                return;
            }

            var presentedStage = Flow.Stage == FirstPlayableFlowStage.Result && IsPresentationBusy
                ? FirstPlayableFlowStage.Match
                : Flow.Stage;
            var screenKind = ScreenKind(presentedStage);
            if (!EnsurePresentationScene(screenKind))
            {
                return;
            }

            EnsureScreenMounted(screenKind);
            RefreshLocalizedStaticText();
            _screen.EnableInClassList("show-login", false);
            _screen.EnableInClassList("show-hub", presentedStage == FirstPlayableFlowStage.Home);

            switch (presentedStage)
            {
                case FirstPlayableFlowStage.Home:
                    RenderHomeChat();
                    Focus("home-start-button");
                    break;
                case FirstPlayableFlowStage.Setup:
                    RenderSetup();
                    Focus("setup-start-button");
                    break;
                case FirstPlayableFlowStage.Loading:
                    RenderLoading();
                    break;
                case FirstPlayableFlowStage.Match:
                    RenderMatch();
                    break;
                case FirstPlayableFlowStage.Result:
                    RenderResult();
                    Focus("result-replay-button");
                    break;
            }

            _screen.EnableInClassList(
                "show-table",
                Flow.Match != null && (Flow.Stage == FirstPlayableFlowStage.Match || Flow.Stage == FirstPlayableFlowStage.Result));
            UpdatePresentationAvailability();
            PresentationChanged?.Invoke();
        }

        private void BindDocumentRoot()
        {
            if (_isDocumentRootBound)
            {
                return;
            }

            _screen = _root;
            _screen.name = "screen-root";
            _screen.AddToClassList("screen-root");
            _screen.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            _isDocumentRootBound = true;
        }

        private void EnsureScreenMounted(FirstPlayableScreenKind screenKind)
        {
            if (_mountedScreenKind == screenKind)
            {
                return;
            }

            var asset = ScreenAsset(screenKind);
            if (asset == null)
            {
                throw new MissingReferenceException(
                    $"The first-playable UI has no {screenKind} screen asset.");
            }

            var canBindDocumentSource =
                _mountedScreenKind == null && _document.visualTreeAsset == asset;
            ClearMountedScreenReferences();
            if (!canBindDocumentSource)
            {
                _root.Clear();
                asset.CloneTree(_root);
                UseSceneLayoutForPreviewRoots();
            }
            _mountedScreenKind = screenKind;
            _mountedStage = Require<VisualElement>(StageElementName(screenKind));
            BindMountedScreen(screenKind);
        }

        private void BindMountedScreen(FirstPlayableScreenKind screenKind)
        {
            switch (screenKind)
            {
                case FirstPlayableScreenKind.Login:
                    BindLoginScreen();
                    break;
                case FirstPlayableScreenKind.Hub:
                    BindHubScreen();
                    break;
                case FirstPlayableScreenKind.Setup:
                    BindSetupScreen();
                    break;
                case FirstPlayableScreenKind.Loading:
                    _loadingSession = Require<Label>("loading-session");
                    Require<Button>("loading-home-button").clicked += () => ReturnHome();
                    break;
                case FirstPlayableScreenKind.Match:
                    BindMatchScreen();
                    break;
                case FirstPlayableScreenKind.Result:
                    _resultOutcome = Require<Label>("result-outcome");
                    _resultScore = Require<Label>("result-score");
                    _resultRules = Require<Label>("result-rules");
                    Require<Button>("result-replay-button").clicked += () => Replay();
                    Require<Button>("result-home-button").clicked += () => ReturnHome();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenKind));
            }
        }

        private void BindLoginScreen()
        {
            _loginFeedback = Require<Label>("login-feedback");
            Require<TextField>("login-password").isPasswordField = true;
            Require<Button>("login-enter-button").clicked += () => EnterGateway();
            Require<Button>("login-forgot-button").clicked += () =>
                ShowLoginFeedback("flow.login.feedback.forgot");
            Require<Button>("login-google-button").clicked += () =>
                ShowLoginFeedback("flow.login.feedback.google");
            Require<Button>("login-apple-button").clicked += () =>
                ShowLoginFeedback("flow.login.feedback.apple");
            Require<Button>("login-create-button").clicked += () =>
                ShowLoginFeedback("flow.login.feedback.create");
        }

        private void BindHubScreen()
        {
            _homeActionStatus = Require<Label>("home-action-status");
            _homeChatDate = Require<Label>("home-chat-date");
            _homeChatMessageOne = Require<Label>("home-chat-message-one");
            _homeChatMessageTwo = Require<Label>("home-chat-message-two");
            _homeChatMessageThree = Require<Label>("home-chat-message-three");
            _homeChatUserMessage = Require<Label>("home-chat-user-message");
            _homeChatInput = Require<TextField>("home-chat-input");
            _homeChatGlobalButton = Require<Button>("home-chat-global-button");
            _homeChatGuildButton = Require<Button>("home-chat-guild-button");
            _homeChatSystemButton = Require<Button>("home-chat-system-button");
            _hubModal = Require<VisualElement>("hub-modal");
            _hubModalTitle = Require<Label>("hub-modal-title");
            _hubModalDescription = Require<Label>("hub-modal-description");
            _hubSettingsContent = Require<VisualElement>("hub-settings-content");
            _homeCasasToggle = Require<Toggle>("home-settings-casas-toggle");
            _homeTrivilinToggle = Require<Toggle>("home-settings-trivilin-toggle");
            _homeAnimationFastToggle = Require<Toggle>("home-settings-animation-fast-toggle");
            _homeAnimationReducedToggle = Require<Toggle>("home-settings-animation-reduced-toggle");
            _homeAudioMasterToggle = Require<Toggle>("home-settings-audio-master-toggle");
            _homeAudioEffectsToggle = Require<Toggle>("home-settings-audio-effects-toggle");
            _homeAudioMusicToggle = Require<Toggle>("home-settings-audio-music-toggle");

            Require<Button>("home-start-button").clicked += () => BeginQuest();
            Require<Button>("home-mail-button").clicked += () =>
                ShowHubPanel("flow.home.mail.title", "flow.home.mail.description");
            Require<Button>("home-settings-button").clicked += () => OpenSettings();
            Require<Button>("home-decks-button").clicked += () =>
                SelectHubDestination(
                    "flow.home.status.decks",
                    "flow.home.decks.title",
                    "flow.home.decks.description");
            Require<Button>("home-bag-button").clicked += () =>
                SelectHubDestination(
                    "flow.home.status.bag",
                    "flow.home.bag.title",
                    "flow.home.bag.description");
            Require<Button>("home-shop-button").clicked += () =>
                SelectHubDestination(
                    "flow.home.status.shop",
                    "flow.home.shop.title",
                    "flow.home.shop.description");
            Require<Button>("home-rank-button").clicked += () =>
                SelectHubDestination(
                    "flow.home.status.rank",
                    "flow.home.rank.title",
                    "flow.home.rank.description");
            _homeChatGlobalButton.clicked += () => SelectHomeChatChannel("global");
            _homeChatGuildButton.clicked += () => SelectHomeChatChannel("guild");
            _homeChatSystemButton.clicked += () => SelectHomeChatChannel("system");
            Require<Button>("home-chat-send-button").clicked += SendHomeChatMessage;
            Require<Button>("hub-modal-close-button").clicked += CloseHubPanel;

            _homeCasasToggle.SetValueWithoutNotify(_homeCasasEnabled);
            _homeTrivilinToggle.SetValueWithoutNotify(_homeTrivilinImmediate);
            _homeAnimationFastToggle.SetValueWithoutNotify(_animationFastForwardEnabled);
            _homeAnimationReducedToggle.SetValueWithoutNotify(_animationReducedMotionEnabled);
            _homeAudioMasterToggle.SetValueWithoutNotify(_audioMasterEnabled);
            _homeAudioEffectsToggle.SetValueWithoutNotify(_audioEffectsEnabled);
            _homeAudioMusicToggle.SetValueWithoutNotify(_audioMusicEnabled);
            _homeChatUserMessage.text = _homeChatUserMessageText ?? string.Empty;

            _homeCasasToggle.RegisterValueChangedCallback(change =>
            {
                _homeCasasEnabled = change.newValue;
                _presentationState.CasasEnabled = change.newValue;
            });
            _homeTrivilinToggle.RegisterValueChangedCallback(change =>
            {
                _homeTrivilinImmediate = change.newValue;
                _presentationState.TrivilinImmediate = change.newValue;
            });
            _homeAnimationFastToggle.RegisterValueChangedCallback(change =>
            {
                _animationFastForwardEnabled = change.newValue;
                _presentationState.AnimationFastForwardEnabled = change.newValue;
                AnimationFastForwardChanged?.Invoke(change.newValue);
            });
            _homeAnimationReducedToggle.RegisterValueChangedCallback(change =>
            {
                _animationReducedMotionEnabled = change.newValue;
                _presentationState.AnimationReducedMotionEnabled = change.newValue;
                AnimationReducedMotionChanged?.Invoke(change.newValue);
            });
            _homeAudioMasterToggle.RegisterValueChangedCallback(change =>
            {
                SetAudioMasterEnabled(change.newValue);
            });
            _homeAudioEffectsToggle.RegisterValueChangedCallback(change =>
            {
                SetAudioEffectsEnabled(change.newValue);
            });
            _homeAudioMusicToggle.RegisterValueChangedCallback(change =>
            {
                SetAudioMusicEnabled(change.newValue);
            });

            SetVisible(_hubModal, false);
            SetVisible(_hubSettingsContent, false);
            SetVisible(_homeChatUserMessage, !string.IsNullOrWhiteSpace(_homeChatUserMessageText));
        }

        private void BindSetupScreen()
        {
            _casasToggle = Require<Toggle>("casas-toggle");
            _trivilinToggle = Require<Toggle>("trivilin-toggle");
            _casasState = Require<Label>("casas-state");
            _trivilinState = Require<Label>("trivilin-state");
            Require<Button>("setup-start-button").clicked += () => StartMatch();
            Require<Button>("setup-back-button").clicked += () => ReturnHome();
            _casasToggle.RegisterValueChangedCallback(change =>
            {
                if (Flow.TryConfigure(change.newValue, _trivilinToggle.value))
                {
                    RenderSetup();
                }
            });
            _trivilinToggle.RegisterValueChangedCallback(change =>
            {
                if (Flow.TryConfigure(_casasToggle.value, change.newValue))
                {
                    RenderSetup();
                }
            });
        }

        private void BindMatchScreen()
        {
            _matchPhase = Require<Label>("match-phase");
            _matchScore = Require<Label>("match-score");
            _matchProgress = Require<Label>("match-progress");
            _matchTurn = Require<Label>("match-turn");
            _matchCanto = Require<Label>("match-canto");
            _matchEvent = Require<Label>("match-event");
            _matchEventCallout = Require<VisualElement>("match-event-callout");
            _matchFeedback = Require<Label>("match-feedback");
            _matchFeedbackSymbol = Require<Label>("match-feedback-symbol");
            _matchFeedbackCallout = Require<VisualElement>("match-feedback-callout");
            _dealerContext = Require<VisualElement>("dealer-context");
            _dealerOptionsMenu = Require<VisualElement>("dealer-options-menu");
            _dealerOptions = Require<VisualElement>("dealer-options");
            _dealerOptionsButton = Require<Button>("dealer-options-button");
            _cantoContext = Require<VisualElement>("canto-context");
            _cantoOptionsMenu = Require<VisualElement>("canto-options-menu");
            _cantoOptions = Require<VisualElement>("canto-options");
            _cantoOptionsButton = Require<Button>("canto-options-button");
            _animationSkipButton = Require<Button>("animation-skip-button");
            Require<Button>("match-home-button").clicked += () => ReturnHome();
            _dealerOptionsButton.clicked += ToggleDealerOptions;
            _cantoOptionsButton.clicked += ToggleCantoOptions;
            _animationSkipButton.clicked += () => AnimationSkipRequested?.Invoke();
        }

        private void ClearMountedScreenReferences()
        {
            _mountedStage = null;
            _casasToggle = null;
            _trivilinToggle = null;
            _casasState = null;
            _trivilinState = null;
            _loadingSession = null;
            _matchPhase = null;
            _matchScore = null;
            _matchProgress = null;
            _matchTurn = null;
            _matchCanto = null;
            _matchEvent = null;
            _matchEventCallout = null;
            _matchFeedback = null;
            _matchFeedbackSymbol = null;
            _matchFeedbackCallout = null;
            _resultOutcome = null;
            _resultScore = null;
            _resultRules = null;
            _dealerContext = null;
            _dealerOptionsMenu = null;
            _dealerOptions = null;
            _dealerOptionsButton = null;
            _cantoContext = null;
            _cantoOptionsMenu = null;
            _cantoOptions = null;
            _cantoOptionsButton = null;
            _animationSkipButton = null;
            _loginFeedback = null;
            _homeActionStatus = null;
            _homeChatDate = null;
            _homeChatMessageOne = null;
            _homeChatMessageTwo = null;
            _homeChatMessageThree = null;
            _homeChatUserMessage = null;
            _homeChatInput = null;
            _homeChatGlobalButton = null;
            _homeChatGuildButton = null;
            _homeChatSystemButton = null;
            _hubModal = null;
            _hubModalTitle = null;
            _hubModalDescription = null;
            _hubSettingsContent = null;
            _homeCasasToggle = null;
            _homeTrivilinToggle = null;
            _homeAnimationFastToggle = null;
            _homeAnimationReducedToggle = null;
            _homeAudioMasterToggle = null;
            _homeAudioEffectsToggle = null;
            _homeAudioMusicToggle = null;
        }

        private void ShowLoginFeedback(string localizationKey)
        {
            _loginFeedback.text = Localize(localizationKey);
        }

        private void SelectHubDestination(string statusKey, string titleKey, string descriptionKey)
        {
            _homeActionStatus.text = Localize(statusKey);
            ShowHubPanel(titleKey, descriptionKey);
        }

        private void ShowHubPanel(string titleKey, string descriptionKey)
        {
            _hubModalTitle.text = Localize(titleKey);
            _hubModalDescription.text = Localize(descriptionKey);
            _hubModal.EnableInClassList("hub-modal-settings", false);
            SetVisible(_hubModalDescription, true);
            SetVisible(_hubSettingsContent, false);
            SetVisible(_hubModal, true);
            Focus("hub-modal-close-button");
        }

        public bool OpenSettings()
        {
            if (!_hasEnteredGateway || Flow.Stage != FirstPlayableFlowStage.Home)
            {
                return false;
            }

            _hubModalTitle.text = Localize("flow.home.settings.title");
            _hubModalDescription.text = Localize("flow.home.settings.description");
            _hubModal.EnableInClassList("hub-modal-settings", true);
            SetVisible(_hubModalDescription, true);
            SetVisible(_hubSettingsContent, true);
            SetVisible(_hubModal, true);
            Focus("home-settings-casas-toggle");
            return true;
        }

        private void CloseHubPanel()
        {
            SetVisible(_hubModal, false);
            Focus("home-start-button");
        }

        private void SelectHomeChatChannel(string channel)
        {
            _homeChatChannel = channel;
            _presentationState.HomeChatChannel = channel;
            RenderHomeChat();
        }

        private void RenderHomeChat()
        {
            _homeChatGlobalButton.EnableInClassList("hub-chat-tab-active", _homeChatChannel == "global");
            _homeChatGuildButton.EnableInClassList("hub-chat-tab-active", _homeChatChannel == "guild");
            _homeChatSystemButton.EnableInClassList("hub-chat-tab-active", _homeChatChannel == "system");
            _homeChatDate.text = Localize("flow.home.chat.date");
            _homeChatMessageOne.text = Localize($"flow.home.chat.{_homeChatChannel}.one");
            _homeChatMessageTwo.text = Localize($"flow.home.chat.{_homeChatChannel}.two");
            _homeChatMessageThree.text = Localize($"flow.home.chat.{_homeChatChannel}.three");
            SetVisible(
                _homeChatUserMessage,
                _homeChatChannel == "global" && !string.IsNullOrWhiteSpace(_homeChatUserMessage.text));
        }

        private void SendHomeChatMessage()
        {
            var message = _homeChatInput.value?.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                _homeActionStatus.text = Localize("flow.home.chat.empty");
                return;
            }

            _homeChatChannel = "global";
            _homeChatUserMessageText = Localize("flow.home.chat.you", message);
            _presentationState.HomeChatChannel = _homeChatChannel;
            _presentationState.HomeChatUserMessageText = _homeChatUserMessageText;
            _homeChatUserMessage.text = _homeChatUserMessageText;
            _homeChatInput.SetValueWithoutNotify(string.Empty);
            _homeActionStatus.text = Localize("flow.home.chat.sent");
            RenderHomeChat();
        }

        private void RenderSetup()
        {
            _casasToggle.SetValueWithoutNotify(Flow.Setup.CasaCantosEnabled);
            _trivilinToggle.SetValueWithoutNotify(Flow.Setup.TrivilinWinsImmediately);
            _casasToggle.text = Localize("flow.setup.casas");
            _trivilinToggle.text = Localize("flow.setup.trivilin");
            _casasState.text = Localize(Flow.Setup.CasaCantosEnabled
                ? "flow.setup.casas-state.enabled"
                : "flow.setup.casas-state.disabled");
            _trivilinState.text = Localize(Flow.Setup.TrivilinWinsImmediately
                ? "flow.setup.trivilin-state.immediate"
                : "flow.setup.trivilin-state.points");
        }

        private void RenderLoading()
        {
            _loadingSession.text = Localize(
                "flow.loading.session",
                Flow.SessionNumber,
                RulesSummary());
        }

        private void RenderMatch()
        {
            var state = Flow.Match.State;
            _matchPhase.text = Localize(MatchPhaseLocalizationKey(state.Phase));
            _matchScore.text = Localize(
                "flow.match.score",
                state.TeamOneScore.Value,
                state.TeamTwoScore.Value,
                state.Rules.VictoryTarget);
            var progressState = Localize(state.IsTieExtension
                ? "flow.match.tie-extension"
                : "flow.match.standard-round");
            if (state.IsFinalDeal)
            {
                progressState = $"{progressState} · {Localize("flow.match.final-deal")}";
            }

            _matchProgress.text = Localize(
                "flow.match.progress",
                state.RoundNumber,
                state.DealNumber,
                progressState);
            _matchTurn.text = state.Phase == MatchPhase.DealerSelection
                ? Localize(
                    "flow.match.turn.dealer-pending",
                    Localize(state.CurrentSeat == Seat.First ? "flow.player.you" : "flow.player.bot"))
                : Localize(
                    "flow.match.turn",
                    Localize(state.DealerSeat == Seat.First ? "flow.player.you" : "flow.player.bot"),
                    Localize(state.CurrentSeat == Seat.First ? "flow.player.you" : "flow.player.bot"));
            _matchCanto.text = CantoSummary(state);
            var latestEvent = Flow.Match.Trace.Events.Count == 0
                ? null
                : Flow.Match.Trace.Events[Flow.Match.Trace.Events.Count - 1];
            _matchEvent.text = latestEvent == null
                ? Localize("flow.match.event.ready")
                : EventSummary(latestEvent);
            ApplyOutcomeClass(latestEvent);
            var legalIntents = IsPresentationBusy
                ? Array.Empty<PlayerIntent>()
                : Flow.Match.GetHumanLegalIntents();
            if (!ReferenceEquals(_contextState, state))
            {
                _contextState = state;
                _isDealerMenuOpen = HasIntent<ChooseDealOptionsIntent>(legalIntents);
                _isCantoMenuOpen = false;
            }

            RenderContextualActions(state, legalIntents);
            if (IsPresentationBusy)
            {
                SetMatchFeedback(
                    "interaction.feedback.temporarily-blocked",
                    AdaptiveUiSemanticState.Blocked,
                    "Ⅱ");
            }
        }

        private void RenderContextualActions(MatchState state, IReadOnlyList<PlayerIntent> legalIntents)
        {
            _dealerOptions.Clear();
            _cantoOptions.Clear();

            var dealerOptionCount = 0;
            var cantoOptionCount = 0;
            for (var index = 0; index < legalIntents.Count; index++)
            {
                var intent = legalIntents[index];
                if (intent is ChooseDealOptionsIntent)
                {
                    _dealerOptions.Add(CreateContextButton(
                        $"dealer-option-{dealerOptionCount}",
                        IntentText(intent, dealerOptionCount),
                        intent));
                    dealerOptionCount++;
                }
                else if (intent is AnnounceCantoIntent)
                {
                    _cantoOptions.Add(CreateContextButton(
                        $"canto-option-{cantoOptionCount}",
                        IntentText(intent, cantoOptionCount),
                        intent));
                    cantoOptionCount++;
                }
            }

            SetVisible(_dealerContext, dealerOptionCount > 0);
            SetVisible(_dealerOptionsMenu, dealerOptionCount > 0 && _isDealerMenuOpen);
            SetVisible(_cantoContext, cantoOptionCount > 0);
            SetVisible(_cantoOptionsMenu, cantoOptionCount > 0 && _isCantoMenuOpen);

            _dealerOptionsButton.tooltip = Localize("flow.context.dealer-tooltip");
            _cantoOptionsButton.tooltip = Localize("flow.context.canto-tooltip");
            SetMatchFeedback(
                state.Phase == MatchPhase.DealerSelection
                    ? "flow.context.dealer-card-prompt"
                    : dealerOptionCount > 0
                        ? "flow.context.dealer-required"
                        : "interaction.feedback.legal",
                AdaptiveUiSemanticState.Legal,
                "+");
        }

        private Button CreateContextButton(string name, string text, PlayerIntent intent)
        {
            var button = new Button(() =>
            {
                _isDealerMenuOpen = false;
                _isCantoMenuOpen = false;
                SubmitHumanIntent(intent);
            })
            {
                name = name,
                text = text,
                tooltip = text,
            };
            button.AddToClassList("context-action-button");
            return button;
        }

        public void ToggleDealerOptions()
        {
            _isDealerMenuOpen = !_isDealerMenuOpen;
            RenderContextualActions(Flow.Match.State, Flow.Match.GetHumanLegalIntents());
        }

        public void ToggleCantoOptions()
        {
            _isCantoMenuOpen = !_isCantoMenuOpen;
            RenderContextualActions(Flow.Match.State, Flow.Match.GetHumanLegalIntents());
        }

        private void RenderResult()
        {
            var state = Flow.Match.State;
            _resultOutcome.text = Localize(
                state.WinnerTeam == TeamId.One ? "flow.result.victory" : "flow.result.defeat");
            _resultScore.text = Localize(
                "flow.result.score",
                state.TeamOneScore.Value,
                state.TeamTwoScore.Value,
                state.RoundNumber);
            _resultRules.text = Localize("flow.result.rules", RulesSummary());
        }

        private string RulesSummary()
        {
            return Localize(
                "flow.rules.summary",
                Localize(Flow.Setup.CasaCantosEnabled
                    ? "flow.rules.casas.enabled"
                    : "flow.rules.casas.disabled"),
                Localize(Flow.Setup.TrivilinWinsImmediately
                    ? "flow.rules.trivilin.immediate"
                    : "flow.rules.trivilin.points"));
        }

        private string IntentText(PlayerIntent intent, int index)
        {
            if (intent is SelectDealerCardIntent)
            {
                return Localize("flow.action.dealer-card", index + 1);
            }

            if (intent is ChooseDealOptionsIntent dealOptions)
            {
                return Localize(
                    "flow.action.deal-options",
                    Localize(dealOptions.DealHandsBeforeTable
                        ? "flow.action.hands-first"
                        : "flow.action.table-first"),
                    Localize(dealOptions.OpeningPattern == OpeningPattern.Ascending
                        ? "flow.action.ascending"
                        : "flow.action.descending"));
            }

            if (intent is AnnounceCantoIntent canto)
            {
                return Localize("flow.action.announce-canto", Localize(CantoLocalizationKey(canto.ClaimedKind)));
            }

            if (intent is PlayCardIntent play)
            {
                return Localize(
                    "flow.action.play-card",
                    (int)play.Card.Rank,
                    Localize(SuitLocalizationKey(play.Card.Suit)));
            }

            return Localize("flow.action.unavailable");
        }

        private string CantoSummary(MatchState state)
        {
            if (state.CantoAnnouncements.Count == 0)
            {
                return Localize("flow.match.canto.none");
            }

            var announcements = new string[state.CantoAnnouncements.Count];
            for (var index = 0; index < state.CantoAnnouncements.Count; index++)
            {
                var announcement = state.CantoAnnouncements[index];
                announcements[index] = Localize(
                    "flow.match.canto.announcement",
                    Localize(announcement.PlayerId == state.GetPlayerAt(Seat.First).Player.Id
                        ? "flow.player.you"
                        : "flow.player.bot"),
                    Localize(CantoLocalizationKey(announcement.ClaimedKind)));
            }

            return Localize("flow.match.canto.summary", string.Join(" · ", announcements));
        }

        private string EventSummary()
        {
            var events = Flow.Match.Trace.Events;
            if (events.Count == 0)
            {
                return Localize("flow.match.event.ready");
            }

            return EventSummary(events[events.Count - 1]);
        }

        private string EventSummary(DomainEvent resolvedEvent)
        {
            if (resolvedEvent is MatchStartedEvent started)
            {
                return Localize("flow.match.event.match-started", started.DealerSpreadCardCount);
            }

            if (resolvedEvent is DealerSelectedEvent dealer)
            {
                return Localize("flow.match.event.dealer-selected", PlayerDisplayName(dealer.PlayerId));
            }

            if (resolvedEvent is DeckShuffledEvent shuffled)
            {
                return Localize("flow.match.event.deck-shuffled", shuffled.RoundNumber);
            }

            if (resolvedEvent is DealStartedEvent deal)
            {
                return Localize("flow.match.event.deal-started", deal.RoundNumber, deal.DealNumber);
            }

            if (resolvedEvent is CardPlayedEvent played)
            {
                return Localize(
                    "flow.match.event.card-played",
                    PlayerDisplayName(played.PlayerId),
                    (int)played.Card.Rank,
                    Localize(SuitLocalizationKey(played.Card.Suit)));
            }

            if (resolvedEvent is CardsCapturedEvent captured)
            {
                return Localize(
                    captured.Cards.Count > 2
                        ? "flow.match.event.cascade-captured"
                        : "flow.match.event.cards-captured",
                    PlayerDisplayName(captured.PlayerId),
                    captured.Cards.Count);
            }

            if (resolvedEvent is CantoAnnouncedEvent canto)
            {
                return Localize(
                    "flow.match.event.canto-announced",
                    PlayerDisplayName(canto.PlayerId),
                    Localize(CantoLocalizationKey(canto.ClaimedKind)));
            }

            if (resolvedEvent is CantoResolvedEvent resolvedCanto)
            {
                return Localize(
                    resolvedCanto.IsValid
                        ? resolvedCanto.DidScore
                            ? "flow.match.event.canto-scored"
                            : "flow.match.event.canto-resolved"
                        : "flow.match.event.canto-rejected",
                    PlayerDisplayName(resolvedCanto.PlayerId),
                    Localize(CantoLocalizationKey(resolvedCanto.ClaimedKind)));
            }

            if (resolvedEvent is ScoreChangedEvent score)
            {
                return Localize(
                    "flow.match.event.score-changed",
                    Localize(ScoreReasonLocalizationKey(score.Reason)),
                    score.PointsAwarded,
                    score.Total.Value);
            }

            if (resolvedEvent is RoundCompletedEvent round)
            {
                return Localize("flow.match.event.round-completed", round.RoundNumber);
            }

            if (resolvedEvent is TieExtensionStartedEvent tie)
            {
                return Localize("flow.match.event.tie-extension", tie.TiedScore.Value);
            }

            if (resolvedEvent is TurnChangedEvent turn)
            {
                return Localize(
                    "flow.match.event.turn-changed",
                    Localize(turn.CurrentSeat == Seat.First ? "flow.player.you" : "flow.player.bot"));
            }

            if (resolvedEvent is MatchCompletedEvent completed)
            {
                return Localize(
                    "flow.match.event.match-completed",
                    Localize(completed.WinnerTeam == TeamId.One ? "flow.player.you" : "flow.player.bot"));
            }

            return Localize("flow.match.event.resolved");
        }

        private void SetMatchFeedback(
            string localizationKey,
            AdaptiveUiSemanticState semanticState,
            string symbol)
        {
            if (_matchFeedback == null || string.IsNullOrWhiteSpace(localizationKey))
            {
                return;
            }

            _matchFeedback.text = Localize(localizationKey);
            _matchFeedbackSymbol.text = symbol;
            AdaptiveUiFoundation.ApplySemanticState(_matchFeedbackCallout, semanticState);
        }

        private static AdaptiveUiSemanticState InteractionSemanticState(
            CardInteractionFeedback feedback)
        {
            switch (feedback)
            {
                case CardInteractionFeedback.Inspected:
                    return AdaptiveUiSemanticState.Inspected;
                case CardInteractionFeedback.Selected:
                    return AdaptiveUiSemanticState.Selected;
                case CardInteractionFeedback.Confirmed:
                    return AdaptiveUiSemanticState.Confirmed;
                case CardInteractionFeedback.Cancelled:
                    return AdaptiveUiSemanticState.Cancelled;
                case CardInteractionFeedback.Rejected:
                    return AdaptiveUiSemanticState.Rejected;
                case CardInteractionFeedback.TemporarilyBlocked:
                    return AdaptiveUiSemanticState.Blocked;
                default:
                    return AdaptiveUiSemanticState.Legal;
            }
        }

        private static string InteractionSymbol(CardInteractionFeedback feedback)
        {
            switch (feedback)
            {
                case CardInteractionFeedback.Inspected:
                    return "?";
                case CardInteractionFeedback.Selected:
                    return "◆";
                case CardInteractionFeedback.Confirmed:
                    return "✓";
                case CardInteractionFeedback.Cancelled:
                    return "↶";
                case CardInteractionFeedback.Rejected:
                    return "×";
                case CardInteractionFeedback.TemporarilyBlocked:
                    return "Ⅱ";
                default:
                    return "+";
            }
        }

        private void ApplyOutcomeClass(DomainEvent resolvedEvent)
        {
            if (_matchEventCallout == null)
            {
                return;
            }

            foreach (var className in MatchOutcomeClasses)
            {
                _matchEventCallout.RemoveFromClassList(className);
            }

            var classToAdd = MatchOutcomeClass(resolvedEvent);
            if (!string.IsNullOrEmpty(classToAdd))
            {
                _matchEventCallout.AddToClassList(classToAdd);
            }
        }

        private static string MatchOutcomeClass(DomainEvent resolvedEvent)
        {
            if (resolvedEvent is ScoreChangedEvent score)
            {
                switch (score.Reason)
                {
                    case ScoreReason.Fall:
                        return "outcome-fall";
                    case ScoreReason.CleanTable:
                        return "outcome-clean-table";
                    case ScoreReason.Canto:
                    case ScoreReason.FalseCantoPenalty:
                        return "outcome-canto";
                    default:
                        return "outcome-score";
                }
            }

            if (resolvedEvent is CardsCapturedEvent)
            {
                return "outcome-capture";
            }

            if (resolvedEvent is CantoAnnouncedEvent || resolvedEvent is CantoResolvedEvent)
            {
                return "outcome-canto";
            }

            if (resolvedEvent is TieExtensionStartedEvent)
            {
                return "outcome-tie";
            }

            return resolvedEvent is MatchCompletedEvent
                ? "outcome-victory"
                : null;
        }

        private void UpdatePresentationAvailability()
        {
            _dealerOptionsButton?.SetEnabled(!IsPresentationBusy);
            _cantoOptionsButton?.SetEnabled(!IsPresentationBusy);
            _dealerOptions?.SetEnabled(!IsPresentationBusy);
            _cantoOptions?.SetEnabled(!IsPresentationBusy);
            _animationSkipButton?.SetEnabled(IsPresentationBusy);
        }

        private string PlayerDisplayName(PlayerId playerId)
        {
            return Localize(playerId == Flow.Match.State.GetPlayerAt(Seat.First).Player.Id
                ? "flow.player.you"
                : "flow.player.bot");
        }

        private void BeginLoadingTransition()
        {
            Render();
            if (_sceneTransitionPending)
            {
                return;
            }

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
            }

            _loadingCoroutine = StartCoroutine(FinishLoadingAfterVisibleFrame(Flow.SessionNumber));
        }

        private IEnumerator FinishLoadingAfterVisibleFrame(int sessionNumber)
        {
            yield return null;
            Flow.TryFinishLoading(sessionNumber);
            _loadingCoroutine = null;
            Render();
        }

        private void RefreshLocalizedStaticText()
        {
            foreach (var binding in LabelLocalizationKeys)
            {
                var label = _root.Q<Label>(binding.Key);
                if (label != null)
                {
                    label.text = Localize(binding.Value);
                }
            }

            foreach (var binding in ButtonLocalizationKeys)
            {
                var button = _root.Q<Button>(binding.Key);
                if (button == null)
                {
                    continue;
                }

                var localizedText = Localize(binding.Value);
                if (button.ClassListContains("icon-only-button"))
                {
                    button.text = string.Empty;
                    button.tooltip = localizedText;
                    continue;
                }

                button.text = localizedText;
            }

            if (_casasToggle != null)
            {
                _casasToggle.text = Localize("flow.setup.casas");
                _trivilinToggle.text = Localize("flow.setup.trivilin");
            }

            if (_homeAudioMasterToggle != null)
            {
                _homeCasasToggle.text = Localize("flow.setup.casas");
                _homeTrivilinToggle.text = Localize("flow.setup.trivilin");
                _homeAnimationFastToggle.text = Localize("flow.animation.fast-forward");
                _homeAnimationReducedToggle.text = Localize("flow.animation.reduced-motion");
                _homeAudioMasterToggle.text = Localize("flow.audio.master");
                _homeAudioEffectsToggle.text = Localize("flow.audio.effects");
                _homeAudioMusicToggle.text = Localize("flow.audio.music");
            }
        }

        private void HandleLocaleChanged(Locale locale)
        {
            Render();
        }

        private void HandleGeometryChanged(GeometryChangedEvent change)
        {
            var viewport = _hasAdaptiveViewportOverride
                ? _adaptiveViewport
                : RuntimeViewport();
            var safeArea = _hasAdaptiveViewportOverride
                ? _adaptiveSafeArea
                : RuntimeSafeArea();
            ApplyAdaptiveLayout(
                viewport,
                safeArea,
                _hasAdaptiveViewportOverride
                    ? CurrentAdaptiveLayout.Profile != AdaptiveUiProfile.Desktop
                    : DeviceApplication.isMobilePlatform);
        }

        public void ApplyViewportForTests(
            Vector2Int viewportPixels,
            Rect safeAreaPixels,
            bool isMobilePlatform)
        {
            _hasAdaptiveViewportOverride = true;
            ApplyAdaptiveLayout(viewportPixels, safeAreaPixels, isMobilePlatform);
        }

        public void ClearViewportOverrideForTests()
        {
            _hasAdaptiveViewportOverride = false;
            ApplyAdaptiveLayout(
                RuntimeViewport(),
                RuntimeSafeArea(),
                DeviceApplication.isMobilePlatform);
        }

        private void ApplyAdaptiveLayout(
            Vector2Int viewportPixels,
            Rect safeAreaPixels,
            bool isMobilePlatform)
        {
            if (_screen == null)
            {
                return;
            }

            _adaptiveViewport = viewportPixels;
            _adaptiveSafeArea = safeAreaPixels;
            var resolvedLayout = AdaptiveUiFoundation.Resolve(
                viewportPixels,
                safeAreaPixels,
                isMobilePlatform);
            CurrentAdaptiveLayout = !UnityEngine.Application.isPlaying
                && !_hasAdaptiveViewportOverride
                    ? new AdaptiveUiLayout(
                        _authoringPreviewProfile,
                        resolvedLayout.ViewportPixels,
                        resolvedLayout.NormalizedSafeArea)
                    : resolvedLayout;
            AdaptiveUiFoundation.ApplyProfileClass(
                _screen,
                CurrentAdaptiveLayout.Profile);
            _screen.EnableInClassList("compact", false);
        }

        private static Vector2Int RuntimeViewport()
        {
            return new Vector2Int(Mathf.Max(1, DeviceScreen.width), Mathf.Max(1, DeviceScreen.height));
        }

        private static Rect RuntimeSafeArea()
        {
            var safeArea = DeviceScreen.safeArea;
            return safeArea.width > 0f && safeArea.height > 0f
                ? safeArea
                : new Rect(0f, 0f, DeviceScreen.width, DeviceScreen.height);
        }

        private AdaptiveUiProfile ResolveAuthoringPreviewProfile()
        {
            return _root.Q<AdaptiveUiPreviewRoot>()?.PreviewProfile
                ?? AdaptiveUiProfile.PhoneLandscape;
        }

        private void UseSceneLayoutForPreviewRoots()
        {
            foreach (var previewRoot in _root.Query<AdaptiveUiPreviewRoot>().ToList())
            {
                previewRoot.UseSceneLayout();
            }
        }

        private string Localize(string key, params object[] arguments)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(TableName, key, arguments);
        }

        private static bool HasIntent<TIntent>(IReadOnlyList<PlayerIntent> intents)
            where TIntent : PlayerIntent
        {
            for (var index = 0; index < intents.Count; index++)
            {
                if (intents[index] is TIntent)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetVisible(VisualElement element, bool isVisible)
        {
            if (element != null)
            {
                element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void Focus(string elementName)
        {
            var element = Require<VisualElement>(elementName);
            element.schedule.Execute(element.Focus);
        }

        private T Require<T>(string name)
            where T : VisualElement
        {
            var element = _root.Q<T>(name);
            if (element == null)
            {
                throw new MissingReferenceException($"The first-playable UI is missing {name}.");
            }

            return element;
        }

        private VisualTreeAsset ScreenAsset(FirstPlayableScreenKind screenKind)
        {
            switch (screenKind)
            {
                case FirstPlayableScreenKind.Login:
                    return _loginScreenAsset;
                case FirstPlayableScreenKind.Hub:
                    return _hubScreenAsset;
                case FirstPlayableScreenKind.Setup:
                    return _setupScreenAsset;
                case FirstPlayableScreenKind.Loading:
                    return _loadingScreenAsset;
                case FirstPlayableScreenKind.Match:
                    return _matchScreenAsset;
                case FirstPlayableScreenKind.Result:
                    return _resultScreenAsset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenKind));
            }
        }

        private static FirstPlayableScreenKind ScreenKind(FirstPlayableFlowStage stage)
        {
            switch (stage)
            {
                case FirstPlayableFlowStage.Home:
                    return FirstPlayableScreenKind.Hub;
                case FirstPlayableFlowStage.Setup:
                    return FirstPlayableScreenKind.Setup;
                case FirstPlayableFlowStage.Loading:
                    return FirstPlayableScreenKind.Loading;
                case FirstPlayableFlowStage.Match:
                    return FirstPlayableScreenKind.Match;
                case FirstPlayableFlowStage.Result:
                    return FirstPlayableScreenKind.Result;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage));
            }
        }

        private static string StageElementName(FirstPlayableScreenKind screenKind)
        {
            switch (screenKind)
            {
                case FirstPlayableScreenKind.Login:
                    return "login-stage";
                case FirstPlayableScreenKind.Hub:
                    return "home-stage";
                case FirstPlayableScreenKind.Setup:
                    return "setup-stage";
                case FirstPlayableScreenKind.Loading:
                    return "loading-stage";
                case FirstPlayableScreenKind.Match:
                    return "match-stage";
                case FirstPlayableScreenKind.Result:
                    return "result-stage";
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenKind));
            }
        }

        private void RestorePresentationState()
        {
            if (_presentationState == null)
            {
                return;
            }

            if (_sceneKind != FirstPlayableSceneKind.Login)
            {
                _presentationState.HasEnteredGateway = true;
            }

            _hasEnteredGateway = _presentationState.HasEnteredGateway;
            _homeCasasEnabled = _presentationState.CasasEnabled;
            _homeTrivilinImmediate = _presentationState.TrivilinImmediate;
            _animationFastForwardEnabled = _presentationState.AnimationFastForwardEnabled;
            _animationReducedMotionEnabled = _presentationState.AnimationReducedMotionEnabled;
            _audioMasterEnabled = _presentationState.AudioMasterEnabled;
            _audioEffectsEnabled = _presentationState.AudioEffectsEnabled;
            _audioMusicEnabled = _presentationState.AudioMusicEnabled;
            _homeChatChannel = _presentationState.HomeChatChannel;
            _homeChatUserMessageText = _presentationState.HomeChatUserMessageText;
        }

        private void PrepareDirectSceneState()
        {
            if (_sceneKind != FirstPlayableSceneKind.Match
                || _presentationState.HasEnteredGateway
                || Flow.Stage != FirstPlayableFlowStage.Home)
            {
                return;
            }

            _presentationState.HasEnteredGateway = true;
            if (Flow.TryOpenSetup())
            {
                Flow.TryConfigure(
                    _presentationState.CasasEnabled,
                    _presentationState.TrivilinImmediate);
                Flow.TryStartMatch();
            }
        }

        private bool EnsurePresentationScene(FirstPlayableScreenKind screenKind)
        {
            var requiredSceneKind = FirstPlayableSceneContract.SceneForScreen(screenKind);
            if (_sceneKind == requiredSceneKind)
            {
                return true;
            }

            if (_sceneTransitionPending)
            {
                return false;
            }

            _sceneTransitionPending = true;
            SceneManager.LoadSceneAsync(
                FirstPlayableSceneContract.SceneName(requiredSceneKind),
                LoadSceneMode.Single);
            return false;
        }

        private static string MatchPhaseLocalizationKey(MatchPhase phase)
        {
            switch (phase)
            {
                case MatchPhase.DealerSelection:
                    return "flow.match.phase.dealer-selection";
                case MatchPhase.AwaitingDealerChoice:
                    return "flow.match.phase.dealer-choice";
                case MatchPhase.Active:
                    return "flow.match.phase.active";
                case MatchPhase.Completed:
                    return "flow.match.phase.completed";
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(phase));
            }
        }

        private static string CantoLocalizationKey(CantoKind canto)
        {
            return "canto." + canto.ToString().ToLowerInvariant();
        }

        private static string SuitLocalizationKey(CardSuit suit)
        {
            return "card.suit." + suit.ToString().ToLowerInvariant();
        }

        private static string ScoreReasonLocalizationKey(ScoreReason reason)
        {
            return "flow.match.score-reason." + reason.ToString().ToLowerInvariant();
        }
    }
}
