using System;
using System.Collections.Generic;
using TMPro;
using TheFall.Application;
using TheFall.Application.Input;
using TheFall.Application.Interaction;
using TheFall.Domain;
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
        private static readonly Vector3 FixedCameraPosition = new Vector3(0f, 7.2f, -5.4f);
        private static readonly Quaternion FixedCameraRotation = Quaternion.Euler(52f, 0f, 0f);

        private static readonly Color Lampblack = FromHex(0x241A14);
        private static readonly Color CharredWalnut = FromHex(0x3B291F);
        private static readonly Color Ochre = FromHex(0xA06F3C);
        private static readonly Color Vellum = FromHex(0xD8C493);
        private static readonly Color Moss = FromHex(0x6B7046);
        private static readonly Color Woad = FromHex(0x465C73);
        private static readonly Color Brass = FromHex(0xB58B3E);

        [SerializeField] private Camera _gameplayCamera;
        [SerializeField] private GameObject _tablePrototypePrefab;
        [SerializeField] private CardVisualCatalog _cardCatalog;

        private readonly Dictionary<Color32, Material> _generatedMaterials = new Dictionary<Color32, Material>();
        private readonly List<FirstPlayableRenderedCard> _renderedCards = new List<FirstPlayableRenderedCard>();
        private readonly List<PrototypeCardView> _localHandViews = new List<PrototypeCardView>();
        private FirstPlayableFlowController _flowController;
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

        public Camera GameplayCamera => _gameplayCamera;

        public GameObject TablePrototypePrefab => _tablePrototypePrefab;

        public CardVisualCatalog CardCatalog => _cardCatalog;

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

            ConfigureFixedCamera();
            BindInputActions();
            _flowController.PresentationChanged += RefreshFromFlow;
            RefreshFromFlow();
        }

        private void Update()
        {
            if (!UnityEngine.Application.isPlaying || Snapshot == null)
            {
                return;
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
            }

            UnbindInputActions();
            ClearInteraction();
            DestroyGeneratedContent();
            DestroyGeneratedMaterials();
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

#if UNITY_EDITOR
        public void Configure(
            Camera gameplayCamera,
            GameObject tablePrototypePrefab,
            CardVisualCatalog cardCatalog)
        {
            _gameplayCamera = gameplayCamera;
            _tablePrototypePrefab = tablePrototypePrefab;
            _cardCatalog = cardCatalog;
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
                Snapshot = null;
                ClearInteraction();
                DestroyGeneratedContent();
                return;
            }

            Snapshot = FirstPlayableTableSnapshot.Create(flow.Match.State);
            if (flow.Stage == FirstPlayableFlowStage.Match && _boundSessionNumber != flow.SessionNumber)
            {
                CreateInteraction(flow.SessionNumber);
            }

            if (_inputAdapter != null)
            {
                _inputAdapter.SetCards(Snapshot.LocalHand);
            }

            Rebuild(RuntimeViewport(), RuntimeSafeArea(RuntimeViewport()));
            ApplyInteractionState();
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

            _interaction.SetTemporarilyBlocked(false);
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

            CreateLighting(_generatedRoot);
            CreateStage(_generatedRoot);
            CreateTable(_generatedRoot);
            CreateSeats(_generatedRoot);
            CreateStateCards(_generatedRoot);
        }

        private void CreateLighting(Transform parent)
        {
            var key = new GameObject("Warm Table Key", typeof(Light));
            key.hideFlags = HideFlags.DontSave;
            key.transform.SetParent(parent, false);
            key.transform.localRotation = Quaternion.Euler(55f, -28f, 0f);
            var light = key.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.78f, 0.55f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
        }

        private void CreateStage(Transform parent)
        {
            CreatePrimitive("Quiet Room Ground", PrimitiveType.Cube, parent,
                new Vector3(0f, -0.08f, 0.2f), new Vector3(5.6f, 0.12f, 5.4f), Quaternion.identity, Lampblack);
            CreatePrimitive("Warm Stage Pool", PrimitiveType.Cylinder, parent,
                Vector3.zero, new Vector3(2.25f, 0.04f, 2.25f), Quaternion.identity, CharredWalnut);
        }

        private void CreateTable(Transform parent)
        {
            if (_tablePrototypePrefab == null)
            {
                throw new MissingReferenceException("The integrated table is missing RoundCardTable.");
            }

            var table = Instantiate(_tablePrototypePrefab, parent, false);
            table.name = "RoundCardTable";
            table.hideFlags = HideFlags.DontSave;
        }

        private void CreateSeats(Transform parent)
        {
            CreateSeat(parent, Seat.First, Snapshot.LocalPlayerName, Snapshot.LocalScore, Moss, new Vector3(0f, 0f, -1.55f));
            CreateSeat(parent, Seat.Second, Snapshot.OpponentPlayerName, Snapshot.OpponentScore, Woad, new Vector3(0f, 0f, 1.55f));
        }

        private void CreateSeat(
            Transform parent,
            Seat seat,
            string playerName,
            int score,
            Color bodyColor,
            Vector3 position)
        {
            var seatObject = new GameObject(seat == Seat.First ? "Local Bottom Seat" : "Opponent Top Seat");
            seatObject.hideFlags = HideFlags.DontSave;
            seatObject.transform.SetParent(parent, false);
            seatObject.transform.localPosition = position;
            seatObject.transform.localRotation = Quaternion.LookRotation(-position.normalized, Vector3.up);

            var isActive = Snapshot.ActiveSeat == seat && Snapshot.Phase != MatchPhase.Completed;
            var isDealer = Snapshot.DealerSeat == seat;
            if (isActive)
            {
                CreatePrimitive("Active Turn Ring", PrimitiveType.Cylinder, seatObject.transform,
                    new Vector3(0f, 0.07f, -0.08f), new Vector3(0.47f, 0.025f, 0.47f), Quaternion.identity, Brass);
            }

            if (isDealer)
            {
                CreatePrimitive("Dealer Diamond Cue", PrimitiveType.Cube, seatObject.transform,
                    new Vector3(0f, 0.12f, -0.32f), new Vector3(0.12f, 0.035f, 0.12f),
                    Quaternion.Euler(0f, 45f, 0f), Brass);
            }

            CreatePrimitive("Upper Body Placeholder", PrimitiveType.Capsule, seatObject.transform,
                new Vector3(0f, 0.72f, -0.12f), new Vector3(0.4f, 0.43f, 0.26f), Quaternion.identity, bodyColor);
            CreatePrimitive("Placeholder Head", PrimitiveType.Sphere, seatObject.transform,
                new Vector3(0f, 1.25f, -0.08f), new Vector3(0.32f, 0.38f, 0.32f), Quaternion.identity, Ochre);
            CreatePrimitive("Left Placeholder Hand", PrimitiveType.Sphere, seatObject.transform,
                new Vector3(-0.3f, 0.79f, 0.23f), new Vector3(0.12f, 0.08f, 0.16f), Quaternion.identity, Ochre);
            CreatePrimitive("Right Placeholder Hand", PrimitiveType.Sphere, seatObject.transform,
                new Vector3(0.3f, 0.79f, 0.23f), new Vector3(0.12f, 0.08f, 0.16f), Quaternion.identity, Ochre);

            var prefix = (isActive ? "> " : string.Empty) + (isDealer ? "D " : string.Empty);
            CreateLabel($"{seat} Seat Status", parent, position + new Vector3(0f, 1.66f, 0f),
                $"{prefix}{playerName}\n{score:00}", isActive ? Brass : Vellum, 3.6f);
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

            CreateLocalHand(parent, Snapshot.LocalHand);
            CreateOpponentHand(parent, Snapshot.OpponentHandCount);
            CreateCapturedPile(parent, Snapshot.LocalCapturedCards, FirstPlayableCardZone.LocalCaptured, new Vector3(-0.88f, 0.80f, -0.55f));
            CreateCapturedPile(parent, Snapshot.OpponentCapturedCards, FirstPlayableCardZone.OpponentCaptured, new Vector3(0.88f, 0.80f, 0.55f));
        }

        private void CreateDealerSpread(Transform parent, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var row = index / 10;
                var column = index % 10;
                CreateCard(parent, $"Face-down Dealer Card {index + 1}",
                    new Vector3((column - 4.5f) * 0.095f, 0.80f + row * 0.002f, (row - 1.5f) * 0.13f),
                    0.42f, FirstPlayableCardZone.DealerSpread, false, null);
            }
        }

        private void CreateDeck(Transform parent, int count)
        {
            for (var index = 0; index < count; index++)
            {
                CreateCard(parent, $"Deck Card {index + 1}",
                    new Vector3(0.72f, 0.80f + index * 0.0015f, 0f),
                    0.66f, FirstPlayableCardZone.Deck, false, null);
            }
        }

        private void CreateTableCards(Transform parent, IReadOnlyList<Card> cards)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                var row = index / 5;
                var column = index % 5;
                CreateCard(parent, $"Table {cards[index]}",
                    new Vector3((column - 2f) * 0.17f, 0.805f + row * 0.002f, (row - 0.5f) * 0.25f),
                    0.72f, FirstPlayableCardZone.Table, true, cards[index]);
            }
        }

        private void CreateLocalHand(Transform parent, IReadOnlyList<Card> cards)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                var x = (index - (cards.Count - 1) * 0.5f) * 0.22f;
                var rendered = CreateCard(parent, $"Local Hand {cards[index]}",
                    new Vector3(x, 0.82f, -0.92f + Mathf.Abs(index - 1) * 0.025f),
                    0.9f, FirstPlayableCardZone.LocalHand, true, cards[index], index, true);
                var view = rendered.gameObject.AddComponent<PrototypeCardView>();
                view.Configure(index);
                _localHandViews.Add(view);
            }
        }

        private void CreateOpponentHand(Transform parent, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var x = (index - (count - 1) * 0.5f) * 0.18f;
                CreateCard(parent, $"Private Opponent Hand Card {index + 1}",
                    new Vector3(-x, 0.82f, 0.92f),
                    0.76f, FirstPlayableCardZone.OpponentHand, false, null);
            }
        }

        private void CreateCapturedPile(
            Transform parent,
            IReadOnlyList<Card> cards,
            FirstPlayableCardZone zone,
            Vector3 origin)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                CreateCard(parent, $"{zone} {cards[index]}",
                    origin + new Vector3((index % 4) * 0.012f, index * 0.002f, (index % 3) * 0.009f),
                    0.48f, zone, true, cards[index]);
            }
        }

        private FirstPlayableRenderedCard CreateCard(
            Transform parent,
            string name,
            Vector3 position,
            float scale,
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
            cardObject.transform.localScale = new Vector3(0.18f * scale, 0.012f, 0.26f * scale);

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

            _flowController.RenderInteractionFeedback(_interaction.State.FeedbackLocalizationKey);
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

            if (TryGetPointedCard(out var card))
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

            if (TryGetPointedCard(out var card))
            {
                if (context.control.device is Touchscreen)
                {
                    _inputAdapter.TouchTap(card);
                }
                else
                {
                    _inputAdapter.MouseSelect(card);
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

        private bool TryGetPointedCard(out Card card)
        {
            if (_gameplayCamera != null && Physics.Raycast(_gameplayCamera.ScreenPointToRay(_pointerPosition), out var hit))
            {
                var rendered = hit.collider.GetComponent<FirstPlayableRenderedCard>();
                if (rendered != null
                    && rendered.Zone == FirstPlayableCardZone.LocalHand
                    && rendered.Card.HasValue)
                {
                    card = rendered.Card.Value;
                    return true;
                }
            }

            card = default;
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

            _gameplayCamera.transform.position = FixedCameraPosition;
            _gameplayCamera.transform.rotation = FixedCameraRotation;
            _gameplayCamera.fieldOfView = 44f;
            _gameplayCamera.nearClipPlane = 0.1f;
            _gameplayCamera.farClipPlane = 50f;
            _gameplayCamera.backgroundColor = Lampblack;
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

        private void CreateLabel(
            string name,
            Transform parent,
            Vector3 position,
            string text,
            Color color,
            float fontSize)
        {
            var labelObject = new GameObject(name, typeof(TextMeshPro));
            labelObject.hideFlags = HideFlags.DontSave;
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = position;
            var awayFromCamera = labelObject.transform.position - _gameplayCamera.transform.position;
            labelObject.transform.rotation = Quaternion.LookRotation(awayFromCamera, _gameplayCamera.transform.up);
            labelObject.transform.localScale = Vector3.one * 0.2f;

            var label = labelObject.GetComponent<TextMeshPro>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = fontSize;
            label.fontStyle = FontStyles.Bold;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.rectTransform.sizeDelta = new Vector2(6f, 2.2f);
        }

        private void DestroyGeneratedContent()
        {
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
    }
}
