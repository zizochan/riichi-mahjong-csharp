using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class TsuuiisouYaku : IYaku
    {
        public string Name => "字一色";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.AllTiles.All(tile => tile.Kind.IsHonor))
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
