namespace Zizo.Mahjong.Yaku.Standard
{
    public class JunchanYaku : IYaku
    {
        public string Name => "純全帯幺九";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (!TerminalHonorAnalysis.AllGroupsTouchTerminalOrHonor(context, allowHonor: false))
            {
                return false;
            }

            // 順子が無い場合は清老頭(ChinroutouYaku)側でのみ成立させる。
            if (!TerminalHonorAnalysis.HasAnySequence(context))
            {
                return false;
            }
            han = context.IsMenzen ? 3 : 2;
            return true;
        }
    }
}
