namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChiitoitsuYaku : IYaku
    {
        public string Name => "七対子";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.IsChiitoitsuShape)
            {
                han = 2;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
