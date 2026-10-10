namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChankanYaku : IYaku
    {
        public string Name => "槍槓";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.Situation.IsChankan)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
