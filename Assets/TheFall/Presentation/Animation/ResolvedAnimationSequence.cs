using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Presentation.Animation
{
    public enum ResolvedAnimationStepKind
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
        SynchronizeFinalState,
    }

    public sealed class ResolvedAnimationStep
    {
        private readonly IReadOnlyList<Card> _cards;

        internal ResolvedAnimationStep(
            ResolvedAnimationStepKind kind,
            DomainEvent sourceEvent,
            int sourceEventIndex,
            PlayerId playerId = default,
            TeamId teamId = default,
            Seat currentSeat = default,
            IEnumerable<Card> cards = null,
            int pointsAwarded = 0,
            Score total = default)
        {
            Kind = kind;
            SourceEvent = sourceEvent;
            SourceEventIndex = sourceEventIndex;
            PlayerId = playerId;
            TeamId = teamId;
            CurrentSeat = currentSeat;
            _cards = Array.AsReadOnly(new List<Card>(cards ?? Array.Empty<Card>()).ToArray());
            PointsAwarded = pointsAwarded;
            Total = total;
        }

        public ResolvedAnimationStepKind Kind { get; }

        public DomainEvent SourceEvent { get; }

        public int SourceEventIndex { get; }

        public PlayerId PlayerId { get; }

        public TeamId TeamId { get; }

        public Seat CurrentSeat { get; }

        public IReadOnlyList<Card> Cards => _cards;

        public int PointsAwarded { get; }

        public Score Total { get; }
    }

    /// <summary>
    /// Converts domain facts into reusable presentation beats, then composes those beats in a
    /// preset-owned order. The authoritative event list and final state remain untouched.
    /// </summary>
    public sealed class ResolvedAnimationSequence
    {
        private readonly IReadOnlyList<ResolvedAnimationStep> _steps;
        private readonly IReadOnlyList<DomainEvent> _sourceEvents;

        private ResolvedAnimationSequence(
            IEnumerable<ResolvedAnimationStep> steps,
            IReadOnlyList<DomainEvent> sourceEvents,
            MatchState finalState)
        {
            _steps = Array.AsReadOnly(new List<ResolvedAnimationStep>(steps).ToArray());
            _sourceEvents = Array.AsReadOnly(new List<DomainEvent>(sourceEvents).ToArray());
            FinalState = finalState;
        }

        public IReadOnlyList<ResolvedAnimationStep> Steps => _steps;

        public IReadOnlyList<DomainEvent> SourceEvents => _sourceEvents;

        public MatchState FinalState { get; }

        public static ResolvedAnimationSequence Create(
            IReadOnlyList<DomainEvent> resolvedEvents,
            MatchState finalState)
        {
            return Create(resolvedEvents, finalState, null);
        }

        public static ResolvedAnimationSequence Create(
            IReadOnlyList<DomainEvent> resolvedEvents,
            MatchState finalState,
            IReadOnlyList<ResolvedAnimationStepKind> beatOrder)
        {
            if (resolvedEvents == null)
            {
                throw new ArgumentNullException(nameof(resolvedEvents));
            }

            if (finalState == null)
            {
                throw new ArgumentNullException(nameof(finalState));
            }

            var mapped = MapEvents(resolvedEvents);
            var composed = new List<ResolvedAnimationStep>();
            if (beatOrder == null)
            {
                composed.AddRange(mapped);
            }
            else
            {
                var usedKinds = new HashSet<ResolvedAnimationStepKind>();
                foreach (var kind in beatOrder)
                {
                    if (kind == ResolvedAnimationStepKind.SynchronizeFinalState || !usedKinds.Add(kind))
                    {
                        continue;
                    }

                    foreach (var step in mapped)
                    {
                        if (step.Kind == kind)
                        {
                            composed.Add(step);
                        }
                    }
                }
            }

            composed.Add(new ResolvedAnimationStep(
                ResolvedAnimationStepKind.SynchronizeFinalState,
                null,
                resolvedEvents.Count));
            return new ResolvedAnimationSequence(composed, resolvedEvents, finalState);
        }

        private static List<ResolvedAnimationStep> MapEvents(IReadOnlyList<DomainEvent> resolvedEvents)
        {
            var steps = new List<ResolvedAnimationStep>();
            for (var eventIndex = 0; eventIndex < resolvedEvents.Count; eventIndex++)
            {
                var resolvedEvent = resolvedEvents[eventIndex];
                switch (resolvedEvent)
                {
                    case MatchStartedEvent _:
                        steps.Add(Step(ResolvedAnimationStepKind.MatchStarted, resolvedEvent, eventIndex));
                        break;
                    case DealerCardSelectedEvent selected:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.DealerSelection,
                            selected,
                            eventIndex,
                            playerId: selected.PlayerId,
                            cards: new[] { selected.Card }));
                        break;
                    case DealerSelectionTiedEvent _:
                    case DealerSelectedEvent _:
                    case DeckShuffledEvent _:
                        steps.Add(Step(ResolvedAnimationStepKind.DealerSelection, resolvedEvent, eventIndex));
                        break;
                    case DealerChoiceMadeEvent _:
                        steps.Add(Step(ResolvedAnimationStepKind.DealerChoice, resolvedEvent, eventIndex));
                        break;
                    case DealStartedEvent _:
                        steps.Add(Step(ResolvedAnimationStepKind.Deal, resolvedEvent, eventIndex));
                        break;
                    case CardDealtEvent dealt:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.Deal,
                            dealt,
                            eventIndex,
                            playerId: dealt.PlayerId,
                            cards: new[] { dealt.Card }));
                        break;
                    case OpeningCardRejectedEvent rejected:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.OpeningRejection,
                            rejected,
                            eventIndex,
                            cards: new[] { rejected.Card }));
                        break;
                    case OpeningCardPlacedEvent opening:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.OpeningPlacement,
                            opening,
                            eventIndex,
                            cards: new[] { opening.Card }));
                        break;
                    case CardPlayedEvent played:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.CardPlay,
                            played,
                            eventIndex,
                            playerId: played.PlayerId,
                            cards: new[] { played.Card }));
                        break;
                    case CardPlacedOnTableEvent placed:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.TablePlacement,
                            placed,
                            eventIndex,
                            playerId: placed.PlayerId,
                            cards: new[] { placed.Card }));
                        break;
                    case CardsCapturedEvent captured:
                        if (captured.Cards.Count < 2)
                        {
                            throw new InvalidOperationException(
                                "A resolved capture event must contain the played and same-rank cards.");
                        }

                        steps.Add(Step(
                            ResolvedAnimationStepKind.NormalCapture,
                            captured,
                            eventIndex,
                            playerId: captured.PlayerId,
                            cards: new[] { captured.Cards[0], captured.Cards[1] }));
                        for (var cardIndex = 2; cardIndex < captured.Cards.Count; cardIndex++)
                        {
                            steps.Add(Step(
                                ResolvedAnimationStepKind.CascadeCapture,
                                captured,
                                eventIndex,
                                playerId: captured.PlayerId,
                                cards: new[] { captured.Cards[cardIndex] }));
                        }

                        break;
                    case CantoAnnouncedEvent announced:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.Canto,
                            announced,
                            eventIndex,
                            playerId: announced.PlayerId));
                        break;
                    case CantoResolvedEvent canto:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.Canto,
                            canto,
                            eventIndex,
                            playerId: canto.PlayerId));
                        break;
                    case ScoreChangedEvent score:
                        steps.Add(Step(
                            ResolveScoreKind(score.Reason),
                            score,
                            eventIndex,
                            teamId: score.TeamId,
                            pointsAwarded: score.PointsAwarded,
                            total: score.Total));
                        break;
                    case DealCompletedEvent _:
                        steps.Add(Step(ResolvedAnimationStepKind.DealCompleted, resolvedEvent, eventIndex));
                        break;
                    case LeftoversCollectedEvent leftovers:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.Leftovers,
                            leftovers,
                            eventIndex,
                            playerId: leftovers.PlayerId,
                            cards: leftovers.Cards));
                        break;
                    case RoundCompletedEvent _:
                        steps.Add(Step(ResolvedAnimationStepKind.Round, resolvedEvent, eventIndex));
                        break;
                    case DealerRotatedEvent rotated:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.DealerRotation,
                            rotated,
                            eventIndex,
                            currentSeat: rotated.CurrentDealerSeat));
                        break;
                    case TieExtensionStartedEvent _:
                        steps.Add(Step(ResolvedAnimationStepKind.TieExtension, resolvedEvent, eventIndex));
                        break;
                    case TurnChangedEvent turn:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.TurnChanged,
                            turn,
                            eventIndex,
                            currentSeat: turn.CurrentSeat));
                        break;
                    case MatchCompletedEvent completed:
                        steps.Add(Step(
                            ResolvedAnimationStepKind.MatchCompleted,
                            completed,
                            eventIndex,
                            teamId: completed.WinnerTeam));
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"No presentation beat mapping exists for {resolvedEvent.GetType().Name}.");
                }
            }

            return steps;
        }

        private static ResolvedAnimationStepKind ResolveScoreKind(ScoreReason reason)
        {
            switch (reason)
            {
                case ScoreReason.Fall:
                    return ResolvedAnimationStepKind.FallScore;
                case ScoreReason.CleanTable:
                    return ResolvedAnimationStepKind.CleanTableScore;
                default:
                    return ResolvedAnimationStepKind.Score;
            }
        }

        private static ResolvedAnimationStep Step(
            ResolvedAnimationStepKind kind,
            DomainEvent sourceEvent,
            int sourceEventIndex,
            PlayerId playerId = default,
            TeamId teamId = default,
            Seat currentSeat = default,
            IEnumerable<Card> cards = null,
            int pointsAwarded = 0,
            Score total = default)
        {
            return new ResolvedAnimationStep(
                kind,
                sourceEvent,
                sourceEventIndex,
                playerId,
                teamId,
                currentSeat,
                cards,
                pointsAwarded,
                total);
        }
    }
}
