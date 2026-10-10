using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class TileTests
    {
        [Test]
        public void Equals_IgnoresRedDoraFlag()
        {
            var normalFive = new Tile(TileSuit.Man, 5, isRedDora: false);
            var redFive = new Tile(TileSuit.Man, 5, isRedDora: true);

            Assert.AreEqual(normalFive, redFive);
            Assert.AreEqual(normalFive.GetHashCode(), redFive.GetHashCode());
        }

        [Test]
        public void Equals_DifferentRank_AreNotEqual()
        {
            var five = new Tile(TileSuit.Man, 5);
            var six = new Tile(TileSuit.Man, 6);

            Assert.AreNotEqual(five, six);
        }

        [Test]
        public void IsNumberTile_TrueOnlyForManPinSou()
        {
            Assert.IsTrue(new TileKind(TileSuit.Man, 1).IsNumberTile);
            Assert.IsTrue(new TileKind(TileSuit.Pin, 1).IsNumberTile);
            Assert.IsTrue(new TileKind(TileSuit.Sou, 1).IsNumberTile);
            Assert.IsFalse(new TileKind(TileSuit.Wind, 1).IsNumberTile);
            Assert.IsFalse(new TileKind(TileSuit.Dragon, 1).IsNumberTile);
        }

        [TestCase(TileSuit.Man, 1, true)]
        [TestCase(TileSuit.Pin, 9, true)]
        [TestCase(TileSuit.Sou, 5, false)]
        [TestCase(TileSuit.Wind, 1, false)]
        [TestCase(TileSuit.Dragon, 3, false)]
        public void IsTerminal_TrueOnlyForNumberOneOrNine(TileSuit suit, int rank, bool expected)
        {
            Assert.AreEqual(expected, new TileKind(suit, rank).IsTerminal);
        }

        [TestCase(TileSuit.Man, 1, true)]
        [TestCase(TileSuit.Sou, 2, false)]
        [TestCase(TileSuit.Wind, 4, true)]
        [TestCase(TileSuit.Dragon, 1, true)]
        public void IsTerminalOrHonor_TrueForTerminalsAndHonors(
            TileSuit suit,
            int rank,
            bool expected
        )
        {
            Assert.AreEqual(expected, new TileKind(suit, rank).IsTerminalOrHonor);
        }

        [TestCase(TileSuit.Man, 0, false)]
        [TestCase(TileSuit.Man, 9, true)]
        [TestCase(TileSuit.Man, 10, false)]
        [TestCase(TileSuit.Wind, 4, true)]
        [TestCase(TileSuit.Wind, 5, false)]
        [TestCase(TileSuit.Dragon, 3, true)]
        [TestCase(TileSuit.Dragon, 4, false)]
        public void IsValid_ChecksRankRangePerSuit(TileSuit suit, int rank, bool expected)
        {
            Assert.AreEqual(expected, new TileKind(suit, rank).IsValid);
        }

        [Test]
        public void CreateStandard_Builds136TilesWithOneRedFivePerNumberSuit()
        {
            List<Tile> tiles = TileSetDefinition.CreateStandard().BuildTiles();

            Assert.AreEqual(136, tiles.Count);
            List<Tile> redTiles = tiles.Where(tile => tile.IsRedDora).ToList();
            Assert.AreEqual(3, redTiles.Count);
            Assert.IsTrue(redTiles.All(tile => tile.Rank == 5 && tile.Kind.IsNumberTile));
            Assert.AreEqual(34, TileSetDefinition.CreateStandard().GetDistinctKinds().Count);
        }

        [Test]
        public void CreateStandard_ZeroRedDora_BuildsNoRedTiles()
        {
            List<Tile> tiles = TileSetDefinition
                .CreateStandard(redDoraCountPerNumberSuit: 0)
                .BuildTiles();

            Assert.IsFalse(tiles.Any(tile => tile.IsRedDora));
        }

        [TestCase(TileSuit.Man, 0)]
        [TestCase(TileSuit.Man, 10)]
        [TestCase(TileSuit.Wind, 5)]
        [TestCase(TileSuit.Dragon, 4)]
        public void TileSetEntry_InvalidKind_Throws(TileSuit suit, int rank)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TileSetEntry(new TileKind(suit, rank), count: 4)
            );
        }

        [Test]
        public void TileSetEntry_RedDoraCountExceedingCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TileSetEntry(new TileKind(TileSuit.Man, 5), count: 1, redDoraCount: 2)
            );
        }
    }
}
