using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Judgment
{
    public readonly struct ShantenResult
    {
        public int Shanten { get; }
        public IReadOnlyList<TileKind> UsefulTileKinds { get; }

        public ShantenResult(int shanten, IReadOnlyList<TileKind> usefulTileKinds)
        {
            Shanten = shanten;
            UsefulTileKinds = usefulTileKinds;
        }
    }
}
