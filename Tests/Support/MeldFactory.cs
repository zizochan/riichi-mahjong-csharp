using System.Collections.Generic;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    // テスト用に鳴き面子を手牌記法から組み立てるヘルパー。鳴いた牌は構成牌の先頭として扱う。
    public static class MeldFactory
    {
        private const int DefaultSourceSeatIndex = 1;

        public static Meld Chi(string notation)
        {
            return CreateCalledMeld(MeldType.Chi, notation);
        }

        public static Meld Pon(string notation)
        {
            return CreateCalledMeld(MeldType.Pon, notation);
        }

        public static Meld OpenKan(string notation)
        {
            return CreateCalledMeld(MeldType.OpenKan, notation);
        }

        public static Meld AddedKan(string notation)
        {
            return CreateCalledMeld(MeldType.AddedKan, notation);
        }

        public static Meld ClosedKan(string notation)
        {
            return new Meld(MeldType.ClosedKan, HandNotation.Parse(notation));
        }

        private static Meld CreateCalledMeld(MeldType type, string notation)
        {
            List<Tile> tiles = HandNotation.Parse(notation);
            Tile calledTile = tiles[0];
            return new Meld(type, tiles, calledTile, DefaultSourceSeatIndex);
        }
    }
}
