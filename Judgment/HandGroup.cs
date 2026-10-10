using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Judgment
{
    // 面子1つ分（刻子または順子）を構成する3つの牌種。
    public readonly struct HandGroup
    {
        public SetShape Shape { get; }
        public IReadOnlyList<TileKind> Kinds { get; }

        public HandGroup(SetShape shape, IReadOnlyList<TileKind> kinds)
        {
            Shape = shape;
            Kinds = kinds;
        }
    }
}
