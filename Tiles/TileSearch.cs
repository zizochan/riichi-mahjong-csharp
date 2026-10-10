using System.Collections.Generic;

namespace Zizo.Mahjong.Tiles
{
    internal static class TileSearch
    {
        // Tile.Equalsは赤ドラを区別しないため、赤ドラフラグまで一致する実体の位置を探す。見つからなければ-1。
        public static int FindExactIndex(IReadOnlyList<Tile> tiles, Tile target)
        {
            for (int index = 0; index < tiles.Count; index++)
            {
                if (
                    tiles[index].Kind.Equals(target.Kind)
                    && tiles[index].IsRedDora == target.IsRedDora
                )
                {
                    return index;
                }
            }
            return -1;
        }
    }
}
