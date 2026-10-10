using System.Collections.Generic;
using Zizo.Mahjong.Flow;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Cpu
{
    // CPUに見せてよい情報だけを抜き出したビュー。イカサマ防止のため、他家の手牌の中身は一切含めない
    // （公開情報である鳴き・捨て牌・ドラ表示牌・残り枚数・自分の手牌のみ）。
    public class PlayerView
    {
        public int SeatIndex { get; }
        public IReadOnlyList<Tile> ConcealedTiles { get; }
        public IReadOnlyList<Meld> OwnMelds { get; }
        public bool IsMenzen { get; }
        public bool IsRiichiDeclared { get; }
        public int Score { get; }
        public TileKind SeatWind { get; }
        public TileKind RoundWind { get; }
        public int RemainingLiveTileCount { get; }
        public IReadOnlyList<Tile> VisibleDoraIndicators { get; }
        public IReadOnlyDictionary<int, IReadOnlyList<DiscardedTile>> DiscardsBySeat { get; }
        public IReadOnlyDictionary<int, IReadOnlyList<Meld>> MeldsBySeat { get; }
        public IReadOnlyDictionary<int, bool> OpponentRiichiDeclaredBySeat { get; }

        public PlayerView(
            int seatIndex,
            IReadOnlyList<Tile> concealedTiles,
            IReadOnlyList<Meld> ownMelds,
            bool isMenzen,
            bool isRiichiDeclared,
            int score,
            TileKind seatWind,
            TileKind roundWind,
            int remainingLiveTileCount,
            IReadOnlyList<Tile> visibleDoraIndicators,
            IReadOnlyDictionary<int, IReadOnlyList<DiscardedTile>> discardsBySeat,
            IReadOnlyDictionary<int, IReadOnlyList<Meld>> meldsBySeat,
            IReadOnlyDictionary<int, bool> opponentRiichiDeclaredBySeat
        )
        {
            SeatIndex = seatIndex;
            ConcealedTiles = concealedTiles;
            OwnMelds = ownMelds;
            IsMenzen = isMenzen;
            IsRiichiDeclared = isRiichiDeclared;
            Score = score;
            SeatWind = seatWind;
            RoundWind = roundWind;
            RemainingLiveTileCount = remainingLiveTileCount;
            VisibleDoraIndicators = visibleDoraIndicators;
            DiscardsBySeat = discardsBySeat;
            MeldsBySeat = meldsBySeat;
            OpponentRiichiDeclaredBySeat = opponentRiichiDeclaredBySeat;
        }

        public static PlayerView Create(RoundController controller, int seatIndex)
        {
            PlayerState player = controller.Players[seatIndex];

            var discardsBySeat = new Dictionary<int, IReadOnlyList<DiscardedTile>>();
            var meldsBySeat = new Dictionary<int, IReadOnlyList<Meld>>();
            var riichiBySeat = new Dictionary<int, bool>();
            foreach (PlayerState other in controller.Players)
            {
                discardsBySeat[other.SeatIndex] = other.DiscardPile.Discards;
                meldsBySeat[other.SeatIndex] = other.Melds;
                if (other.SeatIndex != seatIndex)
                {
                    riichiBySeat[other.SeatIndex] = other.IsRiichiDeclared;
                }
            }

            return new PlayerView(
                seatIndex,
                player.Hand.ConcealedTiles,
                player.Melds,
                player.IsMenzen,
                player.IsRiichiDeclared,
                player.Score,
                controller.GetSeatWind(seatIndex),
                controller.RoundWind,
                controller.Wall.RemainingLiveTileCount,
                controller.Wall.DoraIndicators,
                discardsBySeat,
                meldsBySeat,
                riichiBySeat
            );
        }
    }
}
