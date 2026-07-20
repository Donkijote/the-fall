using System;
using System.Collections.Generic;
using TMPro;
using TheFall.Presentation.Interaction;
using UnityEngine;

namespace TheFall.Presentation.Table
{
    /// <summary>
    /// Builds inexpensive presentation geometry for the stationary-camera table experiment.
    /// It owns no rules and keeps its representative match state unchanged during recomposition.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TableCompositionPrototype : MonoBehaviour
    {
        private static readonly Vector3 FixedCameraPosition = new Vector3(0f, 7.2f, -5.4f);
        private static readonly Quaternion FixedCameraRotation = Quaternion.Euler(52f, 0f, 0f);

        private static readonly Color Lampblack = FromHex(0x241A14);
        private static readonly Color CharredWalnut = FromHex(0x3B291F);
        private static readonly Color Walnut = FromHex(0x68452F);
        private static readonly Color Ochre = FromHex(0xA06F3C);
        private static readonly Color Vellum = FromHex(0xD8C493);
        private static readonly Color Moss = FromHex(0x6B7046);
        private static readonly Color Woad = FromHex(0x465C73);
        private static readonly Color Madder = FromHex(0x8D4238);
        private static readonly Color Brass = FromHex(0xB58B3E);

        [SerializeField]
        private TableSeatingMode _seatingMode = TableSeatingMode.TwoVersusTwo;

        [SerializeField]
        private Camera _gameplayCamera;

        [SerializeField]
        [Min(0)]
        private int _activeLogicalSeat;

        private readonly Dictionary<Color32, Material> _generatedMaterials = new Dictionary<Color32, Material>();
        private readonly List<PrototypeSeatView> _seatViews = new List<PrototypeSeatView>();
        private readonly List<PrototypeCardView> _localHandCardViews = new List<PrototypeCardView>();
        private Transform _generatedRoot;
        private Vector2Int _viewportSize;
        private Rect _safeAreaPixels;
        private int _presentationStateVersion = 1;
        private int _layoutRevision;

        public TableSeatingMode SeatingMode => _seatingMode;

        public int ActiveLogicalSeat => _activeLogicalSeat;

        public int PresentationStateVersion => _presentationStateVersion;

        public int LayoutRevision => _layoutRevision;

        public TableCompositionProfile CurrentProfile { get; private set; }

        public Rect NormalizedSafeArea { get; private set; } = new Rect(0f, 0f, 1f, 1f);

        public IReadOnlyList<PrototypeSeatView> SeatViews => _seatViews;

        public IReadOnlyList<PrototypeCardView> LocalHandCardViews => _localHandCardViews;

        public Camera GameplayCamera => _gameplayCamera;

        public event Action CompositionRebuilt;

        public static Vector3 CameraPosition => FixedCameraPosition;

        public static Quaternion CameraRotation => FixedCameraRotation;

        private void OnEnable()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            EnsureCameraReference();
            var viewport = GetRuntimeViewport();
            Recompose(viewport, GetRuntimeSafeArea(viewport));
        }

        private void Update()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            var viewport = GetRuntimeViewport();
            var safeArea = GetRuntimeSafeArea(viewport);
            if (viewport != _viewportSize || safeArea != _safeAreaPixels)
            {
                Recompose(viewport, safeArea);
            }
        }

        private void OnDisable()
        {
            DestroyGeneratedContent();
        }

        private void OnValidate()
        {
            var seatCount = TableCompositionLayout.GetSeats(_seatingMode).Count;
            _activeLogicalSeat = Mathf.Clamp(_activeLogicalSeat, 0, seatCount - 1);
        }

        public void SetSeatingMode(TableSeatingMode mode)
        {
            if (_seatingMode == mode)
            {
                return;
            }

            _seatingMode = mode;
            _activeLogicalSeat = 0;
            _presentationStateVersion++;
            RecomposeCurrentViewport();
        }

        public void SetActiveLogicalSeat(int logicalSeat)
        {
            var seatCount = TableCompositionLayout.GetSeats(_seatingMode).Count;
            if (logicalSeat < 0 || logicalSeat >= seatCount)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalSeat));
            }

            if (_activeLogicalSeat == logicalSeat)
            {
                return;
            }

            _activeLogicalSeat = logicalSeat;
            _presentationStateVersion++;
            RecomposeCurrentViewport();
        }

        public void ApplyViewportForTests(Vector2Int viewportSize, Rect safeAreaPixels)
        {
            Recompose(viewportSize, safeAreaPixels);
        }

