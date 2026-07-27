using System;
using System.Collections;
using System.Collections.Generic;
using TheFall.Application;
using TheFall.Domain;
using TheFall.Presentation.Bootstrap;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace TheFall.Presentation.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class FirstPlayableFlowController : MonoBehaviour
    {
        private const string TableName = "UI";

        private static readonly string[] StageElementNames =
        {
            "login-stage",
            "home-stage",
            "setup-stage",
            "loading-stage",
            "match-stage",
            "result-stage",
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
                { "home-stat-mode-label", "flow.home.stat.mode-label" },
                { "home-stat-mode-value", "flow.home.stat.mode-value" },
                { "home-stat-target-label", "flow.home.stat.target-label" },
                { "home-stat-target-value", "flow.home.stat.target-value" },
                { "home-stat-deck-label", "flow.home.stat.deck-label" },
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
                { "result-eyebrow", "flow.result.eyebrow" },
                { "result-title", "flow.result.title" },
                { "result-winner-label", "flow.result.winner-label" },
                { "result-next", "flow.result.next" },
                { "result-prompt", "flow.result.prompt" },
            };

        private static readonly IReadOnlyDictionary<string, string> ButtonLocalizationKeys =
            new Dictionary<string, string>
            {
                { "login-enter-button", "flow.login.enter" },
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

        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _screen;
        private readonly List<VisualElement> _stages = new List<VisualElement>();
        private Toggle _casasToggle;
        private Toggle _trivilinToggle;
        private Label _casasState;
        private Label _trivilinState;
        private Label _loadingSession;
        private Label _matchPhase;
        private Label _matchScore;
        private Label _matchTurn;
        private Label _matchEvent;
        private Label _matchFeedback;
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
        private bool _isBound;
        private bool _hasEnteredGateway;
        private bool _isDealerMenuOpen;
        private bool _isCantoMenuOpen;
        private MatchState _contextState;
        private Vector2Int _adaptiveViewport;
        private Rect _adaptiveSafeArea;
        private Vector2 _adaptivePanelSize;
        private bool _hasAdaptiveViewportOverride;
        private string _homeChatChannel = "global";

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

        public bool AudioMasterEnabled => _homeAudioMasterToggle?.value ?? true;

        public bool AudioEffectsEnabled => _homeAudioEffectsToggle?.value ?? true;

        public bool AudioMusicEnabled => _homeAudioMusicToggle?.value ?? false;

        public bool HasEnteredGateway => _hasEnteredGateway;

        public AdaptiveUiLayout CurrentAdaptiveLayout { get; private set; }

        public AdaptiveUiInsets CurrentAdaptivePanelInsets { get; private set; }

        private void OnEnable()
        {
            _document = GetComponent<UIDocument>();
            _root = _document.rootVisualElement;

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
            BindUi();
            ApplyAdaptiveLayout(RuntimeViewport(), RuntimeSafeArea(), UnityEngine.Application.isMobilePlatform);
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
            Render();
        }

        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;
            Render();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
        }

        private void Update()
        {
            if (_hasAdaptiveViewportOverride || _screen == null)
            {
                return;
            }

            var viewport = RuntimeViewport();
            var safeArea = RuntimeSafeArea();
            var panelSize = ResolvePanelSize(viewport);
            if (viewport != _adaptiveViewport
                || safeArea != _adaptiveSafeArea
                || panelSize != _adaptivePanelSize)
            {
                ApplyAdaptiveLayout(viewport, safeArea, UnityEngine.Application.isMobilePlatform);
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

            _casasToggle.SetValueWithoutNotify(_homeCasasToggle.value);
            _trivilinToggle.SetValueWithoutNotify(_homeTrivilinToggle.value);
            if (StartMatch())
            {
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
            Render();
            return true;
        }

        public void RenderInteractionFeedback(string localizationKey)
        {
            if (_matchFeedback != null && !string.IsNullOrWhiteSpace(localizationKey))
            {
                _matchFeedback.text = Localize(localizationKey);
            }
        }

        public void RenderPresentationEvent(DomainEvent resolvedEvent)
        {
            if (_matchEvent != null && resolvedEvent != null)
            {
                _matchEvent.text = EventSummary(resolvedEvent);
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

            RefreshLocalizedStaticText();
            if (!_hasEnteredGateway)
            {
                ShowOnly("login-stage");
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
            ShowOnly(StageElementName(presentedStage));
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

        private void BindUi()
        {
            if (_isBound)
            {
                return;
            }

            _casasToggle = Require<Toggle>("casas-toggle");
            _trivilinToggle = Require<Toggle>("trivilin-toggle");
            _casasState = Require<Label>("casas-state");
            _trivilinState = Require<Label>("trivilin-state");
            _loadingSession = Require<Label>("loading-session");
            _screen = Require<VisualElement>("home-screen");
            foreach (var stageName in StageElementNames)
            {
                _stages.Add(Require<VisualElement>(stageName));
            }
            _matchPhase = Require<Label>("match-phase");
            _matchScore = Require<Label>("match-score");
            _matchTurn = Require<Label>("match-turn");
            _matchEvent = Require<Label>("match-event");
            _matchFeedback = Require<Label>("match-feedback");
            _resultOutcome = Require<Label>("result-outcome");
            _resultScore = Require<Label>("result-score");
            _resultRules = Require<Label>("result-rules");
            _dealerContext = Require<VisualElement>("dealer-context");
            _dealerOptionsMenu = Require<VisualElement>("dealer-options-menu");
            _dealerOptions = Require<VisualElement>("dealer-options");
            _dealerOptionsButton = Require<Button>("dealer-options-button");
            _cantoContext = Require<VisualElement>("canto-context");
            _cantoOptionsMenu = Require<VisualElement>("canto-options-menu");
            _cantoOptions = Require<VisualElement>("canto-options");
            _cantoOptionsButton = Require<Button>("canto-options-button");
            _animationSkipButton = Require<Button>("animation-skip-button");
            _loginFeedback = Require<Label>("login-feedback");
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
            Require<Button>("setup-start-button").clicked += () => StartMatch();
            Require<Button>("setup-back-button").clicked += () => ReturnHome();
            Require<Button>("loading-home-button").clicked += () => ReturnHome();
            Require<Button>("match-home-button").clicked += () => ReturnHome();
            _dealerOptionsButton.clicked += ToggleDealerOptions;
            _cantoOptionsButton.clicked += ToggleCantoOptions;
            _animationSkipButton.clicked += () => AnimationSkipRequested?.Invoke();
            _homeAnimationFastToggle.RegisterValueChangedCallback(change =>
            {
                AnimationFastForwardChanged?.Invoke(change.newValue);
            });
            _homeAnimationReducedToggle.RegisterValueChangedCallback(change =>
            {
                AnimationReducedMotionChanged?.Invoke(change.newValue);
            });
            _homeAudioMasterToggle.RegisterValueChangedCallback(change =>
            {
                AudioMasterChanged?.Invoke(change.newValue);
            });
            _homeAudioEffectsToggle.RegisterValueChangedCallback(change =>
            {
                AudioEffectsChanged?.Invoke(change.newValue);
            });
            _homeAudioMusicToggle.RegisterValueChangedCallback(change =>
            {
                AudioMusicChanged?.Invoke(change.newValue);
            });
            Require<Button>("result-replay-button").clicked += () => Replay();
            Require<Button>("result-home-button").clicked += () => ReturnHome();
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
            _screen.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);

            SetVisible(_hubModal, false);
            SetVisible(_hubSettingsContent, false);
            SetVisible(_homeChatUserMessage, false);
            _isBound = true;
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
            _homeChatUserMessage.text = Localize("flow.home.chat.you", message);
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
            _matchTurn.text = state.Phase == MatchPhase.DealerSelection
                ? Localize(
                    "flow.match.turn.dealer-pending",
                    Localize(state.CurrentSeat == Seat.First ? "flow.player.you" : "flow.player.bot"))
                : Localize(
                    "flow.match.turn",
                    Localize(state.DealerSeat == Seat.First ? "flow.player.you" : "flow.player.bot"),
                    Localize(state.CurrentSeat == Seat.First ? "flow.player.you" : "flow.player.bot"));
            _matchEvent.text = EventSummary();
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
                _matchFeedback.text = Localize("interaction.feedback.temporarily-blocked");
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
            _matchFeedback.text = state.Phase == MatchPhase.DealerSelection
                ? Localize("flow.context.dealer-card-prompt")
                : dealerOptionCount > 0
                    ? Localize("flow.context.dealer-required")
                    : Localize("interaction.feedback.legal");
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
                return Localize("flow.match.event.cards-captured", PlayerDisplayName(captured.PlayerId), captured.Cards.Count);
            }

            if (resolvedEvent is CantoAnnouncedEvent canto)
            {
                return Localize(
                    "flow.match.event.canto-announced",
                    PlayerDisplayName(canto.PlayerId),
                    Localize(CantoLocalizationKey(canto.ClaimedKind)));
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
                Require<Label>(binding.Key).text = Localize(binding.Value);
            }

            foreach (var binding in ButtonLocalizationKeys)
            {
                var button = Require<Button>(binding.Key);
                button.text = Localize(binding.Value);
                if (button.ClassListContains("icon-only-button"))
                {
                    button.tooltip = button.text;
                }
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
                    : UnityEngine.Application.isMobilePlatform);
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
            ApplyAdaptiveLayout(RuntimeViewport(), RuntimeSafeArea(), UnityEngine.Application.isMobilePlatform);
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
            _adaptivePanelSize = ResolvePanelSize(viewportPixels);
            CurrentAdaptiveLayout = AdaptiveUiFoundation.Resolve(
                viewportPixels,
                safeAreaPixels,
                isMobilePlatform);
            CurrentAdaptivePanelInsets = AdaptiveUiFoundation.ResolvePanelInsets(
                CurrentAdaptiveLayout,
                _adaptivePanelSize);
            AdaptiveUiFoundation.ApplyProfileClass(
                _screen,
                CurrentAdaptiveLayout.Profile);
            _screen.EnableInClassList("compact", false);

            foreach (var stage in _stages)
            {
                stage.style.left = CurrentAdaptivePanelInsets.Left;
                stage.style.top = CurrentAdaptivePanelInsets.Top;
                stage.style.right = CurrentAdaptivePanelInsets.Right;
                stage.style.bottom = CurrentAdaptivePanelInsets.Bottom;
            }
        }

        private Vector2 ResolvePanelSize(Vector2Int viewportPixels)
        {
            var width = _screen?.layout.width ?? 0f;
            var height = _screen?.layout.height ?? 0f;
            return float.IsNaN(width)
                || float.IsNaN(height)
                || width <= 0f
                || height <= 0f
                    ? new Vector2(viewportPixels.x, viewportPixels.y)
                    : new Vector2(width, height);
        }

        private static Vector2Int RuntimeViewport()
        {
            return new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));
        }

        private static Rect RuntimeSafeArea()
        {
            var safeArea = Screen.safeArea;
            return safeArea.width > 0f && safeArea.height > 0f
                ? safeArea
                : new Rect(0f, 0f, Screen.width, Screen.height);
        }

        private string Localize(string key, params object[] arguments)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(TableName, key, arguments);
        }

        private void ShowOnly(string visibleName)
        {
            foreach (var stageName in StageElementNames)
            {
                Require<VisualElement>(stageName).style.display = stageName == visibleName
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
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
            element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
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

        private static string StageElementName(FirstPlayableFlowStage stage)
        {
            switch (stage)
            {
                case FirstPlayableFlowStage.Home:
                    return "home-stage";
                case FirstPlayableFlowStage.Setup:
                    return "setup-stage";
                case FirstPlayableFlowStage.Loading:
                    return "loading-stage";
                case FirstPlayableFlowStage.Match:
                    return "match-stage";
                case FirstPlayableFlowStage.Result:
                    return "result-stage";
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(stage));
            }
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
