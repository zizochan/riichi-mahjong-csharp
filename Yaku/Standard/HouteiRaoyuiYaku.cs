namespace Zizo.Mahjong.Yaku.Standard
{
    public class HouteiRaoyuiYaku : IYaku
    {
        public string Name => "河底撈魚";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.Situation.IsHoutei && !context.IsTsumo)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
