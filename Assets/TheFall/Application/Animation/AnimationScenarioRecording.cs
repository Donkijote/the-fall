using System;
using TheFall.Domain;

namespace TheFall.Application.Animation
{
    public enum AnimationScenarioKind
    {
        FallCascadeAndCleanTable,
        NonCapturingPlacement,
    }

    /// <summary>
    /// Immutable input for the presentation workbench. Each scenario resolves through MatchSession
    /// once; transport and tuning replay the recorded result without invoking the rules again.
    /// </summary>
    public sealed class AnimationScenarioRecording
    {
        private AnimationScenarioRecording(
            AnimationScenarioKind kind,
            string displayName,
            MatchState initialState,
            RuleResult result,
            PlayerId actingPlayerId,
            Seat actingSeat)
        {
            Kind = kind;
            DisplayName = displayName;
            InitialState = initialState;
            Result = result;
            ActingPlayerId = actingPlayerId;
            ActingSeat = actingSeat;
        }

        public AnimationScenarioKind Kind { get; }

        public string DisplayName { get; }

        public MatchState InitialState { get; }

        public RuleResult Result { get; }

        public PlayerId ActingPlayerId { get; }

        public Seat ActingSeat { get; }

        public static AnimationScenarioRecording Create(AnimationScenarioKind kind, Seat actingSeat)
        {
            switch (kind)
            {
                case AnimationScenarioKind.FallCascadeAndCleanTable:
                    var representative = RepresentativeAnimationTurn.Create(actingSeat);
                    return new AnimationScenarioRecording(
                        kind,
                        "Fall, cascade, and clean table",
                        representative.InitialState,
                        representative.Result,
                        representative.ActingPlayerId,
                        representative.ActingSeat);
                case AnimationScenarioKind.NonCapturingPlacement:
                    return CreateNonCapturingPlacement(actingSeat);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static AnimationScenarioRecording CreateNonCapturingPlacement(Seat actingSeat)
        {
            if (actingSeat != Seat.First && actingSeat != Seat.Second)
            {
                throw new ArgumentOutOfRangeException(nameof(actingSeat));
            }

            var firstId = new PlayerId("placement-seat-one");
            var secondId = new PlayerId("placement-seat-two");
            var first = new Player(firstId, "Seat One", Seat.First, TeamId.One, PlayerControl.Human);
            var second = new Player(secondId, "Seat Two", Seat.Second, TeamId.Two, PlayerControl.Bot);
            var actor = actingSeat == Seat.First ? first : second;
            var playedCard = new Card(CardSuit.Coins, CardRank.Five);
            var actorHand = new[]
            {
                playedCard,
                new Card(CardSuit.Clubs, CardRank.Seven),
                new Card(CardSuit.Swords, CardRank.Ten),
            };
            var opponentHand = new[]
            {
                new Card(CardSuit.Cups, CardRank.One),
                new Card(CardSuit.Clubs, CardRank.Three),
                new Card(CardSuit.Swords, CardRank.Twelve),
            };
            var initialState = MatchState.CreateOneVersusOne(
                new PlayerState(first, actingSeat == Seat.First ? actorHand : opponentHand),
                new PlayerState(second, actingSeat == Seat.Second ? actorHand : opponentHand),
                actingSeat == Seat.First ? Seat.Second : Seat.First,
                actingSeat,
                new[] { new Card(CardSuit.Cups, CardRank.Two) },
                new Deck(Array.Empty<Card>()));
            var result = new MatchSession(initialState).Submit(new PlayCardIntent(actor.Id, playedCard));
            if (!result.IsAccepted)
            {
                throw new InvalidOperationException("The non-capturing animation recording must resolve successfully.");
            }

            return new AnimationScenarioRecording(
                AnimationScenarioKind.NonCapturingPlacement,
                "Play and table placement",
                initialState,
                result,
                actor.Id,
                actingSeat);
        }
    }
}