#if UNITY_EDITOR
        public void ConfigureCamera(Camera gameplayCamera)
        {
            _gameplayCamera = gameplayCamera;
        }

        public void BuildEditorPreview(
            TableSeatingMode mode,
            Vector2Int viewportSize,
            Rect safeAreaPixels)
        {
            _seatingMode = mode;
            _activeLogicalSeat = 0;
            EnsureCameraReference();
            Recompose(viewportSize, safeAreaPixels);
        }

        public void ClearEditorPreview()
        {
            DestroyGeneratedContent();
        }
#endif

        private void RecomposeCurrentViewport()
        {
            var viewport = _viewportSize.x > 0 && _viewportSize.y > 0
                ? _viewportSize
                : GetRuntimeViewport();
            var safeArea = _safeAreaPixels.width > 0f && _safeAreaPixels.height > 0f
                ? _safeAreaPixels
                : GetRuntimeSafeArea(viewport);
            Recompose(viewport, safeArea);
        }

        private void Recompose(Vector2Int viewportSize, Rect safeAreaPixels)
        {
            if (viewportSize.x <= 0 || viewportSize.y <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(viewportSize));
            }

            _viewportSize = viewportSize;
            _safeAreaPixels = safeAreaPixels;
            CurrentProfile = TableCompositionLayout.ResolveProfile(viewportSize);
            NormalizedSafeArea = TableCompositionLayout.NormalizeSafeArea(viewportSize, safeAreaPixels);
            _layoutRevision++;

            BuildComposition();
        }

        private void BuildComposition()
        {
            DestroyGeneratedContent();
            EnsureCameraReference();

            var generated = new GameObject("Generated Table Composition");
            generated.hideFlags = HideFlags.DontSave;
            generated.transform.SetParent(transform, false);
            _generatedRoot = generated.transform;

            var safeWidthScale = NormalizedSafeArea.width;
            var safeHeightScale = NormalizedSafeArea.height;
            var safeScale = Mathf.Clamp(Mathf.Min(safeWidthScale, safeHeightScale), 0.78f, 1f);
            _generatedRoot.localScale = Vector3.one * (CurrentProfile.ContentScale * safeScale);

            var safeCenter = NormalizedSafeArea.center - new Vector2(0.5f, 0.5f);
            _generatedRoot.localPosition = new Vector3(safeCenter.x * 2.2f, 0f, safeCenter.y * 1.4f);

            CreateRoomStage(_generatedRoot);
            CreateTable(_generatedRoot);
            CreateCentralCards(_generatedRoot);

            foreach (var layout in TableCompositionLayout.GetSeats(_seatingMode))
            {
                _seatViews.Add(CreateSeat(layout));
            }

            CompositionRebuilt?.Invoke();
        }

        private void CreateRoomStage(Transform parent)
        {
            CreatePrimitive(
                "Quiet Room Ground",
                PrimitiveType.Cube,
                parent,
                new Vector3(0f, -0.08f, 0.2f),
                new Vector3(5.6f, 0.12f, 5.4f),
                Quaternion.identity,
                Lampblack);

            CreatePrimitive(
                "Warm Stage Pool",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(0f, 0f, 0f),
                new Vector3(2.25f, 0.04f, 2.25f),
                Quaternion.identity,
                CharredWalnut);
        }

        private void CreateTable(Transform parent)
        {
            CreatePrimitive(
                "Table Rim",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(0f, 0.7f, 0f),
                new Vector3(0.725f, 0.09f, 0.725f),
                Quaternion.identity,
                Walnut);

            CreatePrimitive(
                "Quiet Play Field",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(0f, 0.795f, 0f),
                new Vector3(0.49f, 0.015f, 0.49f),
                Quaternion.identity,
                CharredWalnut);

            CreatePrimitive(
                "Table Pedestal",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(0f, 0.32f, 0f),
                new Vector3(0.23f, 0.32f, 0.23f),
                Quaternion.identity,
                Walnut);
        }

        private void CreateCentralCards(Transform parent)
        {
            var positions = new[]
            {
                new Vector3(-0.23f, 0.83f, 0.14f),
                new Vector3(0.02f, 0.83f, 0.18f),
                new Vector3(-0.12f, 0.83f, -0.12f),
                new Vector3(0.2f, 0.83f, -0.08f),
            };

            for (var index = 0; index < positions.Length; index++)
            {
                CreateCard($"Table Card {index + 1}", parent, positions[index], index * 12f - 18f, true, index + 1);
            }
        }

        private PrototypeSeatView CreateSeat(PrototypeSeatLayout layout)
        {
            var seatObject = new GameObject($"Seat {layout.LogicalIndex + 1} - {layout.DisplayName}");
            seatObject.hideFlags = HideFlags.DontSave;
            seatObject.transform.SetParent(_generatedRoot, false);
            seatObject.transform.localPosition = TableCompositionLayout.PositionAt(
                layout.AnchorAngleDegrees,
                CurrentProfile.SeatRadiusMetres);
            seatObject.transform.localRotation = Quaternion.LookRotation(-seatObject.transform.localPosition.normalized, Vector3.up);

            var bodyColor = layout.TeamIndex == 0 ? Moss : Woad;
            CreateUpperBody(seatObject.transform, bodyColor, layout.LogicalIndex);

            var handAnchor = new GameObject(layout.IsHandPrivate ? "Private Hand" : "Local Readable Hand").transform;
            handAnchor.gameObject.hideFlags = HideFlags.DontSave;
            handAnchor.SetParent(seatObject.transform, false);
            handAnchor.localPosition = new Vector3(0f, 0f, 0.46f);
            CreateHand(handAnchor, !layout.IsHandPrivate);

            var capturedPile = new GameObject("Individually Owned Captured Pile").transform;
            capturedPile.gameObject.hideFlags = HideFlags.DontSave;
            capturedPile.SetParent(seatObject.transform, false);
            capturedPile.localPosition = new Vector3(0.42f, 0f, 0.26f);
            CreateCapturedPile(capturedPile);

            var isActive = layout.LogicalIndex == _activeLogicalSeat;
            if (isActive)
            {
                CreatePrimitive(
                    "Active Seat Shape Cue",
                    PrimitiveType.Cylinder,
                    seatObject.transform,
                    new Vector3(0f, 0.08f, -0.08f),
                    new Vector3(0.47f, 0.025f, 0.47f),
                    Quaternion.identity,
                    Brass);
            }

            CreateNameAndScore(layout, isActive, seatObject.transform);

            var view = seatObject.AddComponent<PrototypeSeatView>();
            view.Configure(layout, isActive, handAnchor, capturedPile);
            return view;
        }

        private void CreateUpperBody(Transform parent, Color bodyColor, int variation)
        {
            CreatePrimitive(
                "Upper Body",
                PrimitiveType.Capsule,
                parent,
                new Vector3(0f, 0.72f, -0.12f),
                new Vector3(0.4f + variation * 0.015f, 0.43f, 0.26f),
                Quaternion.identity,
                bodyColor);

            CreatePrimitive(
                "Head",
                PrimitiveType.Sphere,
                parent,
                new Vector3(0f, 1.25f, -0.08f),
                new Vector3(0.32f, 0.38f, 0.32f),
                Quaternion.identity,
                Ochre);

            CreatePrimitive(
                "Left Character Hand",
                PrimitiveType.Sphere,
                parent,
                new Vector3(-0.3f, 0.79f, 0.23f),
                new Vector3(0.12f, 0.08f, 0.16f),
                Quaternion.Euler(0f, 18f, 0f),
                Ochre);

            CreatePrimitive(
                "Right Character Hand",
                PrimitiveType.Sphere,
                parent,
                new Vector3(0.3f, 0.79f, 0.23f),
                new Vector3(0.12f, 0.08f, 0.16f),
                Quaternion.Euler(0f, -18f, 0f),
                Ochre);
        }

        private void CreateHand(Transform parent, bool faceUp)
        {
            for (var index = 0; index < 3; index++)
            {
                var x = (index - 1) * 0.13f;
                var card = CreateCard(
                    faceUp ? $"Visible Hand Card {index + 1}" : $"Hidden Hand Card {index + 1}",
                    parent,
                    new Vector3(x, 0.83f, Mathf.Abs(index - 1) * 0.02f),
                    (index - 1) * 9f,
                    faceUp,
                    index + 5,
                    isInteractive: faceUp);

                if (faceUp)
                {
                    var view = card.AddComponent<PrototypeCardView>();
                    view.Configure(index);
                    _localHandCardViews.Add(view);
                }
            }
        }

        private void CreateCapturedPile(Transform parent)
        {
            for (var index = 0; index < 3; index++)
            {
                CreateCard(
                    $"Captured Card {index + 1}",
                    parent,
                    new Vector3(index * 0.018f, 0.82f + index * 0.012f, index * 0.012f),
                    index * 3f,
                    false,
                    0,
                    0.78f);
            }
        }

        private GameObject CreateCard(
            string name,
            Transform parent,
            Vector3 position,
            float yawDegrees,
            bool faceUp,
            int pipCount,
            float scale = 1f,
            bool isInteractive = false)
        {
            var card = CreatePrimitive(
                name,
                PrimitiveType.Cube,
                parent,
                position,
                new Vector3(0.18f * scale, 0.012f, 0.26f * scale),
                Quaternion.Euler(0f, yawDegrees, 0f),
                faceUp ? Vellum : Woad,
                isInteractive);

            if (!faceUp)
            {
                CreatePrimitive(
                    "Back Direction-Neutral Mark",
                    PrimitiveType.Cube,
                    card.transform,
                    new Vector3(0f, 0.58f, 0f),
                    new Vector3(0.45f, 0.08f, 0.45f),
                    Quaternion.identity,
                    Lampblack);
                return card;
            }

            var visiblePips = Mathf.Clamp(pipCount, 1, 4);
            for (var pip = 0; pip < visiblePips; pip++)
            {
                var column = pip % 2 == 0 ? -0.23f : 0.23f;
                var row = pip < 2 ? 0.25f : -0.25f;
                CreatePrimitive(
                    $"Readable Pip {pip + 1}",
                    PrimitiveType.Cylinder,
                    card.transform,
                    new Vector3(column, 0.58f, row),
                    new Vector3(0.12f, 0.04f, 0.085f),
                    Quaternion.identity,
                    Madder);
            }

            return card;
        }

        private void CreateNameAndScore(PrototypeSeatLayout layout, bool isActive, Transform seat)
        {
            var labelObject = new GameObject("Readable Name Team And Score", typeof(TextMeshPro));
            labelObject.hideFlags = HideFlags.DontSave;
            labelObject.transform.SetParent(_generatedRoot, false);
            labelObject.transform.localPosition = seat.localPosition + new Vector3(0f, 1.66f, 0f);

            var awayFromCamera = _gameplayCamera != null
                ? labelObject.transform.position - _gameplayCamera.transform.position
                : new Vector3(0f, -5f, 5f);
            var cameraUp = _gameplayCamera != null ? _gameplayCamera.transform.up : Vector3.up;
            labelObject.transform.rotation = Quaternion.LookRotation(awayFromCamera, cameraUp);
            labelObject.transform.localScale = Vector3.one * 0.2f;

            var score = layout.TeamIndex == 0 ? 12 : 9;
            var label = labelObject.GetComponent<TextMeshPro>();
            label.text = $"{(isActive ? "▶ " : string.Empty)}{layout.DisplayName}\nT{layout.TeamIndex + 1}  ◆ {score:00}";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 3.8f;
            label.fontStyle = FontStyles.Bold;
            label.color = isActive ? Brass : Vellum;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.rectTransform.sizeDelta = new Vector2(6f, 2.2f);
        }

        private GameObject CreatePrimitive(
            string name,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Color color,
            bool keepCollider = false)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.hideFlags = HideFlags.DontSave;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = localRotation;
            primitive.transform.localScale = localScale;

            var collider = primitive.GetComponent<Collider>();
            if (!keepCollider)
            {
                DestroyGeneratedObject(collider);
            }

            var renderer = primitive.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateMaterial(name, color);
            return primitive;
        }

        private Material CreateMaterial(string name, Color color)
        {
            var colorKey = (Color32)color;
            if (_generatedMaterials.TryGetValue(colorKey, out var existingMaterial))
            {
                return existingMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                throw new InvalidOperationException("No compatible prototype shader is available.");
            }

            var material = new Material(shader)
            {
                name = $"{name} Prototype Material",
                hideFlags = HideFlags.DontSave,
                color = color,
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            material.SetFloat("_Smoothness", 0.12f);
            _generatedMaterials.Add(colorKey, material);
            return material;
        }

        private void EnsureCameraReference()
        {
            if (_gameplayCamera == null)
            {
                _gameplayCamera = transform.root.GetComponentInChildren<Camera>(true);
            }
        }

        private void DestroyGeneratedContent()
        {
            _seatViews.Clear();
            _localHandCardViews.Clear();

            if (_generatedRoot != null)
            {
                _generatedRoot.gameObject.SetActive(false);
                DestroyGeneratedObject(_generatedRoot.gameObject);
                _generatedRoot = null;
            }

            foreach (var material in _generatedMaterials.Values)
            {
                DestroyGeneratedObject(material);
            }

            _generatedMaterials.Clear();
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

        private static Vector2Int GetRuntimeViewport()
        {
            return Screen.width >= 64 && Screen.height >= 64
                ? new Vector2Int(Screen.width, Screen.height)
                : new Vector2Int(1920, 1080);
        }

        private static Rect GetRuntimeSafeArea(Vector2Int viewport)
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
