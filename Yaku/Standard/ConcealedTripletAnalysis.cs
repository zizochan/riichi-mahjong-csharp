using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;

namespace Zizo.Mahjong.Yaku.Standard
{
    internal static class ConcealedTripletAnalysis
    {
        // 暗刻（暗槓を含む）の数。ロンで完成させた刻子は明刻扱いのため数えない。
        public static int Count(WinningContext context)
        {
            if (context.Decomposition == null)
            {
                return 0;
            }

            int concealedTripletsInHand = context.Sets.Count(set =>
                set.Shape == SetShape.Triplet && !context.IsTripletCompletedByRon(set)
            );
            int closedKanCount = context.Melds.Count(meld => meld.Type == MeldType.ClosedKan);
            return concealedTripletsInHand + closedKanCount;
        }

        // 和了牌で雀頭を完成させたか（単騎待ち）。
        public static bool IsPairCompletion(WinningContext context)
        {
            return context.Decomposition != null
                && context.WinningTile.Kind.Equals(context.PairKind);
        }
    }
}
