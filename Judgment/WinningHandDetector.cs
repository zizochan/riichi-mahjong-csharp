using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Judgment
{
    public static class WinningHandDetector
    {
        public static bool IsComplete(IReadOnlyList<Tile> concealedTiles, int calledMeldCount)
        {
            return ShantenCalculator.IsCompleteHand(concealedTiles, calledMeldCount);
        }

        public static bool IsTenpai(
            IReadOnlyList<Tile> concealedTiles,
            int calledMeldCount,
            IReadOnlyList<TileKind> allTileKinds
        )
        {
            return ShantenCalculator
                    .Calculate(concealedTiles, calledMeldCount, allTileKinds)
                    .Shanten == 0;
        }

        // 聴牌時の待ち牌（=向聴数を進める有効牌と同じ計算で求まる）。聴牌でなければ空を返す。
        public static IReadOnlyList<TileKind> GetWaitingTileKinds(
            IReadOnlyList<Tile> concealedTiles,
            int calledMeldCount,
            IReadOnlyList<TileKind> allTileKinds
        )
        {
            ShantenResult result = ShantenCalculator.Calculate(
                concealedTiles,
                calledMeldCount,
                allTileKinds
            );
            return result.Shanten == 0 ? result.UsefulTileKinds : new List<TileKind>();
        }
    }
}
