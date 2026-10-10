using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class ShantenCalculatorTests
    {
        private static readonly IReadOnlyList<TileKind> AllTileKinds = TileSetDefinition
            .CreateStandard()
            .Entries.Select(entry => entry.Kind)
            .Distinct()
            .ToList();

        [Test]
        public void Calculate_ShanpongTenpai_ShantenIsZero()
        {
            // 123456789m + 11p + 22s: 3面子+2対子(シャンポン待ち)で聴牌。
            List<Tile> tiles = HandNotation.Parse("123456789m11p22s");

            ShantenResult result = ShantenCalculator.Calculate(
                tiles,
                calledMeldCount: 0,
                AllTileKinds
            );

            Assert.AreEqual(0, result.Shanten);
            CollectionAssert.Contains(result.UsefulTileKinds, new TileKind(TileSuit.Pin, 1));
            CollectionAssert.Contains(result.UsefulTileKinds, new TileKind(TileSuit.Sou, 2));
        }

        [Test]
        public void Calculate_OneAwayFromTenpai_ShantenIsOne()
        {
            // 123456789m(3面子) + 22s(雀頭) + 1p単騎浮き牌 + 1z単騎浮き牌: どちらかが対子になれば聴牌。
            List<Tile> tiles = HandNotation.Parse("123456789m22s1p1z");

            ShantenResult result = ShantenCalculator.Calculate(
                tiles,
                calledMeldCount: 0,
                AllTileKinds
            );

            Assert.AreEqual(1, result.Shanten);
            CollectionAssert.Contains(result.UsefulTileKinds, new TileKind(TileSuit.Pin, 1));
        }

        [Test]
        public void Calculate_SevenDistinctPairsPlusOneSingle_ChiitoitsuTenpai()
        {
            // 七対子として6対子+1枚(単騎待ち)で聴牌。通常形としては聴牌にならない形を選んでいる。
            List<Tile> tiles = HandNotation.Parse("11223344m5566p7s");

            ShantenResult result = ShantenCalculator.Calculate(
                tiles,
                calledMeldCount: 0,
                AllTileKinds
            );

            Assert.AreEqual(0, result.Shanten);
            CollectionAssert.Contains(result.UsefulTileKinds, new TileKind(TileSuit.Sou, 7));
        }

        [Test]
        public void Calculate_TwelveKokushiKindsPlusOnePair_KokushiTenpai()
        {
            // 国士無双の13種のうち12種(中を除く)を1枚ずつ+1種(1m)を対子で保持: 中の単騎待ちで聴牌。
            List<Tile> tiles = HandNotation.Parse("119m19p19s123456z");

            ShantenResult result = ShantenCalculator.Calculate(
                tiles,
                calledMeldCount: 0,
                AllTileKinds
            );

            Assert.AreEqual(0, result.Shanten);
        }

        [Test]
        public void Calculate_CalledMeldPresent_IgnoresChiitoitsuAndKokushi()
        {
            // 鳴きがある場合は七対子・国士無双の可能性を無視し、通常形の向聴数のみで判定する。
            List<Tile> tiles = HandNotation.Parse("119m19p19s123456z");

            ShantenResult result = ShantenCalculator.Calculate(
                tiles,
                calledMeldCount: 1,
                AllTileKinds
            );

            Assert.Greater(result.Shanten, 0);
        }

        [Test]
        public void IsCompleteHand_FourSetsPlusPair_ReturnsTrue()
        {
            List<Tile> tiles = HandNotation.Parse("123456789m111p22s");

            Assert.IsTrue(ShantenCalculator.IsCompleteHand(tiles, calledMeldCount: 0));
        }

        [Test]
        public void IsCompleteHand_IsolatedTiles_ReturnsFalse()
        {
            List<Tile> tiles = HandNotation.Parse("147m147p147s12345z");

            Assert.IsFalse(ShantenCalculator.IsCompleteHand(tiles, calledMeldCount: 0));
        }
    }
}
