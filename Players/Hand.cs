using System;
using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Players
{
    public class Hand
    {
        private readonly List<Tile> _concealedTiles;

        public IReadOnlyList<Tile> ConcealedTiles => _concealedTiles;

        public Hand(IEnumerable<Tile> initialTiles)
        {
            _concealedTiles = new List<Tile>(initialTiles);
        }

        public void Draw(Tile tile)
        {
            _concealedTiles.Add(tile);
        }

        public void Discard(Tile tile)
        {
            // 赤ドラかどうかは点数に関わる実体の違いなので、種類（Kind）だけでなく赤ドラフラグも一致させて特定する。
            int index = TileSearch.FindExactIndex(_concealedTiles, tile);
            if (index < 0)
            {
                throw new InvalidOperationException($"手牌に{tile}がありません。");
            }
            _concealedTiles.RemoveAt(index);
        }

        public void RemoveForMeld(IEnumerable<Tile> tiles)
        {
            foreach (Tile tile in tiles)
            {
                Discard(tile);
            }
        }
    }
}
