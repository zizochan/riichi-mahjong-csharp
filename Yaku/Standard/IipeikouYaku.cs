namespace Zizo.Mahjong.Yaku.Standard
{
    public class IipeikouYaku : IYaku
    {
        public string Name => "一盃口";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (context.Decomposition == null || !context.IsMenzen)
            {
                return false;
            }

            // 2単位（2組の一盃口）ある場合は二盃口(RyanpeikouYaku)の方でのみ成立させる。
            if (SequenceAnalysis.CountDuplicateSequenceUnits(context) != 1)
            {
                return false;
            }

            han = 1;
            return true;
        }
    }
}
