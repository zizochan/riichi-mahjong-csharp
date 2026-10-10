namespace Zizo.Mahjong.Yaku.Standard
{
    public class SuuankouYaku : IYaku
    {
        private const int RequiredConcealedTripletCount = 4;

        public string Name => "四暗刻";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            // ロンで刻子側を完成させた場合はその刻子が明刻扱いになり、暗刻が3つに減るため不成立になる。
            if (ConcealedTripletAnalysis.Count(context) != RequiredConcealedTripletCount)
            {
                return false;
            }
            // 単騎待ちはSuuankouTankiYaku(ダブル役満)側で成立させる。
            if (ConcealedTripletAnalysis.IsPairCompletion(context))
            {
                return false;
            }
            han = 1;
            return true;
        }
    }
}
