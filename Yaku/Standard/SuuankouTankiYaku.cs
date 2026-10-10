namespace Zizo.Mahjong.Yaku.Standard
{
    public class SuuankouTankiYaku : IYaku
    {
        private const int RequiredConcealedTripletCount = 4;

        public string Name => "四暗刻単騎";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (
                ConcealedTripletAnalysis.Count(context) != RequiredConcealedTripletCount
                || !ConcealedTripletAnalysis.IsPairCompletion(context)
            )
            {
                return false;
            }
            han = 2; // ダブル役満
            return true;
        }
    }
}
