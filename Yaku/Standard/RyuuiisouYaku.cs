using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class RyuuiisouYaku : IYaku
    {
        public string Name => "緑一色";
        public bool IsYakuman => true;

        private static readonly HashSet<TileKind> GreenKinds = new HashSet<TileKind>
        {
            new TileKind(TileSuit.Sou, 2),
            new TileKind(TileSuit.Sou, 3),
            new TileKind(TileSuit.Sou, 4),
            new TileKind(TileSuit.Sou, 6),
            new TileKind(TileSuit.Sou, 8),
            new TileKind(TileSuit.Dragon, 2), // 發
        };

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.AllTiles.All(tile => GreenKinds.Contains(tile.Kind)))
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
