using System;
using System.Collections.Generic;
using TheFall.Application.Input;
using TheFall.Application.Interaction;
using TheFall.Domain;
using TheFall.Presentation.Input;
using TheFall.Presentation.Table;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFall.Presentation.Interaction
{
    /// <summary>
    /// Runs the representative 1v1 interaction slice and binds shared application state to
    /// touch-, mouse-, and keyboard-capable prototype card views.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TableCompositionPrototype))]
    public sealed class CardInteractionPrototype : MonoBehaviour
    {
        [SerializeField]
        private TableCompositionPrototype _tableComposition;

        private RepresentativeCardTurn _representativeTurn;
        private CardInteractionInputAdapter _inputAdapter;
        private InputIntentSource _inputIntentSource;
        private InputAction _pointAction;
        private InputAction _navigateAction;
        private InputAction _inspectAction;
        private InputAction _selectAction;
        private InputAction _confirmAction;
        private InputAction _cancelAction;
        private Vector2 _pointerPosition;

        public CardInteractionSession Interaction => _representativeTurn?.Interaction;

        public IReadOnlyList<Card> LocalHand => _representativeTurn?.LocalHand ?? Array.Empty<Card>();

        public CardInteractionInputAdapter InputAdapter => _inputAdapter;

        public TableCompositionPrototype TableComposition => _tableComposition;

        private void OnEnable()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            InitializeRepresentativeTurn();
            BindTableComposition();
            BindInputActions();
            RefreshViews();
        }

        private void OnDisable()
        {
            UnbindInputActions();
            if (_tableComposition != null)
            {
                _tableComposition.CompositionRebuilt -= RefreshViews;
            }

            if (_inputAdapter != null)
            {
                _inputAdapter.ResultProduced -= OnResultProduced;
            }
        }

        public void ResetRepresentativeTurnForTests()
        {
            InitializeRepresentativeTurn();
            RefreshViews();
        }

        public void SetTemporarilyBlockedForTests(bool isBlocked)
        {
            Interaction.SetTemporarilyBlocked(isBlocked);
            RefreshViews();
        }

#if UNITY_EDITOR
        public void ConfigureTableComposition(TableCompositionPrototype tableComposition)
        {
            _tableComposition = tableComposition;
        }
#endif

        private void InitializeRepresentativeTurn()
        {
            if (_inputAdapter != null)
            {
                _inputAdapter.ResultProduced -= OnResultProduced;
            }

            _representativeTurn = RepresentativeCardTurn.Create();
            _inputAdapter = new CardInteractionInputAdapter(_representativeTurn.Interaction);
            _inputAdapter.SetCards(_representativeTurn.LocalHand);
            _inputAdapter.ResultProduced += OnResultProduced;
        }

        private void BindTableComposition()
        {
            if (_tableComposition == null)
            {
                _tableComposition = GetComponent<TableCompositionPrototype>();
            }

            _tableComposition.CompositionRebuilt -= RefreshViews;
            _tableComposition.CompositionRebuilt += RefreshViews;
        }

        private void BindInputActions()
        {
            _inputIntentSource = FindAnyObjectByType<InputIntentSource>();
            _pointAction = ResolveInputAction(PlayerIntentKind.Point);
            _navigateAction = ResolveInputAction(PlayerIntentKind.Navigate);
            _inspectAction = ResolveInputAction(PlayerIntentKind.Inspect);
            _selectAction = ResolveInputAction(PlayerIntentKind.Select);
            _confirmAction = ResolveInputAction(PlayerIntentKind.Confirm);
            _cancelAction = ResolveInputAction(PlayerIntentKind.Cancel);
            _pointAction.actionMap.Enable();

            _pointAction.performed += OnPoint;
            _navigateAction.performed += OnNavigate;
            _inspectAction.performed += OnInspect;
            _selectAction.performed += OnSelect;
            _confirmAction.performed += OnConfirm;
            _cancelAction.performed += OnCancel;
        }

        private InputAction ResolveInputAction(PlayerIntentKind intent)
        {
            if (_inputIntentSource != null)
            {
                return _inputIntentSource.GetAction(intent);
            }

            var actions = InputSystem.actions;
            if (actions == null)
            {
                throw new MissingReferenceException("The project-wide The Fall input actions are not configured.");
            }

            return actions.FindAction($"Gameplay/{intent}", true);
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
            var direction = context.ReadValue<Vector2>();
            _inputAdapter.Navigate(Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? Math.Sign(direction.x)
                : -Math.Sign(direction.y));
            RefreshViews();
        }

        private void OnInspect(InputAction.CallbackContext context)
        {
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

                return;
            }

            _inputAdapter.KeyboardInspect();
        }

        private void OnSelect(InputAction.CallbackContext context)
        {
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

                return;
            }

            _inputAdapter.KeyboardSelect();
        }

        private void OnConfirm(InputAction.CallbackContext context)
        {
            _inputAdapter.KeyboardConfirm();
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            _inputAdapter.Cancel();
        }

        private bool TryGetPointedCard(out Card card)
        {
            var camera = _tableComposition != null ? _tableComposition.GameplayCamera : Camera.main;
            if (camera != null && Physics.Raycast(camera.ScreenPointToRay(_pointerPosition), out var hit))
            {
                var view = hit.collider.GetComponent<PrototypeCardView>();
                if (view != null && view.HandIndex >= 0 && view.HandIndex < LocalHand.Count)
                {
                    card = LocalHand[view.HandIndex];
                    return true;
                }
            }

            card = default;
            return false;
        }

        private void OnResultProduced(CardInteractionResult result)
        {
            RefreshViews();
        }

        private void RefreshViews()
        {
            if (_tableComposition == null || Interaction == null)
            {
                return;
            }

            foreach (var view in _tableComposition.LocalHandCardViews)
            {
                if (view.HandIndex < 0 || view.HandIndex >= LocalHand.Count)
                {
                    continue;
                }

                view.Apply(ResolveVisualState(LocalHand[view.HandIndex]));
            }
        }

        private PrototypeCardVisualState ResolveVisualState(Card card)
        {
            var state = Interaction.State;
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
                return Interaction.IsTemporarilyBlocked
                    ? PrototypeCardVisualState.TemporarilyBlocked
                    : PrototypeCardVisualState.Selected;
            }

            return Interaction.IsCardLegal(card)
                ? PrototypeCardVisualState.Legal
                : PrototypeCardVisualState.TemporarilyBlocked;
        }
    }
}
