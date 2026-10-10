namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChuurenPoutouYaku : IYaku
    {
        public string Name => "九蓮宝燈";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (!ChuurenAnalysis.IsChuurenShape(context))
            {
                return false;
            }

            // 純正九蓮宝燈（九面待ち）はChuurenPoutouPureYaku(ダブル役満)側で成立させる。
            if (ChuurenAnalysis.IsPureNineSidedWait(context))
            {
                return false;
            }

            han = 1;
            return true;
        }
    }
}
