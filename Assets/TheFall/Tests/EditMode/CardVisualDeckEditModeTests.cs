using System.Linq;
using NUnit.Framework;
using TheFall.Domain;
using TheFall.Presentation.Cards;
using UnityEditor;
using UnityEngine;

namespace TheFall.Tests.EditMode
{
    public sealed class CardVisualDeckEditModeTests
    {
        private const string CatalogPath = "Assets/TheFall/Content/Cards/Generated/CardVisualCatalog.asset";

        [Test]
        public void SpanishDeckVisualLayout_CoversFortyUniqueCards()
        {
            var layouts = SpanishDeckVisualLayout.Create();

            Assert.That(layouts, Has.Count.EqualTo(40));
            Assert.That(layouts.Select(layout => layout.Card).Distinct().Count(), Is.EqualTo(40));
            Assert.That(layouts.Select(layout => layout.Card), Is.EquivalentTo(Deck.CreateSpanishDeck().Cards));
        }

        [Test]
        public void NumberedLayouts_UseRankCountAndCourtLayoutsUseUniqueIllustrationSlots()
        {
            foreach (var layout in SpanishDeckVisualLayout.Create())
            {
                if ((int)layout.Card.Rank < 10)
                {
                    Assert.That(layout.ArtworkKind, Is.EqualTo(CardFaceArtworkKind.Pips), layout.Card.ToString());
                    Assert.That(layout.PipPlacements, Has.Count.EqualTo((int)layout.Card.Rank), layout.Card.ToString());
                    Assert.That(layout.PipPlacements.All(placement =>
                        placement.NormalizedPosition.x >= 0f && placement.NormalizedPosition.x <= 1f &&
                        placement.NormalizedPosition.y >= 0f && placement.NormalizedPosition.y <= 1f), Is.True, layout.Card.ToString());
                }
                else
                {
                    Assert.That(layout.ArtworkKind, Is.EqualTo(CardFaceArtworkKind.CourtIllustration), layout.Card.ToString());
                    Assert.That(layout.PipPlacements, Is.Empty, layout.Card.ToString());
                }
            }
        }

        [Test]
        public void GeneratedCatalog_MapsEveryDomainCardToOneValidAtlasRegion()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CardVisualCatalog>(CatalogPath);

            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.FaceAtlas, Is.Not.Null);
            Assert.That(catalog.BackTexture, Is.Not.Null);
            Assert.That(catalog.SharedFaceMaterial, Is.Not.Null);
            Assert.That(catalog.SharedFaceMaterial.mainTexture, Is.SameAs(catalog.FaceAtlas));
            Assert.That(catalog.Entries.Count, Is.EqualTo(40));
            Assert.That(catalog.Entries.Select(entry => entry.Card).Distinct().Count(), Is.EqualTo(40));

            foreach (var card in Deck.CreateSpanishDeck().Cards)
            {
                Assert.That(catalog.TryGetAtlasPixelRect(card, out var pixelRect), Is.True, card.ToString());
                Assert.That(pixelRect.size, Is.EqualTo(new Vector2Int(252, 352)), card.ToString());
                Assert.That(catalog.TryGetAtlasUvRect(card, out var uvRect), Is.True, card.ToString());
                Assert.That(uvRect.xMin, Is.GreaterThanOrEqualTo(0f), card.ToString());
                Assert.That(uvRect.yMin, Is.GreaterThanOrEqualTo(0f), card.ToString());
                Assert.That(uvRect.xMax, Is.LessThanOrEqualTo(1f), card.ToString());
                Assert.That(uvRect.yMax, Is.LessThanOrEqualTo(1f), card.ToString());
            }
        }

        [Test]
        public void MaterialBinding_SelectsOneAtlasRegionWithoutCloningTheSharedMaterial()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CardVisualCatalog>(CatalogPath);
            var gameObject = new GameObject("Card face renderer", typeof(MeshRenderer));

            try
            {
                var renderer = gameObject.GetComponent<MeshRenderer>();
                var card = new Card(CardSuit.Swords, CardRank.Seven);
                var propertyBlock = new MaterialPropertyBlock();

                CardVisualMaterialBinding.Apply(renderer, catalog, card, propertyBlock);

                Assert.That(renderer.sharedMaterial, Is.SameAs(catalog.SharedFaceMaterial));
                renderer.GetPropertyBlock(propertyBlock);
                Assert.That(propertyBlock.GetTexture(Shader.PropertyToID("_BaseMap")), Is.SameAs(catalog.FaceAtlas));
                Assert.That(catalog.TryGetAtlasUvRect(card, out var uvRect), Is.True);
                Assert.That(
                    propertyBlock.GetVector(Shader.PropertyToID("_BaseMap_ST")),
                    Is.EqualTo(new Vector4(uvRect.width, uvRect.height, uvRect.x, uvRect.y)));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
