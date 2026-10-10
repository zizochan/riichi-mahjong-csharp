using System.Linq;
using Zizo.Mahjong.Judgment;

namespace Zizo.Mahjong.Yaku.Standard
{
    internal static class SequenceAnalysis
    {
        // 同じ順子（スート+最小数字が同一）が2つ揃っている組を1単位として数える（一盃口=1単位、二盃口=2単位）。
        public static int CountDuplicateSequenceUnits(WinningContext context)
        {
            if (context.Decomposition == null)
            {
                return 0;
            }

            return context
                .Sets.Where(set => set.Shape == SetShape.Sequence)
                .GroupBy(set => (set.Kinds[0].Suit, set.Kinds[0].Rank))
                .Sum(group => group.Count() / 2);
        }
    }
}
