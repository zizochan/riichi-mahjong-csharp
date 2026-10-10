namespace Zizo.Mahjong.Yaku.Standard
{
    public class IppatsuYaku : IYaku
    {
        public string Name => "一発";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (
                context.Situation.IsIppatsu
                && (context.Situation.IsRiichi || context.Situation.IsDoubleRiichi)
            )
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
