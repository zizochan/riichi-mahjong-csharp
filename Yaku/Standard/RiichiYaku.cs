namespace Zizo.Mahjong.Yaku.Standard
{
    public class RiichiYaku : IYaku
    {
        public string Name => "立直";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.Situation.IsRiichi && !context.Situation.IsDoubleRiichi)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
