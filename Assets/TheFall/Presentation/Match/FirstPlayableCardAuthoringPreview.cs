using TheFall.Domain;
using TheFall.Presentation.Cards;
using UnityEngine;

namespace TheFall.Presentation.Match
{
    /// <summary>
    /// Keeps representative scene-authored cards readable in Edit Mode without creating materials.
    /// These objects are authoring references only and are hidden while the runtime table is active.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public sealed class FirstPlayableCardAuthoringPreview : MonoBehaviour
    {
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseMapTransform = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");
        private static readonly int MainTextureTransform = Shader.PropertyToID("_MainTex_ST");

        [SerializeField] private CardVisualCatalog _catalog;
        [SerializeField] private CardSuit _suit;
        [SerializeField] private CardRank _rank = CardRank.One;
        [SerializeField] private bool _faceUp = true;

        private MaterialPropertyBlock _propertyBlock;

#if UNITY_EDITOR
        public void Configure(CardVisualCatalog catalog, Card card, bool faceUp)
        {
            _catalog = catalog;
            _suit = card.Suit;
            _rank = card.Rank;
            _faceUp = faceUp;
            Apply();
        }
#endif

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        private void Apply()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null || _catalog == null || _catalog.SharedFaceMaterial == null)
            {
                return;
            }

            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.Clear();
            if (_faceUp)
            {
                CardVisualMaterialBinding.Apply(renderer, _catalog, new Card(_suit, _rank), _propertyBlock);
                return;
            }

            renderer.sharedMaterial = _catalog.SharedFaceMaterial;
            _propertyBlock.SetTexture(BaseMap, _catalog.BackTexture);
            _propertyBlock.SetVector(BaseMapTransform, new Vector4(1f, 1f, 0f, 0f));
            _propertyBlock.SetTexture(MainTexture, _catalog.BackTexture);
            _propertyBlock.SetVector(MainTextureTransform, new Vector4(1f, 1f, 0f, 0f));
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
