using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    internal static class ChuurenAnalysis
    {
        // 九蓮宝燈の基本形「1112345678999」における、数字ごとの必要枚数（1と9は3枚、2〜8は1枚）。
        private const int TerminalRequiredCount = 3;
        private const int SimpleRequiredCount = 1;

        // 面前のみ・単一スート・1112345678999+同スートの余剰1枚、という九蓮宝燈の形かどうか。
        public static bool IsChuurenShape(WinningContext context)
        {
            if (context.Melds.Count > 0 || !IsSingleNumberSuit(context.ConcealedTiles))
            {
                return false;
            }

            int[] rankCounts = CountByRank(context.ConcealedTiles);
            return AllRanks()
                .All(rank => rankCounts[rank - TileKind.MinRank] >= RequiredCount(rank));
        }

        // 和了牌を除いた13枚が「1112345678999」ちょうど（=九面待ちの形）だったかどうか。
        public static bool IsPureNineSidedWait(WinningContext context)
        {
            List<Tile> tilesBeforeWin = context.GetConcealedTilesBeforeWinningTile();
            if (tilesBeforeWin == null || !IsSingleNumberSuit(tilesBeforeWin))
            {
                return false;
            }

            int[] rankCounts = CountByRank(tilesBeforeWin);
            return AllRanks()
                .All(rank => rankCounts[rank - TileKind.MinRank] == RequiredCount(rank));
        }

        private static bool IsSingleNumberSuit(IReadOnlyList<Tile> tiles)
        {
            return tiles.All(tile => tile.Kind.IsNumberTile)
                && tiles.Select(tile => tile.Suit).Distinct().Count() == 1;
        }

        private static int[] CountByRank(IEnumerable<Tile> tiles)
        {
            var rankCounts = new int[TileKind.MaxNumberRank];
            foreach (Tile tile in tiles)
            {
                rankCounts[tile.Rank - TileKind.MinRank]++;
            }
            return rankCounts;
        }

        private static IEnumerable<int> AllRanks()
        {
            return Enumerable.Range(TileKind.MinRank, TileKind.MaxNumberRank);
        }

        private static int RequiredCount(int rank)
        {
            bool isTerminal = rank == TileKind.MinRank || rank == TileKind.MaxNumberRank;
            return isTerminal ? TerminalRequiredCount : SimpleRequiredCount;
        }
    }
}
