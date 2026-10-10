using System.Collections.Generic;
using NUnit.Framework;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Yaku;
using static Zizo.Mahjong.Tests.YakuTestHelper;

namespace Zizo.Mahjong.Tests
{
    // YakuEvaluatorTestsでカバーしていない標準役と、役同士の排他・鳴きによる食い下がりを検証する。
    public class StandardYakuTests
    {
        // 234567m 345p 99p 678s: 平和形の基本手。和了牌6sは678sの両面待ち。
        private const string PinfuShapeHand = "234567m34599p678s";

        // --- 状況役（偶然役） ---

        [Test]
        public void Ippatsu_WithRiichi_Matches()
        {
            YakuEvaluationResult result = Evaluate(
                PinfuShapeHand,
                "6s",
                isRiichi: true,
                isIppatsu: true
            );

            AssertYakuMatched(result, "一発", 1);
            AssertYakuMatched(result, "立直", 1);
        }

        [Test]
        public void Ippatsu_WithoutRiichi_DoesNotMatch()
        {
            YakuEvaluationResult result = Evaluate(PinfuShapeHand, "6s", isIppatsu: true);

            AssertYakuNotMatched(result, "一発");
        }

        [Test]
        public void HaiteiRaoyue_TsumoOnLastTile_Matches()
        {
            YakuEvaluationResult result = Evaluate(
                PinfuShapeHand,
                "6s",
                isTsumo: true,
                isHaitei: true
            );

            AssertYakuMatched(result, "海底摸月", 1);
            AssertYakuNotMatched(result, "河底撈魚");
        }

        [Test]
        public void HouteiRaoyui_RonOnLastDiscard_Matches()
        {
            YakuEvaluationResult result = Evaluate(PinfuShapeHand, "6s", isHoutei: true);

            AssertYakuMatched(result, "河底撈魚", 1);
            AssertYakuNotMatched(result, "海底摸月");
        }

        [Test]
        public void RinshanKaihou_TsumoOnReplacementTile_Matches()
        {
            var melds = new List<Meld> { MeldFactory.ClosedKan("1111z") };

            YakuEvaluationResult result = Evaluate(
                "234567m99p678s",
                "6s",
                isTsumo: true,
                melds: melds,
                isRinshan: true
            );

            AssertYakuMatched(result, "嶺上開花", 1);
        }

        [Test]
        public void Chankan_RobbingAddedKan_Matches()
        {
            YakuEvaluationResult result = Evaluate(PinfuShapeHand, "6s", isChankan: true);

            AssertYakuMatched(result, "槍槓", 1);
        }

        // --- 第一巡の役満 ---

        [Test]
        public void Tenhou_DealerFirstTsumo_Matches()
        {
            YakuEvaluationResult result = Evaluate(
                PinfuShapeHand,
                "6s",
                isTsumo: true,
                seatWind: East,
                isFirstUninterruptedTurnWin: true
            );

            AssertYakuMatched(result, "天和", 1);
            AssertYakuNotMatched(result, "地和");
            Assert.IsTrue(result.HasYakuman);
        }

        [Test]
        public void Chiihou_NonDealerFirstTsumo_Matches()
        {
            YakuEvaluationResult result = Evaluate(
                PinfuShapeHand,
                "6s",
                isTsumo: true,
                seatWind: South,
                isFirstUninterruptedTurnWin: true
            );

            AssertYakuMatched(result, "地和", 1);
            AssertYakuNotMatched(result, "天和");
        }

        [Test]
        public void Renhou_NonDealerRonBeforeFirstDraw_Matches()
        {
            YakuEvaluationResult result = Evaluate(
                PinfuShapeHand,
                "6s",
                seatWind: South,
                isFirstUninterruptedTurnWin: true
            );

            AssertYakuMatched(result, "人和", 1);
        }

        // --- 面子構成の役 ---

        [Test]
        public void Sanankou_ThreeConcealedTriplets_Matches()
        {
            // 111m 234m 77m 333p 555s: 和了牌4mは順子側のため、3つの刻子はすべて暗刻のまま。
            YakuEvaluationResult result = Evaluate("11123477m333p555s", "4m");

            AssertYakuMatched(result, "三暗刻", 2);
        }

        [Test]
        public void Sanankou_RonCompletingTriplet_DoesNotMatch()
        {
            // 和了牌5sは555sの刻子でしか使えないため、ロンでは明刻扱いになり暗刻は2つに減る。
            YakuEvaluationResult result = Evaluate("11123477m333p555s", "5s");

            AssertYakuNotMatched(result, "三暗刻");
        }

        [Test]
        public void Sanankou_RonTileAlsoFitsSequence_KeepsTripletConcealed()
        {
            // 1111m 23m: ロンした1mは123mの順子側とみなせるため、111mは暗刻のまま数える。
            YakuEvaluationResult result = Evaluate("111123m333p555s77z", "1m");

            AssertYakuMatched(result, "三暗刻", 2);
        }

        [Test]
        public void SanshokuDoukou_SameTripletAcrossThreeSuits_Matches()
        {
            YakuEvaluationResult result = Evaluate("22234577m222p222s", "5m", isTsumo: true);

            AssertYakuMatched(result, "三色同刻", 2);
        }

