using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    public sealed class BiddingPresenter
    {
        private readonly HumanVsAiBiddingSession _session;

        public BiddingPresenter(Player dealer, Dictionary<Player, List<Card>> hands, Player humanPlayer)
        {
            if (hands == null)
                throw new ArgumentNullException(nameof(hands));

            if (!hands.ContainsKey(humanPlayer))
                throw new ArgumentException("İnsan elinin kartları tanımlı değil.", nameof(hands));

            _session = new HumanVsAiBiddingSession(dealer, hands, humanPlayer);
        }

        public HumanVsAiBiddingSession Session => _session;

        public BiddingState State => _session.State;

        public bool IsHumanTurn => _session.IsHumanTurn;

        public bool TryHumanBid(string bidText)
        {
            return _session.TryHumanBid(bidText);
        }

        public Bid PerformAiTurn(Player aiPlayer)
        {
            return _session.PerformAiTurn(aiPlayer);
        }

        public void SetCurrentPlayer(Player player)
        {
            State.CurrentPlayer = player;
        }

        public void ApplyHistory(IEnumerable<string> bidTexts, Player startingPlayer)
        {
            if (bidTexts == null)
                return;

            State.Reset(State.Dealer, startingPlayer);

            var currentPlayer = startingPlayer;
            foreach (var bidText in bidTexts)
            {
                if (string.IsNullOrWhiteSpace(bidText))
                    continue;

                var parsed = Bid.Parse(bidText);
                if (!BidValidator.IsLegal(State, currentPlayer, parsed))
                    continue;

                State.ApplyBid(currentPlayer, parsed);
                currentPlayer = State.CurrentPlayer;
            }

            State.CurrentPlayer = currentPlayer;
        }

        public List<string> GetBidHistoryStrings()
        {
            return State.History
                .Select(x => x.Bid.ToDisplayString())
                .ToList();
        }

        public string GetLastContractDisplay()
        {
            var lastContract = State.LastRealBid();
            return lastContract.IsContract ? lastContract.ToDisplayString() : string.Empty;
        }

        public bool IsAuctionFinished()
        {
            return State.IsAuctionFinished();
        }
    }
}
