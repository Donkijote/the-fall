using System;
using System.Collections.Generic;
using TheFall.Domain;
using UnityEngine;

namespace TheFall.Presentation.Cards
{
    [Serializable]
    public struct CardVisualEntry
    {
        [SerializeField] private CardSuit suit;
        [SerializeField] private CardRank rank;
        [SerializeField] private RectInt atlasPixelRect;

        public CardVisualEntry(Card card, RectInt atlasPixelRect)
        {
            suit = card.Suit;
            rank = card.Rank;
            this.atlasPixelRect = atlasPixelRect;
        }

        public Card Card => new Card(suit, rank);

        public RectInt AtlasPixelRect => atlasPixelRect;
    }

    [CreateAssetMenu(fileName = "CardVisualCatalog", menuName = "The Fall/Cards/Visual Catalog")]
    public sealed class CardVisualCatalog : ScriptableObject
    {
        [SerializeField] private Texture2D faceAtlas;
        [SerializeField] private Texture2D backTexture;
        [SerializeField] private Material sharedFaceMaterial;
        [SerializeField] private CardVisualEntry[] entries = Array.Empty<CardVisualEntry>();

        public Texture2D FaceAtlas => faceAtlas;

        public Texture2D BackTexture => backTexture;

        public Material SharedFaceMaterial => sharedFaceMaterial;

        public IReadOnlyList<CardVisualEntry> Entries => entries;

        public void Configure(
            Texture2D atlas,
            Texture2D back,
            Material faceMaterial,
            IReadOnlyList<CardVisualEntry> visualEntries)
        {
            faceAtlas = atlas != null ? atlas : throw new ArgumentNullException(nameof(atlas));
            backTexture = back != null ? back : throw new ArgumentNullException(nameof(back));
            sharedFaceMaterial = faceMaterial != null ? faceMaterial : throw new ArgumentNullException(nameof(faceMaterial));
            if (visualEntries == null)
            {
                throw new ArgumentNullException(nameof(visualEntries));
            }

            entries = new CardVisualEntry[visualEntries.Count];
            for (var index = 0; index < visualEntries.Count; index++)
            {
                entries[index] = visualEntries[index];
            }
        }

        public bool TryGetAtlasPixelRect(Card card, out RectInt pixelRect)
        {
            foreach (var entry in entries)
            {
                if (entry.Card != card)
                {
                    continue;
                }

                pixelRect = entry.AtlasPixelRect;
                return true;
            }

            pixelRect = default;
            return false;
        }

        public bool TryGetAtlasUvRect(Card card, out Rect uvRect)
        {
            if (faceAtlas == null || !TryGetAtlasPixelRect(card, out var pixelRect))
            {
                uvRect = default;
                return false;
            }

            uvRect = new Rect(
                pixelRect.x / (float)faceAtlas.width,
                pixelRect.y / (float)faceAtlas.height,
                pixelRect.width / (float)faceAtlas.width,
                pixelRect.height / (float)faceAtlas.height);
            return true;
        }
    }
}
