using System.Linq;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class HonroutouYaku : IYaku
    {
        public string Name => "混老頭";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;

            bool allTerminalOrHonor = context.AllTiles.All(tile => tile.Kind.IsTerminalOrHonor);
            if (!allTerminalOrHonor)
            {
                return false;
            }

            bool hasNumberTile = context.AllTiles.Any(tile => tile.Kind.IsNumberTile);
            bool hasHonorTile = context.AllTiles.Any(tile => tile.Kind.IsHonor);
            if (!hasNumberTile || !hasHonorTile)
            {
                // 老頭牌のみ→清老頭、字牌のみ→字一色（いずれも役満）側で成立させる。
                return false;
            }

            han = 2;
            return true;
        }
    }
}
