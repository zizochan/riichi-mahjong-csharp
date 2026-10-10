using System;
using System.Collections.Generic;
using NUnit.Framework;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;
using Zizo.Mahjong.Yaku.Standard;

namespace Zizo.Mahjong.Tests
{
    public class MahjongRuleSetTests
    {
        [Test]
        public void CreateStandardFourPlayer_HasStandardSettings()
        {
            MahjongRuleSet ruleSet = MahjongRuleSet.CreateStandardFourPlayer();

            Assert.AreEqual(4, ruleSet.PlayerCount);
            Assert.AreEqual(136, ruleSet.TileSet.BuildTiles().Count);
            Assert.AreEqual(StandardYakuSet.CreateAll().Count, ruleSet.EnabledYaku.Count);
        }

        [TestCase(1)]
        [TestCase(5)]
        public void Constructor_UnsupportedPlayerCount_Throws(int playerCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MahjongRuleSet(
                    playerCount,
                    TileSetDefinition.CreateStandard(),
                    StandardYakuSet.CreateAll(),
                    new WallRule(),
                    new ScoringRule()
                )
            );
        }

        [Test]
        public void Constructor_NullTileSet_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MahjongRuleSet(
                    4,
                    null,
                    StandardYakuSet.CreateAll(),
                    new WallRule(),
                    new ScoringRule()
                )
            );
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void ScoringRule_NonPositiveFuRoundingUnit_Throws(int fuRoundingUnit)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ScoringRule(fuRoundingUnit: fuRoundingUnit)
            );
        }

        [Test]
        public void ScoringRule_NonPositivePointRoundingUnit_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScoringRule(pointRoundingUnit: 0));
        }

        [Test]
        public void StandardYakuSet_YakuNamesAreUnique()
        {
            var names = new HashSet<string>();
            foreach (IYaku yaku in StandardYakuSet.CreateAll())
            {
                Assert.IsTrue(names.Add(yaku.Name), $"役名「{yaku.Name}」が重複しています。");
            }
        }
    }
}
