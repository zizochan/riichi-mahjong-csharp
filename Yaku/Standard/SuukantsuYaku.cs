using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class SuukantsuYaku : IYaku
    {
        private const int RequiredKanCount = 4;

        public string Name => "四槓子";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            int kanCount = context.Melds.Count(meld => meld.IsKan);
            if (kanCount != RequiredKanCount)
            {
                return false;
            }

            han = 1;
            return true;
        }
    }
}
