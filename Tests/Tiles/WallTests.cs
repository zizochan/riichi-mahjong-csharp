using System;
using NUnit.Framework;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    // SequentialRandomSourceでシャッフルを無効化し、標準牌構成の定義順がそのまま山の並びになることを利用する。
    // 王牌はその末尾14枚: [北, 北, 白×4, 發×4, 中×4]。
    public class WallTests
    {
        private const int StandardTileCount = 136;
        private const int DeadWallTileCount = 14;

        private static Wall CreateWall()
        {
            return new Wall(
                TileSetDefinition.CreateStandard(),
                new WallRule(),
                new SequentialRandomSource()
            );
        }

        [Test]
        public void Constructor_SeparatesDeadWallAndRevealsOneDoraIndicator()
        {
            Wall wall = CreateWall();

            Assert.AreEqual(StandardTileCount - DeadWallTileCount, wall.RemainingLiveTileCount);
            Assert.AreEqual(1, wall.DoraIndicators.Count);
            Assert.AreEqual(0, wall.UraDoraIndicators.Count);
            // ドラ表示牌は王牌の末尾から使う。
            Assert.AreEqual(HandNotation.Single("7z"), wall.DoraIndicators[0]);
        }

        [Test]
        public void DrawFromLiveWall_TakesTilesInWallOrder()
        {
            Wall wall = CreateWall();

            Assert.AreEqual(HandNotation.Single("1m"), wall.DrawFromLiveWall());
            Assert.AreEqual(StandardTileCount - DeadWallTileCount - 1, wall.RemainingLiveTileCount);
        }

        [Test]
        public void DrawFromLiveWall_WhenEmpty_Throws()
        {
            Wall wall = CreateWall();
            while (wall.RemainingLiveTileCount > 0)
            {
                wall.DrawFromLiveWall();
            }

            Assert.Throws<InvalidOperationException>(() => wall.DrawFromLiveWall());
        }

        [Test]
        public void DrawFromDeadWall_ShrinksLiveWallToKeepDeadWallSize()
        {
            Wall wall = CreateWall();
            int liveCountBefore = wall.RemainingLiveTileCount;

            Tile rinshanTile = wall.DrawFromDeadWall();

            Assert.AreEqual(HandNotation.Single("4z"), rinshanTile);
            Assert.AreEqual(liveCountBefore - 1, wall.RemainingLiveTileCount);
        }

        [Test]
        public void DrawFromDeadWall_BeyondMaxKanCount_Throws()
        {
            Wall wall = CreateWall();
            for (int kanIndex = 0; kanIndex < new WallRule().MaxKanCount; kanIndex++)
            {
                wall.DrawFromDeadWall();
            }

            Assert.IsFalse(wall.CanDrawFromDeadWall);
            Assert.Throws<InvalidOperationException>(() => wall.DrawFromDeadWall());
        }

        [Test]
        public void RevealUraDoraIndicatorsToMatchDora_RevealsSameCountFromSeparateTiles()
        {
            Wall wall = CreateWall();
            wall.RevealNextDoraIndicator();

            wall.RevealUraDoraIndicatorsToMatchDora();
            wall.RevealUraDoraIndicatorsToMatchDora();

            Assert.AreEqual(2, wall.DoraIndicators.Count);
            Assert.AreEqual(2, wall.UraDoraIndicators.Count);
            // 裏ドラ表示牌は嶺上牌(先頭4枚)の直後から使う: 白, 白。表ドラ表示牌は末尾から: 中, 中。
            Assert.AreEqual(HandNotation.Single("5z"), wall.UraDoraIndicators[0]);
            Assert.AreEqual(HandNotation.Single("7z"), wall.DoraIndicators[1]);
        }

        [Test]
        public void RevealNextDoraIndicator_BeyondLimit_Throws()
        {
            Wall wall = CreateWall();
            for (int kanIndex = 0; kanIndex < new WallRule().MaxKanCount; kanIndex++)
            {
                wall.RevealNextDoraIndicator();
            }

            Assert.Throws<InvalidOperationException>(() => wall.RevealNextDoraIndicator());
        }

        [Test]
        public void WallRule_DeadWallTooSmallForIndicators_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WallRule(deadWallTileCount: 10));
        }
    }
}
