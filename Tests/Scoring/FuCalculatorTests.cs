using System.Collections.Generic;
using NUnit.Framework;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Scoring;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;
using Zizo.Mahjong.Yaku.Standard;

namespace Zizo.Mahjong.Tests
{
    public class FuCalculatorTests
    {
        private static readonly List<IYaku> EnabledYaku = StandardYakuSet.CreateAll();
        private static readonly TileKind East = new TileKind(TileSuit.Wind, 1);
        private static readonly ScoringRule ScoringRule = new ScoringRule();

        private static WinningContext EvaluateContext(
            string handNotation,
            Tile winningTile,
            bool isTsumo,
            IReadOnlyList<Meld> melds = null
        )
        {
            List<Tile> concealedTiles = HandNotation.Parse(handNotation);
            var situation = new WinningSituation(
                melds: melds ?? new List<Meld>(),
                winningTile: winningTile,
                isTsumo: isTsumo,
                seatWind: East,
                roundWind: East
            );
            int calledMeldCount = melds?.Count ?? 0;
            YakuEvaluationResult result = YakuEvaluator.EvaluateBest(
                concealedTiles,
                calledMeldCount,
                situation,
                EnabledYaku
            );
            return result.Context;
        }

        [Test]
        public void Calculate_Pinfu_Ron_IsFixedThirtyFu()
        {
            WinningContext context = EvaluateContext(
                "234567m34599p678s",
                HandNotation.Single("6s"),
                isTsumo: false
            );

            Assert.AreEqual(30, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_Pinfu_Tsumo_IsFixedTwentyFu()
        {
            WinningContext context = EvaluateContext(
                "234567m34599p678s",
                HandNotation.Single("6s"),
                isTsumo: true
            );

            Assert.AreEqual(20, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_WinningTileFitsBothRyanmenAndKanchan_UsesHigherKanchanFu()
        {
            // 234m+345mの4mツモは、23mの両面待ちとも35mの嵌張待ちとも解釈できる。高点法により嵌張(2符)を採用する。
            // 符内訳: 20(base) + 2(ツモ) + 8(111sの暗刻・老頭牌) + 2(嵌張) = 32 → 40。
            WinningContext context = EvaluateContext(
                "234345m678p99p111s",
                HandNotation.Single("4m"),
                isTsumo: true
            );

            Assert.AreEqual(40, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_PenchanWait_AddsTwoFu()
        {
            // 符内訳: 20(base) + 2(ツモ) + 8(111sの暗刻・老頭牌) + 2(12mの3m待ち=辺張) = 32 → 40。
            WinningContext context = EvaluateContext(
                "123m456p789p99p111s",
                HandNotation.Single("3m"),
                isTsumo: true
            );

            Assert.AreEqual(40, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_OpenAndClosedKans_AddKanFu()
        {
            // 符内訳: 20(base) + 16(東の明槓・字牌) + 16(5pの暗槓・中張牌) + 0(両面) = 52 → 60。
            var melds = new List<Meld>
            {
                MeldFactory.OpenKan("1111z"),
                MeldFactory.ClosedKan("5555p"),
            };
            WinningContext context = EvaluateContext(
                "234m678s99m",
                HandNotation.Single("4m"),
                isTsumo: false,
                melds
            );

            Assert.AreEqual(60, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_MenzenRonWithConcealedHonorTripletAndDragonPair_RoundsUpToForty()
        {
            // 符内訳: 20(base) + 10(門前ロン) + 2(白の対子) + 0(23pの両面待ち) + 8(東の暗刻・字牌) = 40。
            WinningContext context = EvaluateContext(
                "111z456m789s55z123p",
                HandNotation.Single("1p"),
                isTsumo: false
            );

            Assert.AreEqual(40, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_OpenHandTankiWaitWithTerminalPon_RoundsUpToThirty()
        {
            // 符内訳: 20(base) + 2(単騎待ち) + 4(9pの明刻) = 26 -> 切り上げ30。
            var melds = new List<Meld>
            {
                new Meld(
                    MeldType.Pon,
                    HandNotation.Parse("999p"),
                    calledTile: HandNotation.Single("9p"),
                    sourceSeatIndex: 1
                ),
            };

            WinningContext context = EvaluateContext(
                "234m567p789s55m",
                HandNotation.Single("5m"),
                isTsumo: false,
                melds: melds
            );

            Assert.AreEqual(30, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_OpenPinfuShapeRon_IsThirtyFu()
        {
            // 符内訳: 20(base) + 0(55mの雀頭) + 0(67sの両面待ち) + 0(チー) = 20 -> 喰い平和形のため30。
            var melds = new List<Meld>
            {
                new Meld(
                    MeldType.Chi,
                    HandNotation.Parse("345p"),
                    calledTile: HandNotation.Single("3p"),
                    sourceSeatIndex: 3
                ),
            };

            WinningContext context = EvaluateContext(
                "234m567m678s55m",
                HandNotation.Single("8s"),
                isTsumo: false,
                melds: melds
            );

            Assert.AreEqual(30, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_OpenPinfuShapeTsumo_RoundsUpToThirty()
        {
            // 符内訳: 20(base) + 2(ツモ) = 22 -> 切り上げ30。
            var melds = new List<Meld>
            {
                new Meld(
                    MeldType.Chi,
                    HandNotation.Parse("345p"),
                    calledTile: HandNotation.Single("3p"),
                    sourceSeatIndex: 3
                ),
            };

            WinningContext context = EvaluateContext(
                "234m567m678s55m",
                HandNotation.Single("8s"),
                isTsumo: true,
                melds: melds
            );

            Assert.AreEqual(30, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_Chiitoitsu_IsFixedTwentyFiveFu()
        {
            WinningContext context = EvaluateContext(
                "1133557799m2244p",
                HandNotation.Single("4p"),
                isTsumo: false
            );

            Assert.AreEqual(25, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_KanchanWait_AddsTwoFu()
        {
            // 符内訳: 20(base) + 10(門前ロン) + 2(123pの嵌張待ち) = 32 -> 切り上げ40。
            WinningContext context = EvaluateContext(
                "23455m123567p789s",
                HandNotation.Single("2p"),
                isTsumo: false
            );

            Assert.AreEqual(40, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_DoubleWindPair_AddsFourFu()
        {
            // 符内訳: 20(base) + 10(門前ロン) + 4(自風かつ場風の東の対子) = 34 -> 切り上げ40。
            WinningContext context = EvaluateContext(
                "234567m345p678s11z",
                HandNotation.Single("6s"),
                isTsumo: false
            );

            Assert.AreEqual(40, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_RonCompletingSimpleTriplet_CountsAsOpenTriplet()
        {
            // 符内訳: 20(base) + 10(門前ロン) + 2(ロンで完成した222sは明刻扱い) = 32 -> 切り上げ40。
            WinningContext context = EvaluateContext(
                "234m567p222s789s55m",
                HandNotation.Single("2s"),
                isTsumo: false
            );

            Assert.AreEqual(40, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_TsumoCompletingSimpleTriplet_CountsAsConcealedTriplet()
        {
            // 符内訳: 20(base) + 2(ツモ) + 4(222sの暗刻) = 26 -> 切り上げ30。
            WinningContext context = EvaluateContext(
                "234m567p222s789s55m",
                HandNotation.Single("2s"),
                isTsumo: true
            );

            Assert.AreEqual(30, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_RonTileAlsoFitsSequence_KeepsTripletConcealed()
        {
            // ロンした1mを123mの順子側とみなせるため、111mは暗刻のまま数える。
            // 符内訳: 20 + 10(門前ロン) + 2(中の対子) + 8(111m暗刻) + 4(333p暗刻) + 4(555s暗刻) = 48 -> 50。
            WinningContext context = EvaluateContext(
                "111123m333p555s77z",
                HandNotation.Single("1m"),
                isTsumo: false
            );

            Assert.AreEqual(50, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_MenzenRonWithClosedKan_KeepsMenzenRonBonus()
        {
            // 暗槓は面前を崩さないため門前ロンの10符が付く。
            // 符内訳: 20(base) + 10(門前ロン) + 32(9pの暗槓・老頭牌) = 62 -> 切り上げ70。
            var melds = new List<Meld> { MeldFactory.ClosedKan("9999p") };

            WinningContext context = EvaluateContext(
                "234567m345p55s",
                HandNotation.Single("3p"),
                isTsumo: false,
                melds: melds
            );

            Assert.AreEqual(70, FuCalculator.Calculate(context, ScoringRule));
        }

        [Test]
        public void Calculate_KokushiShape_ReturnsZero()
        {
            WinningContext context = EvaluateContext(
                "119m19p19s1234567z",
                HandNotation.Single("1m"),
                isTsumo: false
            );

            Assert.AreEqual(0, FuCalculator.Calculate(context, ScoringRule));
        }
    }
}