        [Test]
        public void Sankantsu_ThreeKans_Matches()
        {
            var melds = new List<Meld>
            {
                MeldFactory.OpenKan("2222p"),
                MeldFactory.ClosedKan("7777s"),
                MeldFactory.AddedKan("9999m"),
            };

            YakuEvaluationResult result = Evaluate("234m55p", "5p", melds: melds);

            AssertYakuMatched(result, "三槓子", 2);
        }

        [Test]
        public void Honroutou_AllTerminalsAndHonors_ExcludesChanta()
        {
            // 順子を含まない幺九牌のみの手は混老頭で、混全帯幺九とは複合しない。
            var melds = new List<Meld> { MeldFactory.Pon("111m") };

            YakuEvaluationResult result = Evaluate("999p111s11122z", "2z", melds: melds);

            AssertYakuMatched(result, "混老頭", 2);
            AssertYakuMatched(result, "対々和", 2);
            AssertYakuNotMatched(result, "混全帯幺九");
        }

        [Test]
        public void Chinroutou_AllTerminals_ExcludesJunchan()
        {
            YakuEvaluationResult result = Evaluate("111999m111999p11s", "1s");

            AssertYakuMatched(result, "清老頭", 1);
            AssertYakuNotMatched(result, "純全帯幺九");
            Assert.IsTrue(result.HasYakuman);
        }

        // --- 門前・鳴きの扱い ---

        [Test]
        public void MenzenTsumo_WithClosedKan_StillMatches()
        {
            // 暗槓は面前を崩さない。
            var melds = new List<Meld> { MeldFactory.ClosedKan("9999p") };

            YakuEvaluationResult result = Evaluate(
                "234567m345p55s",
                "3p",
                isTsumo: true,
                melds: melds
            );

            AssertYakuMatched(result, "門前清自摸和", 1);
        }

        [Test]
        public void Iipeikou_WithClosedKan_StillMatches()
        {
            var melds = new List<Meld> { MeldFactory.ClosedKan("9999s") };

            YakuEvaluationResult result = Evaluate("112233m456p55s", "3m", melds: melds);

            AssertYakuMatched(result, "一盃口", 1);
        }

        [Test]
        public void Iipeikou_OpenHand_DoesNotMatch()
        {
            var melds = new List<Meld> { MeldFactory.Chi("789s") };

            YakuEvaluationResult result = Evaluate("112233m456p55s", "3m", melds: melds);

            AssertYakuNotMatched(result, "一盃口");
        }

        [Test]
        public void Ittsu_OpenHand_LosesOneHan()
        {
            var melds = new List<Meld> { MeldFactory.Chi("123m") };

            YakuEvaluationResult result = Evaluate("456789m111p55s", "5s", melds: melds);

            AssertYakuMatched(result, "一気通貫", 1);
        }

        [Test]
        public void SanshokuDoujun_OpenHand_LosesOneHan()
        {
            var melds = new List<Meld> { MeldFactory.Chi("123p") };

            YakuEvaluationResult result = Evaluate("123456m99p123s", "6m", melds: melds);

            AssertYakuMatched(result, "三色同順", 1);
        }

        [Test]
        public void Honitsu_OpenHand_LosesOneHan()
        {
            var melds = new List<Meld> { MeldFactory.Pon("555z") };

            YakuEvaluationResult result = Evaluate("12345678999m", "9m", melds: melds);

            AssertYakuMatched(result, "混一色", 2);
        }

        [Test]
        public void Chinitsu_OpenHand_LosesOneHan()
        {
            var melds = new List<Meld> { MeldFactory.Chi("123m") };

            YakuEvaluationResult result = Evaluate("23456788811m", "1m", melds: melds);

            AssertYakuMatched(result, "清一色", 5);
        }

        [Test]
        public void Chanta_OpenHand_LosesOneHan()
        {
            var melds = new List<Meld> { MeldFactory.Chi("123m") };

            YakuEvaluationResult result = Evaluate("789p111z78999s", "9s", melds: melds);

            AssertYakuMatched(result, "混全帯幺九", 1);
        }

        [Test]
        public void Tanyao_OpenHand_StillMatches()
        {
            // 喰いタンあり。
            var melds = new List<Meld> { MeldFactory.Chi("234m") };

            YakuEvaluationResult result = Evaluate("567m345678p55s", "5s", melds: melds);

            AssertYakuMatched(result, "断幺九", 1);
        }

        // --- 平和の条件 ---

        [Test]
        public void Pinfu_KanchanWait_DoesNotMatch()
        {
            // 和了牌7sは678sの真ん中（嵌張待ち）。
            YakuEvaluationResult result = Evaluate(PinfuShapeHand, "7s");

            AssertYakuNotMatched(result, "平和");
        }

        [Test]
        public void Pinfu_YakuhaiPair_DoesNotMatch()
        {
            YakuEvaluationResult result = Evaluate("234567m345p678s55z", "6s");

            AssertYakuNotMatched(result, "平和");
        }

