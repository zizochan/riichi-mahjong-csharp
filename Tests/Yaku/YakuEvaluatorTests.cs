using System.Collections.Generic;
using NUnit.Framework;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;
using static Zizo.Mahjong.Tests.YakuTestHelper;

namespace Zizo.Mahjong.Tests
{
    // 個々のIYaku実装ではなく、YakuEvaluator.EvaluateBest経由の「実際に成立する役の一覧」を検証する。
    // 内部の面子分解ロジックが変わっても、狙った役が(他の付随役とともに)成立し続けることだけを見るため、
    // 期待する役の翻数を持つエントリがMatchedYakuに含まれているかどうかで判定する（合計翻数の完全一致は見ない）。
    public class YakuEvaluatorTests
    {
        [Test]
        public void Pinfu_SequencesOnlyWithRyanmenWaitAndNonYakuhaiPair_Matches()
        {
            YakuEvaluationResult result = Evaluate("234567m34599p678s", "6s");

            AssertYakuMatched(result, "平和", 1);
        }

        [Test]
        public void Tanyao_NoTerminalOrHonorTile_Matches()
        {
            YakuEvaluationResult result = Evaluate("234567m345678p55s", "5s");

            AssertYakuMatched(result, "断幺九", 1);
        }

        [Test]
        public void Yakuhai_DragonTriplet_MatchesOneHan()
        {
            YakuEvaluationResult result = Evaluate("234567m34599p555z", "5z");

            AssertYakuMatched(result, "役牌", 1);
        }

        [Test]
        public void Yakuhai_SeatAndRoundWindTriplet_MatchesTwoHan()
        {
            YakuEvaluationResult result = Evaluate(
                "234567m34599p111z",
                "1z",
                seatWind: East,
                roundWind: East
            );

            AssertYakuMatched(result, "役牌", 2);
        }

        [Test]
        public void Riichi_DeclaredAndNotDouble_Matches()
        {
            YakuEvaluationResult result = Evaluate("234567m34599p678s", "6s", isRiichi: true);

            AssertYakuMatched(result, "立直", 1);
        }

        [Test]
        public void DoubleRiichi_ExcludesRegularRiichi()
        {
            YakuEvaluationResult result = Evaluate(
                "234567m34599p678s",
                "6s",
                isRiichi: true,
                isDoubleRiichi: true
            );

            AssertYakuMatched(result, "両立直", 2);
            AssertYakuNotMatched(result, "立直");
        }

        [Test]
        public void MenzenTsumo_MenzenAndTsumo_Matches()
        {
            YakuEvaluationResult result = Evaluate("234567m34599p678s", "6s", isTsumo: true);

            AssertYakuMatched(result, "門前清自摸和", 1);
        }

        [Test]
        public void Iipeikou_OneDuplicateSequenceUnit_Matches()
        {
            YakuEvaluationResult result = Evaluate("112233m456p78999s", "3m");

            AssertYakuMatched(result, "一盃口", 1);
        }

        [Test]
        public void Ryanpeikou_TwoDuplicateSequenceUnits_ExcludesIipeikou()
        {
            YakuEvaluationResult result = Evaluate("112233m445566p99s", "6p");

            AssertYakuMatched(result, "二盃口", 3);
            AssertYakuNotMatched(result, "一盃口");
        }

        [Test]
        public void Chiitoitsu_SevenDistinctPairs_Matches()
        {
            YakuEvaluationResult result = Evaluate("1133557799m2244p", "4p");

            AssertYakuMatched(result, "七対子", 2);
        }

        [Test]
        public void Toitoi_AllTriplets_Matches()
        {
            YakuEvaluationResult result = Evaluate("111222m333444p55s", "5s");

            AssertYakuMatched(result, "対々和", 2);
        }

        [Test]
        public void Honitsu_SingleSuitPlusHonors_MatchesMenzenValue()
        {
            YakuEvaluationResult result = Evaluate("12345678999m555z", "5z");

            AssertYakuMatched(result, "混一色", 3);
        }

