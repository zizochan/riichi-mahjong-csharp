using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Players
{
    public class DiscardPile
    {
        private readonly List<DiscardedTile> _discards = new List<DiscardedTile>();

        public IReadOnlyList<DiscardedTile> Discards => _discards;

        public DiscardedTile Add(Tile tile, bool isRiichiDeclaration = false)
        {
            var discardedTile = new DiscardedTile(tile, isRiichiDeclaration);
            _discards.Add(discardedTile);
            return discardedTile;
        }
    }
}
