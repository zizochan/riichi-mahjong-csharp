using System.Collections.Generic;

namespace Zizo.Mahjong.Tiles
{
    internal static class TileCounter
    {
        // 牌の種類ごとの枚数（赤ドラかどうかは区別しない）。
        public static Dictionary<TileKind, int> CountByKind(IEnumerable<Tile> tiles)
        {
            var counts = new Dictionary<TileKind, int>();
            foreach (Tile tile in tiles)
            {
                counts.TryGetValue(tile.Kind, out int currentCount);
                counts[tile.Kind] = currentCount + 1;
            }
            return counts;
        }

        // 指定した数牌スートについて、数字(1〜9)ごとの枚数を配列(添字0=1)で返す。
        public static int[] BuildRankCounts(Dictionary<TileKind, int> countsByKind, TileSuit suit)
        {
            var rankCounts = new int[TileKind.MaxNumberRank];
            foreach ((TileKind kind, int count) in countsByKind)
            {
                if (kind.Suit == suit)
                {
                    rankCounts[kind.Rank - TileKind.MinRank] = count;
                }
            }
            return rankCounts;
        }
    }
}
