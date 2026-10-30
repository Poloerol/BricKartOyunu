using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    public enum BidSuit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades,
        NT
    }

    public enum BidType
    {
        Pass,
        Bid,
        Double,
        Redouble
    }

    public sealed class Bid : IEquatable<Bid>
    {
        public BidType Type { get; private set; }
        public int Level { get; private set; }
        public BidSuit Suit { get; private set; }

        private Bid()
        {
        }

        public static Bid Pass()
        {
            return new Bid { Type = BidType.Pass, Level = 0, Suit = BidSuit.NT };
        }

        public static Bid Double()
        {
            return new Bid { Type = BidType.Double, Level = 0, Suit = BidSuit.NT };
        }

        public static Bid Redouble()
        {
            return new Bid { Type = BidType.Redouble, Level = 0, Suit = BidSuit.NT };
        }

        public static Bid Contract(int level, BidSuit suit)
        {
            if (level < 1 || level > 7)
                throw new ArgumentOutOfRangeException(nameof(level));

            return new Bid
            {
                Type = BidType.Bid,
                Level = level,
                Suit = suit
            };
        }

        public bool IsPass => Type == BidType.Pass;
        public bool IsDouble => Type == BidType.Double;
        public bool IsRedouble => Type == BidType.Redouble;
        public bool IsContract => Type == BidType.Bid;

        public string ToDisplayString()
        {
            if (IsPass) return "Pas";
            if (IsDouble) return "Dbl";
            if (IsRedouble) return "RDbl";

            return string.Format("{0}{1}", Level, SuitToSymbol(Suit));
        }

        public static string SuitToSymbol(BidSuit suit)
        {
            switch (suit)
            {
                case BidSuit.Clubs:
                    return "♣";
                case BidSuit.Diamonds:
                    return "♦";
                case BidSuit.Hearts:
                    return "♥";
                case BidSuit.Spades:
                    return "♠";
                case BidSuit.NT:
                    return "NT";
                default:
                    return string.Empty;
            }
        }

        public static Bid Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Pass();

            var trimmed = value.Trim();

            if (trimmed.Equals("Pas", StringComparison.OrdinalIgnoreCase))
                return Pass();

            if (trimmed.Equals("Dbl", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("Kontr", StringComparison.OrdinalIgnoreCase))
                return Double();

            if (trimmed.Equals("RDbl", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("S.Kontr", StringComparison.OrdinalIgnoreCase))
                return Redouble();

            if (trimmed.Length < 2)
                throw new ArgumentException("Geçersiz teklif: " + value);

            var levelChar = trimmed[0];
            int level;
            if (!int.TryParse(levelChar.ToString(), out level))
                throw new ArgumentException("Geçersiz seviye: " + value);

            var suitPart = trimmed.Substring(1).Trim();

            BidSuit suit;
            switch (suitPart)
            {
                case "♣":
                case "C":
                case "Sinek":
                    suit = BidSuit.Clubs;
                    break;
                case "♦":
                case "D":
                case "Karo":
                    suit = BidSuit.Diamonds;
                    break;
                case "♥":
                case "H":
                case "Kupa":
                    suit = BidSuit.Hearts;
                    break;
                case "♠":
                case "S":
                case "Maça":
                    suit = BidSuit.Spades;
                    break;
                case "NT":
                case "N":
                    suit = BidSuit.NT;
                    break;
                default:
                    throw new ArgumentException("Geçersiz koz: " + value);
            }

            return Contract(level, suit);
        }

        public override string ToString()
        {
            return ToDisplayString();
        }

        public bool Equals(Bid other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Type == other.Type && Level == other.Level && Suit == other.Suit;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Bid);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + Level.GetHashCode();
                hash = hash * 23 + Suit.GetHashCode();
                return hash;
            }
        }
    }

    public sealed class BidHistoryEntry
    {
        public Player Player { get; set; }
        public Bid Bid { get; set; }
        public int TurnNumber { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string ToDisplay()
        {
            return string.Format("{0} - {1}", Player, Bid.ToDisplayString());
        }
    }

    public sealed class BiddingState
    {
        public Player CurrentPlayer { get; set; } = Player.Guney;
        public Player Dealer { get; set; } = Player.Guney;
        public List<BidHistoryEntry> History { get; set; } = new List<BidHistoryEntry>();
        public int PassCount { get; set; }
        public Bid LastContract { get; private set; } = Bid.Pass();
        public Player? LastContractMaker { get; private set; }
        public bool AuctionFinished { get; private set; }

        public void Reset(Player dealer, Player? currentPlayer = null)
        {
            Dealer = dealer;
            CurrentPlayer = currentPlayer ?? dealer;
            History.Clear();
            PassCount = 0;
            LastContract = Bid.Pass();
            LastContractMaker = null;
            AuctionFinished = false;
        }

        public void ApplyBid(Player player, Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException(nameof(bid));

            if (AuctionFinished)
                throw new InvalidOperationException("İhale zaten bitti.");

            History.Add(new BidHistoryEntry
            {
                Player = player,
                Bid = bid,
                TurnNumber = History.Count + 1
            });

            if (bid.IsContract)
            {
                LastContract = bid;
                LastContractMaker = player;
                PassCount = 0;
            }
            else if (bid.IsPass)
            {
                PassCount++;
            }
            else
            {
                PassCount = 0;
            }

            CurrentPlayer = NextPlayer(player);
            AuctionFinished = IsAuctionFinished();
        }

        public Player NextPlayer(Player player)
        {
            var order = new[] { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney };
            var idx = Array.IndexOf(order, player);
            return order[(idx + 1) % order.Length];
        }

        public Bid LastRealBid()
        {
            for (int i = History.Count - 1; i >= 0; i--)
            {
                var bid = History[i].Bid;
                if (bid.IsContract)
                    return bid;
            }

            return Bid.Pass();
        }

        public bool IsAuctionFinished()
        {
            if (History.Count == 0)
                return false;

            if (PassCount >= 4)
                return true;

            var realBids = History
                .Where(x => x.Bid.IsContract)
                .Select(x => x.Bid)
                .ToList();

            if (realBids.Count == 0)
                return false;

            var lastThree = History.Skip(Math.Max(0, History.Count - 3)).ToList();
            if (lastThree.Count == 3 && lastThree.All(x => x.Bid.IsPass))
                return true;

            return false;
        }
    }

    public static class BidValidator
    {
        public static bool IsLegal(BiddingState state, Player player, Bid bid)
        {
            if (state == null || bid == null)
                return false;

            if (state.AuctionFinished)
                return false;

            if (bid.IsPass)
                return true;

            if (bid.IsDouble || bid.IsRedouble)
                return IsDoubleRedoubleLegal(state, bid);

            return IsContractLegal(state, bid);
        }

        private static bool IsContractLegal(BiddingState state, Bid bid)
        {
            if (state.History.Count == 0)
                return true;

            var lastContract = state.LastRealBid();
            if (lastContract.IsPass)
                return true;

            if (bid.Level < lastContract.Level)
                return false;

            if (bid.Level == lastContract.Level && bid.Suit == lastContract.Suit)
                return false;

            return true;
        }

        private static bool IsDoubleRedoubleLegal(BiddingState state, Bid bid)
        {
            if (state.History.Count == 0)
                return false;

            var lastBid = state.History.Last().Bid;
            if (!lastBid.IsContract)
                return false;

            if (bid.IsDouble)
            {
                return !state.History.Any(x => x.Bid.IsDouble || x.Bid.IsRedouble);
            }

            if (bid.IsRedouble)
            {
                var prevEntry = state.History.Take(state.History.Count - 1).LastOrDefault();
                return prevEntry != null && prevEntry.Bid.IsDouble;
            }

            return false;
        }
    }

    public interface IBidAiStrategy
    {
        Bid Decide(BiddingState state, Player aiPlayer, List<Card> hand);
    }

    public sealed class BasicBidAiStrategy : IBidAiStrategy
    {
        public Bid Decide(BiddingState state, Player aiPlayer, List<Card> hand)
        {
            if (hand == null || hand.Count == 0)
                return Bid.Pass();

            var hcp = ElDegerlendirici.HCP(hand);
            var renkler = ElDegerlendirici.RenkSayilari(hand);
            bool balanced = ElDegerlendirici.DengeliEl(hand);

            if (state.History.Count == 0)
            {
                if (hcp >= 15 && balanced)
                    return Bid.Contract(1, BidSuit.NT);

                var major = BestMajorOpening(renkler);
                if (major.HasValue && hcp >= 11)
                    return Bid.Contract(1, major.Value);

                var minor = BestMinorOpening(renkler);
                if (minor.HasValue && hcp >= 12)
                    return Bid.Contract(1, minor.Value);

                return Bid.Pass();
            }

            var lastBid = state.History.LastOrDefault();
            if (lastBid == null)
                return Bid.Pass();

            if (lastBid.Bid.IsDouble)
            {
                return hcp >= 10 ? Bid.Redouble() : Bid.Pass();
            }

            if (lastBid.Bid.IsRedouble)
                return Bid.Pass();

            var lastRealBid = state.LastRealBid();
            if (!lastRealBid.IsContract)
                return Bid.Pass();

            if (state.PassCount >= 3)
                return Bid.Pass();

            if (lastRealBid.Suit == BidSuit.NT)
            {
                if (hcp >= 8 && balanced)
                    return Bid.Contract(lastRealBid.Level + 1, BidSuit.NT);
                return Bid.Pass();
            }

            var suitName = SuitName(lastRealBid.Suit);
            var suitLength = renkler[suitName];
            if (suitLength >= 4 && hcp >= 6)
            {
                return Bid.Contract(lastRealBid.Level + 1, lastRealBid.Suit);
            }

            if (suitLength >= 5 && hcp >= 3)
            {
                return Bid.Contract(lastRealBid.Level + 1, lastRealBid.Suit);
            }

            if (hcp >= 12)
            {
                var major = BestMajorOpening(renkler);
                if (major.HasValue && major.Value != lastRealBid.Suit)
                    return Bid.Contract(1, major.Value);
            }

            return Bid.Pass();
        }

        private static BidSuit? BestMajorOpening(Dictionary<string, int> renkler)
        {
            if (renkler["Maça"] >= 5) return BidSuit.Spades;
            if (renkler["Kupa"] >= 5) return BidSuit.Hearts;
            return null;
        }

        private static BidSuit? BestMinorOpening(Dictionary<string, int> renkler)
        {
            if (renkler["Karo"] >= 5) return BidSuit.Diamonds;
            if (renkler["Sinek"] >= 5) return BidSuit.Clubs;
            return null;
        }

        private static string SuitName(BidSuit suit)
        {
            switch (suit)
            {
                case BidSuit.Clubs: return "Sinek";
                case BidSuit.Diamonds: return "Karo";
                case BidSuit.Hearts: return "Kupa";
                case BidSuit.Spades: return "Maça";
                case BidSuit.NT: return "NT";
                default: return "Maça";
            }
        }
    }

    public sealed class BiddingEngine
    {
        private readonly BiddingState _state;
        private readonly Player _humanPlayer;

        public BiddingEngine(Player dealer, Player humanPlayer)
        {
            _humanPlayer = humanPlayer;
            _state = new BiddingState
            {
                Dealer = dealer,
                CurrentPlayer = dealer,
                PassCount = 0
            };
        }

        public BiddingState State => _state;

        public bool TryHumanBid(Bid bid)
        {
            if (_state.CurrentPlayer != _humanPlayer)
                throw new InvalidOperationException("Şimdi insanın sırası değil.");

            if (!BidValidator.IsLegal(_state, _humanPlayer, bid))
                return false;

            _state.ApplyBid(_humanPlayer, bid);
            return true;
        }

        public bool IsHumanTurn()
        {
            return _state.CurrentPlayer == _humanPlayer;
        }

        public Bid AskAiForBid(Player aiPlayer, List<Card> hand, IBidAiStrategy strategy)
        {
            if (_state.CurrentPlayer != aiPlayer)
                throw new InvalidOperationException(string.Format("Şimdi {0} sırası değil.", aiPlayer));

            var bid = strategy.Decide(_state, aiPlayer, hand);

            if (!BidValidator.IsLegal(_state, aiPlayer, bid))
                bid = Bid.Pass();

            _state.ApplyBid(aiPlayer, bid);
            return bid;
        }

        public bool IsAuctionFinished()
        {
            return _state.IsAuctionFinished();
        }
    }

    public sealed class HumanVsAiBiddingSession
    {
        private readonly BiddingEngine _engine;
        private readonly Dictionary<Player, List<Card>> _hands;
        private readonly Dictionary<Player, IBidAiStrategy> _aiStrategies;

        public HumanVsAiBiddingSession(Player dealer, Dictionary<Player, List<Card>> hands, Player humanPlayer)
        {
            if (hands == null || !hands.ContainsKey(humanPlayer))
                throw new ArgumentException("İnsan elinin kartları tanımlı değil.");

            _hands = hands;
            _engine = new BiddingEngine(dealer, humanPlayer);

            _aiStrategies = new Dictionary<Player, IBidAiStrategy>
            {
                { Player.Kuzey, new BasicBidAiStrategy() },
                { Player.Bati, new BasicBidAiStrategy() },
                { Player.Dogu, new BasicBidAiStrategy() }
            };
        }

        public BiddingState State => _engine.State;

        public bool IsHumanTurn => _engine.IsHumanTurn();

        public bool TryHumanBid(string bidText)
        {
            var bid = Bid.Parse(bidText);
            return _engine.TryHumanBid(bid);
        }

        public Bid PerformAiTurn(Player aiPlayer)
        {
            if (!IsAiPlayer(aiPlayer))
                throw new InvalidOperationException(string.Format("{0} AI oyuncusu değil.", aiPlayer));

            if (_engine.State.CurrentPlayer != aiPlayer)
                throw new InvalidOperationException(string.Format("Şimdi {0} sırası değil.", aiPlayer));

            var hand = _hands[aiPlayer];
            var strategy = _aiStrategies[aiPlayer];
            return _engine.AskAiForBid(aiPlayer, hand, strategy);
        }

        public bool IsAiPlayer(Player player)
        {
            return player != Player.Guney;
        }


    }
}
