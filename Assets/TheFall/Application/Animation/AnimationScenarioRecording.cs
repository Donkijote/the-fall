using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Application.Animation
{
    public enum AnimationScenarioKind
    {
        DealerCardSelection,
        DealCard,
        OpeningRejection,
        OpeningPlacement,
        PlayCard,
        HandReflow,
        NormalCapture,
        CascadeCapture,
        CollectLeftovers,
    }

    public enum AnimationRecordingBeat
    {
        MatchStarted,
        DealerSelection,
        DealerChoice,
        Deal,
        OpeningRejection,
        OpeningPlacement,
        CardPlay,
        TablePlacement,
        NormalCapture,
        CascadeCapture,
        Canto,
        Score,
        FallScore,
        CleanTableScore,
        DealCompleted,
        Leftovers,
        Round,
        DealerRotation,
        TieExtension,
        TurnChanged,
        MatchCompleted,
        HandReflow,
        CaptureCollection = 23,
    }

    /// <summary>
    /// Immutable focused input for the presentation workbench. A recording contains only the
    /// domain facts and reusable beats needed to preview one isolated animation scenario.
    /// </summary>
    public sealed class AnimationScenarioRecording
    {
        private readonly IReadOnlyList<AnimationRecordingBeat> _previewBeats;
        private readonly IReadOnlyList<AnimationRecordingBeat> _warmupBeats;

        private AnimationScenarioRecording(
            AnimationScenarioKind kind,
            string displayName,
            AnimationRecordingBeat beatKind,
            MatchState initialState,
            RuleResult result,
            PlayerId actingPlayerId,
            Seat actingSeat,
            IReadOnlyList<AnimationRecordingBeat> previewBeats,
            params AnimationRecordingBeat[] warmupBeats)
        {
            Kind = kind;
            DisplayName = displayName;
            BeatKind = beatKind;
            InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            Result = result ?? throw new ArgumentNullException(nameof(result));
            ActingPlayerId = actingPlayerId;
            ActingSeat = actingSeat;
            _previewBeats = Array.AsReadOnly(
                new List<AnimationRecordingBeat>(
                    previewBeats ?? throw new ArgumentNullException(nameof(previewBeats)))
                    .ToArray());
            _warmupBeats = Array.AsReadOnly(warmupBeats ?? Array.Empty<AnimationRecordingBeat>());
        }

        public AnimationScenarioKind Kind { get; }

        public string DisplayName { get; }

        public AnimationRecordingBeat BeatKind { get; }

        public MatchState InitialState { get; }

        public RuleResult Result { get; }

        public PlayerId ActingPlayerId { get; }

        public Seat ActingSeat { get; }

        public IReadOnlyList<AnimationRecordingBeat> PreviewBeats => _previewBeats;

        public IReadOnlyList<AnimationRecordingBeat> WarmupBeats => _warmupBeats;

        public static IReadOnlyList<string> DisplayNames { get; } = CreateDisplayNames();

        public static AnimationScenarioRecording Create(AnimationScenarioKind kind, Seat actingSeat)
        {
            var context = new ScenarioContext(actingSeat);
            switch (kind)
            {
                case AnimationScenarioKind.DealerCardSelection:
                    return CreateDealerCardSelection(context);
                case AnimationScenarioKind.DealCard:
                    return CreateDealCard(context);
                case AnimationScenarioKind.OpeningRejection:
                    return CreateOpeningRejection(context);
                case AnimationScenarioKind.OpeningPlacement:
                    return CreateOpeningPlacement(context);
                case AnimationScenarioKind.PlayCard:
                    return CreateCardPlay(context, kind, "Play card", AnimationRecordingBeat.CardPlay);
                case AnimationScenarioKind.HandReflow:
                    return CreateCardPlay(
                        context,
                        kind,
                        "Reflow remaining hand",
                        AnimationRecordingBeat.HandReflow,
                        AnimationRecordingBeat.CardPlay);
                case AnimationScenarioKind.NormalCapture:
                    return CreateNormalCapture(context);
                case AnimationScenarioKind.CascadeCapture:
                    return CreateCascadeCapture(context);
                case AnimationScenarioKind.CollectLeftovers:
                    return CreateLeftovers(context);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static AnimationScenarioRecording CreateDealerCardSelection(ScenarioContext context)
        {
            var start = OneVersusOneRules.StartMatch(
                context.First,
                context.Second,
                RuleConfiguration.Standard,
                new PredictableRandomSource());
            var selectedCard = start.State.Deck.Cards[0];
            var result = new MatchSession(start.State).Submit(
                new SelectDealerCardIntent(start.State.GetPlayerAt(Seat.First).Player.Id, selectedCard));
            return Recording(
                context,
                AnimationScenarioKind.DealerCardSelection,
                "Select dealer card",
                AnimationRecordingBeat.DealerSelection,
                start.State,
                result.State,
                new DomainEvent[] { result.Events[0] });
        }

        private static AnimationScenarioRecording CreateDealCard(ScenarioContext context)
        {
            var actorDealt = context.Card(CardSuit.Coins, CardRank.Seven);
            var opponentDealt = context.Card(CardSuit.Cups, CardRank.Six);
            var opponentHand = new[]
            {
                context.Card(CardSuit.Cups, CardRank.Five),
                context.Card(CardSuit.Coins, CardRank.Ten),
            };
            var initialDeck = new List<Card>(Deck.CreateSpanishDeck().Cards);
            initialDeck.Remove(context.HandOne);
            initialDeck.Remove(context.HandTwo);
            initialDeck.Remove(opponentHand[0]);
            initialDeck.Remove(opponentHand[1]);
            var finalDeck = new List<Card>(initialDeck);
            finalDeck.Remove(actorDealt);
            finalDeck.Remove(opponentDealt);
            var initial = context.State(
                actorHand: new[] { context.HandOne, context.HandTwo },
                opponentHand: opponentHand,
                deck: initialDeck);
            var final = context.State(
                actorHand: new[] { context.HandOne, context.HandTwo, actorDealt },
                opponentHand: new[] { opponentHand[0], opponentHand[1], opponentDealt },
                deck: finalDeck);
            return Recording(context, AnimationScenarioKind.DealCard, "Deal one card",
                AnimationRecordingBeat.Deal, initial, final,
                new DomainEvent[]
                {
                    new CardDealtEvent(context.Actor.Id, actorDealt, 2),
                    new CardDealtEvent(context.Opponent.Id, opponentDealt, 2),
                });
        }

        private static AnimationScenarioRecording CreateOpeningRejection(ScenarioContext context)
        {
            var rejected = context.Card(CardSuit.Coins, CardRank.Five);
            var initialDeck = new List<Card>(Deck.CreateSpanishDeck().Cards);
            initialDeck.Remove(context.TableCard);
            initialDeck.Remove(rejected);
            var reinsertedDeckIndex = initialDeck.Count / 2;
            var finalDeck = new List<Card>(initialDeck);
            finalDeck.Insert(reinsertedDeckIndex, rejected);
            var initial = context.State(
                table: new[] { context.TableCard, rejected },
                deck: initialDeck);
            var final = context.State(
                table: new[] { context.TableCard },
                deck: finalDeck);
            return Recording(context, AnimationScenarioKind.OpeningRejection, "Reject opening card",
                AnimationRecordingBeat.OpeningRejection, initial, final,
                new DomainEvent[]
                {
                    new OpeningCardRejectedEvent(rejected, 1, reinsertedDeckIndex),
                });
        }

        private static AnimationScenarioRecording CreateOpeningPlacement(ScenarioContext context)
        {
            var opening = context.Card(CardSuit.Coins, CardRank.Five);
            var initialDeck = new List<Card>(Deck.CreateSpanishDeck().Cards);
            initialDeck.Remove(context.TableCard);
            var finalDeck = new List<Card>(initialDeck);
            finalDeck.Remove(opening);
            var initial = context.State(
                table: new[] { context.TableCard },
                deck: initialDeck);
            var final = context.State(
                table: new[] { context.TableCard, opening },
                deck: finalDeck);
            return Recording(context, AnimationScenarioKind.OpeningPlacement, "Place opening card",
                AnimationRecordingBeat.OpeningPlacement, initial, final,
                new DomainEvent[] { new OpeningCardPlacedEvent(opening, 1) });
        }

        private static AnimationScenarioRecording CreateCardPlay(
            ScenarioContext context,
            AnimationScenarioKind kind,
            string name,
            AnimationRecordingBeat beat,
            params AnimationRecordingBeat[] warmup)
        {
            var initial = context.State(
                actorHand: new[] { context.PlayedCard, context.HandOne, context.HandTwo },
                table: new[] { context.TableCard });
            var final = context.State(
                actorHand: new[] { context.HandOne, context.HandTwo },
                table: new[] { context.TableCard, context.PlayedCard });
            return Recording(context, kind, name, beat, initial, final,
                new DomainEvent[] { new CardPlayedEvent(context.Actor.Id, context.PlayedCard) }, warmup);
        }

        private static AnimationScenarioRecording CreateNormalCapture(ScenarioContext context)
        {
            var initial = context.State(
                actorHand: new[] { context.CaptureCard, context.HandOne, context.HandTwo },
                table: new[] { context.MatchingCard, context.TableCard });
            var final = context.State(
                actorHand: new[] { context.HandOne, context.HandTwo },
                actorCaptured: new[] { context.CaptureCard, context.MatchingCard },
                table: new[] { context.TableCard });
            return Recording(context, AnimationScenarioKind.NormalCapture, "Capture matching pair",
                AnimationRecordingBeat.NormalCapture, initial, final,
                new DomainEvent[]
                {
                    new CardPlayedEvent(context.Actor.Id, context.CaptureCard),
                    new CardsCapturedEvent(context.Actor.Id, new[] { context.CaptureCard, context.MatchingCard }),
                });
        }

        private static AnimationScenarioRecording CreateCascadeCapture(ScenarioContext context)
        {
            var initial = context.State(
                actorHand: new[] { context.CaptureCard, context.HandOne, context.HandTwo },
                table: new[]
                {
                    context.MatchingCard,
                    context.CascadeCard,
                    context.CascadeFour,
                    context.CascadeFive,
                    context.TableCard,
                });
            var final = context.State(
                actorHand: new[] { context.HandOne, context.HandTwo },
                actorCaptured: new[]
                {
                    context.CaptureCard,
                    context.MatchingCard,
                    context.CascadeCard,
                    context.CascadeFour,
                    context.CascadeFive,
                },
                table: new[] { context.TableCard });
            return RecordingWithPreview(
                context,
                AnimationScenarioKind.CascadeCapture,
                "Capture cascade card",
                AnimationRecordingBeat.CascadeCapture, initial, final,
                new DomainEvent[]
                {
                    new CardPlayedEvent(context.Actor.Id, context.CaptureCard),
                    new CardsCapturedEvent(
                        context.Actor.Id,
                        new[]
                        {
                            context.CaptureCard,
                            context.MatchingCard,
                            context.CascadeCard,
                            context.CascadeFour,
                            context.CascadeFive,
                        }),
                },
                new[]
                {
                    AnimationRecordingBeat.NormalCapture,
                    AnimationRecordingBeat.CascadeCapture,
                    AnimationRecordingBeat.CaptureCollection,
                });
        }

        private static AnimationScenarioRecording CreateLeftovers(ScenarioContext context)
        {
            var initial = context.State(
                actorCaptured: new[] { context.CaptureCard },
                table: new[] { context.MatchingCard, context.CascadeCard });
            var final = context.State(
                actorCaptured: new[] { context.CaptureCard, context.MatchingCard, context.CascadeCard });
            return Recording(context, AnimationScenarioKind.CollectLeftovers, "Collect leftover cards",
                AnimationRecordingBeat.Leftovers, initial, final,
                new DomainEvent[]
                {
                    new LeftoversCollectedEvent(
                        context.Actor.Id,
                        new[] { context.MatchingCard, context.CascadeCard }),
                });
        }

        private static AnimationScenarioRecording Recording(
            ScenarioContext context,
            AnimationScenarioKind kind,
            string name,
            AnimationRecordingBeat beat,
            MatchState initial,
            MatchState final,
            IReadOnlyList<DomainEvent> events,
            params AnimationRecordingBeat[] warmup)
        {
            return new AnimationScenarioRecording(
                kind,
                name,
                beat,
                initial,
                RuleResult.Accepted(final, events),
                context.Actor.Id,
                context.Actor.Seat,
                new[] { beat },
                warmup);
        }

        private static AnimationScenarioRecording RecordingWithPreview(
            ScenarioContext context,
            AnimationScenarioKind kind,
            string name,
            AnimationRecordingBeat beat,
            MatchState initial,
            MatchState final,
            IReadOnlyList<DomainEvent> events,
            IReadOnlyList<AnimationRecordingBeat> previewBeats,
            params AnimationRecordingBeat[] warmup)
        {
            return new AnimationScenarioRecording(
                kind,
                name,
                beat,
                initial,
                RuleResult.Accepted(final, events),
                context.Actor.Id,
                context.Actor.Seat,
                previewBeats,
                warmup);
        }

        private static IReadOnlyList<string> CreateDisplayNames()
        {
            var names = new string[Enum.GetValues(typeof(AnimationScenarioKind)).Length];
            for (var index = 0; index < names.Length; index++)
            {
                names[index] = Create((AnimationScenarioKind)index, Seat.First).DisplayName;
            }

            return Array.AsReadOnly(names);
        }

        private sealed class ScenarioContext
        {
            public ScenarioContext(Seat actingSeat)
            {
                if (actingSeat != Seat.First && actingSeat != Seat.Second)
                {
                    throw new ArgumentOutOfRangeException(nameof(actingSeat));
                }

                First = new Player(
                    new PlayerId("animation-seat-one"),
                    "Seat One",
                    Seat.First,
                    TeamId.One,
                    PlayerControl.Human);
                Second = new Player(
                    new PlayerId("animation-seat-two"),
                    "Seat Two",
                    Seat.Second,
                    TeamId.Two,
                    PlayerControl.Bot);
                Actor = actingSeat == Seat.First ? First : Second;
                Opponent = actingSeat == Seat.First ? Second : First;
            }

            public Player First { get; }

            public Player Second { get; }

            public Player Actor { get; }

            public Player Opponent { get; }

            public Card PlayedCard => Card(CardSuit.Coins, CardRank.Five);

            public Card CaptureCard => Card(CardSuit.Coins, CardRank.Two);

            public Card MatchingCard => Card(CardSuit.Cups, CardRank.Two);

            public Card CascadeCard => Card(CardSuit.Clubs, CardRank.Three);

            public Card CascadeFour => Card(CardSuit.Swords, CardRank.Four);

            public Card CascadeFive => Card(CardSuit.Cups, CardRank.Five);

            public Card TableCard => Card(CardSuit.Swords, CardRank.Ten);

            public Card HandOne => Card(CardSuit.Clubs, CardRank.Seven);

            public Card HandTwo => Card(CardSuit.Swords, CardRank.Twelve);

            public Card Card(CardSuit suit, CardRank rank)
            {
                return new Card(suit, rank);
            }

            public MatchState State(
                IReadOnlyList<Card> actorHand = null,
                IReadOnlyList<Card> opponentHand = null,
                IReadOnlyList<Card> table = null,
                IReadOnlyList<Card> actorCaptured = null,
                IReadOnlyList<Card> opponentCaptured = null,
                IReadOnlyList<Card> deck = null,
                int actorScore = 0,
                int opponentScore = 0,
                Seat? dealerSeat = null,
                Seat? currentSeat = null)
            {
                var actorState = new PlayerState(
                    Actor,
                    actorHand ?? Array.Empty<Card>(),
                    actorCaptured ?? Array.Empty<Card>());
                var opponentState = new PlayerState(
                    Opponent,
                    opponentHand ?? Array.Empty<Card>(),
                    opponentCaptured ?? Array.Empty<Card>());
                var firstState = Actor.Seat == Seat.First ? actorState : opponentState;
                var secondState = Actor.Seat == Seat.Second ? actorState : opponentState;
                var teamOneScore = Actor.TeamId == TeamId.One ? actorScore : opponentScore;
                var teamTwoScore = Actor.TeamId == TeamId.Two ? actorScore : opponentScore;
                return MatchState.CreateOneVersusOne(
                    firstState,
                    secondState,
                    dealerSeat ?? Opponent.Seat,
                    currentSeat ?? Actor.Seat,
                    table ?? Array.Empty<Card>(),
                    new Deck(deck ?? Array.Empty<Card>()),
                    teamOneScore: new Score(teamOneScore),
                    teamTwoScore: new Score(teamTwoScore));
            }
        }

        private sealed class PredictableRandomSource : IRandomSource
        {
            public int NextInt(int exclusiveUpperBound)
            {
                return 0;
            }
        }
    }
}
