using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class TanyaoYaku : IYaku
    {
        public string Name => "断幺九";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            bool hasTerminalOrHonor = context.AllTiles.Any(tile => tile.Kind.IsTerminalOrHonor);

            if (!hasTerminalOrHonor)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
