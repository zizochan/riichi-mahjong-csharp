using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    internal static class FlushAnalysis
    {
        // 和了形に含まれる数牌のスートの種類数（字牌は数えない）。混一色/清一色の判定に使う。
        public static int CountNumberSuits(WinningContext context)
        {
            return context
                .AllTiles.Where(tile => tile.Kind.IsNumberTile)
                .Select(tile => tile.Suit)
                .Distinct()
                .Count();
        }

        public static bool HasHonorTile(WinningContext context)
        {
            return context.AllTiles.Any(tile => tile.Kind.IsHonor);
        }
    }
}
