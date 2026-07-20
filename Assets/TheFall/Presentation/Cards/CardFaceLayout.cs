using System;
using System.Collections.Generic;
using System.Globalization;
using TheFall.Domain;
using UnityEngine;

namespace TheFall.Presentation.Cards
{
    public enum CardFaceArtworkKind
    {
        Pips,
        CourtIllustration,
    }

    [Serializable]
    public readonly struct CardPipPlacement
    {
        public CardPipPlacement(Vector2 normalizedPosition, bool inverted, float scale = 1f)
        {
            if (normalizedPosition.x < 0f || normalizedPosition.x > 1f ||
                normalizedPosition.y < 0f || normalizedPosition.y > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(normalizedPosition));
            }

            if (scale <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(scale));
            }

            NormalizedPosition = normalizedPosition;
            Inverted = inverted;
            Scale = scale;
        }

        public Vector2 NormalizedPosition { get; }

        public bool Inverted { get; }

        public float Scale { get; }
    }

    public sealed class CardFaceLayout
    {
        private readonly IReadOnlyList<CardPipPlacement> _pipPlacements;

        public CardFaceLayout(
            Card card,
            CardFaceArtworkKind artworkKind,
            IReadOnlyList<CardPipPlacement> pipPlacements)
        {
            Card = card;
            ArtworkKind = artworkKind;
            _pipPlacements = pipPlacements ?? throw new ArgumentNullException(nameof(pipPlacements));
            RankLabel = ((int)card.Rank).ToString(CultureInfo.InvariantCulture);
        }

        public Card Card { get; }

        public string RankLabel { get; }

        public CardFaceArtworkKind ArtworkKind { get; }

        public IReadOnlyList<CardPipPlacement> PipPlacements => _pipPlacements;
    }

    /// <summary>
    /// Project-owned visual definitions for the forty-card Spanish deck. These definitions contain
    /// no textures: Editor tooling combines them with replaceable source components into an atlas.
    /// </summary>
    public static class SpanishDeckVisualLayout
    {
        private static readonly IReadOnlyList<CardFaceLayout> Layouts = BuildLayouts();

        public static IReadOnlyList<CardFaceLayout> Create()
        {
            return Layouts;
        }

        private static IReadOnlyList<CardFaceLayout> BuildLayouts()
        {
            var layouts = new List<CardFaceLayout>(40);
            foreach (var card in Deck.CreateSpanishDeck().Cards)
            {
                var isCourt = (int)card.Rank >= 10;
                layouts.Add(new CardFaceLayout(
                    card,
                    isCourt ? CardFaceArtworkKind.CourtIllustration : CardFaceArtworkKind.Pips,
                    isCourt ? Array.Empty<CardPipPlacement>() : CreatePipPlacements(card.Rank)));
            }

            return Array.AsReadOnly(layouts.ToArray());
        }

        private static IReadOnlyList<CardPipPlacement> CreatePipPlacements(CardRank rank)
        {
            switch (rank)
            {
                case CardRank.One:
                    return Array.AsReadOnly(new[]
                    {
                        Pip(0.50f, 0.50f, false, 1.45f),
                    });
                case CardRank.Two:
                    return Array.AsReadOnly(new[]
                    {
                        Pip(0.50f, 0.72f, false, 1.10f),
                        Pip(0.50f, 0.28f, true, 1.10f),
                    });
                case CardRank.Three:
                    return Array.AsReadOnly(new[]
                    {
                        Pip(0.50f, 0.75f, false),
                        Pip(0.50f, 0.50f, false),
                        Pip(0.50f, 0.25f, true),
                    });
                case CardRank.Four:
                    return Array.AsReadOnly(new[]
                    {
                        Pip(0.32f, 0.72f, false),
                        Pip(0.68f, 0.72f, false),
                        Pip(0.32f, 0.28f, true),
                        Pip(0.68f, 0.28f, true),
                    });
                case CardRank.Five:
                    return Array.AsReadOnly(new[]
                    {
                        Pip(0.31f, 0.75f, false, 0.92f),
                        Pip(0.69f, 0.75f, false, 0.92f),
                        Pip(0.50f, 0.50f, false, 0.92f),
                        Pip(0.31f, 0.25f, true, 0.92f),
                        Pip(0.69f, 0.25f, true, 0.92f),
                    });
                case CardRank.Six:
                    return Array.AsReadOnly(new[]
                    {
                        Pip(0.31f, 0.77f, false, 0.84f),
                        Pip(0.69f, 0.77f, false, 0.84f),
                        Pip(0.31f, 0.50f, false, 0.84f),
                        Pip(0.69f, 0.50f, false, 0.84f),
                        Pip(0.31f, 0.23f, true, 0.84f),
                        Pip(0.69f, 0.23f, true, 0.84f),
                    });
                case CardRank.Seven:
                    return Array.AsReadOnly(new[]
                    {
                        Pip(0.29f, 0.78f, false, 0.76f),
                        Pip(0.71f, 0.78f, false, 0.76f),
                        Pip(0.29f, 0.51f, false, 0.76f),
                        Pip(0.71f, 0.51f, false, 0.76f),
                        Pip(0.50f, 0.37f, true, 0.76f),
                        Pip(0.29f, 0.22f, true, 0.76f),
                        Pip(0.71f, 0.22f, true, 0.76f),
                    });
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, "Court ranks do not use pip layouts.");
            }
        }

        private static CardPipPlacement Pip(float x, float y, bool inverted, float scale = 1f)
        {
            return new CardPipPlacement(new Vector2(x, y), inverted, scale);
        }
    }
}
