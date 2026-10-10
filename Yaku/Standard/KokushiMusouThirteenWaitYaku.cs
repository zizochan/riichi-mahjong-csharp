namespace Zizo.Mahjong.Yaku.Standard
{
    public class KokushiMusouThirteenWaitYaku : IYaku
    {
        public string Name => "国士無双十三面待ち";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.IsKokushiShape && context.IsKokushiThirteenWait())
            {
                han = 2; // ダブル役満
                return true;
            }
            han = 0;
            return false;
        }
    }
}
