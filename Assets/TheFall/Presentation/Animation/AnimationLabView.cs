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

        private readonly Transform _owner;
        private readonly Camera _camera;
        private readonly GameObject _tablePrefab;
        private readonly CardVisualCatalog _cardCatalog;
        private readonly Dictionary<Card, Transform> _cardViews = new Dictionary<Card, Transform>();
        private readonly Dictionary<TeamId, TextMeshPro> _scoreLabels = new Dictionary<TeamId, TextMeshPro>();
        private readonly List<Material> _ownedMaterials = new List<Material>();
        private readonly List<CardMotion> _motions = new List<CardMotion>();
        private Transform _generatedRoot;
        private TextMeshPro _eventCue;
        private PlayerId _actingPlayerId;

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

        public int CardViewCount => _cardViews.Count;

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
            EnsureCardViews(state);
            RenderImmediate(state);
        }

        public void PrepareTransition(
            AnimationPresentationState state,
            ResolvedAnimationStep step)
        {
            EnsureCardViews(state);
            _motions.Clear();
            foreach (var entry in _cardViews)
            {
                _motions.Add(new CardMotion(
                    entry.Value,
                    entry.Value.localPosition,
                    ResolveCardPosition(state, entry.Key)));
            }

            RefreshLabels(state);
            SetEventCue(step);
        }

        public void ApplyTransition(float progress)
        {
            var eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
            foreach (var motion in _motions)
            {
                motion.Transform.localPosition = Vector3.LerpUnclamped(
                    motion.Start,
                    motion.Target,
                    eased);
            }
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
            _eventCue = null;

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

                visibleCards.UnionWith(state.GetCaptured(player.Id));
            }

            foreach (var card in visibleCards)
            {
                if (_cardViews.ContainsKey(card))
                {
                    continue;
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
        }

        private Vector3 ResolveCardPosition(AnimationPresentationState state, Card card)
        {
            var tableIndex = IndexOf(state.Table, card);
            if (tableIndex >= 0)
            {
                var tablePositions = new[]
                {
                    new Vector3(-0.23f, 0.82f, 0.08f),
                    new Vector3(0f, 0.825f, 0.12f),
                    new Vector3(0.23f, 0.83f, 0.08f),
                    new Vector3(-0.12f, 0.835f, -0.16f),
                    new Vector3(0.12f, 0.84f, -0.16f),
                };
                return tablePositions[Mathf.Min(tableIndex, tablePositions.Length - 1)];
            }

            foreach (var player in state.Players)
            {
                var handIndex = IndexOf(state.GetHand(player.Id), card);
                if (player.Id == _actingPlayerId && handIndex >= 0)
                {
                    return GetSeatCardPosition(player.Seat, 1.02f, handIndex, 0.19f, 0.84f);
                }

                var capturedIndex = IndexOf(state.GetCaptured(player.Id), card);
                if (capturedIndex >= 0)
                {
                    return GetSeatCardPosition(player.Seat, 0.78f, capturedIndex, 0.025f, 0.84f + capturedIndex * 0.006f);
                }
            }

            return new Vector3(0f, -5f, 0f);
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
                case ResolvedAnimationStepKind.CardPlay:
                    _eventCue.text = "PLAY";
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
                case ResolvedAnimationStepKind.FallScore:
                    _eventCue.text = $"FALL  +{step.PointsAwarded}";
                    break;
                case ResolvedAnimationStepKind.CleanTableScore:
                    _eventCue.text = $"CLEAN TABLE  +{step.PointsAwarded}";
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
            float spacing,
            float height)
        {
            var angle = GetSeatAngle(seat);
            var radians = angle * Mathf.Deg2Rad;
            var basePosition = TableCompositionLayout.PositionAt(angle, radius, height);
            var tangent = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians));
            return basePosition + tangent * ((index - 1) * spacing);
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
    }
}
