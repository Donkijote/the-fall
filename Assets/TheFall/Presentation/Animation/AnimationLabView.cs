using System;
using System.Collections.Generic;
using TMPro;
using TheFall.Domain;
using TheFall.Presentation.Cards;
using TheFall.Presentation.Table;
using UnityEngine;

namespace TheFall.Presentation.Animation
{
    internal sealed class AnimationLabView
    {
        private static readonly Color Lampblack = FromHex(0x241A14);
        private static readonly Color CharredWalnut = FromHex(0x3B291F);
        private static readonly Color Moss = FromHex(0x6B7046);
        private static readonly Color Woad = FromHex(0x465C73);
        private static readonly Color Vellum = FromHex(0xD8C493);
        private static readonly Color Brass = FromHex(0xB58B3E);
        private const int DealerSpreadColumns = 8;
        private const float DealerCardWidth = 0.24f;
        private const float DealerCardHeight = 0.34f;
        private const float DealerFlipLift = 0.13f;
        private const float DealerSelectedRestHeight = 0.015f;
        private const float CapturePlayEndProgress = 0.38f;
        private const float CapturePickupStartProgress = 0.46f;
        private const float CapturedPileRadius = 0.92f;
        private const float CapturedPileLeftOffset = 0.52f;

        private readonly Transform _owner;
        private readonly Camera _camera;
        private readonly GameObject _tablePrefab;
        private readonly CardVisualCatalog _cardCatalog;
        private readonly Dictionary<Card, Transform> _cardViews = new Dictionary<Card, Transform>();
        private readonly Dictionary<TeamId, TextMeshPro> _scoreLabels = new Dictionary<TeamId, TextMeshPro>();
        private readonly List<Material> _ownedMaterials = new List<Material>();
        private readonly List<CardMotion> _motions = new List<CardMotion>();
        private readonly List<DealerSpreadCardView> _dealerSpreadViews = new List<DealerSpreadCardView>();
        private readonly List<HiddenCardView> _deckViews = new List<HiddenCardView>();
        private readonly List<HiddenCardView> _opponentHandViews = new List<HiddenCardView>();
        private readonly List<HiddenCardView> _capturedPileViews = new List<HiddenCardView>();
        private readonly List<CapturePairMotion> _capturePairMotions = new List<CapturePairMotion>();
        private readonly List<CascadeStackMotion> _cascadeStackMotions = new List<CascadeStackMotion>();
        private readonly List<CollectedCardMotion> _leftoverMotions = new List<CollectedCardMotion>();
        private readonly List<DeckSplitMotion> _rejectionDeckSplit = new List<DeckSplitMotion>();
        private Transform _generatedRoot;
        private TextMeshPro _eventCue;
        private Material _cardBackMaterial;
        private DealerSpreadCardView _activeDealerCard;
        private HiddenCardView _activeDealCard;
        private HiddenCardView _activeRejectedCard;
        private Vector3 _activeDealerCardStart;
        private Vector3 _activeDealStart;
        private Vector3 _activeDealTarget;
        private Vector3 _activeRejectionStart;
        private Vector3 _activeRejectionTarget;
        private float _dealerFlipDirection = 1f;
        private float _dealFlipDirection = 1f;
        private float _dealerFlipLift;
        private float _dealerFlipDegrees;
        private float _dealFlipDegrees;
        private float _rejectionFlipDegrees;
        private float _rejectionDeckGap;
        private float _capturePairFlipDegrees;
        private float _cascadeStackFlipDegrees;
        private float _leftoverFlipDegrees;
        private bool _activeDealFaceUp;
        private bool _dealRevealsFace;
        private bool _activeRejectedCardFaceDown;
        private bool _capturePairFaceDown;
        private bool _captureContinuesToCascade;
        private bool _cascadeStackFaceDown;
        private bool _cascadeStackCompletesCapture;
        private bool _leftoversFaceDown;
        private PlayerId _actingPlayerId;
        private AnimationBeatEasing _easing;
        private Vector3 _trajectoryOffset;
        private float _emphasis = 1f;

        public AnimationLabView(
            Transform owner,
            Camera camera,
            GameObject tablePrefab,
            CardVisualCatalog cardCatalog)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _tablePrefab = tablePrefab ?? throw new ArgumentNullException(nameof(tablePrefab));
            _cardCatalog = cardCatalog ?? throw new ArgumentNullException(nameof(cardCatalog));
        }

        public TableCompositionProfile CurrentProfile { get; private set; }

        public int CardViewCount =>
            _cardViews.Count +
            _dealerSpreadViews.Count +
            _deckViews.Count +
            _opponentHandViews.Count +
            _capturedPileViews.Count;

        public int DealerSpreadViewCount => _dealerSpreadViews.Count;

        public int DeckViewCount => _deckViews.Count;

        public int OpponentHandViewCount => _opponentHandViews.Count;

        public bool ActiveDealCardIsFaceUp => _activeDealCard != null && _activeDealFaceUp;

        public float DealCardFlipDegrees => _dealFlipDegrees;

        public bool ActiveDeckCardIsFaceUp => ActiveDealCardIsFaceUp;

        public float DeckCardFlipDegrees => DealCardFlipDegrees;

        public bool ActiveRejectedCardIsFaceDown =>
            _activeRejectedCard != null && _activeRejectedCardFaceDown;

        public float RejectedCardFlipDegrees => _rejectionFlipDegrees;

        public float RejectionDeckGap => _rejectionDeckGap;

        public int CapturePairViewCount => _capturePairMotions.Count;

        public int FaceDownCapturePairViewCount =>
            _capturePairFaceDown ? _capturePairMotions.Count : 0;

        public float CapturePairFlipDegrees => _capturePairFlipDegrees;

        public int CapturedPileViewCount => _capturedPileViews.Count;

        public int CascadeStackViewCount => _cascadeStackMotions.Count;

        public int FaceDownCascadeStackViewCount =>
            _cascadeStackFaceDown ? _cascadeStackMotions.Count : 0;

        public float CascadeStackFlipDegrees => _cascadeStackFlipDegrees;

        public int LeftoverCollectionViewCount => _leftoverMotions.Count;

        public int FaceDownLeftoverCollectionViewCount =>
            _leftoversFaceDown ? _leftoverMotions.Count : 0;

        public float LeftoverCollectionFlipDegrees => _leftoverFlipDegrees;

