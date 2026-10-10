using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Players
{
    public class DiscardedTile
    {
        public Tile Tile { get; }
        public bool IsRiichiDeclaration { get; }
        public bool WasCalled { get; private set; }

        public DiscardedTile(Tile tile, bool isRiichiDeclaration)
        {
            Tile = tile;
            IsRiichiDeclaration = isRiichiDeclaration;
        }

        public void MarkAsCalled()
        {
            WasCalled = true;
        }
    }
}
