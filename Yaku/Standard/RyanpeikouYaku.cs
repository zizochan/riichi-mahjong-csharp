namespace Zizo.Mahjong.Yaku.Standard
{
    public class RyanpeikouYaku : IYaku
    {
        public string Name => "二盃口";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (context.Decomposition == null || !context.IsMenzen)
            {
                return false;
            }

            if (SequenceAnalysis.CountDuplicateSequenceUnits(context) != 2)
            {
                return false;
            }

            han = 3;
            return true;
        }
    }
}
