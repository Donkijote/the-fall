using System;
using System.Collections.Generic;
using TheFall.Domain;

namespace TheFall.Application.Interaction
{
    /// <summary>
    /// Deterministic application fixture used by the V0 interaction prototype.
    /// It exercises an equal-rank capture and cascade without adding rules to presentation.
    /// </summary>
    public sealed class RepresentativeCardTurn
    {
        private static readonly PlayerId LocalId = new PlayerId("local-human");
        private static readonly PlayerId BotId = new PlayerId("prototype-bot");

        private RepresentativeCardTurn(
            CardInteractionSession interaction,
            IReadOnlyList<Card> localHand)
        {
            Interaction = interaction;
            LocalHand = localHand;
        }

        public CardInteractionSession Interaction { get; }

        public IReadOnlyList<Card> LocalHand { get; }

        public static RepresentativeCardTurn Create()
        {
            var localHand = Array.AsReadOnly(new[]
            {
                new Card(CardSuit.Coins, CardRank.Two),
                new Card(CardSuit.Clubs, CardRank.Five),
                new Card(CardSuit.Swords, CardRank.Twelve),
            });
            var botHand = new[]
            {
                new Card(CardSuit.Coins, CardRank.One),
                new Card(CardSuit.Cups, CardRank.Seven),
                new Card(CardSuit.Clubs, CardRank.Ten),
            };
            var table = new[]
            {
                new Card(CardSuit.Cups, CardRank.Two),
                new Card(CardSuit.Clubs, CardRank.Three),
                new Card(CardSuit.Swords, CardRank.Four),
                new Card(CardSuit.Cups, CardRank.Six),
            };
            var local = new Player(LocalId, "Local", Seat.First, TeamId.One, PlayerControl.Human);
            var bot = new Player(BotId, "Bot", Seat.Second, TeamId.Two, PlayerControl.Bot);
            var state = MatchState.CreateOneVersusOne(
                new PlayerState(local, localHand),
                new PlayerState(bot, botHand),
                Seat.Second,
                Seat.First,
                table,
                new Deck(Array.Empty<Card>()));
            var match = new MatchSession(state);
            return new RepresentativeCardTurn(
                new CardInteractionSession(match, LocalId),
                localHand);
        }
    }
}
