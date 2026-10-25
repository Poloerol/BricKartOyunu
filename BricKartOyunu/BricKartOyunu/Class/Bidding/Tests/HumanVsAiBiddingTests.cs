using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Tests
{
    public static class HumanVsAiBiddingTests
    {
        public static void RunAll()
        {
            var failures = new List<string>();

            if (!PassSequenceEndsAuction())
                failures.Add("PassSequenceEndsAuction");

            if (!DoubleAndRedoubleRulesAreLegal())
                failures.Add("DoubleAndRedoubleRulesAreLegal");

            if (!AiTurnProgressesToNextPlayer())
                failures.Add("AiTurnProgressesToNextPlayer");

            if (failures.Count > 0)
                throw new InvalidOperationException("HumanVsAiBidding test failure: " + string.Join(", ", failures));
        }

        public static bool PassSequenceEndsAuction()
        {
            var state = new BiddingState
            {
                CurrentPlayer = Player.Guney,
                Dealer = Player.Guney
            };

            state.ApplyBid(Player.Guney, Bid.Pass());
            state.ApplyBid(Player.Bati, Bid.Pass());
            state.ApplyBid(Player.Dogu, Bid.Pass());
            state.ApplyBid(Player.Kuzey, Bid.Pass());

            return state.IsAuctionFinished() && state.PassCount == 4;
        }

        public static bool DoubleAndRedoubleRulesAreLegal()
        {
            var state = new BiddingState();
            state.ApplyBid(Player.Kuzey, Bid.Contract(1, BidSuit.Spades));

            var doubleLegal = BidValidator.IsLegal(state, Player.Guney, Bid.Double());
            state.ApplyBid(Player.Guney, Bid.Double());

            var redoubleLegal = BidValidator.IsLegal(state, Player.Bati, Bid.Redouble());
            state.ApplyBid(Player.Bati, Bid.Redouble());

            var secondDoubleIllegal = BidValidator.IsLegal(state, Player.Dogu, Bid.Double());
            return doubleLegal && redoubleLegal && !secondDoubleIllegal;
        }

        public static bool AiTurnProgressesToNextPlayer()
        {
            var hands = new Dictionary<Player, List<Card>>
            {
                { Player.Kuzey, TestHelpers.ElOlustur("AKQJ", "A32", "AKQ", "AKQ") },
                { Player.Guney, TestHelpers.ElOlustur("KQJ2", "KQJ", "A32", "AQJ") },
                { Player.Bati, TestHelpers.ElOlustur("AQJ2", "AQJ", "AKQ", "A32") },
                { Player.Dogu, TestHelpers.ElOlustur("QJ32", "QJ3", "AKQ", "A32") }
            };

            var session = new HumanVsAiBiddingSession(Player.Kuzey, hands, Player.Guney);
            session.State.CurrentPlayer = Player.Bati;

            var bid = session.PerformAiTurn(Player.Bati);
            return bid != null && session.State.CurrentPlayer == Player.Dogu;
        }
    }
}
