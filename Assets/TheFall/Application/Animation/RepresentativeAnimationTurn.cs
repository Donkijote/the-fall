using System;
using TheFall.Domain;

namespace TheFall.Application.Animation
{
    /// <summary>
    /// Records one deterministic Fall, cascade, and clean-table turn for presentation experiments.
    /// The rule result is produced once through the application boundary; presentation only replays it.
    /// </summary>
    public sealed class RepresentativeAnimationTurn
    {
        private RepresentativeAnimationTurn(
            MatchState initialState,
            RuleResult result,
            PlayerId actingPlayerId,
            Seat actingSeat)
        {
            InitialState = initialState;
            Result = result;
            ActingPlayerId = actingPlayerId;
            ActingSeat = actingSeat;
        }

        public MatchState InitialState { get; }

        public RuleResult Result { get; }

        public PlayerId ActingPlayerId { get; }

        public Seat ActingSeat { get; }

        public static RepresentativeAnimationTurn Create(Seat actingSeat)
        {
            if (actingSeat != Seat.First && actingSeat != Seat.Second)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(actingSeat),
                    "The representative 1v1 recording supports the first and second seats.");
            }

            var firstId = new PlayerId("animation-seat-one");
            var secondId = new PlayerId("animation-seat-two");
            var first = new Player(firstId, "Seat One", Seat.First, TeamId.One, PlayerControl.Human);
            var second = new Player(secondId, "Seat Two", Seat.Second, TeamId.Two, PlayerControl.Bot);
            var actor = actingSeat == Seat.First ? first : second;
            var opponent = actingSeat == Seat.First ? second : first;
            var playedCard = new Card(CardSuit.Coins, CardRank.Two);
            var previousCard = new Card(CardSuit.Cups, CardRank.Two);
            var actorHand = new[]
            {
                playedCard,
                new Card(CardSuit.Clubs, CardRank.Five),
                new Card(CardSuit.Swords, CardRank.Twelve),
            };
            var opponentHand = new[]
            {
                new Card(CardSuit.Coins, CardRank.One),
                new Card(CardSuit.Cups, CardRank.Seven),
                new Card(CardSuit.Clubs, CardRank.Ten),
            };

            var firstState = new PlayerState(first, actingSeat == Seat.First ? actorHand : opponentHand);
            var secondState = new PlayerState(second, actingSeat == Seat.Second ? actorHand : opponentHand);
            var state = MatchState.CreateOneVersusOne(
                firstState,
                secondState,
                opponent.Seat,
                actor.Seat,
                new[]
                {
                    previousCard,
                    new Card(CardSuit.Clubs, CardRank.Three),
                    new Card(CardSuit.Swords, CardRank.Four),
                },
                new Deck(Array.Empty<Card>()),
                teamOneScore: actingSeat == Seat.First ? new Score(7) : new Score(4),
                teamTwoScore: actingSeat == Seat.Second ? new Score(7) : new Score(4),
                previousPlay: new PreviousPlay(opponent.Id, previousCard, false));
            var session = new MatchSession(state);
            var result = session.Submit(new PlayCardIntent(actor.Id, playedCard));

            if (!result.IsAccepted)
            {
                throw new InvalidOperationException("The representative animation recording must resolve successfully.");
            }

            return new RepresentativeAnimationTurn(state, result, actor.Id, actingSeat);
        }
    }
}