        [Test]
        public void Pinfu_OpenHand_DoesNotMatch()
        {
            var melds = new List<Meld> { MeldFactory.Chi("234m") };

            YakuEvaluationResult result = Evaluate("567m34599p678s", "6s", melds: melds);

            AssertYakuNotMatched(result, "平和");
        }

        [Test]
        public void Pinfu_Tsumo_CombinesWithMenzenTsumo()
        {
            YakuEvaluationResult result = Evaluate(PinfuShapeHand, "6s", isTsumo: true);

            AssertYakuMatched(result, "平和", 1);
            AssertYakuMatched(result, "門前清自摸和", 1);
        }

        // --- 役牌 ---

        [Test]
        public void Yakuhai_RoundWindOnlyForNonDealer_MatchesOneHan()
        {
            YakuEvaluationResult result = Evaluate("234567m34599p111z", "1z", seatWind: South);

            AssertYakuMatched(result, "役牌", 1);
        }

        [Test]
        public void Yakuhai_GuestWindTriplet_DoesNotMatch()
        {
            // 北(4z)は自風(南)でも場風(東)でもない客風牌。
            YakuEvaluationResult result = Evaluate("234567m34599p444z", "4z", seatWind: South);

            AssertYakuNotMatched(result, "役牌");
        }

        // --- 役満 ---

        [Test]
        public void Tsuuiisou_AllHonorTriplets_Matches()
        {
            YakuEvaluationResult result = Evaluate("11122233355566z", "6z");

            AssertYakuMatched(result, "字一色", 1);
        }

        [Test]
        public void Tsuuiisou_SevenHonorPairs_CombinesWithChiitoitsuShape()
        {
            YakuEvaluationResult result = Evaluate("11223344556677z", "7z");

            AssertYakuMatched(result, "字一色", 1);
            Assert.IsTrue(result.Context.IsChiitoitsuShape);
        }

        [Test]
        public void Ryuuiisou_GreenTilesOnly_Matches()
        {
            YakuEvaluationResult result = Evaluate("223344666888s66z", "8s");

            AssertYakuMatched(result, "緑一色", 1);
        }

        [Test]
        public void Shousuushi_ThreeWindTripletsAndWindPair_Matches()
        {
            YakuEvaluationResult result = Evaluate("123m11122233344z", "3m");

            AssertYakuMatched(result, "小四喜", 1);
            AssertYakuNotMatched(result, "大四喜");
        }

        [Test]
        public void Daisuushi_FourWindTriplets_MatchesDoubleYakuman()
        {
            YakuEvaluationResult result = Evaluate("11m111222333444z", "1m");

            AssertYakuMatched(result, "大四喜", 2);
            AssertYakuNotMatched(result, "小四喜");
        }

        [Test]
        public void Suukantsu_FourKans_Matches()
        {
            var melds = new List<Meld>
            {
                MeldFactory.OpenKan("2222p"),
                MeldFactory.ClosedKan("7777s"),
                MeldFactory.AddedKan("9999m"),
                MeldFactory.OpenKan("3333m"),
            };

            YakuEvaluationResult result = Evaluate("55p", "5p", melds: melds);

            AssertYakuMatched(result, "四槓子", 1);
            AssertYakuNotMatched(result, "三槓子");
        }

        [Test]
        public void ChuurenPoutou_NonNineSidedWait_MatchesSingleYakuman()
        {
            // 和了牌を除いた13枚(1123455678999m)は九面待ちの形ではない。
            YakuEvaluationResult result = Evaluate("11123455678999m", "1m");

            AssertYakuMatched(result, "九蓮宝燈", 1);
            AssertYakuNotMatched(result, "純正九蓮宝燈");
        }

        [Test]
        public void ChuurenPoutouPure_NineSidedWait_MatchesDoubleYakuman()
        {
            // 和了牌を除いた13枚が1112345678999mちょうど（九面待ち）。
            YakuEvaluationResult result = Evaluate("11123455678999m", "5m");

            AssertYakuMatched(result, "純正九蓮宝燈", 2);
            AssertYakuNotMatched(result, "九蓮宝燈");
        }

        [Test]
        public void Suuankou_RonCompletingTriplet_DoesNotMatch()
        {
            // シャンポン待ちのロンは刻子の1つが明刻になるため、四暗刻ではなく三暗刻+対々和になる。
            YakuEvaluationResult result = Evaluate("111222m333444p55s", "4p");

            AssertYakuNotMatched(result, "四暗刻");
            AssertYakuNotMatched(result, "四暗刻単騎");
            AssertYakuMatched(result, "三暗刻", 2);
            AssertYakuMatched(result, "対々和", 2);
        }

        [Test]
        public void Yakuman_PresentAlongsideRegularYaku_ScoresOnlyYakuman()
        {
            YakuEvaluationResult result = Evaluate("123m99p555666777z", "7z", isRiichi: true);

            Assert.IsTrue(result.HasYakuman);
            AssertYakuMatched(result, "立直", 1);
            Assert.AreEqual(1, result.TotalHan, "役満がある場合は通常役の翻数を合算しない。");
        }
    }
}