        public int RevealedDealerCardViewCount
        {
            get
            {
                var count = 0;
                foreach (var view in _dealerSpreadViews)
                {
                    if (view.IsFaceUp)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public float RevealedDealerCardClearance
        {
            get
            {
                var highestFaceDown = float.NegativeInfinity;
                var lowestRevealed = float.PositiveInfinity;
                foreach (var view in _dealerSpreadViews)
                {
                    if (view.IsFaceUp)
                    {
                        lowestRevealed = Mathf.Min(
                            lowestRevealed,
                            view.Transform.localPosition.y);
                    }
                    else
                    {
                        highestFaceDown = Mathf.Max(
                            highestFaceDown,
                            view.Transform.localPosition.y);
                    }
                }

                return float.IsNegativeInfinity(highestFaceDown) ||
                    float.IsPositiveInfinity(lowestRevealed)
                        ? 0f
                        : lowestRevealed - highestFaceDown;
            }
        }

        public float DealerCardFlipDegrees => _dealerFlipDegrees;

        public Transform GeneratedRoot => _generatedRoot;

        public void Build(
            AnimationPresentationState state,
            PlayerId actingPlayerId,
            Vector2Int viewport)
        {
            Destroy();
            _actingPlayerId = actingPlayerId;
            CurrentProfile = TableCompositionLayout.ResolveProfile(viewport);

            var rootObject = new GameObject("Generated Animation Laboratory");
            rootObject.hideFlags = HideFlags.DontSave;
            rootObject.transform.SetParent(_owner, false);
            _generatedRoot = rootObject.transform;
            _generatedRoot.localScale = Vector3.one * CurrentProfile.ContentScale;

            CreateStage();
            CreateSeats(state);
            CreateEventCue();
            CreateDealerSpread(state);
            CreateDeck(state);
            CreateOpponentHands(state);
            CreateCapturedPiles(state);
            EnsureCardViews(state);
            RenderImmediate(state);
        }

        public void PrepareTransition(
            AnimationPresentationState state,
            ResolvedAnimationStep step,
            AnimationBeatConfiguration beat,
            bool reducedMotion,
            float reducedMotionTrajectoryScale)
        {
            var existingCards = new HashSet<Card>(_cardViews.Keys);
            _motions.Clear();
            _easing = beat?.Easing ?? AnimationBeatEasing.EaseInOut;
            _trajectoryOffset = beat?.TrajectoryOffset ?? Vector3.zero;
            if (reducedMotion)
            {
                _trajectoryOffset *= Mathf.Clamp01(reducedMotionTrajectoryScale);
            }

            _emphasis = beat?.Emphasis ?? 1f;
            if (step.Kind == ResolvedAnimationStepKind.DealerSelection && step.Cards.Count > 0)
            {
                PrepareDealerCardFlip(state, step);
                RefreshLabels(state);
                SetEventCue(step);
                return;
            }

            if (step.Kind == ResolvedAnimationStepKind.Deal &&
                step.SourceEvent is CardDealtEvent &&
                step.Cards.Count > 0)
            {
                PrepareDealCard(state, step);
                RefreshLabels(state);
                SetEventCue(step);
                return;
            }

            if (step.Kind == ResolvedAnimationStepKind.OpeningPlacement &&
                step.Cards.Count > 0)
            {
                PrepareOpeningCard(state, step);
                RefreshLabels(state);
                SetEventCue(step);
                return;
            }

            if (step.Kind == ResolvedAnimationStepKind.OpeningRejection &&
                step.SourceEvent is OpeningCardRejectedEvent &&
                step.Cards.Count > 0)
            {
                PrepareOpeningRejection(step);
                RefreshLabels(state);
                SetEventCue(step);
                return;
            }

            if (step.Kind == ResolvedAnimationStepKind.NormalCapture &&
                step.Cards.Count >= 2)
            {
                PrepareNormalCapture(state, step);
                RefreshLabels(state);
                SetEventCue(step);
                return;
            }

            if ((step.Kind == ResolvedAnimationStepKind.CascadeCapture
                    || step.Kind == ResolvedAnimationStepKind.CaptureCollection) &&
                step.SourceEvent is CardsCapturedEvent &&
                step.Cards.Count > 0)
            {
                PrepareCascadeCapture(state, step);
                RefreshLabels(state);
                SetEventCue(step);
                return;
            }

            if (step.Kind == ResolvedAnimationStepKind.Leftovers &&
                step.SourceEvent is LeftoversCollectedEvent &&
                step.Cards.Count > 0)
            {
                PrepareLeftoverCollection(state, step);
                RefreshLabels(state);
                SetEventCue(step);
                return;
            }

            EnsureCardViews(state);
            for (var index = 0; index < step.Cards.Count; index++)
            {
                EnsureCardView(step.Cards[index]);
            }

            foreach (var entry in _cardViews)
            {
                if (!existingCards.Contains(entry.Key) && Contains(step.Cards, entry.Key))
                {
                    entry.Value.localPosition = ResolveSyntheticSource(step, state, entry.Key);
                }

                var target = ResolveTransitionTarget(state, step, entry.Key);
                if (step.Kind == ResolvedAnimationStepKind.CardPlay && !Contains(step.Cards, entry.Key))
                {
                    target = entry.Value.localPosition;
                }

                _motions.Add(new CardMotion(
                    entry.Value,
                    entry.Value.localPosition,
                    target));
            }
            RefreshLabels(state);
            SetEventCue(step);
        }

        public void ApplyTransition(float progress)
        {
            var clamped = Mathf.Clamp01(progress);
            foreach (var motion in _motions)
            {
                motion.Transform.localPosition =
                    Vector3.SqrMagnitude(motion.Target - motion.Start) <= 0.000001f
                        ? motion.Target
                        : AnimationBeatEvaluator.EvaluatePosition(
                            motion.Start,
                            motion.Target,
                            clamped,
                            _easing,
                            _trajectoryOffset);
            }

            ApplyDealerCardFlip(clamped);
            ApplyDealCard(clamped);
            ApplyOpeningRejection(clamped);
            ApplyNormalCapture(clamped);
            ApplyCascadeCapture(clamped);
            ApplyLeftoverCollection(clamped);

            if (_eventCue != null)
            {
                var pulse = 1f + Mathf.Sin(clamped * Mathf.PI) * 0.12f * Mathf.Max(0f, _emphasis);
                _eventCue.transform.localScale = Vector3.one * 0.18f * pulse;
            }
        }

        public bool TryGetPrimaryMotion(out AnimationMotionPreview preview)
        {
            if (_leftoverMotions.Count > 0 && _generatedRoot != null)
            {
                var leftover = _leftoverMotions[0];
                preview = new AnimationMotionPreview(
                    _generatedRoot.TransformPoint(leftover.Start),
                    _generatedRoot.TransformPoint(leftover.Target),
                    _generatedRoot);
                return true;
            }

            if (_cascadeStackMotions.Count > 0 && _generatedRoot != null)
            {
                var cascade = _cascadeStackMotions[0];
                preview = new AnimationMotionPreview(
                    _generatedRoot.TransformPoint(cascade.Start),
                    _generatedRoot.TransformPoint(cascade.Target),
                    _generatedRoot);
                return true;
            }

            if (_capturePairMotions.Count > 0 && _generatedRoot != null)
            {
                var capture = _capturePairMotions[0];
                preview = new AnimationMotionPreview(
                    _generatedRoot.TransformPoint(capture.Start),
                    _generatedRoot.TransformPoint(capture.Target),
                    _generatedRoot);
                return true;
            }

            if (_activeRejectedCard != null &&
                _generatedRoot != null &&
                Vector3.SqrMagnitude(_activeRejectionTarget - _activeRejectionStart) > 0.000001f)
            {
                preview = new AnimationMotionPreview(
                    _generatedRoot.TransformPoint(_activeRejectionStart),
                    _generatedRoot.TransformPoint(_activeRejectionTarget),
                    _generatedRoot);
                return true;
            }

            if (_activeDealCard != null &&
                _generatedRoot != null &&
                Vector3.SqrMagnitude(_activeDealTarget - _activeDealStart) > 0.000001f)
            {
                preview = new AnimationMotionPreview(
                    _generatedRoot.TransformPoint(_activeDealStart),
                    _generatedRoot.TransformPoint(_activeDealTarget),
                    _generatedRoot);
                return true;
            }

            CardMotion? selected = null;
            var greatestDistance = 0f;
            foreach (var motion in _motions)
            {
                var distance = Vector3.SqrMagnitude(motion.Target - motion.Start);
                if (distance <= greatestDistance)
                {
                    continue;
                }

                greatestDistance = distance;
                selected = motion;
            }

            if (!selected.HasValue || greatestDistance <= 0.000001f || _generatedRoot == null)
            {
                preview = default;
                return false;
            }

            preview = new AnimationMotionPreview(
                _generatedRoot.TransformPoint(selected.Value.Start),
                _generatedRoot.TransformPoint(selected.Value.Target),
                _generatedRoot);
            return true;
        }

        public void RenderImmediate(AnimationPresentationState state)
        {
            EnsureCardViews(state);
            foreach (var entry in _cardViews)
            {
                entry.Value.localPosition = ResolveCardPosition(state, entry.Key);
            }

            RefreshLabels(state);
        }

        public void SetCompletionCue(AnimationSequenceCompletionReason reason)
        {
            if (_eventCue == null)
            {
                return;
            }

            switch (reason)
            {
                case AnimationSequenceCompletionReason.Completed:
                    _eventCue.text = "RESOLVED";
                    break;
                case AnimationSequenceCompletionReason.Skipped:
                    _eventCue.text = "SKIPPED · RESOLVED";
                    break;
                case AnimationSequenceCompletionReason.Interrupted:
                    _eventCue.text = "INTERRUPTED · RESOLVED";
                    break;
                case AnimationSequenceCompletionReason.Cancelled:
                    _eventCue.text = "CANCELLED · RESOLVED";
                    break;
                default:
                    _eventCue.text = string.Empty;
                    break;
            }
        }

        public void Destroy()
        {
            _cardViews.Clear();
            _scoreLabels.Clear();
            _motions.Clear();
            _dealerSpreadViews.Clear();
            _deckViews.Clear();
            _opponentHandViews.Clear();
            _capturedPileViews.Clear();
            _capturePairMotions.Clear();
            _cascadeStackMotions.Clear();
            _leftoverMotions.Clear();
            _rejectionDeckSplit.Clear();
            _eventCue = null;
            _cardBackMaterial = null;
            _activeDealerCard = null;
            _activeDealCard = null;
            _activeRejectedCard = null;
            _dealerFlipDegrees = 0f;
            _dealFlipDegrees = 0f;
            _rejectionFlipDegrees = 0f;
            _rejectionDeckGap = 0f;
            _capturePairFlipDegrees = 0f;
            _cascadeStackFlipDegrees = 0f;
            _leftoverFlipDegrees = 0f;
            _activeDealFaceUp = false;
            _dealRevealsFace = false;
            _activeRejectedCardFaceDown = false;
            _capturePairFaceDown = false;
            _captureContinuesToCascade = false;
            _cascadeStackFaceDown = false;
            _cascadeStackCompletesCapture = false;
            _leftoversFaceDown = false;

            if (_generatedRoot != null)
            {
                DestroyObject(_generatedRoot.gameObject);
                _generatedRoot = null;
            }

            foreach (var material in _ownedMaterials)
            {
                DestroyObject(material);
            }

            _ownedMaterials.Clear();
        }

        private void CreateDealerSpread(AnimationPresentationState state)
        {
            var revealedCards = state.DealerSelectionCards;
            var totalCount = state.DealerSpreadCount + revealedCards.Count;
            if (totalCount <= 0)
            {
                return;
            }

            EnsureCardBackMaterial();
            var revealedSlots = new Dictionary<int, Card>();
            for (var index = 0; index < revealedCards.Count; index++)
            {
                revealedSlots[GetDealerSelectionSlot(totalCount, index)] = revealedCards[index];
            }

            for (var slot = 0; slot < totalCount; slot++)
            {
                revealedSlots.TryGetValue(slot, out var revealedCard);
                var isRevealed = revealedSlots.ContainsKey(slot);
                CreateDealerSpreadCard(slot, isRevealed ? revealedCard : (Card?)null);
            }

            _dealerFlipDegrees = revealedCards.Count > 0 ? 180f : 0f;
        }

        private void CreateDealerSpreadCard(int slot, Card? revealedCard)
        {
            var rootObject = new GameObject(
                revealedCard.HasValue
                    ? $"Revealed Dealer Card {revealedCard.Value}"
                    : $"Face-down Dealer Card {slot + 1}");
            rootObject.hideFlags = HideFlags.DontSave;
            rootObject.transform.SetParent(_generatedRoot, false);
            rootObject.transform.localPosition =
                ResolveDealerSpreadPosition(slot) +
                (revealedCard.HasValue
                    ? Vector3.up * DealerSelectedRestHeight
                    : Vector3.zero);
            rootObject.transform.localRotation = revealedCard.HasValue
                ? Quaternion.AngleAxis(180f, Vector3.forward)
                : Quaternion.identity;

            var backRenderer = CreateCardSurface(
                rootObject.transform,
                "Card Back",
                new Vector3(0f, 0.001f, 0f),
                Quaternion.Euler(90f, 0f, 0f));
            backRenderer.sharedMaterial = _cardBackMaterial;

            var faceRenderer = CreateCardSurface(
                rootObject.transform,
                "Card Face",
                new Vector3(0f, -0.001f, 0f),
                Quaternion.Inverse(Quaternion.AngleAxis(180f, Vector3.forward)) *
                Quaternion.Euler(90f, 0f, 0f));
            if (revealedCard.HasValue)
            {
                CardVisualMaterialBinding.Apply(faceRenderer, _cardCatalog, revealedCard.Value);
            }
            else
            {
                faceRenderer.gameObject.SetActive(false);
            }

            _dealerSpreadViews.Add(new DealerSpreadCardView(
                rootObject.transform,
                backRenderer,
                faceRenderer,
                slot,
                revealedCard.HasValue));
        }

        private Renderer CreateCardSurface(
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion localRotation)
        {
            var surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            surface.name = name;
            surface.hideFlags = HideFlags.DontSave;
            surface.transform.SetParent(parent, false);
            surface.transform.localPosition = localPosition;
            surface.transform.localRotation = localRotation;
            surface.transform.localScale = new Vector3(DealerCardWidth, DealerCardHeight, 1f);
            DestroyObject(surface.GetComponent<Collider>());
            return surface.GetComponent<Renderer>();
        }

        private void CreateDeck(AnimationPresentationState state)
        {
            if (state.Phase == MatchPhase.DealerSelection || state.DeckCount <= 0)
            {
                return;
            }

            EnsureCardBackMaterial();
            for (var index = 0; index < state.DeckCount; index++)
            {
                _deckViews.Add(CreateHiddenCard(
                    $"Face-down Deck Card {index + 1}",
                    ResolveDeckPosition(index)));
            }
        }

        private void CreateOpponentHands(AnimationPresentationState state)
        {
            EnsureCardBackMaterial();
            foreach (var player in state.Players)
            {
                if (player.Id == _actingPlayerId)
                {
                    continue;
                }

                var hand = state.GetHand(player.Id);
                var slots = Math.Max(3, state.GetHandLayoutSlotCount(player.Id));
                for (var index = 0; index < hand.Count; index++)
                {
                    _opponentHandViews.Add(CreateHiddenCard(
                        $"Face-down {player.DisplayName} Hand Card {index + 1}",
                        GetSeatCardPosition(
                            player.Seat,
                            1.02f,
                            state.GetHandLayoutIndex(player.Id, hand[index]),
                            slots,
                            0.19f,
                            0.84f)));
                }
            }
        }

        private void CreateCapturedPiles(AnimationPresentationState state)
        {
            EnsureCardBackMaterial();
            foreach (var player in state.Players)
            {
                var captured = state.GetCaptured(player.Id);
                for (var index = 0; index < captured.Count; index++)
                {
                    _capturedPileViews.Add(CreateHiddenCard(
                        $"Face-down {player.DisplayName} Captured Card {index + 1}",
                        ResolveCapturedPilePosition(
                            player.Seat,
                            index)));
                }
            }
        }

        private HiddenCardView CreateHiddenCard(string name, Vector3 position)
        {
            var rootObject = new GameObject(name);
            rootObject.hideFlags = HideFlags.DontSave;
            rootObject.transform.SetParent(_generatedRoot, false);
            rootObject.transform.localPosition = position;

            var backRenderer = CreateCardSurface(
                rootObject.transform,
                "Card Back",
                new Vector3(0f, 0.001f, 0f),
                Quaternion.Euler(90f, 0f, 0f));
            backRenderer.sharedMaterial = _cardBackMaterial;
            var faceRenderer = CreateCardSurface(
                rootObject.transform,
                "Card Face",
                new Vector3(0f, -0.001f, 0f),
                Quaternion.Inverse(Quaternion.AngleAxis(180f, Vector3.forward)) *
                Quaternion.Euler(90f, 0f, 0f));
            faceRenderer.gameObject.SetActive(false);
            return new HiddenCardView(rootObject.transform, faceRenderer);
        }

        private void PrepareDealCard(
            AnimationPresentationState state,
            ResolvedAnimationStep step)
        {
            var recipient = FindPlayer(state, step.PlayerId);
            var handIndex = state.GetHandLayoutIndex(step.PlayerId, step.Cards[0]);
            PrepareDeckCardMotion(
                step.Cards[0],
                GetSeatCardPosition(
                    recipient.Seat,
                    1.02f,
                    handIndex,
                    Math.Max(3, state.GetHandLayoutSlotCount(step.PlayerId)),
                    0.19f,
                    0.84f),
                step.PlayerId == _actingPlayerId,
                recipient.Seat,
                $"Dealt Card to {step.PlayerId}");
        }

        private void PrepareOpeningCard(
            AnimationPresentationState state,
            ResolvedAnimationStep step)
        {
            PrepareDeckCardMotion(
                step.Cards[0],
                ResolveCardPosition(state, step.Cards[0]),
                true,
                FindPlayer(state, _actingPlayerId).Seat,
                $"Opening Card {step.Cards[0]}");
        }

        private void PrepareDeckCardMotion(
            Card card,
            Vector3 target,
            bool revealFace,
            Seat recipientSeat,
            string name)
        {
            if (_deckViews.Count == 0)
            {
                return;
            }

            _activeDealCard = _deckViews[_deckViews.Count - 1];
            _activeDealCard.Transform.gameObject.name = name;
            _activeDealStart = _activeDealCard.Transform.localPosition;
            _activeDealTarget = target;
            _dealRevealsFace = revealFace;
            _activeDealFaceUp = false;
            _dealFlipDirection = recipientSeat == Seat.First ? 1f : -1f;
            _dealFlipDegrees = 0f;
            _activeDealCard.Transform.localRotation = Quaternion.identity;
            _activeDealCard.FaceRenderer.gameObject.SetActive(_dealRevealsFace);
            if (_dealRevealsFace)
            {
                CardVisualMaterialBinding.Apply(
                    _activeDealCard.FaceRenderer,
                    _cardCatalog,
                    card);
            }
        }

        private void ApplyDealCard(float progress)
        {
            if (_activeDealCard == null)
            {
                return;
            }

            _activeDealCard.Transform.localPosition = AnimationBeatEvaluator.EvaluatePosition(
                _activeDealStart,
                _activeDealTarget,
                progress,
                _easing,
                _trajectoryOffset);
            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(progress, _easing);
            _dealFlipDegrees = _dealRevealsFace ? eased * 180f : 0f;
            _activeDealCard.Transform.localRotation = Quaternion.AngleAxis(
                _dealFlipDegrees * _dealFlipDirection,
                Vector3.forward);
            _activeDealFaceUp = _dealRevealsFace && progress >= 0.5f;
        }

        private void PrepareOpeningRejection(ResolvedAnimationStep step)
        {
            if (!_cardViews.TryGetValue(step.Cards[0], out var rejectedView))
            {
                return;
            }

            var rejected = (OpeningCardRejectedEvent)step.SourceEvent;
            var insertionIndex = Mathf.Clamp(
                rejected.ReinsertedDeckIndex,
                0,
                _deckViews.Count);
            rejectedView.gameObject.SetActive(false);
            _activeRejectedCard = CreateHiddenCard(
                $"Rejected Opening Card {step.Cards[0]}",
                rejectedView.localPosition);
            _activeRejectedCard.FaceRenderer.gameObject.SetActive(true);
            CardVisualMaterialBinding.Apply(
                _activeRejectedCard.FaceRenderer,
                _cardCatalog,
                step.Cards[0]);
            _activeRejectedCard.Transform.localRotation =
                Quaternion.AngleAxis(180f, Vector3.forward);
            _activeRejectionStart = rejectedView.localPosition;
            _activeRejectionTarget = ResolveDeckPosition(insertionIndex);
            _rejectionFlipDegrees = 180f;
            _activeRejectedCardFaceDown = false;
            _rejectionDeckGap = 0f;
            _rejectionDeckSplit.Clear();
            for (var index = insertionIndex; index < _deckViews.Count; index++)
            {
                _rejectionDeckSplit.Add(new DeckSplitMotion(
                    _deckViews[index].Transform,
                    _deckViews[index].Transform.localPosition));
            }
        }

        private void ApplyOpeningRejection(float progress)
        {
            if (_activeRejectedCard == null)
            {
                return;
            }

            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(progress, _easing);
            _activeRejectedCard.Transform.localPosition = AnimationBeatEvaluator.EvaluatePosition(
                _activeRejectionStart,
                _activeRejectionTarget,
                progress,
                _easing,
                _trajectoryOffset);
            _rejectionFlipDegrees = 180f + eased * 180f;
            _activeRejectedCard.Transform.localRotation = Quaternion.AngleAxis(
                _rejectionFlipDegrees,
                Vector3.forward);
            _activeRejectedCardFaceDown = progress >= 0.5f;

            _rejectionDeckGap = Mathf.Sin(progress * Mathf.PI);
            var splitOffset = new Vector3(0.055f, 0.035f, 0f) * _rejectionDeckGap;
            foreach (var motion in _rejectionDeckSplit)
            {
                motion.Transform.localPosition = motion.Start + splitOffset;
            }
        }

        private void PrepareNormalCapture(
            AnimationPresentationState state,
            ResolvedAnimationStep step)
        {
            _capturePairMotions.Clear();
            if (!_cardViews.TryGetValue(step.Cards[0], out var playedSource) ||
                !_cardViews.TryGetValue(step.Cards[1], out var matchingSource))
            {
                return;
            }

            var player = FindPlayer(state, step.PlayerId);
            var capturedBaseIndex = Math.Max(0, state.GetCaptured(player.Id).Count - 2);
            var capturedEvent = step.SourceEvent as CardsCapturedEvent;
            _captureContinuesToCascade =
                capturedEvent != null &&
                capturedEvent.Cards.Count > 2;
            for (var index = 0; index < 2; index++)
            {
                var card = step.Cards[index];
                var source = index == 0 ? playedSource : matchingSource;
                source.gameObject.SetActive(false);
                var moving = CreateHiddenCard($"Captured Pair Card {card}", source.localPosition);
                moving.FaceRenderer.gameObject.SetActive(true);
                CardVisualMaterialBinding.Apply(moving.FaceRenderer, _cardCatalog, card);
                moving.Transform.localRotation = Quaternion.AngleAxis(180f, Vector3.forward);
                _capturePairMotions.Add(new CapturePairMotion(
                    moving,
                    source.localPosition,
                    matchingSource.localPosition + Vector3.up * (index == 0 ? 0.012f : 0f),
                    _captureContinuesToCascade
                        ? matchingSource.localPosition + Vector3.up * (index == 0 ? 0.012f : 0f)
                        : ResolveCapturedPilePosition(
                            player.Seat,
                            capturedBaseIndex + index),
                    index == 0));
            }

            _capturePairFlipDegrees = 180f;
            _capturePairFaceDown = false;
        }

        private void ApplyNormalCapture(float progress)
        {
            if (_capturePairMotions.Count == 0)
            {
                return;
            }

            var isCapturing = progress >= CapturePickupStartProgress;
            var playProgress = Mathf.InverseLerp(0f, CapturePlayEndProgress, progress);
            var captureProgress = Mathf.InverseLerp(
                CapturePickupStartProgress,
                1f,
                progress);
            var captureEased = AnimationBeatEvaluator.EvaluateEasedProgress(
                captureProgress,
                _easing);
            _capturePairFlipDegrees = _captureContinuesToCascade
                ? 180f
                : 180f + captureEased * 180f;
            foreach (var motion in _capturePairMotions)
            {
                if (!isCapturing)
                {
                    motion.View.Transform.localPosition = motion.IsPlayedCard
                        ? AnimationBeatEvaluator.EvaluatePosition(
                            motion.Start,
                            motion.StackStart,
                            playProgress,
                            _easing,
                            _trajectoryOffset * 0.55f)
                        : motion.StackStart;
                }
                else
                {
                    motion.View.Transform.localPosition = _captureContinuesToCascade
                        ? motion.StackStart
                        : AnimationBeatEvaluator.EvaluatePosition(
                            motion.StackStart,
                            motion.Target,
                            captureProgress,
                            _easing,
                            _trajectoryOffset);
                }

                motion.View.Transform.localRotation = Quaternion.AngleAxis(
                    _capturePairFlipDegrees,
                    Vector3.forward);
            }

            _capturePairFaceDown =
                !_captureContinuesToCascade &&
                isCapturing &&
                captureProgress >= 0.5f;
        }

        private void PrepareCascadeCapture(
            AnimationPresentationState state,
            ResolvedAnimationStep step)
        {
            _cascadeStackMotions.Clear();
            var capturedEvent = (CardsCapturedEvent)step.SourceEvent;
            var isCollectionStep = step.Cards.Count > 1;
            var currentIndex = isCollectionStep
                ? capturedEvent.Cards.Count - 1
                : IndexOf(capturedEvent.Cards, step.Cards[0]);
            if (currentIndex < 2)
            {
                return;
            }

            foreach (var capturedView in _capturedPileViews)
            {
                capturedView.Transform.gameObject.SetActive(false);
            }

            Transform currentSource = null;
            if (!isCollectionStep &&
                !_cardViews.TryGetValue(capturedEvent.Cards[currentIndex], out currentSource))
            {
                return;
            }

            if (currentSource != null)
            {
                currentSource.gameObject.SetActive(false);
            }

            _cascadeStackCompletesCapture = isCollectionStep;
            var player = FindPlayer(state, step.PlayerId);
            var existingCapturedCount = Math.Max(
                0,
                state.GetCaptured(player.Id).Count - capturedEvent.Cards.Count);
            var sourceBase = ResolveTablePosition(
                isCollectionStep ? capturedEvent.Cards.Count - 2 : currentIndex - 2);
            var targetBase = isCollectionStep
                ? Vector3.zero
                : ResolveTablePosition(currentIndex - 1);
            for (var cardIndex = 0; cardIndex <= currentIndex; cardIndex++)
            {
                var card = capturedEvent.Cards[cardIndex];
                var start = sourceBase +
                    Vector3.up * ((currentIndex - cardIndex) * 0.012f);
                var target = _cascadeStackCompletesCapture
                    ? ResolveCapturedPilePosition(
                        player.Seat,
                        existingCapturedCount + cardIndex)
                    : targetBase +
                        Vector3.up * ((currentIndex - cardIndex) * 0.012f);
                if (!isCollectionStep && cardIndex == currentIndex)
                {
                    start = target;
                }

                var moving = CreateHiddenCard($"Cascade Stack Card {card}", start);
                moving.FaceRenderer.gameObject.SetActive(true);
                CardVisualMaterialBinding.Apply(moving.FaceRenderer, _cardCatalog, card);
                moving.Transform.localRotation = Quaternion.AngleAxis(180f, Vector3.forward);
                _cascadeStackMotions.Add(new CascadeStackMotion(
                    moving,
                    start,
                    target,
                    !isCollectionStep && cardIndex == currentIndex));
            }

            _cascadeStackFlipDegrees = 180f;
            _cascadeStackFaceDown = false;
        }

        private void ApplyCascadeCapture(float progress)
        {
            if (_cascadeStackMotions.Count == 0)
            {
                return;
            }

            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(progress, _easing);
            _cascadeStackFlipDegrees = _cascadeStackCompletesCapture
                ? 180f + eased * 180f
                : 180f;
            foreach (var motion in _cascadeStackMotions)
            {
                motion.View.Transform.localPosition = motion.IsStationaryTarget
                    ? motion.Target
                    : AnimationBeatEvaluator.EvaluatePosition(
                        motion.Start,
                        motion.Target,
                        progress,
                        _easing,
                        _trajectoryOffset);
                motion.View.Transform.localRotation = Quaternion.AngleAxis(
                    _cascadeStackFlipDegrees,
                    Vector3.forward);
            }

            _cascadeStackFaceDown =
                _cascadeStackCompletesCapture &&
                progress >= 0.5f;
        }

        private void PrepareLeftoverCollection(
            AnimationPresentationState state,
            ResolvedAnimationStep step)
        {
            _leftoverMotions.Clear();
            var player = FindPlayer(state, step.PlayerId);
            var existingCapturedCount = Math.Max(
                0,
                state.GetCaptured(player.Id).Count - step.Cards.Count);
            for (var index = 0; index < step.Cards.Count; index++)
            {
                var card = step.Cards[index];
                if (!_cardViews.TryGetValue(card, out var source))
                {
                    continue;
                }

                source.gameObject.SetActive(false);
                var moving = CreateHiddenCard(
                    $"Leftover Collection Card {card}",
                    source.localPosition);
                moving.FaceRenderer.gameObject.SetActive(true);
                CardVisualMaterialBinding.Apply(moving.FaceRenderer, _cardCatalog, card);
                moving.Transform.localRotation = Quaternion.AngleAxis(180f, Vector3.forward);
                _leftoverMotions.Add(new CollectedCardMotion(
                    moving,
                    source.localPosition,
                    ResolveCapturedPilePosition(
                        player.Seat,
                        existingCapturedCount + index)));
            }

            _leftoverFlipDegrees = 180f;
            _leftoversFaceDown = false;
        }

        private void ApplyLeftoverCollection(float progress)
        {
            if (_leftoverMotions.Count == 0)
            {
                return;
            }

            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(progress, _easing);
            _leftoverFlipDegrees = 180f + eased * 180f;
            foreach (var motion in _leftoverMotions)
            {
                motion.View.Transform.localPosition = AnimationBeatEvaluator.EvaluatePosition(
                    motion.Start,
                    motion.Target,
                    progress,
                    _easing,
                    _trajectoryOffset);
                motion.View.Transform.localRotation = Quaternion.AngleAxis(
                    _leftoverFlipDegrees,
                    Vector3.forward);
            }

            _leftoversFaceDown = progress >= 0.5f;
        }

        private void PrepareDealerCardFlip(
            AnimationPresentationState state,
            ResolvedAnimationStep step)
        {
            var selectionIndex = IndexOf(state.DealerSelectionCards, step.Cards[0]);
            var totalCount = state.DealerSpreadCount + state.DealerSelectionCards.Count;
            var selectedSlot = GetDealerSelectionSlot(totalCount, Math.Max(0, selectionIndex));
            _activeDealerCard = FindDealerSpreadView(selectedSlot);
            if (_activeDealerCard == null)
            {
                return;
            }

            _activeDealerCard.Transform.gameObject.name = $"Selected Dealer Card {step.Cards[0]}";
            _activeDealerCard.FaceRenderer.gameObject.SetActive(true);
            CardVisualMaterialBinding.Apply(_activeDealerCard.FaceRenderer, _cardCatalog, step.Cards[0]);
            _activeDealerCard.IsFaceUp = false;
            _activeDealerCardStart = ResolveDealerSpreadPosition(selectedSlot);
            _activeDealerCard.Transform.localPosition = _activeDealerCardStart;
            _activeDealerCard.Transform.localRotation = Quaternion.identity;
            _dealerFlipDirection = FindPlayer(state, step.PlayerId).Seat == Seat.First ? 1f : -1f;
            _dealerFlipLift = DealerFlipLift + Mathf.Max(0f, _trajectoryOffset.y);
            _dealerFlipDegrees = 0f;
        }

        private void ApplyDealerCardFlip(float progress)
        {
            if (_activeDealerCard == null)
            {
                return;
            }

            var eased = AnimationBeatEvaluator.EvaluateEasedProgress(progress, _easing);
            _dealerFlipDegrees = eased * 180f;
            _activeDealerCard.Transform.localRotation =
                Quaternion.AngleAxis(_dealerFlipDegrees * _dealerFlipDirection, Vector3.forward);
            _activeDealerCard.Transform.localPosition =
                _activeDealerCardStart +
                Vector3.up * (
                    Mathf.Sin(progress * Mathf.PI) * _dealerFlipLift +
                    eased * DealerSelectedRestHeight);
            _activeDealerCard.IsFaceUp = progress >= 0.5f;
        }

        private DealerSpreadCardView FindDealerSpreadView(int slot)
        {
            foreach (var view in _dealerSpreadViews)
            {
                if (view.Slot == slot)
                {
                    return view;
                }
            }

            return null;
        }

        private void EnsureCardBackMaterial()
        {
            if (_cardBackMaterial != null)
            {
                return;
            }

            if (_cardCatalog.SharedFaceMaterial == null || _cardCatalog.BackTexture == null)
            {
                throw new MissingReferenceException(
                    "The animation workbench is missing the shared card material or card-back texture.");
            }

            _cardBackMaterial = new Material(_cardCatalog.SharedFaceMaterial)
            {
                name = "Animation Lab Card Back",
                hideFlags = HideFlags.DontSave,
            };
            _cardBackMaterial.SetTexture("_BaseMap", _cardCatalog.BackTexture);
            _cardBackMaterial.SetTexture("_MainTex", _cardCatalog.BackTexture);
            _cardBackMaterial.SetColor("_BaseColor", Color.white);
            _cardBackMaterial.SetColor("_Color", Color.white);
            _ownedMaterials.Add(_cardBackMaterial);
        }

        private static int GetDealerSelectionSlot(int totalCount, int selectionIndex)
        {
            var firstSlot = Math.Max(0, (totalCount - 1) / 2);
            return Mathf.Clamp(firstSlot + selectionIndex, 0, Math.Max(0, totalCount - 1));
        }

        private static Vector3 ResolveDealerSpreadPosition(int slot)
        {
            var row = slot / DealerSpreadColumns;
            var column = slot % DealerSpreadColumns;
            return new Vector3(
                (column - 3.5f) * 0.17f,
                0.845f + row * 0.002f,
                (row - 2f) * 0.21f);
        }

        private static Vector3 ResolveDeckPosition(int index)
        {
            return new Vector3(0.72f, 0.845f + index * 0.0012f, 0.24f);
        }

        private Vector3 ResolveCapturedPilePosition(Seat seat, int stackIndex)
        {
            var angle = GetSeatAngle(seat);
            var radians = angle * Mathf.Deg2Rad;
            var playerLeft = new Vector3(-Mathf.Cos(radians), 0f, -Mathf.Sin(radians));
            return TableCompositionLayout.PositionAt(
                    angle,
                    CapturedPileRadius,
                    0.84f + stackIndex * 0.006f) +
                playerLeft * CapturedPileLeftOffset;
        }

        private void CreateStage()
        {
            CreatePrimitive(
                "Animation Lab Ground",
                PrimitiveType.Cube,
                new Vector3(0f, -0.08f, 0.15f),
                new Vector3(5.2f, 0.12f, 5.0f),
                Lampblack);
            var table = UnityEngine.Object.Instantiate(_tablePrefab, _generatedRoot, false);
            table.name = "Approved V0 Round Card Table";
            table.hideFlags = HideFlags.DontSave;
        }

        private void CreateSeats(AnimationPresentationState state)
        {
            foreach (var player in state.Players)
            {
                var angle = GetSeatAngle(player.Seat);
                var seatPosition = TableCompositionLayout.PositionAt(angle, CurrentProfile.SeatRadiusMetres);
                var body = CreatePrimitive(
                    $"{player.DisplayName} Presentation Placeholder",
                    PrimitiveType.Capsule,
                    seatPosition + new Vector3(0f, 0.65f, 0f),
                    new Vector3(0.38f, 0.42f, 0.28f),
                    player.TeamId == TeamId.One ? Moss : Woad);
                body.transform.localRotation = Quaternion.LookRotation(-seatPosition.normalized, Vector3.up);

                var labelObject = new GameObject($"{player.DisplayName} Score", typeof(TextMeshPro));
                labelObject.hideFlags = HideFlags.DontSave;
                labelObject.transform.SetParent(_generatedRoot, false);
                labelObject.transform.localPosition = seatPosition + new Vector3(0f, 1.38f, 0f);
                FaceCamera(labelObject.transform);
                labelObject.transform.localScale = Vector3.one * 0.18f;
                var label = labelObject.GetComponent<TextMeshPro>();
                label.alignment = TextAlignmentOptions.Center;
                label.fontSize = 3.5f;
                label.fontStyle = FontStyles.Bold;
                label.color = Vellum;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.rectTransform.sizeDelta = new Vector2(6f, 1.6f);
                _scoreLabels[player.TeamId] = label;
            }
        }

        private void CreateEventCue()
        {
            var cueObject = new GameObject("Resolved Event Cue", typeof(TextMeshPro));
            cueObject.hideFlags = HideFlags.DontSave;
            cueObject.transform.SetParent(_generatedRoot, false);
            cueObject.transform.localPosition = new Vector3(0f, 1.34f, 0f);
            FaceCamera(cueObject.transform);
            cueObject.transform.localScale = Vector3.one * 0.18f;
            _eventCue = cueObject.GetComponent<TextMeshPro>();
            _eventCue.alignment = TextAlignmentOptions.Center;
            _eventCue.fontSize = 4.2f;
            _eventCue.fontStyle = FontStyles.Bold;
            _eventCue.color = Brass;
            _eventCue.textWrappingMode = TextWrappingModes.NoWrap;
            _eventCue.rectTransform.sizeDelta = new Vector2(8f, 1.6f);
        }

        private void EnsureCardViews(AnimationPresentationState state)
        {
            var visibleCards = new HashSet<Card>(state.Table);
            foreach (var player in state.Players)
            {
                if (player.Id == _actingPlayerId)
                {
                    visibleCards.UnionWith(state.GetHand(player.Id));
                }
            }

            foreach (var card in visibleCards)
            {
                EnsureCardView(card);
            }
        }

        private void EnsureCardView(Card card)
        {
            if (_cardViews.ContainsKey(card))
            {
                return;
            }

            var cardObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cardObject.name = card.ToString();
            cardObject.hideFlags = HideFlags.DontSave;
            cardObject.transform.SetParent(_generatedRoot, false);
            cardObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cardObject.transform.localScale = new Vector3(0.24f, 0.34f, 1f);
            DestroyObject(cardObject.GetComponent<Collider>());
            CardVisualMaterialBinding.Apply(cardObject.GetComponent<Renderer>(), _cardCatalog, card);
            _cardViews[card] = cardObject.transform;
        }

        private Vector3 ResolveCardPosition(AnimationPresentationState state, Card card)
        {
            var tableIndex = IndexOf(state.Table, card);
            if (tableIndex >= 0)
            {
                for (var index = 0; index < tableIndex; index++)
                {
                    if (state.Table[index].Rank == card.Rank)
                    {
                        return ResolveTablePosition(
                            state.GetTableLayoutIndex(state.Table[index])) +
                            Vector3.up * 0.012f;
                    }
                }

                return ResolveTablePosition(state.GetTableLayoutIndex(card));
            }

            foreach (var player in state.Players)
            {
                var handIndex = IndexOf(state.GetHand(player.Id), card);
                if (player.Id == _actingPlayerId && handIndex >= 0)
                {
                    return GetSeatCardPosition(
                        player.Seat,
                        1.02f,
                        state.GetHandLayoutIndex(player.Id, card),
                        Math.Max(3, state.GetHandLayoutSlotCount(player.Id)),
                        0.19f,
                        0.84f);
                }

                var capturedIndex = IndexOf(state.GetCaptured(player.Id), card);
                if (capturedIndex >= 0)
                {
                    return GetSeatCardPosition(
                        player.Seat,
                        0.78f,
                        capturedIndex,
                        Math.Max(1, state.GetCaptured(player.Id).Count),
                        0.025f,
                        0.84f + capturedIndex * 0.006f);
                }
            }

            return new Vector3(0f, -5f, 0f);
        }

        private Vector3 ResolveSyntheticSource(
            ResolvedAnimationStep step,
            AnimationPresentationState state,
            Card card)
        {
            switch (step.Kind)
            {
                case ResolvedAnimationStepKind.DealerSelection:
                    return new Vector3(-0.24f, 0.84f, 0.04f);
                case ResolvedAnimationStepKind.Deal:
                case ResolvedAnimationStepKind.OpeningPlacement:
                    return new Vector3(0.72f, 0.86f, 0.24f);
                case ResolvedAnimationStepKind.OpeningRejection:
                    return new Vector3(0f, 0.86f, 0.10f);
                default:
                    return ResolveCardPosition(state, card);
            }
        }

        private Vector3 ResolveTransitionTarget(
            AnimationPresentationState state,
            ResolvedAnimationStep step,
            Card card)
        {
            if (step.Kind == ResolvedAnimationStepKind.DealerSelection && Contains(step.Cards, card))
            {
                var actor = FindPlayer(state, step.PlayerId);
                return GetSeatCardPosition(actor.Seat, 0.72f, 0, 1, 0f, 0.90f);
            }

            if (step.Kind == ResolvedAnimationStepKind.OpeningRejection && Contains(step.Cards, card))
            {
                return new Vector3(0.72f, 0.86f, 0.24f);
            }

            if (step.Kind == ResolvedAnimationStepKind.CardPlay &&
                step.Cards.Count >= 2 &&
                card == step.Cards[0])
            {
                return ResolveCardPosition(state, step.Cards[1]) + Vector3.up * 0.012f;
            }

            return ResolveCardPosition(state, card);
        }

        private static Vector3 ResolveTablePosition(int index)
        {
            var tablePositions = new[]
            {
                new Vector3(-0.23f, 0.82f, 0.08f),
                new Vector3(0f, 0.825f, 0.12f),
                new Vector3(0.23f, 0.83f, 0.08f),
                new Vector3(-0.12f, 0.835f, -0.16f),
                new Vector3(0.12f, 0.84f, -0.16f),
            };
            return tablePositions[Mathf.Min(index, tablePositions.Length - 1)];
        }

        private void RefreshLabels(AnimationPresentationState state)
        {
            foreach (var player in state.Players)
            {
                if (!_scoreLabels.TryGetValue(player.TeamId, out var label))
                {
                    continue;
                }

                var active = state.CurrentSeat == player.Seat;
                label.text = $"{(active ? "> " : string.Empty)}{player.DisplayName}  T{(int)player.TeamId + 1} {state.GetScore(player.TeamId).Value:00}";
                label.color = active ? Brass : Vellum;
            }
        }

        private void SetEventCue(ResolvedAnimationStep step)
        {
            if (_eventCue == null)
            {
                return;
            }

            switch (step.Kind)
            {
                case ResolvedAnimationStepKind.MatchStarted:
                    _eventCue.text = "MATCH START";
                    break;
                case ResolvedAnimationStepKind.DealerSelection:
                    _eventCue.text = "DEALER SELECTION";
                    break;
                case ResolvedAnimationStepKind.DealerChoice:
                    _eventCue.text = "DEALER CHOICE";
                    break;
                case ResolvedAnimationStepKind.Deal:
                    _eventCue.text = "DEAL";
                    break;
                case ResolvedAnimationStepKind.OpeningRejection:
                    _eventCue.text = "OPENING REJECTED";
                    break;
                case ResolvedAnimationStepKind.OpeningPlacement:
                    _eventCue.text = "OPENING TABLE";
                    break;
                case ResolvedAnimationStepKind.CardPlay:
                    _eventCue.text = "PLAY";
                    break;
                case ResolvedAnimationStepKind.HandReflow:
                    _eventCue.text = "HAND REFLOW";
                    break;
                case ResolvedAnimationStepKind.TablePlacement:
                    _eventCue.text = "TABLE";
                    break;
                case ResolvedAnimationStepKind.NormalCapture:
                    _eventCue.text = "CAPTURE";
                    break;
                case ResolvedAnimationStepKind.CascadeCapture:
                    _eventCue.text = "CASCADE";
                    break;
                case ResolvedAnimationStepKind.CaptureCollection:
                    _eventCue.text = "COLLECT CAPTURE";
                    break;
                case ResolvedAnimationStepKind.FallScore:
                    _eventCue.text = $"FALL  +{step.PointsAwarded}";
                    break;
                case ResolvedAnimationStepKind.CleanTableScore:
                    _eventCue.text = $"CLEAN TABLE  +{step.PointsAwarded}";
                    break;
                case ResolvedAnimationStepKind.Canto:
                    _eventCue.text = "CANTO";
                    break;
                case ResolvedAnimationStepKind.Score:
                    _eventCue.text = $"SCORE  {step.PointsAwarded:+#;-#;0}";
                    break;
                case ResolvedAnimationStepKind.DealCompleted:
                    _eventCue.text = "DEAL COMPLETE";
                    break;
                case ResolvedAnimationStepKind.Leftovers:
                    _eventCue.text = "COLLECT LEFTOVERS";
                    break;
                case ResolvedAnimationStepKind.Round:
                    _eventCue.text = "ROUND COMPLETE";
                    break;
                case ResolvedAnimationStepKind.DealerRotation:
                    _eventCue.text = "DEALER ROTATES";
                    break;
                case ResolvedAnimationStepKind.TieExtension:
                    _eventCue.text = "TIE EXTENSION";
                    break;
                case ResolvedAnimationStepKind.TurnChanged:
                    _eventCue.text = "NEXT TURN";
                    break;
                case ResolvedAnimationStepKind.MatchCompleted:
                    _eventCue.text = "MATCH RESOLVED";
                    break;
                case ResolvedAnimationStepKind.SynchronizeFinalState:
                    _eventCue.text = "RESOLVED";
                    break;
            }
        }

        private GameObject CreatePrimitive(
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.hideFlags = HideFlags.DontSave;
            primitive.transform.SetParent(_generatedRoot, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            DestroyObject(primitive.GetComponent<Collider>());

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = $"{name} Animation Lab Material",
                hideFlags = HideFlags.DontSave,
                color = color,
            };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            material.SetFloat("_Smoothness", 0.12f);
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            _ownedMaterials.Add(material);
            return primitive;
        }

        private Vector3 GetSeatCardPosition(
            Seat seat,
            float radius,
            int index,
            int count,
            float spacing,
            float height)
        {
            var angle = GetSeatAngle(seat);
            var radians = angle * Mathf.Deg2Rad;
            var basePosition = TableCompositionLayout.PositionAt(angle, radius, height);
            var tangent = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians));
            return basePosition + tangent * ((index - (count - 1) * 0.5f) * spacing);
        }

        private static Player FindPlayer(AnimationPresentationState state, PlayerId playerId)
        {
            foreach (var player in state.Players)
            {
                if (player.Id == playerId)
                {
                    return player;
                }
            }

            throw new InvalidOperationException($"The animation state has no player {playerId}.");
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

        private void FaceCamera(Transform target)
        {
            var awayFromCamera = target.position - _camera.transform.position;
            target.rotation = Quaternion.LookRotation(awayFromCamera, _camera.transform.up);
        }

        private static float GetSeatAngle(Seat seat)
        {
            switch (seat)
            {
                case Seat.First:
                    return 0f;
                case Seat.Second:
                    return 180f;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(seat),
                        seat,
                        "The animation recording currently supports the two 1v1 seats.");
            }
        }

        private static int IndexOf(IReadOnlyList<Card> cards, Card expected)
        {
            for (var index = 0; index < cards.Count; index++)
            {
                if (cards[index] == expected)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Color FromHex(int rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }

        private readonly struct CardMotion
        {
            public CardMotion(Transform transform, Vector3 start, Vector3 target)
            {
                Transform = transform;
                Start = start;
                Target = target;
            }

            public Transform Transform { get; }

            public Vector3 Start { get; }

            public Vector3 Target { get; }
        }

        private readonly struct DeckSplitMotion
        {
            public DeckSplitMotion(Transform transform, Vector3 start)
            {
                Transform = transform;
                Start = start;
            }

            public Transform Transform { get; }

            public Vector3 Start { get; }
        }

        private readonly struct CapturePairMotion
        {
            public CapturePairMotion(
                HiddenCardView view,
                Vector3 start,
                Vector3 stackStart,
                Vector3 target,
                bool isPlayedCard)
            {
                View = view;
                Start = start;
                StackStart = stackStart;
                Target = target;
                IsPlayedCard = isPlayedCard;
            }

            public HiddenCardView View { get; }

            public Vector3 Start { get; }

            public Vector3 StackStart { get; }

            public Vector3 Target { get; }

            public bool IsPlayedCard { get; }
        }

        private readonly struct CascadeStackMotion
        {
            public CascadeStackMotion(
                HiddenCardView view,
                Vector3 start,
                Vector3 target,
                bool isStationaryTarget)
            {
                View = view;
                Start = start;
                Target = target;
                IsStationaryTarget = isStationaryTarget;
            }

            public HiddenCardView View { get; }

            public Vector3 Start { get; }

            public Vector3 Target { get; }

            public bool IsStationaryTarget { get; }
        }

        private readonly struct CollectedCardMotion
        {
            public CollectedCardMotion(HiddenCardView view, Vector3 start, Vector3 target)
            {
                View = view;
                Start = start;
                Target = target;
            }

            public HiddenCardView View { get; }

            public Vector3 Start { get; }

            public Vector3 Target { get; }
        }

        private sealed class DealerSpreadCardView
        {
            public DealerSpreadCardView(
                Transform transform,
                Renderer backRenderer,
                Renderer faceRenderer,
                int slot,
                bool isFaceUp)
            {
                Transform = transform;
                BackRenderer = backRenderer;
                FaceRenderer = faceRenderer;
                Slot = slot;
                IsFaceUp = isFaceUp;
            }

            public Transform Transform { get; }

            public Renderer BackRenderer { get; }

            public Renderer FaceRenderer { get; }

            public int Slot { get; }

            public bool IsFaceUp { get; set; }
        }

        private sealed class HiddenCardView
        {
            public HiddenCardView(Transform transform, Renderer faceRenderer)
            {
                Transform = transform;
                FaceRenderer = faceRenderer;
            }

            public Transform Transform { get; }

            public Renderer FaceRenderer { get; }
        }
    }
}
