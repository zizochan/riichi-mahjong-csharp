namespace Zizo.Mahjong.Yaku.Standard
{
    public class DoubleRiichiYaku : IYaku
    {
        public string Name => "両立直";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.Situation.IsDoubleRiichi)
            {
                han = 2;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
