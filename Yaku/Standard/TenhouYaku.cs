namespace Zizo.Mahjong.Yaku.Standard
{
    // 親の第一ツモで、他家の副露が一切入らないまま和了した場合の役満。
    public class TenhouYaku : IYaku
    {
        public string Name => "天和";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            bool isDealer = context.Situation.IsDealer;
            if (isDealer && context.IsTsumo && context.Situation.IsFirstUninterruptedTurnWin)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
