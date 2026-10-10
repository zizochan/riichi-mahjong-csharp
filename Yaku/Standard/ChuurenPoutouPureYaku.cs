namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChuurenPoutouPureYaku : IYaku
    {
        public string Name => "純正九蓮宝燈";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (
                !ChuurenAnalysis.IsChuurenShape(context)
                || !ChuurenAnalysis.IsPureNineSidedWait(context)
            )
            {
                return false;
            }

            han = 2; // ダブル役満
            return true;
        }
    }
}
