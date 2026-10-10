namespace Zizo.Mahjong.Yaku.Standard
{
    public class SanankouYaku : IYaku
    {
        private const int RequiredConcealedTripletCount = 3;

        public string Name => "三暗刻";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            // 4つ揃う場合は四暗刻(役満)が優先されるため、ここでは3つ以上を条件にする。
            if (ConcealedTripletAnalysis.Count(context) < RequiredConcealedTripletCount)
            {
                return false;
            }
            han = 2;
            return true;
        }
    }
}
