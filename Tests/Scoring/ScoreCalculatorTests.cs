using NUnit.Framework;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Scoring;

namespace Zizo.Mahjong.Tests
{
    // ScoreCalculator/ScoreResultの点数表が標準的な麻雀の点数早見表と一致するかを検証する。
    public class ScoreCalculatorTests
    {
        private static readonly ScoringRule ScoringRule = new ScoringRule();

        [Test]
        public void Calculate_FourHanThirtyFu_NonDealerRon_Is7700()
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han: 4,
                fu: 30,
                winnerIsDealer: false,
                ScoringRule
            );

            Assert.AreEqual(7700, result.RonPayment);
        }

        [Test]
        public void Calculate_ThreeHanThirtyFu_DealerTsumo_Is2000All()
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han: 3,
                fu: 30,
                winnerIsDealer: true,
                ScoringRule
            );

            Assert.AreEqual(2000, result.TsumoPaymentFromNonDealer);
        }

        [Test]
        public void Calculate_HanFuOverManganThreshold_IsCappedAtManganBasePoints()
        {
            // 4翻40符は符×2^(2+4)=2560だが満貫(2000)で頭打ちになる。
            ScoreResult result = ScoreCalculator.Calculate(
                han: 4,
                fu: 40,
                winnerIsDealer: false,
                ScoringRule
            );

            Assert.AreEqual(ScoringRule.ManganBasePoints, result.BasePoints);
            Assert.AreEqual(8000, result.RonPayment);
        }

        [Test]
        public void Calculate_SixHan_UsesHanemanBasePointsRegardlessOfFu()
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han: 6,
                fu: 30,
                winnerIsDealer: true,
                ScoringRule
            );

            Assert.AreEqual(ScoringRule.HanemanBasePoints, result.BasePoints);
            Assert.AreEqual(18000, result.RonPayment);
        }

        [Test]
        public void Calculate_ThirteenHan_CountedYakumanUsesYakumanBasePoints()
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han: 13,
                fu: 30,
                winnerIsDealer: false,
                ScoringRule
            );

            Assert.AreEqual(ScoringRule.YakumanBasePoints, result.BasePoints);
        }

        [Test]
        public void Calculate_SingleYakuman_DealerRon_Is48000()
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han: 0,
                fu: 0,
                winnerIsDealer: true,
                ScoringRule,
                yakumanMultiplier: 1
            );

            Assert.IsTrue(result.IsYakuman);
            Assert.AreEqual(48000, result.RonPayment);
        }

        [Test]
        public void Calculate_DoubleYakuman_BasePointsDouble()
        {
            ScoreResult single = ScoreCalculator.Calculate(
                han: 0,
                fu: 0,
                winnerIsDealer: false,
                ScoringRule,
                yakumanMultiplier: 1
            );
            ScoreResult doubled = ScoreCalculator.Calculate(
                han: 0,
                fu: 0,
                winnerIsDealer: false,
                ScoringRule,
                yakumanMultiplier: 2
            );

            Assert.AreEqual(single.BasePoints * 2, doubled.BasePoints);
        }

        [Test]
        public void GetTotalTsumoPoints_DealerTsumo_EqualsPaymentFromEachOfThreeNonDealers()
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han: 3,
                fu: 30,
                winnerIsDealer: true,
                ScoringRule
            );

            Assert.AreEqual(
                result.TsumoPaymentFromNonDealer * 3,
                result.GetTotalTsumoPoints(otherPlayerCount: 3)
            );
        }

        [TestCase(1, 30, 1000)]
        [TestCase(2, 30, 2000)]
        [TestCase(3, 30, 3900)]
        [TestCase(1, 40, 1300)]
        [TestCase(2, 25, 1600)]
        [TestCase(3, 70, 8000)]
        [TestCase(5, 30, 8000)]
        [TestCase(6, 30, 12000)]
        [TestCase(8, 30, 16000)]
        [TestCase(11, 30, 24000)]
        public void Calculate_NonDealerRon_MatchesStandardScoreTable(int han, int fu, int expected)
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han,
                fu,
                winnerIsDealer: false,
                ScoringRule
            );

            Assert.AreEqual(expected, result.RonPayment);
        }

        [TestCase(1, 30, 1500)]
        [TestCase(2, 40, 3900)]
        [TestCase(3, 30, 5800)]
        [TestCase(4, 40, 12000)]
        public void Calculate_DealerRon_MatchesStandardScoreTable(int han, int fu, int expected)
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han,
                fu,
                winnerIsDealer: true,
                ScoringRule
            );

            Assert.AreEqual(expected, result.RonPayment);
        }

        [Test]
        public void Calculate_NonDealerTsumo_DealerPaysDoubleOfNonDealer()
        {
            // 子の4翻30符ツモ: 親3900・子2000（合計7900）。
            ScoreResult result = ScoreCalculator.Calculate(
                han: 4,
                fu: 30,
                winnerIsDealer: false,
                ScoringRule
            );

            Assert.AreEqual(3900, result.TsumoPaymentFromDealer);
            Assert.AreEqual(2000, result.TsumoPaymentFromNonDealer);
            Assert.AreEqual(7900, result.GetTotalTsumoPoints(otherPlayerCount: 3));
        }

        [Test]
        public void GetTotalTsumoPoints_ThreePlayerNonDealerTsumo_SumsDealerAndOneNonDealer()
        {
            ScoreResult result = ScoreCalculator.Calculate(
                han: 4,
                fu: 30,
                winnerIsDealer: false,
                ScoringRule
            );

            Assert.AreEqual(3900 + 2000, result.GetTotalTsumoPoints(otherPlayerCount: 2));
        }

        [TestCase(4, 30)]
        [TestCase(3, 60)]
        public void Calculate_KiriageManganEnabled_RoundsUpToMangan(int han, int fu)
        {
            var kiriageRule = new ScoringRule(useKiriageMangan: true);

            ScoreResult withKiriage = ScoreCalculator.Calculate(
                han,
                fu,
                winnerIsDealer: false,
                kiriageRule
            );
            ScoreResult withoutKiriage = ScoreCalculator.Calculate(
                han,
                fu,
                winnerIsDealer: false,
                ScoringRule
            );

            Assert.AreEqual(8000, withKiriage.RonPayment);
            Assert.AreEqual(7700, withoutKiriage.RonPayment);
        }

        [Test]
        public void Calculate_KiriageManganEnabled_DoesNotAffectLowerScores()
        {
            var kiriageRule = new ScoringRule(useKiriageMangan: true);

            ScoreResult result = ScoreCalculator.Calculate(
                han: 3,
                fu: 50,
                winnerIsDealer: false,
                kiriageRule
            );

            Assert.AreEqual(6400, result.RonPayment);
        }
    }
}