        [Test]
        public void Chinitsu_SingleSuitOnly_MatchesMenzenValue()
        {
            YakuEvaluationResult result = Evaluate("12345555678889m", "5m", isTsumo: true);

            AssertYakuMatched(result, "清一色", 6);
        }

        [Test]
        public void Ittsu_OneTwoThreeRunInSameSuit_Matches()
        {
            YakuEvaluationResult result = Evaluate("123456789m111p55s", "1p");

            AssertYakuMatched(result, "一気通貫", 2);
        }

        [Test]
        public void SanshokuDoujun_SameSequenceAcrossThreeSuits_Matches()
        {
            YakuEvaluationResult result = Evaluate("123456m12399p123s", "6m");

            AssertYakuMatched(result, "三色同順", 2);
        }

        [Test]
        public void Chanta_AllGroupsTouchTerminalOrHonor_MatchesMenzenValue()
        {
            YakuEvaluationResult result = Evaluate("123m789p111z78999s", "9s");

            AssertYakuMatched(result, "混全帯幺九", 2);
        }

        [Test]
        public void Junchan_NoHonorTile_ExcludesChanta()
        {
            YakuEvaluationResult result = Evaluate("123789m789p11199s", "9p");

            AssertYakuMatched(result, "純全帯幺九", 3);
            AssertYakuNotMatched(result, "混全帯幺九");
        }

        [Test]
        public void KokushiMusou_SingleWaitCompletion_ExcludesThirteenWait()
        {
            List<Tile> hand = HandNotation.Parse("119m19p19s1234567z");

            YakuEvaluationResult result = YakuEvaluator.EvaluateBest(
                hand,
                calledMeldCount: 0,
                new WinningSituation(
                    melds: new List<Meld>(),
                    winningTile: HandNotation.Single("7z"),
                    isTsumo: false,
                    seatWind: East,
                    roundWind: East
                ),
                EnabledYaku
            );

            AssertYakuMatched(result, "国士無双", 1);
            AssertYakuNotMatched(result, "国士無双十三面待ち");
        }

        [Test]
        public void KokushiMusou_ThirteenSidedWaitCompletion_MatchesDoubleYakuman()
        {
            List<Tile> hand = HandNotation.Parse("119m19p19s1234567z");

            YakuEvaluationResult result = YakuEvaluator.EvaluateBest(
                hand,
                calledMeldCount: 0,
                new WinningSituation(
                    melds: new List<Meld>(),
                    winningTile: HandNotation.Single("1m"),
                    isTsumo: false,
                    seatWind: East,
                    roundWind: East
                ),
                EnabledYaku
            );

            AssertYakuMatched(result, "国士無双十三面待ち", 2);
            AssertYakuNotMatched(result, "国士無双");
            Assert.IsTrue(result.HasYakuman);
        }

        [Test]
        public void Suuankou_TsumoCompletingTriplet_ExcludesTanki()
        {
            YakuEvaluationResult result = Evaluate("111222m333444p55s", "3p", isTsumo: true);

            AssertYakuMatched(result, "四暗刻", 1);
            AssertYakuNotMatched(result, "四暗刻単騎");
        }

        [Test]
        public void SuuankouTanki_RonCompletingPair_MatchesDoubleYakumanInsteadOfSuuankou()
        {
            YakuEvaluationResult result = Evaluate("111222m333444p55s", "5s");

            AssertYakuMatched(result, "四暗刻単騎", 2);
            AssertYakuNotMatched(result, "四暗刻");
        }

        [Test]
        public void Daisangen_ThreeDragonTriplets_Matches()
        {
            YakuEvaluationResult result = Evaluate("123m99p555666777z", "7z");

            AssertYakuMatched(result, "大三元", 1);
            Assert.IsTrue(result.HasYakuman);
        }

        [Test]
        public void Shousangen_TwoDragonTripletsPlusDragonPair_ExcludesDaisangen()
        {
            YakuEvaluationResult result = Evaluate("123m456p55566677z", "6z");

            AssertYakuMatched(result, "小三元", 2);
            AssertYakuNotMatched(result, "大三元");
        }
    }
}
