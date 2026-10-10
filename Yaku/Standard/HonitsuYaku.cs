namespace Zizo.Mahjong.Yaku.Standard
{
    public class HonitsuYaku : IYaku
    {
        public string Name => "混一色";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;

            int distinctSuitCount = FlushAnalysis.CountNumberSuits(context);
            bool hasHonorTile = FlushAnalysis.HasHonorTile(context);

            // 字牌を含まない場合は清一色(ChinitsuYaku)側でのみ成立させる。
            if (distinctSuitCount > 1 || !hasHonorTile)
            {
                return false;
            }

            han = context.IsMenzen ? 3 : 2;
            return true;
        }
    }
}
