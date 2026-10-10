using System.Linq;
using Zizo.Mahjong.Judgment;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class PinfuYaku : IYaku
    {
        public string Name => "平和";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;

            if (context.Decomposition == null || context.Melds.Count > 0)
            {
                return false;
            }
            if (context.Sets.Any(set => set.Shape != SetShape.Sequence))
            {
                return false;
            }
            if (context.IsSetKindYakuhai(context.PairKind))
            {
                return false;
            }
            if (!HasRyanmenWaitForWinningTile(context))
            {
                return false;
            }

            han = 1;
            return true;
        }

        // 「和了牌の種類を含む順子のうち、両面待ちとして成立しうるものが1つでもあるか」で判定する
        // （平和が付く解釈の方が高くなるため、両面と嵌張のどちらとも取れる場合は両面を採用する）。
        private static bool HasRyanmenWaitForWinningTile(WinningContext context)
        {
            return WaitAnalysis
                .GetSequencesContainingWinningTile(context)
                .Any(sequence =>
                    WaitAnalysis.IsRyanmenCompletion(sequence, context.WinningTile.Kind)
                );
        }
    }
}
