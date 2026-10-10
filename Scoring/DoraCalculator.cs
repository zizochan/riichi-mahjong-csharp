using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Scoring
{
    // ドラ・裏ドラ・赤ドラは役ではなく加算翻として別枠で数える（役無しの手ではドラがあっても和了できない）。
    public static class DoraCalculator
    {
        public static int CountDora(
            IReadOnlyList<Tile> allTiles,
            IReadOnlyList<Tile> doraIndicators
        )
        {
            int count = 0;
            foreach (Tile indicator in doraIndicators)
            {
                TileKind doraKind = GetNextKind(indicator.Kind);
                count += allTiles.Count(tile => tile.Kind.Equals(doraKind));
            }
            return count;
        }

        public static int CountRedDora(IReadOnlyList<Tile> allTiles)
        {
            return allTiles.Count(tile => tile.IsRedDora);
        }

        // ドラ表示牌の次の牌がドラになる。各スートの最後の牌（9・北・中）の次は最初の牌に戻る。
        private static TileKind GetNextKind(TileKind kind)
        {
            int nextRank =
                kind.Rank == TileKind.GetMaxRank(kind.Suit) ? TileKind.MinRank : kind.Rank + 1;
            return new TileKind(kind.Suit, nextRank);
        }
    }
}
