using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class SankantsuYaku : IYaku
    {
        private const int RequiredKanCount = 3;

        public string Name => "三槓子";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            int kanCount = context.Melds.Count(meld => meld.IsKan);
            if (kanCount != RequiredKanCount)
            {
                return false;
            }

            han = 2;
            return true;
        }
    }
}
