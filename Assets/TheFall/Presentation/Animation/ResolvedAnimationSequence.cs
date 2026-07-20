using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Presentation.Animation
{
    public enum ResolvedAnimationStepKind
    {
        CardPlay,
        TablePlacement,
        NormalCapture,
        CascadeCapture,
        FallScore,
        CleanTableScore,
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
            PlayerId playerId = default,
            TeamId teamId = default,
            Seat currentSeat = default,
            IEnumerable<Card> cards = null,
            int pointsAwarded = 0,
            Score total = default)
        {
            Kind = kind;
            SourceEvent = sourceEvent;
            PlayerId = playerId;
            TeamId = teamId;
            CurrentSeat = currentSeat;
            _cards = Array.AsReadOnly(new List<Card>(cards ?? Array.Empty<Card>()).ToArray());
            PointsAwarded = pointsAwarded;
            Total = total;
        }

        public ResolvedAnimationStepKind Kind { get; }

        public DomainEvent SourceEvent { get; }

        public PlayerId PlayerId { get; }

        public TeamId TeamId { get; }

        public Seat CurrentSeat { get; }

        public IReadOnlyList<Card> Cards => _cards;

        public int PointsAwarded { get; }

        public Score Total { get; }
    }

    /// <summary>
    /// Converts facts already resolved by the domain into deterministic presentation beats.
    /// It does not infer captures, cascades, scoring, or legal outcomes from card state.
    /// </summary>
    public sealed class ResolvedAnimationSequence
    {
        private readonly IReadOnlyList<ResolvedAnimationStep> _steps;

        private ResolvedAnimationSequence(
            IEnumerable<ResolvedAnimationStep> steps,
            MatchState finalState)
        {
            _steps = Array.AsReadOnly(new List<ResolvedAnimationStep>(steps).ToArray());
            FinalState = finalState;
        }

        public IReadOnlyList<ResolvedAnimationStep> Steps => _steps;

        public MatchState FinalState { get; }

        public static ResolvedAnimationSequence Create(
            IReadOnlyList<DomainEvent> resolvedEvents,
            MatchState finalState)
        {
            if (resolvedEvents == null)
            {
                throw new ArgumentNullException(nameof(resolvedEvents));
            }

            if (finalState == null)
            {
                throw new ArgumentNullException(nameof(finalState));
            }

            var steps = new List<ResolvedAnimationStep>();
            foreach (var resolvedEvent in resolvedEvents)
            {
                if (resolvedEvent is CardPlayedEvent cardPlayed)
                {
                    steps.Add(new ResolvedAnimationStep(
                        ResolvedAnimationStepKind.CardPlay,
                        cardPlayed,
                        playerId: cardPlayed.PlayerId,
                        cards: new[] { cardPlayed.Card }));
                    continue;
                }

                if (resolvedEvent is CardPlacedOnTableEvent cardPlaced)
                {
                    steps.Add(new ResolvedAnimationStep(
                        ResolvedAnimationStepKind.TablePlacement,
                        cardPlaced,
                        playerId: cardPlaced.PlayerId,
                        cards: new[] { cardPlaced.Card }));
                    continue;
                }

                if (resolvedEvent is CardsCapturedEvent captured)
                {
                    if (captured.Cards.Count < 2)
                    {
                        throw new InvalidOperationException(
                            "A resolved capture event must contain the played and same-rank cards.");
                    }

                    steps.Add(new ResolvedAnimationStep(
                        ResolvedAnimationStepKind.NormalCapture,
                        captured,
                        playerId: captured.PlayerId,
                        cards: new[] { captured.Cards[0], captured.Cards[1] }));
                    for (var index = 2; index < captured.Cards.Count; index++)
                    {
                        steps.Add(new ResolvedAnimationStep(
                            ResolvedAnimationStepKind.CascadeCapture,
                            captured,
                            playerId: captured.PlayerId,
                            cards: new[] { captured.Cards[index] }));
                    }

                    continue;
                }

                if (resolvedEvent is ScoreChangedEvent scoreChanged)
                {
                    ResolvedAnimationStepKind kind;
                    switch (scoreChanged.Reason)
                    {
                        case ScoreReason.Fall:
                            kind = ResolvedAnimationStepKind.FallScore;
                            break;
                        case ScoreReason.CleanTable:
                            kind = ResolvedAnimationStepKind.CleanTableScore;
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"No presentation score beat exists for {scoreChanged.Reason}.");
                    }
                    steps.Add(new ResolvedAnimationStep(
                        kind,
                        scoreChanged,
                        teamId: scoreChanged.TeamId,
                        pointsAwarded: scoreChanged.PointsAwarded,
                        total: scoreChanged.Total));
                    continue;
                }

                if (resolvedEvent is TurnChangedEvent turnChanged)
                {
                    steps.Add(new ResolvedAnimationStep(
                        ResolvedAnimationStepKind.TurnChanged,
                        turnChanged,
                        currentSeat: turnChanged.CurrentSeat));
                    continue;
                }

                if (resolvedEvent is MatchCompletedEvent matchCompleted)
                {
                    steps.Add(new ResolvedAnimationStep(
                        ResolvedAnimationStepKind.MatchCompleted,
                        matchCompleted,
                        teamId: matchCompleted.WinnerTeam));
                    continue;
                }

                throw new InvalidOperationException(
                    $"No presentation sequence mapping exists for {resolvedEvent.GetType().Name}.");
            }

            steps.Add(new ResolvedAnimationStep(
                ResolvedAnimationStepKind.SynchronizeFinalState,
                null));
            return new ResolvedAnimationSequence(steps, finalState);
        }
    }
}
