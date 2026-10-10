namespace Zizo.Mahjong.Yaku.Standard
{
    public class RinshanKaihouYaku : IYaku
    {
        public string Name => "嶺上開花";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.Situation.IsRinshan)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
