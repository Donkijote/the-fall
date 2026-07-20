using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheFall.Domain;
using TheFall.Presentation.Cards;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TheFall.Editor
{
    public static class CardDeckAssetGenerator
    {
        private const string Root = "Assets/TheFall/Content/Cards";
        private const string SourceRoot = Root + "/Source";
        private const string GeneratedRoot = Root + "/Generated";
        private const string BaseFacePath = SourceRoot + "/CardBase.png";
        private const string BackPath = SourceRoot + "/CardBack.png";
        private const string SuitAtlasPath = SourceRoot + "/CardSuitAtlas.png";
        private const string RankAtlasPath = SourceRoot + "/CardRankAtlas.png";
        private const string CourtAtlasPath = SourceRoot + "/CardCourtAtlas.png";
        private const string FaceAtlasPath = GeneratedRoot + "/CardFaceAtlas.png";
        private const string SharedMaterialPath = GeneratedRoot + "/CardFaceShared.mat";
        private const string CatalogPath = GeneratedRoot + "/CardVisualCatalog.asset";

        private const int FaceWidth = 252;
        private const int FaceHeight = 352;
        private const int AtlasSize = 2048;
        private const int AtlasColumns = 8;
        private const int AtlasRows = 5;
        private const int AtlasSlotWidth = 256;
        private const int AtlasSlotHeight = 400;
        private const int AtlasFaceInsetX = 2;
        private const int AtlasFaceInsetY = 24;
        private const int SuitCell = 128;
        private const int RankCell = 96;
        private const int CourtCell = 256;

        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        private static readonly Color32 Lampblack = Hex(0x241A14);
        private static readonly Color32 CharredWalnut = Hex(0x3B291F);
        private static readonly Color32 Walnut = Hex(0x68452F);
        private static readonly Color32 Vellum = Hex(0xD8C493);
        private static readonly Color32 LightVellum = Hex(0xE9DCB8);
        private static readonly Color32 Moss = Hex(0x6B7046);
        private static readonly Color32 Woad = Hex(0x465C73);
        private static readonly Color32 Madder = Hex(0x8D4238);
        private static readonly Color32 Brass = Hex(0xB58B3E);
        private static readonly Color32 Iron = Hex(0xA7A39A);

        private static readonly CardRank[] RankOrder =
        {
            CardRank.One,
            CardRank.Two,
            CardRank.Three,
            CardRank.Four,
            CardRank.Five,
            CardRank.Six,
            CardRank.Seven,
            CardRank.Ten,
            CardRank.Eleven,
            CardRank.Twelve,
        };

        [MenuItem("The Fall/Cards/Generate Complete Deck")]
        public static void GenerateAll()
        {
            EnsureDirectories();
            EnsurePrototypeComponentSources();
            ConfigureSourceImporters();
            var entries = GenerateFaceAtlas();
            ConfigureTextureImporter(FaceAtlasPath, true, true, AtlasSize);
            ConfigureTextureImporter(BackPath, true, false, 512);
            ConfigureCatalog(entries);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("The Fall modular forty-card visual deck generated and validated.");
        }

        [MenuItem("The Fall/Cards/Validate Complete Deck")]
        public static void Validate()
        {
            var errors = new List<string>();
            foreach (var path in new[]
                     {
                         BaseFacePath,
                         BackPath,
                         SuitAtlasPath,
                         RankAtlasPath,
                         CourtAtlasPath,
                         FaceAtlasPath,
                         SharedMaterialPath,
                         CatalogPath,
                     })
            {
                if (!File.Exists(path))
                {
                    errors.Add($"Missing card visual asset: {path}");
                }
            }

            var catalog = AssetDatabase.LoadAssetAtPath<CardVisualCatalog>(CatalogPath);
            if (catalog == null)
            {
                errors.Add("CardVisualCatalog could not be loaded.");
            }
            else
            {
                if (catalog.FaceAtlas == null ||
                    catalog.FaceAtlas.width != AtlasSize ||
                    catalog.FaceAtlas.height != AtlasSize)
                {
                    errors.Add($"Card face atlas must be {AtlasSize}x{AtlasSize}.");
                }

                if (catalog.SharedFaceMaterial == null || catalog.SharedFaceMaterial.mainTexture != catalog.FaceAtlas)
                {
                    errors.Add("Card visual catalog must use one shared material backed by the generated face atlas.");
                }

                if (catalog.BackTexture == null ||
                    catalog.BackTexture.width != FaceWidth ||
                    catalog.BackTexture.height != FaceHeight)
                {
                    errors.Add($"Card back must preserve the {FaceWidth}x{FaceHeight} 63:88 ratio.");
                }

                if (catalog.Entries.Count != 40 || catalog.Entries.Select(entry => entry.Card).Distinct().Count() != 40)
                {
                    errors.Add("Card visual catalog must contain forty unique entries.");
                }

                foreach (var card in Deck.CreateSpanishDeck().Cards)
                {
                    if (!catalog.TryGetAtlasPixelRect(card, out var rect))
                    {
                        errors.Add($"Card visual catalog is missing {card}.");
                        continue;
                    }

                    if (rect.width != FaceWidth || rect.height != FaceHeight ||
                        rect.x < 0 || rect.y < 0 || rect.xMax > AtlasSize || rect.yMax > AtlasSize)
                    {
                        errors.Add($"Card visual atlas rectangle is invalid for {card}: {rect}.");
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new BuildFailedException("The Fall card visual deck validation failed:\n- " + string.Join("\n- ", errors));
            }
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(SourceRoot);
            Directory.CreateDirectory(GeneratedRoot);
            AssetDatabase.Refresh();
        }

        private static void EnsurePrototypeComponentSources()
        {
            WriteIfMissing(BaseFacePath, CreateBaseFace());
            WriteIfMissing(BackPath, CreateCardBack());
            WriteIfMissing(SuitAtlasPath, CreateSuitAtlas());
            WriteIfMissing(RankAtlasPath, CreateRankAtlas());
            WriteIfMissing(CourtAtlasPath, CreateCourtAtlas());
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigureSourceImporters()
        {
            ConfigureTextureImporter(BaseFacePath, false, false, 512);
            ConfigureTextureImporter(SuitAtlasPath, false, false, 1024);
            ConfigureTextureImporter(RankAtlasPath, false, false, 512);
            ConfigureTextureImporter(CourtAtlasPath, false, false, 1024);
        }

        private static void ConfigureTextureImporter(string path, bool mipmaps, bool compressed, int maxSize)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                throw new InvalidOperationException($"Unable to configure texture importer for {path}.");
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = mipmaps;
            importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = maxSize;
            importer.textureCompression = compressed
                ? TextureImporterCompression.CompressedHQ
                : TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static IReadOnlyList<CardVisualEntry> GenerateFaceAtlas()
        {
            using (var components = new ComponentTextures(
                       LoadPng(BaseFacePath),
                       LoadPng(SuitAtlasPath),
                       LoadPng(RankAtlasPath),
                       LoadPng(CourtAtlasPath)))
            {
                var atlas = new PixelCanvas(AtlasSize, AtlasSize, Clear);
                var entries = new List<CardVisualEntry>(40);
                var layouts = SpanishDeckVisualLayout.Create();

                for (var index = 0; index < layouts.Count; index++)
                {
                    var layout = layouts[index];
                    var face = new PixelCanvas(components.BaseFace);
                    ComposeCorners(face, components, layout.Card);
                    if (layout.ArtworkKind == CardFaceArtworkKind.CourtIllustration)
                    {
                        ComposeCourt(face, components, layout.Card);
                    }
                    else
                    {
                        ComposePips(face, components, layout);
                    }

                    var column = index % AtlasColumns;
                    var row = index / AtlasColumns;
                    var rect = new RectInt(
                        (column * AtlasSlotWidth) + AtlasFaceInsetX,
                        (row * AtlasSlotHeight) + AtlasFaceInsetY,
                        FaceWidth,
                        FaceHeight);
                    atlas.Blit(face, new RectInt(0, 0, FaceWidth, FaceHeight), rect, false);
                    entries.Add(new CardVisualEntry(layout.Card, rect));
                }

                WritePng(FaceAtlasPath, atlas);
                AssetDatabase.ImportAsset(FaceAtlasPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                return entries;
            }
        }

        private static void ComposeCorners(PixelCanvas face, ComponentTextures components, Card card)
        {
            var rankRect = RankSourceRect(card.Rank);
            var suitRect = SuitSourceRect(card.Suit);
            var rankWidth = (int)card.Rank >= 10 ? 30 : 23;
            const int rankHeight = 28;
            const int suitSize = 25;
            var left = 31;
            var right = FaceWidth - 31;
            var topRank = FaceHeight - 31;
            var topSuit = FaceHeight - 61;
            var bottomRank = 31;
            var bottomSuit = 61;

            BlitCentered(face, components.RankAtlas, rankRect, left, topRank, rankWidth, rankHeight, false);
            BlitCentered(face, components.RankAtlas, rankRect, right, topRank, rankWidth, rankHeight, false);
            BlitCentered(face, components.SuitAtlas, suitRect, left, topSuit, suitSize, suitSize, false);
            BlitCentered(face, components.SuitAtlas, suitRect, right, topSuit, suitSize, suitSize, false);

            BlitCentered(face, components.RankAtlas, rankRect, left, bottomRank, rankWidth, rankHeight, true);
            BlitCentered(face, components.RankAtlas, rankRect, right, bottomRank, rankWidth, rankHeight, true);
            BlitCentered(face, components.SuitAtlas, suitRect, left, bottomSuit, suitSize, suitSize, true);
            BlitCentered(face, components.SuitAtlas, suitRect, right, bottomSuit, suitSize, suitSize, true);
        }

        private static void ComposePips(PixelCanvas face, ComponentTextures components, CardFaceLayout layout)
        {
            var source = SuitSourceRect(layout.Card.Suit);
            var safeField = new Rect(53f, 73f, 146f, 206f);
            var baseSize = PipSize(layout.Card.Suit);

            foreach (var placement in layout.PipPlacements)
            {
                var centerX = Mathf.RoundToInt(safeField.x + (placement.NormalizedPosition.x * safeField.width));
                var centerY = Mathf.RoundToInt(safeField.y + (placement.NormalizedPosition.y * safeField.height));
                var width = Mathf.RoundToInt(baseSize.x * placement.Scale);
                var height = Mathf.RoundToInt(baseSize.y * placement.Scale);
                BlitCentered(
                    face,
                    components.SuitAtlas,
                    source,
                    centerX,
                    centerY,
                    width,
                    height,
                    placement.Inverted);
            }
        }

        private static void ComposeCourt(PixelCanvas face, ComponentTextures components, Card card)
        {
            var source = CourtSourceRect(card.Suit, card.Rank);
            face.Blit(components.CourtAtlas, source, new RectInt(47, 69, 158, 214), false);
        }

        private static void BlitCentered(
            PixelCanvas destination,
            PixelCanvas source,
            RectInt sourceRect,
            int centerX,
            int centerY,
            int width,
            int height,
            bool rotate180)
        {
            destination.Blit(
                source,
                sourceRect,
                new RectInt(centerX - (width / 2), centerY - (height / 2), width, height),
                rotate180);
        }

        private static Vector2Int PipSize(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Coins:
                    return new Vector2Int(42, 42);
                case CardSuit.Cups:
                    return new Vector2Int(47, 43);
                case CardSuit.Swords:
                    return new Vector2Int(28, 66);
                case CardSuit.Clubs:
                    return new Vector2Int(33, 66);
                default:
                    throw new ArgumentOutOfRangeException(nameof(suit), suit, null);
            }
        }

        private static RectInt SuitSourceRect(CardSuit suit)
        {
            return new RectInt((int)suit * SuitCell, 0, SuitCell, SuitCell);
        }

        private static RectInt RankSourceRect(CardRank rank)
        {
            var index = Array.IndexOf(RankOrder, rank);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            return new RectInt((index % 5) * RankCell, (index / 5) * RankCell, RankCell, RankCell);
        }

        private static RectInt CourtSourceRect(CardSuit suit, CardRank rank)
        {
            var rankIndex = (int)rank - 10;
            if (rankIndex < 0 || rankIndex > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Only ranks 10–12 use court illustrations.");
            }

            return new RectInt((int)suit * CourtCell, rankIndex * CourtCell, CourtCell, CourtCell);
        }

        private static void ConfigureCatalog(IReadOnlyList<CardVisualEntry> entries)
        {
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(FaceAtlasPath);
            var back = AssetDatabase.LoadAssetAtPath<Texture2D>(BackPath);
            if (atlas == null || back == null)
            {
                throw new InvalidOperationException("Generated card textures did not import correctly.");
            }

            var sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(SharedMaterialPath);
            if (sharedMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                if (shader == null)
                {
                    throw new InvalidOperationException("Unable to find a card-compatible unlit shader.");
                }

                sharedMaterial = new Material(shader)
                {
                    name = "Card Face Shared",
                };
                AssetDatabase.CreateAsset(sharedMaterial, SharedMaterialPath);
            }

            sharedMaterial.mainTexture = atlas;
            if (sharedMaterial.HasProperty("_BaseMap"))
            {
                sharedMaterial.SetTexture("_BaseMap", atlas);
            }
            EditorUtility.SetDirty(sharedMaterial);

            var catalog = AssetDatabase.LoadAssetAtPath<CardVisualCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<CardVisualCatalog>();
                catalog.name = "Card Visual Catalog";
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.Configure(atlas, back, sharedMaterial, entries);
            EditorUtility.SetDirty(catalog);
        }

        private static PixelCanvas CreateBaseFace()
        {
            return Supersampled(FaceWidth, FaceHeight, (canvas, scale) =>
            {
                canvas.FillWithNoise(LightVellum, 5, 0x51A7);
                canvas.StrokeRoundedRect(
                    new RectInt(6 * scale, 6 * scale, (FaceWidth - 12) * scale, (FaceHeight - 12) * scale),
                    12 * scale,
                    3 * scale,
                    Lampblack);

                var cornerWidth = 55 * scale;
                var cornerHeight = 72 * scale;
                var left = 8 * scale;
                var right = (FaceWidth - 63) * scale;
                var bottom = 8 * scale;
                var top = (FaceHeight - 80) * scale;
                foreach (var rect in new[]
                         {
                             new RectInt(left, top, cornerWidth, cornerHeight),
                             new RectInt(right, top, cornerWidth, cornerHeight),
                             new RectInt(left, bottom, cornerWidth, cornerHeight),
                             new RectInt(right, bottom, cornerWidth, cornerHeight),
                         })
                {
                    canvas.StrokeRoundedRect(rect, 11 * scale, 2 * scale, Lampblack);
                }
            });
        }

        private static PixelCanvas CreateCardBack()
        {
            return Supersampled(FaceWidth, FaceHeight, (canvas, scale) =>
            {
                canvas.FillWithNoise(CharredWalnut, 5, 0xBAC4);
                canvas.StrokeRoundedRect(
                    new RectInt(6 * scale, 6 * scale, (FaceWidth - 12) * scale, (FaceHeight - 12) * scale),
                    12 * scale,
                    3 * scale,
                    LightVellum);
                canvas.StrokeRoundedRect(
                    new RectInt(16 * scale, 16 * scale, (FaceWidth - 32) * scale, (FaceHeight - 32) * scale),
                    20 * scale,
                    3 * scale,
                    Woad);

                var center = new Vector2((FaceWidth * scale) / 2f, (FaceHeight * scale) / 2f);
                for (var ring = 0; ring < 4; ring++)
                {
                    canvas.StrokeCircle(center, (24 + (ring * 18)) * scale, 3 * scale, ring % 2 == 0 ? Brass : LightVellum);
                }

                var halfWidth = FaceWidth * scale * 0.34f;
                var halfHeight = FaceHeight * scale * 0.34f;
                canvas.DrawLine(center + new Vector2(-halfWidth, -halfHeight), center + new Vector2(halfWidth, halfHeight), 5 * scale, Woad);
                canvas.DrawLine(center + new Vector2(-halfWidth, halfHeight), center + new Vector2(halfWidth, -halfHeight), 5 * scale, Woad);
                canvas.FillCircle(center, 12 * scale, Brass);
                canvas.StrokeCircle(center, 12 * scale, 3 * scale, Lampblack);
            });
        }

        private static PixelCanvas CreateSuitAtlas()
        {
            return Supersampled(SuitCell * 4, SuitCell, (canvas, scale) =>
            {
                canvas.Fill(Clear);
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                {
                    DrawSuit(canvas, suit, new RectInt((int)suit * SuitCell * scale, 0, SuitCell * scale, SuitCell * scale));
                }
            });
        }

        private static PixelCanvas CreateRankAtlas()
        {
            return Supersampled(RankCell * 5, RankCell * 2, (canvas, scale) =>
            {
                canvas.Fill(Clear);
                for (var index = 0; index < RankOrder.Length; index++)
                {
                    var cell = new RectInt(
                        (index % 5) * RankCell * scale,
                        (index / 5) * RankCell * scale,
                        RankCell * scale,
                        RankCell * scale);
                    DrawRankLabel(canvas, ((int)RankOrder[index]).ToString(), cell);
                }
            });
        }

        private static PixelCanvas CreateCourtAtlas()
        {
            return Supersampled(CourtCell * 4, CourtCell * 3, (canvas, scale) =>
            {
                canvas.Fill(Clear);
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                {
                    for (var rankIndex = 0; rankIndex < 3; rankIndex++)
                    {
                        var cell = new RectInt(
                            (int)suit * CourtCell * scale,
                            rankIndex * CourtCell * scale,
                            CourtCell * scale,
                            CourtCell * scale);
                        DrawCourtPlaceholder(canvas, suit, (CardRank)(rankIndex + 10), cell);
                    }
                }
            });
        }

        private static void DrawSuit(PixelCanvas canvas, CardSuit suit, RectInt cell)
        {
            var center = new Vector2(cell.center.x, cell.center.y);
            var unit = cell.width / 128f;
            switch (suit)
            {
                case CardSuit.Coins:
                    canvas.FillCircle(center, 43f * unit, Lampblack);
                    canvas.FillCircle(center, 38f * unit, Brass);
                    canvas.FillCircle(center + new Vector2(4f, -3f) * unit, 24f * unit, CharredWalnut);
                    canvas.StrokeCircle(center + new Vector2(4f, -3f) * unit, 24f * unit, 4f * unit, Woad);
                    for (var index = 0; index < 4; index++)
                    {
                        var angle = index * Mathf.PI * 0.5f;
                        var start = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (30f * unit);
                        var end = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (41f * unit);
                        canvas.DrawLine(start, end, 7f * unit, Lampblack);
                    }
                    break;
                case CardSuit.Cups:
                    canvas.FillPolygon(new[]
                    {
                        center + new Vector2(-43f, 31f) * unit,
                        center + new Vector2(43f, 31f) * unit,
                        center + new Vector2(31f, -7f) * unit,
                        center + new Vector2(17f, -24f) * unit,
                        center + new Vector2(-17f, -24f) * unit,
                        center + new Vector2(-31f, -7f) * unit,
                    }, Lampblack);
                    canvas.FillPolygon(new[]
                    {
                        center + new Vector2(-37f, 25f) * unit,
                        center + new Vector2(37f, 25f) * unit,
                        center + new Vector2(26f, -5f) * unit,
                        center + new Vector2(14f, -18f) * unit,
                        center + new Vector2(-14f, -18f) * unit,
                        center + new Vector2(-26f, -5f) * unit,
                    }, Brass);
                    canvas.FillRect(new RectInt(
                        Mathf.RoundToInt(center.x - (8f * unit)),
                        Mathf.RoundToInt(center.y - (46f * unit)),
                        Mathf.RoundToInt(16f * unit),
                        Mathf.RoundToInt(28f * unit)), Brass);
                    canvas.FillRoundedRect(new RectInt(
                        Mathf.RoundToInt(center.x - (30f * unit)),
                        Mathf.RoundToInt(center.y - (53f * unit)),
                        Mathf.RoundToInt(60f * unit),
                        Mathf.RoundToInt(12f * unit)), 4f * unit, Lampblack);
                    canvas.FillRoundedRect(new RectInt(
                        Mathf.RoundToInt(center.x - (25f * unit)),
                        Mathf.RoundToInt(center.y - (50f * unit)),
                        Mathf.RoundToInt(50f * unit),
                        Mathf.RoundToInt(6f * unit)), 2f * unit, Brass);
                    canvas.FillRect(new RectInt(
                        Mathf.RoundToInt(center.x - (35f * unit)),
                        Mathf.RoundToInt(center.y + (14f * unit)),
                        Mathf.RoundToInt(70f * unit),
                        Mathf.RoundToInt(7f * unit)), Madder);
                    break;
                case CardSuit.Swords:
                    canvas.FillPolygon(new[]
                    {
                        center + new Vector2(0f, 54f) * unit,
                        center + new Vector2(14f, 37f) * unit,
                        center + new Vector2(10f, -12f) * unit,
                        center + new Vector2(-10f, -12f) * unit,
                        center + new Vector2(-14f, 37f) * unit,
                    }, Lampblack);
                    canvas.FillPolygon(new[]
                    {
                        center + new Vector2(0f, 48f) * unit,
                        center + new Vector2(9f, 34f) * unit,
                        center + new Vector2(6f, -8f) * unit,
                        center + new Vector2(-6f, -8f) * unit,
                        center + new Vector2(-9f, 34f) * unit,
                    }, Iron);
                    canvas.FillRoundedRect(new RectInt(
                        Mathf.RoundToInt(center.x - (28f * unit)),
                        Mathf.RoundToInt(center.y - (17f * unit)),
                        Mathf.RoundToInt(56f * unit),
                        Mathf.RoundToInt(11f * unit)), 4f * unit, Lampblack);
                    canvas.FillRect(new RectInt(
                        Mathf.RoundToInt(center.x - (23f * unit)),
                        Mathf.RoundToInt(center.y - (14f * unit)),
                        Mathf.RoundToInt(46f * unit),
                        Mathf.RoundToInt(5f * unit)), Brass);
                    canvas.DrawLine(center + new Vector2(0f, -13f) * unit, center + new Vector2(0f, -43f) * unit, 13f * unit, Lampblack);
                    canvas.DrawLine(center + new Vector2(0f, -14f) * unit, center + new Vector2(0f, -42f) * unit, 8f * unit, Woad);
                    canvas.FillCircle(center + new Vector2(0f, -49f) * unit, 10f * unit, Lampblack);
                    canvas.FillCircle(center + new Vector2(0f, -49f) * unit, 6f * unit, Brass);
                    break;
                case CardSuit.Clubs:
                    canvas.FillPolygon(new[]
                    {
                        center + new Vector2(-23f, 49f) * unit,
                        center + new Vector2(15f, 55f) * unit,
                        center + new Vector2(29f, 34f) * unit,
                        center + new Vector2(18f, 8f) * unit,
                        center + new Vector2(9f, -14f) * unit,
                        center + new Vector2(8f, -51f) * unit,
                        center + new Vector2(-9f, -51f) * unit,
                        center + new Vector2(-12f, -12f) * unit,
                        center + new Vector2(-30f, 18f) * unit,
                    }, Lampblack);
                    canvas.FillPolygon(new[]
                    {
                        center + new Vector2(-18f, 43f) * unit,
                        center + new Vector2(11f, 48f) * unit,
                        center + new Vector2(23f, 32f) * unit,
                        center + new Vector2(13f, 7f) * unit,
                        center + new Vector2(4f, -15f) * unit,
                        center + new Vector2(4f, -47f) * unit,
                        center + new Vector2(-5f, -47f) * unit,
                        center + new Vector2(-7f, -10f) * unit,
                        center + new Vector2(-24f, 18f) * unit,
                    }, Walnut);
                    canvas.FillRect(new RectInt(
                        Mathf.RoundToInt(center.x - (8f * unit)),
                        Mathf.RoundToInt(center.y - (30f * unit)),
                        Mathf.RoundToInt(16f * unit),
                        Mathf.RoundToInt(5f * unit)), Brass);
                    for (var wrap = 0; wrap < 4; wrap++)
                    {
                        var y = center.y - ((35f + (wrap * 5f)) * unit);
                        canvas.DrawLine(
                            new Vector2(center.x - (6f * unit), y + (2f * unit)),
                            new Vector2(center.x + (6f * unit), y - (2f * unit)),
                            2f * unit,
                            Moss);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(suit), suit, null);
            }
        }

        private static void DrawRankLabel(PixelCanvas canvas, string label, RectInt cell)
        {
            var patterns = label.Select(DigitPattern).ToArray();
            const int digitWidth = 5;
            const int digitHeight = 7;
            const int spacing = 2;
            var totalColumns = (patterns.Length * digitWidth) + ((patterns.Length - 1) * spacing);
            var unit = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(cell.width / (float)(totalColumns + 4), cell.height / 11f)));
            var totalWidth = totalColumns * unit;
            var totalHeight = digitHeight * unit;
            var originX = cell.x + ((cell.width - totalWidth) / 2);
            var originY = cell.y + ((cell.height - totalHeight) / 2);

            for (var digitIndex = 0; digitIndex < patterns.Length; digitIndex++)
            {
                var pattern = patterns[digitIndex];
                for (var row = 0; row < digitHeight; row++)
                {
                    for (var column = 0; column < digitWidth; column++)
                    {
                        if (pattern[digitHeight - 1 - row][column] != '1')
                        {
                            continue;
                        }

                        canvas.FillRoundedRect(
                            new RectInt(
                                originX + (((digitIndex * (digitWidth + spacing)) + column) * unit),
                                originY + (row * unit),
                                unit,
                                unit),
                            unit * 0.18f,
                            Lampblack);
                    }
                }
            }
        }

        private static string[] DigitPattern(char digit)
        {
            switch (digit)
            {
                case '0': return new[] { "01110", "11011", "11011", "11011", "11011", "11011", "01110" };
                case '1': return new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" };
                case '2': return new[] { "01110", "11011", "00011", "00110", "01100", "11000", "11111" };
                case '3': return new[] { "11110", "00011", "00011", "01110", "00011", "00011", "11110" };
                case '4': return new[] { "00110", "01110", "11010", "11010", "11111", "00010", "00010" };
                case '5': return new[] { "11111", "11000", "11000", "11110", "00011", "00011", "11110" };
                case '6': return new[] { "01110", "11000", "11000", "11110", "11011", "11011", "01110" };
                case '7': return new[] { "11111", "00011", "00110", "00110", "01100", "01100", "01100" };
                default: throw new ArgumentOutOfRangeException(nameof(digit), digit, "Unsupported rank digit.");
            }
        }

        private static void DrawCourtPlaceholder(PixelCanvas canvas, CardSuit suit, CardRank rank, RectInt cell)
        {
            var center = new Vector2(cell.center.x, cell.center.y);
            var unit = cell.width / 256f;
            var primary = SuitPrimaryColor(suit);
            canvas.StrokeRoundedRect(
                new RectInt(
                    Mathf.RoundToInt(center.x - (86f * unit)),
                    Mathf.RoundToInt(center.y - (104f * unit)),
                    Mathf.RoundToInt(172f * unit),
                    Mathf.RoundToInt(208f * unit)),
                34f * unit,
                7f * unit,
                Lampblack);

            var rankOffset = (int)rank - 10;
            if (rankOffset == 0)
            {
                canvas.FillCircle(center + new Vector2(0f, 47f) * unit, 25f * unit, primary);
                canvas.FillPolygon(new[]
                {
                    center + new Vector2(-61f, -63f) * unit,
                    center + new Vector2(-38f, 24f) * unit,
                    center + new Vector2(38f, 24f) * unit,
                    center + new Vector2(61f, -63f) * unit,
                }, primary);
            }
            else if (rankOffset == 1)
            {
                canvas.FillCircle(center + new Vector2(-19f, 45f) * unit, 24f * unit, primary);
                canvas.FillPolygon(new[]
                {
                    center + new Vector2(-72f, -61f) * unit,
                    center + new Vector2(-48f, 18f) * unit,
                    center + new Vector2(42f, 36f) * unit,
                    center + new Vector2(68f, -45f) * unit,
                }, primary);
                canvas.DrawLine(center + new Vector2(-61f, -74f) * unit, center + new Vector2(64f, 69f) * unit, 10f * unit, Brass);
            }
            else
            {
                canvas.FillCircle(center + new Vector2(0f, 46f) * unit, 27f * unit, primary);
                canvas.FillPolygon(new[]
                {
                    center + new Vector2(-75f, -66f) * unit,
                    center + new Vector2(-57f, 21f) * unit,
                    center + new Vector2(0f, 7f) * unit,
                    center + new Vector2(57f, 21f) * unit,
                    center + new Vector2(75f, -66f) * unit,
                }, primary);
                canvas.FillCircle(center + new Vector2(-32f, 85f) * unit, 8f * unit, Brass);
                canvas.FillCircle(center + new Vector2(0f, 96f) * unit, 8f * unit, Brass);
                canvas.FillCircle(center + new Vector2(32f, 85f) * unit, 8f * unit, Brass);
            }

            DrawSuit(canvas, suit, new RectInt(
                Mathf.RoundToInt(center.x - (38f * unit)),
                Mathf.RoundToInt(center.y - (76f * unit)),
                Mathf.RoundToInt(76f * unit),
                Mathf.RoundToInt(76f * unit)));
        }

        private static Color32 SuitPrimaryColor(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Coins: return Brass;
                case CardSuit.Cups: return Madder;
                case CardSuit.Swords: return Woad;
                case CardSuit.Clubs: return Walnut;
                default: throw new ArgumentOutOfRangeException(nameof(suit), suit, null);
            }
        }

        private static PixelCanvas Supersampled(int width, int height, Action<PixelCanvas, int> draw)
        {
            const int scale = 2;
            var large = new PixelCanvas(width * scale, height * scale, Clear);
            draw(large, scale);
            return large.Downsample2X();
        }

        private static void WriteIfMissing(string path, PixelCanvas canvas)
        {
            if (!File.Exists(path))
            {
                WritePng(path, canvas);
            }
        }

        private static void WritePng(string path, PixelCanvas canvas)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Root);
            var texture = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false, false);
            try
            {
                texture.SetPixels32(canvas.Pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static PixelCanvas LoadPng(string path)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (!texture.LoadImage(File.ReadAllBytes(path), false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidOperationException($"Unable to decode {path}.");
            }

            var canvas = new PixelCanvas(texture.width, texture.height, texture.GetPixels32());
            UnityEngine.Object.DestroyImmediate(texture);
            return canvas;
        }

        private static Color32 Hex(int rgb)
        {
            return new Color32(
                (byte)((rgb >> 16) & 0xFF),
                (byte)((rgb >> 8) & 0xFF),
                (byte)(rgb & 0xFF),
                255);
        }

        private sealed class ComponentTextures : IDisposable
        {
            public ComponentTextures(PixelCanvas baseFace, PixelCanvas suitAtlas, PixelCanvas rankAtlas, PixelCanvas courtAtlas)
            {
                BaseFace = baseFace;
                SuitAtlas = suitAtlas;
                RankAtlas = rankAtlas;
                CourtAtlas = courtAtlas;
            }

            public PixelCanvas BaseFace { get; }
            public PixelCanvas SuitAtlas { get; }
            public PixelCanvas RankAtlas { get; }
            public PixelCanvas CourtAtlas { get; }

            public void Dispose()
            {
            }
        }

        private sealed class PixelCanvas
        {
            private readonly Color32[] _pixels;

            public PixelCanvas(int width, int height, Color32 fill)
            {
                Width = width;
                Height = height;
                _pixels = new Color32[width * height];
                Fill(fill);
            }

            public PixelCanvas(int width, int height, Color32[] pixels)
            {
                Width = width;
                Height = height;
                if (pixels == null || pixels.Length != width * height)
                {
                    throw new ArgumentException("Pixel data does not match canvas dimensions.", nameof(pixels));
                }

                _pixels = (Color32[])pixels.Clone();
            }

            public PixelCanvas(PixelCanvas source)
                : this(source.Width, source.Height, source._pixels)
            {
            }

            public int Width { get; }
            public int Height { get; }
            public Color32[] Pixels => (Color32[])_pixels.Clone();

            public void Fill(Color32 color)
            {
                for (var index = 0; index < _pixels.Length; index++)
                {
                    _pixels[index] = color;
                }
            }

            public void FillWithNoise(Color32 color, int amplitude, int seed)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        var hash = unchecked((x * 73856093) ^ (y * 19349663) ^ seed);
                        var positiveHash = hash & int.MaxValue;
                        var offset = (positiveHash % ((amplitude * 2) + 1)) - amplitude;
                        _pixels[(y * Width) + x] = new Color32(
                            ClampByte(color.r + offset),
                            ClampByte(color.g + offset),
                            ClampByte(color.b + offset),
                            color.a);
                    }
                }
            }

            public void FillRect(RectInt rect, Color32 color)
            {
                var clipped = Clip(rect);
                for (var y = clipped.yMin; y < clipped.yMax; y++)
                {
                    for (var x = clipped.xMin; x < clipped.xMax; x++)
                    {
                        Blend(x, y, color);
                    }
                }
            }

            public void FillRoundedRect(RectInt rect, float radius, Color32 color)
            {
                var clipped = Clip(rect);
                for (var y = clipped.yMin; y < clipped.yMax; y++)
                {
                    for (var x = clipped.xMin; x < clipped.xMax; x++)
                    {
                        if (InsideRoundedRect(x + 0.5f, y + 0.5f, rect, radius))
                        {
                            Blend(x, y, color);
                        }
                    }
                }
            }

            public void StrokeRoundedRect(RectInt rect, float radius, float thickness, Color32 color)
            {
                var clipped = Clip(rect);
                var inner = new RectInt(
                    Mathf.RoundToInt(rect.x + thickness),
                    Mathf.RoundToInt(rect.y + thickness),
                    Mathf.Max(0, Mathf.RoundToInt(rect.width - (thickness * 2f))),
                    Mathf.Max(0, Mathf.RoundToInt(rect.height - (thickness * 2f))));
                var innerRadius = Mathf.Max(0f, radius - thickness);

                for (var y = clipped.yMin; y < clipped.yMax; y++)
                {
                    for (var x = clipped.xMin; x < clipped.xMax; x++)
                    {
                        var px = x + 0.5f;
                        var py = y + 0.5f;
                        if (InsideRoundedRect(px, py, rect, radius) && !InsideRoundedRect(px, py, inner, innerRadius))
                        {
                            Blend(x, y, color);
                        }
                    }
                }
            }

            public void FillCircle(Vector2 center, float radius, Color32 color)
            {
                var bounds = Clip(new RectInt(
                    Mathf.FloorToInt(center.x - radius),
                    Mathf.FloorToInt(center.y - radius),
                    Mathf.CeilToInt(radius * 2f) + 1,
                    Mathf.CeilToInt(radius * 2f) + 1));
                var radiusSquared = radius * radius;
                for (var y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (var x = bounds.xMin; x < bounds.xMax; x++)
                    {
                        var delta = new Vector2((x + 0.5f) - center.x, (y + 0.5f) - center.y);
                        if (delta.sqrMagnitude <= radiusSquared)
                        {
                            Blend(x, y, color);
                        }
                    }
                }
            }

            public void StrokeCircle(Vector2 center, float radius, float thickness, Color32 color)
            {
                var bounds = Clip(new RectInt(
                    Mathf.FloorToInt(center.x - radius),
                    Mathf.FloorToInt(center.y - radius),
                    Mathf.CeilToInt(radius * 2f) + 1,
                    Mathf.CeilToInt(radius * 2f) + 1));
                var outerSquared = radius * radius;
                var innerSquared = Mathf.Max(0f, radius - thickness) * Mathf.Max(0f, radius - thickness);
                for (var y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (var x = bounds.xMin; x < bounds.xMax; x++)
                    {
                        var delta = new Vector2((x + 0.5f) - center.x, (y + 0.5f) - center.y);
                        if (delta.sqrMagnitude <= outerSquared && delta.sqrMagnitude >= innerSquared)
                        {
                            Blend(x, y, color);
                        }
                    }
                }
            }

            public void DrawLine(Vector2 start, Vector2 end, float thickness, Color32 color)
            {
                var half = thickness * 0.5f;
                var bounds = Clip(new RectInt(
                    Mathf.FloorToInt(Mathf.Min(start.x, end.x) - half),
                    Mathf.FloorToInt(Mathf.Min(start.y, end.y) - half),
                    Mathf.CeilToInt(Mathf.Abs(end.x - start.x) + thickness) + 1,
                    Mathf.CeilToInt(Mathf.Abs(end.y - start.y) + thickness) + 1));
                var segment = end - start;
                var lengthSquared = segment.sqrMagnitude;
                for (var y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (var x = bounds.xMin; x < bounds.xMax; x++)
                    {
                        var point = new Vector2(x + 0.5f, y + 0.5f);
                        var t = lengthSquared <= Mathf.Epsilon
                            ? 0f
                            : Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
                        var closest = start + (segment * t);
                        if ((point - closest).sqrMagnitude <= half * half)
                        {
                            Blend(x, y, color);
                        }
                    }
                }
            }

            public void FillPolygon(IReadOnlyList<Vector2> points, Color32 color)
            {
                if (points == null || points.Count < 3)
                {
                    return;
                }

                var minX = points.Min(point => point.x);
                var maxX = points.Max(point => point.x);
                var minY = points.Min(point => point.y);
                var maxY = points.Max(point => point.y);
                var bounds = Clip(new RectInt(
                    Mathf.FloorToInt(minX),
                    Mathf.FloorToInt(minY),
                    Mathf.CeilToInt(maxX - minX) + 1,
                    Mathf.CeilToInt(maxY - minY) + 1));

                for (var y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (var x = bounds.xMin; x < bounds.xMax; x++)
                    {
                        if (InsidePolygon(new Vector2(x + 0.5f, y + 0.5f), points))
                        {
                            Blend(x, y, color);
                        }
                    }
                }
            }

            public void Blit(PixelCanvas source, RectInt sourceRect, RectInt destinationRect, bool rotate180)
            {
                var clipped = Clip(destinationRect);
                for (var y = clipped.yMin; y < clipped.yMax; y++)
                {
                    var v = ((y + 0.5f) - destinationRect.y) / destinationRect.height;
                    for (var x = clipped.xMin; x < clipped.xMax; x++)
                    {
                        var u = ((x + 0.5f) - destinationRect.x) / destinationRect.width;
                        if (rotate180)
                        {
                            u = 1f - u;
                            v = 1f - v;
                        }

                        var sourceX = sourceRect.x + Mathf.Clamp(Mathf.FloorToInt(u * sourceRect.width), 0, sourceRect.width - 1);
                        var sourceY = sourceRect.y + Mathf.Clamp(Mathf.FloorToInt(v * sourceRect.height), 0, sourceRect.height - 1);
                        Blend(x, y, source.Get(sourceX, sourceY));
                    }
                }
            }

            public PixelCanvas Downsample2X()
            {
                if ((Width & 1) != 0 || (Height & 1) != 0)
                {
                    throw new InvalidOperationException("Supersampled canvas dimensions must be even.");
                }

                var result = new PixelCanvas(Width / 2, Height / 2, Clear);
                for (var y = 0; y < result.Height; y++)
                {
                    for (var x = 0; x < result.Width; x++)
                    {
                        var a = Get(x * 2, y * 2);
                        var b = Get((x * 2) + 1, y * 2);
                        var c = Get(x * 2, (y * 2) + 1);
                        var d = Get((x * 2) + 1, (y * 2) + 1);
                        result._pixels[(y * result.Width) + x] = new Color32(
                            (byte)((a.r + b.r + c.r + d.r) / 4),
                            (byte)((a.g + b.g + c.g + d.g) / 4),
                            (byte)((a.b + b.b + c.b + d.b) / 4),
                            (byte)((a.a + b.a + c.a + d.a) / 4));
                    }
                }

                return result;
            }

            private Color32 Get(int x, int y)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                {
                    return Clear;
                }

                return _pixels[(y * Width) + x];
            }

            private void Blend(int x, int y, Color32 source)
            {
                if (source.a == 0 || x < 0 || x >= Width || y < 0 || y >= Height)
                {
                    return;
                }

                var index = (y * Width) + x;
                if (source.a == 255)
                {
                    _pixels[index] = source;
                    return;
                }

                var destination = _pixels[index];
                var alpha = source.a / 255f;
                var inverse = 1f - alpha;
                _pixels[index] = new Color32(
                    (byte)Mathf.RoundToInt((source.r * alpha) + (destination.r * inverse)),
                    (byte)Mathf.RoundToInt((source.g * alpha) + (destination.g * inverse)),
                    (byte)Mathf.RoundToInt((source.b * alpha) + (destination.b * inverse)),
                    (byte)Mathf.RoundToInt(source.a + (destination.a * inverse)));
            }

            private RectInt Clip(RectInt rect)
            {
                var xMin = Mathf.Clamp(rect.xMin, 0, Width);
                var yMin = Mathf.Clamp(rect.yMin, 0, Height);
                var xMax = Mathf.Clamp(rect.xMax, 0, Width);
                var yMax = Mathf.Clamp(rect.yMax, 0, Height);
                return new RectInt(xMin, yMin, Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));
            }

            private static bool InsideRoundedRect(float x, float y, RectInt rect, float radius)
            {
                if (rect.width <= 0 || rect.height <= 0 || x < rect.xMin || x >= rect.xMax || y < rect.yMin || y >= rect.yMax)
                {
                    return false;
                }

                var clampedRadius = Mathf.Min(radius, Mathf.Min(rect.width, rect.height) * 0.5f);
                var nearestX = Mathf.Clamp(x, rect.xMin + clampedRadius, rect.xMax - clampedRadius);
                var nearestY = Mathf.Clamp(y, rect.yMin + clampedRadius, rect.yMax - clampedRadius);
                var deltaX = x - nearestX;
                var deltaY = y - nearestY;
                return (deltaX * deltaX) + (deltaY * deltaY) <= clampedRadius * clampedRadius;
            }

            private static bool InsidePolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
            {
                var inside = false;
                for (int current = 0, previous = polygon.Count - 1; current < polygon.Count; previous = current++)
                {
                    var a = polygon[current];
                    var b = polygon[previous];
                    var intersects = ((a.y > point.y) != (b.y > point.y)) &&
                                     (point.x < ((b.x - a.x) * (point.y - a.y) / ((b.y - a.y) + Mathf.Epsilon)) + a.x);
                    if (intersects)
                    {
                        inside = !inside;
                    }
                }

                return inside;
            }

            private static byte ClampByte(int value)
            {
                return (byte)Mathf.Clamp(value, 0, 255);
            }
        }
    }
}
