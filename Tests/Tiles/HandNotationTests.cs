using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    // 他の全テストが依存するHandNotationパーサー自体の正しさを検証する。
    public class HandNotationTests
    {
        [Test]
        public void Parse_MixedSuits_ProducesExpectedTileCountsAndKinds()
        {
            List<Tile> tiles = HandNotation.Parse("123m456p789s11z");

            Assert.AreEqual(11, tiles.Count);
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Man, 1));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Man, 2));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Man, 3));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Pin, 4));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Pin, 5));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Pin, 6));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Sou, 7));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Sou, 8));
            CollectionAssert.Contains(tiles, new Tile(TileSuit.Sou, 9));
            Assert.AreEqual(
                2,
                tiles.Count(tile => tile.Kind.Equals(new TileKind(TileSuit.Wind, 1)))
            );
        }

        [Test]
        public void Parse_HonorSuffix_MapsWindsAndDragonsByDigit()
        {
            List<Tile> tiles = HandNotation.Parse("1234567z");

            Assert.AreEqual(7, tiles.Count);
            Assert.AreEqual(new Tile(TileSuit.Wind, 1), tiles[0]);
            Assert.AreEqual(new Tile(TileSuit.Wind, 2), tiles[1]);
            Assert.AreEqual(new Tile(TileSuit.Wind, 3), tiles[2]);
            Assert.AreEqual(new Tile(TileSuit.Wind, 4), tiles[3]);
            Assert.AreEqual(new Tile(TileSuit.Dragon, 1), tiles[4]);
            Assert.AreEqual(new Tile(TileSuit.Dragon, 2), tiles[5]);
            Assert.AreEqual(new Tile(TileSuit.Dragon, 3), tiles[6]);
        }

        [Test]
        public void Parse_ZeroDigit_ProducesRedFive()
        {
            Tile tile = HandNotation.Single("0m");

            Assert.AreEqual(new Tile(TileSuit.Man, 5), tile);
            Assert.IsTrue(tile.IsRedDora);
        }
    }
}
