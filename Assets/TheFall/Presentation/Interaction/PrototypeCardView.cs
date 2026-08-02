using TMPro;
using UnityEngine;

namespace TheFall.Presentation.Interaction
{
    public enum PrototypeCardVisualState
    {
        Legal,
        Inspected,
        Selected,
        Confirmed,
        Rejected,
        TemporarilyBlocked,
    }

    /// <summary>
    /// Hover-independent card feedback using color, scale, and a persistent shape/text cue.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeCardView : MonoBehaviour
    {
        private static readonly Color Vellum = FromHex(0xD8C493);
        private static readonly Color Woad = FromHex(0x465C73);
        private static readonly Color Brass = FromHex(0xB58B3E);
        private static readonly Color Madder = FromHex(0x8D4238);
        private static readonly Color Ochre = FromHex(0xA06F3C);
        private static readonly Color Lampblack = FromHex(0x241A14);

        private MaterialPropertyBlock _propertyBlock;
        private Renderer _cardRenderer;
        private TextMeshPro _stateCue;
        private Vector3 _baseScale;

        public int HandIndex { get; private set; }

        public PrototypeCardVisualState VisualState { get; private set; }

        public void Configure(int handIndex)
        {
            HandIndex = handIndex;
            _cardRenderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _baseScale = transform.localScale;
            CreateStateCue();
            Apply(PrototypeCardVisualState.Legal);
        }

        public void Apply(PrototypeCardVisualState state)
        {
            VisualState = state;
            var color = Vellum;
            var cue = "+";
            var scale = 1f;

            switch (state)
            {
                case PrototypeCardVisualState.Inspected:
                    color = Vellum;
                    cue = "?";
                    scale = 1.10f;
                    break;
                case PrototypeCardVisualState.Selected:
                    color = Woad;
                    cue = "*";
                    scale = 1.14f;
                    break;
                case PrototypeCardVisualState.Confirmed:
                    color = Brass;
                    cue = "OK";
                    scale = 1.08f;
                    break;
                case PrototypeCardVisualState.Rejected:
                    color = Madder;
                    cue = "X";
                    scale = 0.96f;
                    break;
                case PrototypeCardVisualState.TemporarilyBlocked:
                    color = Ochre;
                    cue = "||";
                    scale = 0.94f;
                    break;
            }

            transform.localScale = _baseScale * scale;
            if (_cardRenderer != null)
            {
                _cardRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_BaseColor", color);
                _propertyBlock.SetColor("_Color", color);
                _cardRenderer.SetPropertyBlock(_propertyBlock);
            }

            if (_stateCue != null)
            {
                _stateCue.text = cue;
                _stateCue.color = state == PrototypeCardVisualState.Selected ? Vellum : Lampblack;
            }
        }

        private void CreateStateCue()
        {
            var cueObject = new GameObject("Persistent Interaction State Cue", typeof(TextMeshPro));
            cueObject.hideFlags = HideFlags.DontSave;
            cueObject.transform.SetParent(transform, false);
            cueObject.transform.localPosition = new Vector3(0f, 0.62f, 0f);
            cueObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            const float cueWorldScale = 0.065f;
            cueObject.transform.localScale = new Vector3(
                cueWorldScale / _baseScale.x,
                cueWorldScale / _baseScale.z,
                1f);

            _stateCue = cueObject.GetComponent<TextMeshPro>();
            _stateCue.alignment = TextAlignmentOptions.Center;
            _stateCue.fontSize = 2.5f;
            _stateCue.fontStyle = FontStyles.Bold;
            _stateCue.textWrappingMode = TextWrappingModes.NoWrap;
            _stateCue.rectTransform.sizeDelta = new Vector2(1f, 1f);
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
