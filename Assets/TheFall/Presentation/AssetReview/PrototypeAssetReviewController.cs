using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFall.Presentation.AssetReview
{
    /// <summary>
    /// Provides inspection-only camera controls for generated prototype assets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeAssetReviewController : MonoBehaviour
    {
        private const float DefaultYawDegrees = 35f;
        private const float DefaultPitchDegrees = 28f;
        private const float DefaultDistanceMetres = 3.2f;

        [SerializeField]
        private Camera _reviewCamera;

        [SerializeField]
        private Transform _focusTarget;

        [SerializeField]
        [Min(0.01f)]
        private float _orbitSensitivity = 0.18f;

        [SerializeField]
        [Min(0.01f)]
        private float _keyboardOrbitSpeed = 65f;

        [SerializeField]
        [Min(0.01f)]
        private float _zoomSensitivity = 0.0025f;

        [SerializeField]
        private Vector2 _distanceRangeMetres = new Vector2(1.7f, 5.5f);

        private float _yawDegrees = DefaultYawDegrees;
        private float _pitchDegrees = DefaultPitchDegrees;
        private float _distanceMetres = DefaultDistanceMetres;

        public Camera ReviewCamera => _reviewCamera;

        public Transform FocusTarget => _focusTarget;

        public float DistanceMetres => _distanceMetres;

        private void Awake()
        {
            ApplyCameraPose();
        }

        private void Update()
        {
            var changed = ApplyMouseInput();
            changed |= ApplyKeyboardInput();

            if (changed)
            {
                ApplyCameraPose();
            }
        }

        private void OnValidate()
        {
            _distanceRangeMetres.x = Mathf.Max(0.1f, _distanceRangeMetres.x);
            _distanceRangeMetres.y = Mathf.Max(_distanceRangeMetres.x, _distanceRangeMetres.y);
            _distanceMetres = Mathf.Clamp(_distanceMetres, _distanceRangeMetres.x, _distanceRangeMetres.y);
        }

        public void ResetView()
        {
            _yawDegrees = DefaultYawDegrees;
            _pitchDegrees = DefaultPitchDegrees;
            _distanceMetres = DefaultDistanceMetres;
            ApplyCameraPose();
        }

#if UNITY_EDITOR
        public void Configure(Camera reviewCamera, Transform focusTarget)
        {
            _reviewCamera = reviewCamera;
            _focusTarget = focusTarget;
            ResetView();
        }
#endif

        private bool ApplyMouseInput()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return false;
            }

            var changed = false;
            if (mouse.leftButton.isPressed)
            {
                var delta = mouse.delta.ReadValue();
                if (delta.sqrMagnitude > 0f)
                {
                    _yawDegrees += delta.x * _orbitSensitivity;
                    _pitchDegrees = Mathf.Clamp(_pitchDegrees - delta.y * _orbitSensitivity, 10f, 78f);
                    changed = true;
                }
            }

            var scroll = mouse.scroll.ReadValue().y;
            if (!Mathf.Approximately(scroll, 0f))
            {
                _distanceMetres = Mathf.Clamp(
                    _distanceMetres - scroll * _zoomSensitivity,
                    _distanceRangeMetres.x,
                    _distanceRangeMetres.y);
                changed = true;
            }

            return changed;
        }

        private bool ApplyKeyboardInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetView();
                return false;
            }

            var horizontal = (keyboard.rightArrowKey.isPressed ? 1f : 0f) -
                (keyboard.leftArrowKey.isPressed ? 1f : 0f);
            var vertical = (keyboard.upArrowKey.isPressed ? 1f : 0f) -
                (keyboard.downArrowKey.isPressed ? 1f : 0f);
            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                return false;
            }

            _yawDegrees += horizontal * _keyboardOrbitSpeed * Time.unscaledDeltaTime;
            _pitchDegrees = Mathf.Clamp(
                _pitchDegrees + vertical * _keyboardOrbitSpeed * Time.unscaledDeltaTime,
                10f,
                78f);
            return true;
        }

        private void ApplyCameraPose()
        {
            if (_reviewCamera == null || _focusTarget == null)
            {
                return;
            }

            var orbitRotation = Quaternion.Euler(_pitchDegrees, _yawDegrees, 0f);
            _reviewCamera.transform.position =
                _focusTarget.position - orbitRotation * Vector3.forward * _distanceMetres;
            _reviewCamera.transform.rotation = orbitRotation;
        }

        private void OnGUI()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            GUI.Box(
                new Rect(18f, 18f, 390f, 72f),
                "TABLE ASSET REVIEW\nDrag left mouse or use arrow keys to orbit  •  Scroll to zoom  •  R to reset");
        }
    }
}
