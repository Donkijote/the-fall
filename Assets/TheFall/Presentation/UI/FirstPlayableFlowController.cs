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
            "home-stage",
            "setup-stage",
            "loading-stage",
            "match-stage",
            "result-stage",
        };

        private static readonly IReadOnlyDictionary<string, string> LabelLocalizationKeys =
            new Dictionary<string, string>
            {
                { "home-eyebrow", "flow.home.eyebrow" },
                { "home-title", "app.title" },
                { "home-subtitle", "flow.home.subtitle" },
                { "home-mode", "flow.home.mode" },
                { "home-prompt", "flow.home.prompt" },
                { "setup-eyebrow", "flow.setup.eyebrow" },
                { "setup-title", "flow.setup.title" },
                { "setup-subtitle", "flow.setup.subtitle" },
                { "casas-description", "flow.setup.casas-description" },
                { "trivilin-description", "flow.setup.trivilin-description" },
                { "setup-fixed", "flow.setup.fixed" },
                { "setup-prompt", "flow.setup.prompt" },
                { "loading-eyebrow", "flow.loading.eyebrow" },
                { "loading-title", "flow.loading.title" },
                { "loading-message", "flow.loading.message" },
                { "match-eyebrow", "flow.match.eyebrow" },
                { "match-title", "flow.match.title" },
                { "match-actions-title", "flow.match.actions-title" },
                { "match-prompt", "flow.match.prompt" },
                { "result-eyebrow", "flow.result.eyebrow" },
                { "result-title", "flow.result.title" },
                { "result-prompt", "flow.result.prompt" },
            };

        private static readonly IReadOnlyDictionary<string, string> ButtonLocalizationKeys =
            new Dictionary<string, string>
            {
                { "home-start-button", "flow.home.start" },
                { "setup-start-button", "flow.setup.start" },
                { "setup-back-button", "flow.common.back" },
                { "match-home-button", "flow.common.return-home" },
                { "result-replay-button", "flow.result.replay" },
                { "result-home-button", "flow.common.return-home" },
            };

        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _screen;
        private Toggle _casasToggle;
        private Toggle _trivilinToggle;
        private Label _matchPhase;
        private Label _matchScore;
        private Label _matchProgress;
        private Label _matchTurn;
        private Label _matchCanto;
        private Label _matchEvent;
        private Label _matchFeedback;
        private Label _resultOutcome;
        private Label _resultScore;
        private VisualElement _matchActions;
        private Coroutine _loadingCoroutine;
        private bool _isBound;

        public FirstPlayableFlow Flow { get; private set; }

        public event Action PresentationChanged;

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

        public bool OpenSetup()
        {
            if (!Flow.TryOpenSetup())
            {
                return false;
            }

            Render();
            return true;
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
            if (!Flow.TrySubmitHumanIntent(intent, out result))
            {
                return false;
            }

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

        public void Render()
        {
            if (_root == null || Flow == null)
            {
                return;
            }

            RefreshLocalizedStaticText();
            ShowOnly(StageElementName(Flow.Stage));

            switch (Flow.Stage)
            {
                case FirstPlayableFlowStage.Home:
                    Focus("home-start-button");
                    break;
                case FirstPlayableFlowStage.Setup:
                    RenderSetup();
                    Focus("setup-start-button");
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
            _screen = Require<VisualElement>("home-screen");
            _matchPhase = Require<Label>("match-phase");
            _matchScore = Require<Label>("match-score");
            _matchProgress = Require<Label>("match-progress");
            _matchTurn = Require<Label>("match-turn");
            _matchCanto = Require<Label>("match-canto");
            _matchEvent = Require<Label>("match-event");
            _matchFeedback = Require<Label>("match-feedback");
            _resultOutcome = Require<Label>("result-outcome");
            _resultScore = Require<Label>("result-score");
            _matchActions = Require<VisualElement>("match-actions");

            Require<Button>("home-start-button").clicked += () => OpenSetup();
            Require<Button>("setup-start-button").clicked += () => StartMatch();
            Require<Button>("setup-back-button").clicked += () => ReturnHome();
            Require<Button>("match-home-button").clicked += () => ReturnHome();
            Require<Button>("result-replay-button").clicked += () => Replay();
            Require<Button>("result-home-button").clicked += () => ReturnHome();
            _casasToggle.RegisterValueChangedCallback(change =>
                Flow.TryConfigure(change.newValue, _trivilinToggle.value));
            _trivilinToggle.RegisterValueChangedCallback(change =>
                Flow.TryConfigure(_casasToggle.value, change.newValue));
            _screen.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);

            _isBound = true;
        }

        private void RenderSetup()
        {
            _casasToggle.SetValueWithoutNotify(Flow.Setup.CasaCantosEnabled);
            _trivilinToggle.SetValueWithoutNotify(Flow.Setup.TrivilinWinsImmediately);
            _casasToggle.text = Localize("flow.setup.casas");
            _trivilinToggle.text = Localize("flow.setup.trivilin");
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
            _matchProgress.text = Localize(
                "flow.match.progress",
                state.RoundNumber,
                state.DealNumber,
                state.IsTieExtension ? Localize("flow.match.tie-extension") : Localize("flow.match.standard-round"));
            _matchTurn.text = Localize(
                "flow.match.turn",
                Localize(state.DealerSeat == Seat.First ? "flow.player.you" : "flow.player.bot"),
                Localize(state.CurrentSeat == Seat.First ? "flow.player.you" : "flow.player.bot"));
            _matchCanto.text = CantoSummary(state);
            _matchEvent.text = EventSummary();
            _matchFeedback.text = Localize("interaction.feedback.legal");

            _matchActions.Clear();
            var legalIntents = Flow.Match.GetHumanLegalIntents();
            for (var index = 0; index < legalIntents.Count; index++)
            {
                var intent = legalIntents[index];
                var actionIndex = index;
                var button = new Button(() => SubmitHumanIntent(intent))
                {
                    name = $"match-action-{actionIndex}",
                    text = IntentText(intent, actionIndex),
                };
                button.AddToClassList("action-button");
                button.tooltip = button.text;
                _matchActions.Add(button);
            }

            var firstAction = _matchActions.Q<Button>();
            firstAction?.schedule.Execute(firstAction.Focus);
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

            var resolvedEvent = events[events.Count - 1];
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
                Require<Button>(binding.Key).text = Localize(binding.Value);
            }

            if (_casasToggle != null)
            {
                _casasToggle.text = Localize("flow.setup.casas");
                _trivilinToggle.text = Localize("flow.setup.trivilin");
            }
        }

        private void HandleLocaleChanged(Locale locale)
        {
            Render();
        }

        private void HandleGeometryChanged(GeometryChangedEvent change)
        {
            _screen.EnableInClassList("compact", change.newRect.width < 900f);
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
