using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class WinningHandDetectorTests
    {
        private static readonly IReadOnlyList<TileKind> AllTileKinds = TileSetDefinition
            .CreateStandard()
            .Entries.Select(entry => entry.Kind)
            .Distinct()
            .ToList();

        [Test]
        public void IsComplete_FourSetsPlusPair_ReturnsTrue()
        {
            List<Tile> tiles = HandNotation.Parse("123456789m111p22s");

            Assert.IsTrue(WinningHandDetector.IsComplete(tiles, calledMeldCount: 0));
        }

        [Test]
        public void IsComplete_MissingOneTileForCompletion_ReturnsFalse()
        {
            List<Tile> tiles = HandNotation.Parse("123456789m11p22s");

            Assert.IsFalse(WinningHandDetector.IsComplete(tiles, calledMeldCount: 0));
        }

        [Test]
        public void IsTenpai_ShanpongWait_ReturnsTrue()
        {
            List<Tile> tiles = HandNotation.Parse("123456789m11p22s");

            Assert.IsTrue(WinningHandDetector.IsTenpai(tiles, calledMeldCount: 0, AllTileKinds));
        }

        [Test]
        public void IsTenpai_FarFromComplete_ReturnsFalse()
        {
            List<Tile> tiles = HandNotation.Parse("147m147p147s1234z");

            Assert.IsFalse(WinningHandDetector.IsTenpai(tiles, calledMeldCount: 0, AllTileKinds));
        }

        [Test]
        public void GetWaitingTileKinds_ShanpongWait_ReturnsBothPairKinds()
        {
            List<Tile> tiles = HandNotation.Parse("123456789m11p22s");

            IReadOnlyList<TileKind> waits = WinningHandDetector.GetWaitingTileKinds(
                tiles,
                calledMeldCount: 0,
                AllTileKinds
            );

            Assert.AreEqual(2, waits.Count);
            CollectionAssert.Contains(waits, new TileKind(TileSuit.Pin, 1));
            CollectionAssert.Contains(waits, new TileKind(TileSuit.Sou, 2));
        }

        [Test]
        public void GetWaitingTileKinds_RyanmenWait_ReturnsBothSidedTiles()
        {
            // 123456m + 789p + 22s(雀頭) + 45s(両面): 3s/6sの両面待ち。
            List<Tile> tiles = HandNotation.Parse("123456m789p22s45s");

            IReadOnlyList<TileKind> waits = WinningHandDetector.GetWaitingTileKinds(
                tiles,
                calledMeldCount: 0,
                AllTileKinds
            );

            CollectionAssert.Contains(waits, new TileKind(TileSuit.Sou, 3));
            CollectionAssert.Contains(waits, new TileKind(TileSuit.Sou, 6));
        }

        [Test]
        public void GetWaitingTileKinds_NotTenpai_ReturnsEmpty()
        {
            List<Tile> tiles = HandNotation.Parse("147m147p147s1234z");

            IReadOnlyList<TileKind> waits = WinningHandDetector.GetWaitingTileKinds(
                tiles,
                calledMeldCount: 0,
                AllTileKinds
            );

            Assert.IsEmpty(waits);
        }
    }
}
