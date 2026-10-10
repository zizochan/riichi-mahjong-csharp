using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class ChinroutouYaku : IYaku
    {
        public string Name => "清老頭";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            bool allTerminals = context.AllTiles.All(tile => tile.Kind.IsTerminal);
            if (allTerminals)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
