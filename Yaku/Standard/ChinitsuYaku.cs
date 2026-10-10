namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChinitsuYaku : IYaku
    {
        public string Name => "清一色";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;

            int distinctSuitCount = FlushAnalysis.CountNumberSuits(context);
            bool hasHonorTile = FlushAnalysis.HasHonorTile(context);

            if (distinctSuitCount != 1 || hasHonorTile)
            {
                return false;
            }

            han = context.IsMenzen ? 6 : 5;
            return true;
        }
    }
}
