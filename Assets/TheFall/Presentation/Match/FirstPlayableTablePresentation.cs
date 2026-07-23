using System;
using System.Collections.Generic;
using TheFall.Application;
using TheFall.Application.Input;
using TheFall.Application.Interaction;
using TheFall.Domain;
using TheFall.Presentation.Animation;
using TheFall.Presentation.Cards;
using TheFall.Presentation.Input;
using TheFall.Presentation.Interaction;
using TheFall.Presentation.Table;
using TheFall.Presentation.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFall.Presentation.Match
{
    /// <summary>
    /// Renders the complete first-playable 1v1 table from authoritative application state and
    /// maps card controls back to shared application intents. It never evaluates game rules.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FirstPlayableTablePresentation : MonoBehaviour
    {
        private static readonly Vector3 FixedCameraPosition = new Vector3(0f, 8.6f, -2.35f);
        private static readonly Quaternion FixedCameraRotation = Quaternion.Euler(74f, 0f, 0f);
        private const float FixedCameraFieldOfView = 36f;

        private static readonly Color Lampblack = FromHex(0x241A14);
        private static readonly Color Brass = FromHex(0xB58B3E);

        [SerializeField] private Camera _gameplayCamera;
        [SerializeField] private GameObject _tablePrototypePrefab;
        [SerializeField] private CardVisualCatalog _cardCatalog;
        [SerializeField] private FirstPlayableTableLayout _authoredLayout;
        [SerializeField] private AnimationSequenceConfiguration _animationPreset;

        private readonly Dictionary<Color32, Material> _generatedMaterials = new Dictionary<Color32, Material>();
        private readonly List<FirstPlayableRenderedCard> _renderedCards = new List<FirstPlayableRenderedCard>();
        private readonly List<PrototypeCardView> _localHandViews = new List<PrototypeCardView>();
        private readonly List<CardMotion> _cardMotions = new List<CardMotion>();
        private FirstPlayableFlowController _flowController;
        private FirstPlayableAnimationPlayer _animationPlayer;
        private Transform _generatedRoot;
        private Material _cardBackMaterial;
        private CardInteractionSession _interaction;
        private CardInteractionInputAdapter _inputAdapter;
        private InputAction _pointAction;
        private InputAction _navigateAction;
        private InputAction _inspectAction;
        private InputAction _selectAction;
        private InputAction _confirmAction;
        private InputAction _cancelAction;
        private Vector2 _pointerPosition;
        private Vector2Int _viewportSize;
        private Rect _safeAreaPixels;
        private int _boundSessionNumber = -1;
        private int _animationVisualRevision = -1;
        private ResolvedAnimationStep _presentedStep;
        private long _presentationCpuTicks;
        private long _presentationPeakUpdateTicks;

        public Camera GameplayCamera => _gameplayCamera;

        public GameObject TablePrototypePrefab => _tablePrototypePrefab;

        public CardVisualCatalog CardCatalog => _cardCatalog;

        public FirstPlayableTableLayout AuthoredLayout => _authoredLayout;

        public AnimationSequenceConfiguration AnimationPreset => _animationPreset;

        public FirstPlayableAnimationPlayer AnimationPlayer => _animationPlayer;

        public bool IsPresentationBusy => _animationPlayer?.IsBusy == true;

        public AnimationSequenceCompletionReason AnimationCompletionReason =>
            _animationPlayer?.CompletionReason ?? AnimationSequenceCompletionReason.None;

        public double AnimationPresentationCpuMilliseconds =>
            TicksToMilliseconds(_presentationCpuTicks);

        public double AnimationPresentationPeakUpdateCpuMilliseconds =>
            TicksToMilliseconds(_presentationPeakUpdateTicks);

        public FirstPlayableTableSnapshot Snapshot { get; private set; }

        public MatchState RenderedState => Snapshot?.AuthoritativeState;

        public CardInteractionSession Interaction => _interaction;

        public CardInteractionInputAdapter InputAdapter => _inputAdapter;

        public IReadOnlyList<FirstPlayableRenderedCard> RenderedCards => _renderedCards;

        public IReadOnlyList<PrototypeCardView> LocalHandViews => _localHandViews;

        public TableCompositionProfile CurrentProfile { get; private set; }

        public int LayoutRevision { get; private set; }

        public static Vector3 CameraPosition => FixedCameraPosition;

        public static Quaternion CameraRotation => FixedCameraRotation;

        public static float CameraFieldOfView => FixedCameraFieldOfView;

        private void OnEnable()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            _flowController = GetComponent<FirstPlayableFlowController>()
                ?? FindAnyObjectByType<FirstPlayableFlowController>();
            if (_flowController == null)
            {
                Debug.LogError("The integrated table requires the first-playable flow controller.", this);
                enabled = false;
                return;
            }

            if (_authoredLayout == null || !_authoredLayout.IsConfigured)
            {
                Debug.LogError("The integrated table requires a configured scene-authored layout.", this);
                enabled = false;
                return;
            }

            if (_animationPreset == null)
            {
                Debug.LogError("The integrated table requires a versioned animation presentation preset.", this);
                enabled = false;
                return;
            }

            _authoredLayout.gameObject.SetActive(false);
            _animationPlayer = new FirstPlayableAnimationPlayer(_animationPreset);
            ConfigureFixedCamera();
            BindInputActions();
            _flowController.PresentationChanged += RefreshFromFlow;
            _flowController.MatchAdvanced += HandleMatchAdvanced;
            _flowController.AnimationFastForwardChanged += SetFastForward;
            _flowController.AnimationReducedMotionChanged += SetReducedMotion;
            _flowController.AnimationSkipRequested += SkipPresentation;
            RefreshFromFlow();
        }

        private void Update()
        {
            if (!UnityEngine.Application.isPlaying || Snapshot == null)
            {
                return;
            }

            if (_animationPlayer?.IsBusy == true)
            {
                var animationUpdateStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                _animationPlayer.Tick(Time.unscaledDeltaTime);
                if (_animationVisualRevision != _animationPlayer.VisualRevision)
                {
                    RefreshFromAnimation(true);
                }

                ApplyCardMotions();
                PresentActiveEvent();
                if (!_animationPlayer.IsBusy)
                {
                    FinishPresentationBatch();
                }

                var animationUpdateTicks = System.Diagnostics.Stopwatch.GetTimestamp() - animationUpdateStartedAt;
                _presentationCpuTicks += animationUpdateTicks;
                _presentationPeakUpdateTicks = Math.Max(_presentationPeakUpdateTicks, animationUpdateTicks);
            }

            var viewport = RuntimeViewport();
            var safeArea = RuntimeSafeArea(viewport);
            if (viewport != _viewportSize || safeArea != _safeAreaPixels)
            {
                Rebuild(viewport, safeArea);
            }
        }

        private void OnDisable()
        {
            if (_flowController != null)
            {
                _flowController.PresentationChanged -= RefreshFromFlow;
                _flowController.MatchAdvanced -= HandleMatchAdvanced;
                _flowController.AnimationFastForwardChanged -= SetFastForward;
                _flowController.AnimationReducedMotionChanged -= SetReducedMotion;
                _flowController.AnimationSkipRequested -= SkipPresentation;
            }

            if (_animationPlayer?.IsBusy == true)
            {
                _animationPlayer.InterruptAndSynchronize();
            }

            _flowController?.SetPresentationBusy(false);

            UnbindInputActions();
            ClearInteraction();
            DestroyGeneratedContent();
            DestroyGeneratedMaterials();
            _animationPlayer = null;
            if (_authoredLayout != null)
            {
                _authoredLayout.gameObject.SetActive(true);
            }
        }

        public void ApplyViewportForTests(Vector2Int viewportSize, Rect safeAreaPixels)
        {
            if (Snapshot == null)
            {
                throw new InvalidOperationException("The table has no active match snapshot.");
            }

            Rebuild(viewportSize, safeAreaPixels);
            ApplyInteractionState();
        }

        public void SetTemporarilyBlocked(bool isBlocked)
        {
            _interaction?.SetTemporarilyBlocked(isBlocked);
            ApplyInteractionState();
        }

        public void SetFastForward(bool enabled)
        {
            _animationPlayer?.SetFastForward(enabled);
        }

        public void SetReducedMotion(bool enabled)
        {
            _animationPlayer?.SetReducedMotion(enabled);
        }

        public void SkipPresentation()
        {
            if (_animationPlayer?.IsBusy != true)
            {
                return;
            }

            _animationPlayer.SkipAndSynchronize();
            FinishPresentationBatch();
        }

        public void InterruptPresentation()
        {
            if (_animationPlayer?.IsBusy != true)
            {
                return;
            }

            _animationPlayer.InterruptAndSynchronize();
            FinishPresentationBatch();
        }

        public void CancelPresentation()
        {
            if (_animationPlayer?.IsBusy != true)
            {
                return;
            }

            _animationPlayer.CancelAndSynchronize();
            FinishPresentationBatch();
        }

        public bool ActivateDealerCard(int interactionIndex)
        {
            var legalIntents = _flowController?.Flow?.Match?.GetHumanLegalIntents();
            if (legalIntents == null || interactionIndex < 0)
            {
                return false;
            }

            var dealerCardIndex = 0;
            for (var index = 0; index < legalIntents.Count; index++)
            {
                if (!(legalIntents[index] is SelectDealerCardIntent dealerCard))
                {
                    continue;
                }

                if (dealerCardIndex++ == interactionIndex)
                {
                    return _flowController.SubmitHumanIntent(dealerCard);
                }
            }

            return false;
        }

#if UNITY_EDITOR
        public void Configure(
            Camera gameplayCamera,
            GameObject tablePrototypePrefab,
            CardVisualCatalog cardCatalog,
            FirstPlayableTableLayout authoredLayout,
            AnimationSequenceConfiguration animationPreset)
        {
            _gameplayCamera = gameplayCamera;
            _tablePrototypePrefab = tablePrototypePrefab;
            _cardCatalog = cardCatalog;
            _authoredLayout = authoredLayout;
            _animationPreset = animationPreset;
        }
#endif

        private void RefreshFromFlow()
        {
            var flow = _flowController?.Flow;
            var isVisible = flow != null
                && flow.Match != null
                && (flow.Stage == FirstPlayableFlowStage.Match || flow.Stage == FirstPlayableFlowStage.Result);
            if (!isVisible)
            {
                if (_animationPlayer?.IsBusy == true)
                {
                    _animationPlayer.InterruptAndSynchronize();
                }

                _flowController?.SetPresentationBusy(false);
                Snapshot = null;
                ClearInteraction();
                DestroyGeneratedContent();
                return;
            }

            if (flow.Stage == FirstPlayableFlowStage.Match && _boundSessionNumber != flow.SessionNumber)
            {
                _presentationCpuTicks = 0;
                _presentationPeakUpdateTicks = 0;
                Snapshot = FirstPlayableTableSnapshot.Create(flow.Match.Trace.InitialState);
                CreateInteraction(flow.SessionNumber);
                _animationPlayer.PlayInitialTrace(flow.Match.Trace);
                _flowController.SetPresentationBusy(_animationPlayer.IsBusy);
                RefreshFromAnimation(false);
                return;
            }

            if (_animationPlayer?.IsBusy == true)
            {
                RefreshFromAnimation(false);
                return;
            }

            Snapshot = FirstPlayableTableSnapshot.Create(flow.Match.State);
            if (_inputAdapter != null)
            {
                _inputAdapter.SetCards(Snapshot.LocalHand);
            }

            Rebuild(RuntimeViewport(), RuntimeSafeArea(RuntimeViewport()));
            ApplyInteractionState();
        }

        private void HandleMatchAdvanced(MatchAdvanceResult advance)
        {
            _animationPlayer.PlayAdvance(advance);
            if (!_animationPlayer.IsBusy)
            {
                return;
            }

            _interaction?.SetTemporarilyBlocked(true);
            _flowController.SetPresentationBusy(true);
            _animationVisualRevision = -1;
            RefreshFromAnimation(true);
        }

        private void RefreshFromAnimation(bool animateChangedCards)
        {
            if (_animationPlayer?.RenderedState == null || _animationPlayer.RenderedReferenceState == null)
            {
                return;
            }

            var sourcePositions = animateChangedCards ? CapturePresentationCardPositions() : null;
            Snapshot = FirstPlayableTableSnapshot.Create(
                _animationPlayer.RenderedState,
                _animationPlayer.RenderedReferenceState);
            _inputAdapter?.SetCards(Snapshot.LocalHand);
            Rebuild(RuntimeViewport(), RuntimeSafeArea(RuntimeViewport()));
            _animationVisualRevision = _animationPlayer.VisualRevision;
            PrepareCardMotions(sourcePositions);
            ApplyInteractionState();
            PresentActiveEvent();
        }

        private void FinishPresentationBatch()
        {
            _cardMotions.Clear();
            _presentedStep = null;
            var flow = _flowController?.Flow;
            if (flow?.Match != null)
            {
                Snapshot = FirstPlayableTableSnapshot.Create(flow.Match.State);
                _inputAdapter?.SetCards(Snapshot.LocalHand);
                Rebuild(RuntimeViewport(), RuntimeSafeArea(RuntimeViewport()));
            }

            _interaction?.SetTemporarilyBlocked(false);
            ApplyInteractionState();
            _flowController?.SetPresentationBusy(false);
        }

        private void CreateInteraction(int sessionNumber)
        {
            ClearInteraction();
            _boundSessionNumber = sessionNumber;
            _interaction = new CardInteractionSession(
                Snapshot.LocalPlayerId,
                () => _flowController.Flow.Match.State,
                playerId => playerId == Snapshot.LocalPlayerId
                    ? _flowController.Flow.Match.GetHumanLegalIntents()
                    : Array.Empty<PlayerIntent>(),
                SubmitConfirmedPlay);
            _inputAdapter = new CardInteractionInputAdapter(_interaction);
            _inputAdapter.SetCards(Snapshot.LocalHand);
            _inputAdapter.ResultProduced += HandleInteractionResult;
        }

        private void ClearInteraction()
        {
            if (_inputAdapter != null)
            {
                _inputAdapter.ResultProduced -= HandleInteractionResult;
            }

            _interaction = null;
            _inputAdapter = null;
            _boundSessionNumber = -1;
        }

        private RuleResult SubmitConfirmedPlay(PlayCardIntent intent)
        {
            var unchangedState = _flowController.Flow.Match.State;
            _interaction.SetTemporarilyBlocked(true);
            if (!_flowController.TrySubmitHumanIntent(intent, out var advanceResult))
            {
                _interaction.SetTemporarilyBlocked(false);
                return RuleResult.Rejected(unchangedState, RuleError.WrongPhase);
            }

            if (!IsPresentationBusy)
            {
                _interaction.SetTemporarilyBlocked(false);
            }

            return advanceResult.HumanResult;
        }

        private void HandleInteractionResult(CardInteractionResult result)
        {
            var flow = _flowController.Flow;
            if (flow.Match != null)
            {
                Snapshot = FirstPlayableTableSnapshot.Create(flow.Match.State);
                _inputAdapter?.SetCards(Snapshot.LocalHand);
            }

            ApplyInteractionState();
        }

        private void Rebuild(Vector2Int viewportSize, Rect safeAreaPixels)
        {
            if (viewportSize.x <= 0 || viewportSize.y <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(viewportSize));
            }

            _viewportSize = viewportSize;
            _safeAreaPixels = safeAreaPixels;
            CurrentProfile = TableCompositionLayout.ResolveProfile(viewportSize);
            LayoutRevision++;

            DestroyGeneratedContent();
            ConfigureFixedCamera();
            EnsureCardBackMaterial();

            var root = new GameObject("Generated First Playable Table");
            root.hideFlags = HideFlags.DontSave;
            root.transform.SetParent(transform, false);
            _generatedRoot = root.transform;

            var safeArea = TableCompositionLayout.NormalizeSafeArea(viewportSize, safeAreaPixels);
            var safeScale = Mathf.Clamp(Mathf.Min(safeArea.width, safeArea.height), 0.78f, 1f);
            _generatedRoot.localScale = Vector3.one * (CurrentProfile.ContentScale * safeScale);
            var safeCenter = safeArea.center - new Vector2(0.5f, 0.5f);
            _generatedRoot.localPosition = new Vector3(safeCenter.x * 2.2f, 0f, safeCenter.y * 1.4f);

            var authoredRoot = new GameObject("Authored Table Layout");
            authoredRoot.hideFlags = HideFlags.DontSave;
            authoredRoot.transform.SetParent(_generatedRoot, false);
            CopyLocalTransform(_authoredLayout.transform, authoredRoot.transform);

            CloneAuthoredObject(_authoredLayout.Environment, authoredRoot.transform, "Table Environment");
            CreateTable(authoredRoot.transform);
            CreateSeats(authoredRoot.transform);
            var cardZones = CreateRuntimeAnchor(
                authoredRoot.transform,
                _authoredLayout.CardZonesRoot,
                "Card Zone Anchors");
            CreateStateCards(cardZones);
        }

        private CardPositionSnapshot CapturePresentationCardPositions()
        {
            var snapshot = new CardPositionSnapshot();
            for (var index = 0; index < _renderedCards.Count; index++)
            {
                var rendered = _renderedCards[index];
                if (rendered.PresentationCard.HasValue)
                {
                    snapshot.Cards[rendered.PresentationCard.Value] = rendered.transform.position;
                }

                if (rendered.Zone == FirstPlayableCardZone.Deck)
                {
                    snapshot.DeckPosition = rendered.transform.position;
                    snapshot.HasDeckPosition = true;
                }
                else if (rendered.Zone == FirstPlayableCardZone.OpponentHand)
                {
                    snapshot.OpponentHandPositions[rendered.InteractionIndex] = rendered.transform.position;
                }
            }

            return snapshot;
        }

        private void PrepareCardMotions(CardPositionSnapshot source)
        {
            _cardMotions.Clear();
            var step = _animationPlayer?.ActiveStep;
            if (source == null || step == null || _animationPlayer.IsDelayingActiveStep
                || _animationPlayer.ActiveStepProgress <= 0f)
            {
                return;
            }

            var beat = _animationPreset.GetBeat(step.Kind);
            var trajectory = beat?.TrajectoryOffset ?? Vector3.zero;
            if (_animationPlayer.ReducedMotion)
            {
                trajectory *= _animationPreset.ReducedMotionTrajectoryScale;
            }

            if (_generatedRoot != null)
            {
                trajectory = _generatedRoot.TransformVector(trajectory);
            }

            for (var index = 0; index < _renderedCards.Count; index++)
            {
                var rendered = _renderedCards[index];
                if (!rendered.PresentationCard.HasValue
                    || !source.Cards.TryGetValue(rendered.PresentationCard.Value, out var start))
                {
                    continue;
                }

                if (step.Kind == ResolvedAnimationStepKind.CardPlay
                    && !Contains(step.Cards, rendered.PresentationCard.Value))
                {
                    rendered.transform.position = start;
                    continue;
                }

                AddCardMotion(rendered.transform, start, rendered.transform.position, beat, trajectory);
            }

            if (step.Kind == ResolvedAnimationStepKind.HandReflow
                && step.PlayerId == Snapshot.OpponentPlayerId)
            {
                for (var index = 0; index < _renderedCards.Count; index++)
                {
                    var rendered = _renderedCards[index];
                    if (rendered.Zone == FirstPlayableCardZone.OpponentHand
                        && source.OpponentHandPositions.TryGetValue(
                            rendered.InteractionIndex,
                            out var opponentStart))
                    {
                        AddCardMotion(
                            rendered.transform,
                            opponentStart,
                            rendered.transform.position,
                            beat,
                            trajectory);
                    }
                }
            }

            if (!source.HasDeckPosition || step.Cards.Count == 0)
            {
                return;
            }

            if (step.Kind == ResolvedAnimationStepKind.Deal)
            {
                FirstPlayableRenderedCard target = null;
                if (step.PlayerId == Snapshot.LocalPlayerId)
                {
                    target = FindRenderedCard(step.Cards[0]);
                }
                else
                {
                    for (var index = _renderedCards.Count - 1; index >= 0; index--)
                    {
                        if (_renderedCards[index].Zone == FirstPlayableCardZone.OpponentHand)
                        {
                            target = _renderedCards[index];
                            break;
                        }
                    }
                }

                if (target != null && !HasMotion(target.transform))
                {
                    AddCardMotion(target.transform, source.DeckPosition, target.transform.position, beat, trajectory);
                }
            }
            else if (step.Kind == ResolvedAnimationStepKind.OpeningPlacement)
            {
                var target = FindRenderedCard(step.Cards[0]);
                if (target != null && !HasMotion(target.transform))
                {
                    AddCardMotion(target.transform, source.DeckPosition, target.transform.position, beat, trajectory);
                }
            }
        }

        private void AddCardMotion(
            Transform card,
            Vector3 start,
            Vector3 target,
            AnimationBeatConfiguration beat,
            Vector3 trajectory)
        {
            if (card == null || Vector3.SqrMagnitude(target - start) <= 0.000001f)
            {
                return;
            }

            card.position = start;
            _cardMotions.Add(new CardMotion(
                card,
                start,
                target,
                beat?.Easing ?? AnimationBeatEasing.EaseInOut,
                trajectory));
        }

        private void ApplyCardMotions()
        {
            if (_animationPlayer == null || _cardMotions.Count == 0)
            {
                return;
            }

            var progress = _animationPlayer.ActiveStepProgress;
            for (var index = 0; index < _cardMotions.Count; index++)
            {
                var motion = _cardMotions[index];
                if (motion.Card != null)
                {
                    motion.Card.position = AnimationBeatEvaluator.EvaluatePosition(
                        motion.Start,
                        motion.Target,
                        progress,
                        motion.Easing,
                        motion.Trajectory);
                }
            }
        }

        private FirstPlayableRenderedCard FindRenderedCard(Card card)
        {
            for (var index = 0; index < _renderedCards.Count; index++)
            {
                if (_renderedCards[index].PresentationCard == card)
                {
                    return _renderedCards[index];
                }
            }

            return null;
        }

        private bool HasMotion(Transform card)
        {
            for (var index = 0; index < _cardMotions.Count; index++)
            {
                if (_cardMotions[index].Card == card)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(IReadOnlyList<Card> cards, Card expected)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                if (cards[index] == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private void PresentActiveEvent()
        {
            var step = _animationPlayer?.ActiveStep;
            if (ReferenceEquals(step, _presentedStep))
            {
                return;
            }

            _presentedStep = step;
            if (step?.SourceEvent != null)
            {
                _flowController?.RenderPresentationEvent(step.SourceEvent);
            }
        }

        private void CreateTable(Transform parent)
        {
            if (_authoredLayout?.Table == null)
            {
                throw new MissingReferenceException("The integrated table is missing its authored table object.");
            }

            CloneAuthoredObject(_authoredLayout.Table, parent, "RoundCardTable");
        }

        private void CreateSeats(Transform parent)
        {
            CreateSeat(parent, Seat.First, _authoredLayout.LocalSeat);
            CreateSeat(parent, Seat.Second, _authoredLayout.OpponentSeat);
        }

        private void CreateSeat(
            Transform parent,
            Seat seat,
            GameObject authoredSeat)
        {
            if (authoredSeat == null)
            {
                throw new MissingReferenceException($"The integrated table is missing the authored {seat} seat.");
            }

            var seatObject = CloneAuthoredObject(
                authoredSeat,
                parent,
                seat == Seat.First ? "Local Bottom Seat" : "Opponent Top Seat");

            var isActive = Snapshot.ActiveSeat == seat && Snapshot.Phase != MatchPhase.Completed;
            var isDealer = Snapshot.Phase != MatchPhase.DealerSelection && Snapshot.DealerSeat == seat;
            if (isActive)
            {
                CreatePrimitive("Active Turn Ring", PrimitiveType.Cylinder, seatObject.transform,
                    new Vector3(0f, 0.05f, -0.04f), new Vector3(0.34f, 0.02f, 0.34f), Quaternion.identity, Brass);
            }

            if (isDealer)
            {
                CreatePrimitive("Dealer Diamond Cue", PrimitiveType.Cube, seatObject.transform,
                    new Vector3(0f, 0.10f, -0.25f), new Vector3(0.10f, 0.03f, 0.10f),
                    Quaternion.Euler(0f, 45f, 0f), Brass);
            }

        }

        private void CreateStateCards(Transform parent)
        {
            if (Snapshot.Phase == MatchPhase.DealerSelection)
            {
                CreateDealerSpread(parent, Snapshot.DealerSpreadCount);
            }
            else
            {
                CreateDeck(parent, Snapshot.DeckCount);
                CreateTableCards(parent, Snapshot.TableCards);
            }

            CreateLocalHand(
                parent,
                Snapshot.LocalHand,
                Snapshot.LocalHandLayoutIndices,
                Snapshot.LocalHandLayoutSlotCount);
            CreateOpponentHand(
                parent,
                Snapshot.OpponentHandCount,
                Snapshot.OpponentHandLayoutIndices,
                Snapshot.OpponentHandLayoutSlotCount);
            CreateCapturedPile(parent, Snapshot.LocalCapturedCards, FirstPlayableCardZone.LocalCaptured);
            CreateCapturedPile(parent, Snapshot.OpponentCapturedCards, FirstPlayableCardZone.OpponentCaptured);
        }

        private void CreateDealerSpread(Transform parent, int count)
        {
            var zoneParent = CreateRuntimeAnchor(parent, _authoredLayout.DealerSpreadAnchor, "Dealer Spread Zone");
            for (var index = 0; index < count; index++)
            {
                var row = index / 8;
                var column = index % 8;
                CreateCard(zoneParent, $"Face-down Dealer Card {index + 1}",
                    new Vector3((column - 3.5f) * 0.17f, row * 0.002f, (row - 2f) * 0.21f),
                    FirstPlayableCardZone.DealerSpread, false, null, index, true);
            }
        }

        private void CreateDeck(Transform parent, int count)
        {
            var zoneParent = CreateRuntimeAnchor(parent, _authoredLayout.DeckAnchor, "Deck Zone");
            for (var index = 0; index < count; index++)
            {
                CreateCard(zoneParent, $"Deck Card {index + 1}",
                    new Vector3(0f, index * 0.0015f, 0f),
                    FirstPlayableCardZone.Deck, false, null, index);
            }
        }

        private void CreateTableCards(Transform parent, IReadOnlyList<Card> cards)
        {
            var zoneParent = CreateRuntimeAnchor(parent, _authoredLayout.TableCardsAnchor, "Table Cards Zone");
            for (var index = 0; index < cards.Count; index++)
            {
                var row = index / 5;
                var column = index % 5;
                CreateCard(zoneParent, $"Table {cards[index]}",
                    new Vector3((column - 2f) * 0.23f, row * 0.002f, row * 0.31f),
                    FirstPlayableCardZone.Table, true, cards[index], index);
            }
        }

        private void CreateLocalHand(
            Transform parent,
            IReadOnlyList<Card> cards,
            IReadOnlyList<int> layoutIndices,
            int layoutSlotCount)
        {
            var zoneParent = CreateRuntimeAnchor(parent, _authoredLayout.LocalHandAnchor, "Local Hand Zone");
            for (var index = 0; index < cards.Count; index++)
            {
                var layoutIndex = layoutIndices[index];
                var x = (layoutIndex - (layoutSlotCount - 1) * 0.5f) * 0.29f;
                var rendered = CreateCard(zoneParent, $"Local Hand {cards[index]}",
                    new Vector3(
                        x,
                        0f,
                        Mathf.Abs(layoutIndex - (layoutSlotCount - 1) * 0.5f) * 0.025f),
                    FirstPlayableCardZone.LocalHand, true, cards[index], index, true);
                var view = rendered.gameObject.AddComponent<PrototypeCardView>();
                view.Configure(index);
                _localHandViews.Add(view);
            }
        }

        private void CreateOpponentHand(
            Transform parent,
            int count,
            IReadOnlyList<int> layoutIndices,
            int layoutSlotCount)
        {
            var zoneParent = CreateRuntimeAnchor(parent, _authoredLayout.OpponentHandAnchor, "Opponent Hand Zone");
            for (var index = 0; index < count; index++)
            {
                var layoutIndex = layoutIndices[index];
                var x = (layoutIndex - (layoutSlotCount - 1) * 0.5f) * 0.25f;
                CreateCard(zoneParent, $"Private Opponent Hand Card {index + 1}",
                    new Vector3(-x, 0f, 0f),
                    FirstPlayableCardZone.OpponentHand, false, null, index);
            }
        }

        private void CreateCapturedPile(
            Transform parent,
            IReadOnlyList<Card> cards,
            FirstPlayableCardZone zone)
        {
            var authoredAnchor = zone == FirstPlayableCardZone.LocalCaptured
                ? _authoredLayout.LocalCapturedAnchor
                : _authoredLayout.OpponentCapturedAnchor;
            var zoneParent = CreateRuntimeAnchor(parent, authoredAnchor, $"{zone} Zone");
            for (var index = 0; index < cards.Count; index++)
            {
                CreateCard(zoneParent, $"{zone} Card {index + 1}",
                    new Vector3((index % 4) * 0.012f, index * 0.002f, (index % 3) * 0.009f),
                    zone, false, cards[index], index);
            }
        }

        private FirstPlayableRenderedCard CreateCard(
            Transform parent,
            string name,
            Vector3 position,
            FirstPlayableCardZone zone,
            bool faceUp,
            Card? card,
            int handIndex = -1,
            bool interactive = false)
        {
            var cardObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cardObject.name = name;
            cardObject.hideFlags = HideFlags.DontSave;
            cardObject.transform.SetParent(parent, false);
            cardObject.transform.localPosition = position;
            cardObject.transform.localRotation = faceUp
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;
            cardObject.transform.localScale = _authoredLayout.CardScale;

            var collider = cardObject.GetComponent<Collider>();
            if (!interactive)
            {
                DestroyGeneratedObject(collider);
            }

            var renderer = cardObject.GetComponent<Renderer>();
            if (faceUp && card.HasValue)
            {
                CardVisualMaterialBinding.Apply(renderer, _cardCatalog, card.Value);
            }
            else
            {
                renderer.sharedMaterial = _cardBackMaterial;
            }

            var rendered = cardObject.AddComponent<FirstPlayableRenderedCard>();
            rendered.Configure(zone, faceUp, card, handIndex);
            _renderedCards.Add(rendered);
            return rendered;
        }

        private void ApplyInteractionState()
        {
            if (_interaction == null)
            {
                return;
            }

            for (var index = 0; index < _localHandViews.Count && index < Snapshot.LocalHand.Count; index++)
            {
                _localHandViews[index].Apply(ResolveVisualState(Snapshot.LocalHand[index]));
            }

            var legalIntents = _flowController.Flow.Match.GetHumanLegalIntents();
            for (var index = 0; index < legalIntents.Count; index++)
            {
                if (legalIntents[index] is PlayCardIntent)
                {
                    _flowController.RenderInteractionFeedback(_interaction.State.FeedbackLocalizationKey);
                    break;
                }
            }
        }

        private PrototypeCardVisualState ResolveVisualState(Card card)
        {
            var state = _interaction.State;
            if (state.FeedbackCard == card)
            {
                switch (state.Feedback)
                {
                    case CardInteractionFeedback.Confirmed:
                        return PrototypeCardVisualState.Confirmed;
                    case CardInteractionFeedback.Rejected:
                        return PrototypeCardVisualState.Rejected;
                    case CardInteractionFeedback.TemporarilyBlocked:
                        return PrototypeCardVisualState.TemporarilyBlocked;
                }
            }

            if (state.SelectedCard == card)
            {
                return _interaction.IsTemporarilyBlocked
                    ? PrototypeCardVisualState.TemporarilyBlocked
                    : PrototypeCardVisualState.Selected;
            }

            return _interaction.IsCardLegal(card)
                ? PrototypeCardVisualState.Legal
                : PrototypeCardVisualState.TemporarilyBlocked;
        }

        private void BindInputActions()
        {
            var source = FindAnyObjectByType<InputIntentSource>();
            _pointAction = ResolveInputAction(source, PlayerIntentKind.Point);
            _navigateAction = ResolveInputAction(source, PlayerIntentKind.Navigate);
            _inspectAction = ResolveInputAction(source, PlayerIntentKind.Inspect);
            _selectAction = ResolveInputAction(source, PlayerIntentKind.Select);
            _confirmAction = ResolveInputAction(source, PlayerIntentKind.Confirm);
            _cancelAction = ResolveInputAction(source, PlayerIntentKind.Cancel);
            _pointAction.actionMap.Enable();

            _pointAction.performed += OnPoint;
            _navigateAction.performed += OnNavigate;
            _inspectAction.performed += OnInspect;
            _selectAction.performed += OnSelect;
            _confirmAction.performed += OnConfirm;
            _cancelAction.performed += OnCancel;
        }

        private static InputAction ResolveInputAction(InputIntentSource source, PlayerIntentKind kind)
        {
            if (source != null)
            {
                return source.GetAction(kind);
            }

            return InputSystem.actions?.FindAction($"Gameplay/{kind}", true)
                ?? throw new MissingReferenceException("The Fall input actions are unavailable.");
        }

        private void UnbindInputActions()
        {
            if (_pointAction != null)
            {
                _pointAction.performed -= OnPoint;
                _navigateAction.performed -= OnNavigate;
                _inspectAction.performed -= OnInspect;
                _selectAction.performed -= OnSelect;
                _confirmAction.performed -= OnConfirm;
                _cancelAction.performed -= OnCancel;
            }

            _pointAction = null;
            _navigateAction = null;
            _inspectAction = null;
            _selectAction = null;
            _confirmAction = null;
            _cancelAction = null;
        }

        private void OnPoint(InputAction.CallbackContext context)
        {
            _pointerPosition = context.ReadValue<Vector2>();
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (_inputAdapter == null)
            {
                return;
            }

            var direction = context.ReadValue<Vector2>();
            _inputAdapter.Navigate(Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? Math.Sign(direction.x)
                : -Math.Sign(direction.y));
            ApplyInteractionState();
        }

        private void OnInspect(InputAction.CallbackContext context)
        {
            if (_inputAdapter == null)
            {
                return;
            }

            if (TryGetPointedLocalHandCard(out var card))
            {
                if (context.control.device is Touchscreen)
                {
                    _inputAdapter.TouchInspect(card);
                }
                else
                {
                    _inputAdapter.MouseInspect(card);
                }
            }
            else
            {
                _inputAdapter.KeyboardInspect();
            }
        }

        private void OnSelect(InputAction.CallbackContext context)
        {
            if (_inputAdapter == null)
            {
                return;
            }

            if (TryGetPointedRenderedCard(out var rendered))
            {
                if (rendered.Zone == FirstPlayableCardZone.DealerSpread)
                {
                    ActivateDealerCard(rendered.InteractionIndex);
                }
                else if (rendered.Zone == FirstPlayableCardZone.LocalHand && rendered.Card.HasValue)
                {
                    _inputAdapter.TouchTap(rendered.Card.Value);
                }
            }
            else
            {
                _inputAdapter.KeyboardSelect();
            }
        }

        private void OnConfirm(InputAction.CallbackContext context)
        {
            _inputAdapter?.KeyboardConfirm();
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            _inputAdapter?.Cancel();
        }

        private bool TryGetPointedLocalHandCard(out Card card)
        {
            if (TryGetPointedRenderedCard(out var rendered)
                && rendered.Zone == FirstPlayableCardZone.LocalHand
                && rendered.Card.HasValue)
            {
                card = rendered.Card.Value;
                return true;
            }

            card = default;
            return false;
        }

        private bool TryGetPointedRenderedCard(out FirstPlayableRenderedCard rendered)
        {
            if (_gameplayCamera != null
                && Physics.Raycast(_gameplayCamera.ScreenPointToRay(_pointerPosition), out var hit))
            {
                rendered = hit.collider.GetComponent<FirstPlayableRenderedCard>();
                return rendered != null;
            }

            rendered = null;
            return false;
        }

        private void ConfigureFixedCamera()
        {
            if (_gameplayCamera == null)
            {
                _gameplayCamera = Camera.main;
            }

            if (_gameplayCamera == null)
            {
                throw new MissingReferenceException("The Home scene has no gameplay camera.");
            }

            _gameplayCamera.nearClipPlane = 0.1f;
            _gameplayCamera.farClipPlane = 50f;
            _gameplayCamera.backgroundColor = Lampblack;
        }

        private static GameObject CloneAuthoredObject(GameObject source, Transform parent, string name)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var clone = Instantiate(source, parent, false);
            clone.name = name;
            clone.hideFlags = HideFlags.DontSave;
            clone.SetActive(true);
            return clone;
        }

        private static Transform CreateRuntimeAnchor(Transform parent, Transform source, string name)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var anchor = new GameObject(name);
            anchor.hideFlags = HideFlags.DontSave;
            anchor.transform.SetParent(parent, false);
            CopyLocalTransform(source, anchor.transform);
            return anchor.transform;
        }

        private static void CopyLocalTransform(Transform source, Transform destination)
        {
            destination.localPosition = source.localPosition;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;
        }

        private void EnsureCardBackMaterial()
        {
            if (_cardCatalog == null || _cardCatalog.SharedFaceMaterial == null || _cardCatalog.BackTexture == null)
            {
                throw new MissingReferenceException("The integrated table is missing the complete card visual catalog.");
            }

            if (_cardBackMaterial != null)
            {
                return;
            }

            _cardBackMaterial = new Material(_cardCatalog.SharedFaceMaterial)
            {
                name = "Runtime Shared Card Back",
                hideFlags = HideFlags.HideAndDontSave,
            };
            _cardBackMaterial.SetTexture("_BaseMap", _cardCatalog.BackTexture);
            _cardBackMaterial.SetTexture("_MainTex", _cardCatalog.BackTexture);
            _cardBackMaterial.SetColor("_BaseColor", Color.white);
            _cardBackMaterial.SetColor("_Color", Color.white);
        }

        private GameObject CreatePrimitive(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            Color color)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.hideFlags = HideFlags.DontSave;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = position;
            primitive.transform.localScale = scale;
            primitive.transform.localRotation = rotation;
            DestroyGeneratedObject(primitive.GetComponent<Collider>());
            primitive.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
            return primitive;
        }

        private Material MaterialFor(Color color)
        {
            var key = (Color32)color;
            if (_generatedMaterials.TryGetValue(key, out var material))
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader)
            {
                name = $"First Playable {ColorUtility.ToHtmlStringRGB(color)}",
                color = color,
                hideFlags = HideFlags.HideAndDontSave,
            };
            _generatedMaterials.Add(key, material);
            return material;
        }

        private void DestroyGeneratedContent()
        {
            _cardMotions.Clear();
            _renderedCards.Clear();
            _localHandViews.Clear();
            if (_generatedRoot != null)
            {
                DestroyGeneratedObject(_generatedRoot.gameObject);
                _generatedRoot = null;
            }
        }

        private void DestroyGeneratedMaterials()
        {
            foreach (var material in _generatedMaterials.Values)
            {
                DestroyGeneratedObject(material);
            }

            _generatedMaterials.Clear();
            DestroyGeneratedObject(_cardBackMaterial);
            _cardBackMaterial = null;
        }

        private static void DestroyGeneratedObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (UnityEngine.Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static Vector2Int RuntimeViewport()
        {
            return new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));
        }

        private static Rect RuntimeSafeArea(Vector2Int viewport)
        {
            var safeArea = Screen.safeArea;
            return safeArea.width > 0f && safeArea.height > 0f
                ? safeArea
                : new Rect(0f, 0f, viewport.x, viewport.y);
        }

        private static Color FromHex(int rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }

        private static double TicksToMilliseconds(long ticks)
        {
            return ticks * 1000d / System.Diagnostics.Stopwatch.Frequency;
        }

        private sealed class CardPositionSnapshot
        {
            public Dictionary<Card, Vector3> Cards { get; } = new Dictionary<Card, Vector3>();

            public Dictionary<int, Vector3> OpponentHandPositions { get; } =
                new Dictionary<int, Vector3>();

            public bool HasDeckPosition { get; set; }

            public Vector3 DeckPosition { get; set; }
        }

        private readonly struct CardMotion
        {
            public CardMotion(
                Transform card,
                Vector3 start,
                Vector3 target,
                AnimationBeatEasing easing,
                Vector3 trajectory)
            {
                Card = card;
                Start = start;
                Target = target;
                Easing = easing;
                Trajectory = trajectory;
            }

            public Transform Card { get; }

            public Vector3 Start { get; }

            public Vector3 Target { get; }

            public AnimationBeatEasing Easing { get; }

            public Vector3 Trajectory { get; }
        }
    }
}
