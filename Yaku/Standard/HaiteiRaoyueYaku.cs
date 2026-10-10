namespace Zizo.Mahjong.Yaku.Standard
{
    public class HaiteiRaoyueYaku : IYaku
    {
        public string Name => "海底摸月";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.Situation.IsHaitei && context.IsTsumo)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
