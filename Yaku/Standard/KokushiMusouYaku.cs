namespace Zizo.Mahjong.Yaku.Standard
{
    public class KokushiMusouYaku : IYaku
    {
        public string Name => "国士無双";

        // 役満(1倍)。十三面待ちの場合は別クラスKokushiMusouThirteenWaitYaku(2倍)が代わりに成立する。
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.IsKokushiShape && !context.IsKokushiThirteenWait())
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
