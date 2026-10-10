namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChiihouYaku : IYaku
    {
        public string Name => "地和";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            bool isDealer = context.Situation.IsDealer;
            if (!isDealer && context.IsTsumo && context.Situation.IsFirstUninterruptedTurnWin)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
