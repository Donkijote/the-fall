using System;
using TheFall.Domain;
using UnityEngine;

namespace TheFall.Presentation.Cards
{
    /// <summary>
    /// Applies one catalog entry to a renderer without cloning the shared card-face material.
    /// The card mesh must use normalized 0–1 UVs across its face.
    /// </summary>
    public static class CardVisualMaterialBinding
    {
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseMapTransform = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");
        private static readonly int MainTextureTransform = Shader.PropertyToID("_MainTex_ST");

        public static void Apply(
            Renderer renderer,
            CardVisualCatalog catalog,
            Card card,
            MaterialPropertyBlock propertyBlock = null)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (!catalog.TryGetAtlasUvRect(card, out var uvRect))
            {
                throw new ArgumentException($"The visual catalog does not contain {card}.", nameof(card));
            }

            if (catalog.SharedFaceMaterial == null || catalog.FaceAtlas == null)
            {
                throw new InvalidOperationException("The visual catalog is missing its shared face material or atlas.");
            }

            renderer.sharedMaterial = catalog.SharedFaceMaterial;
            var block = propertyBlock ?? new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            var transform = new Vector4(uvRect.width, uvRect.height, uvRect.x, uvRect.y);
            block.SetTexture(BaseMap, catalog.FaceAtlas);
            block.SetVector(BaseMapTransform, transform);
            block.SetTexture(MainTexture, catalog.FaceAtlas);
            block.SetVector(MainTextureTransform, transform);
            renderer.SetPropertyBlock(block);
        }
    }
}
