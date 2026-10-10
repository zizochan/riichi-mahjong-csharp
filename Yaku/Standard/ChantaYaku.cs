using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChantaYaku : IYaku
    {
        public string Name => "混全帯幺九";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (!TerminalHonorAnalysis.AllGroupsTouchTerminalOrHonor(context, allowHonor: true))
            {
                return false;
            }

            // 字牌を含まない場合は純全帯幺九(JunchanYaku)側でのみ成立させる。
            if (!context.AllTiles.Any(tile => tile.Kind.IsHonor))
            {
                return false;
            }
            // 順子が無い場合は混老頭(HonroutouYaku)側でのみ成立させる。
            if (!TerminalHonorAnalysis.HasAnySequence(context))
            {
                return false;
            }

            han = context.IsMenzen ? 2 : 1;
            return true;
        }
    }
}
