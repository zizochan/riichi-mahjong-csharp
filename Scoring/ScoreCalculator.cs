using Zizo.Mahjong.Rules;

namespace Zizo.Mahjong.Scoring
{
    // 翻数・符から基本点を求め、立場（親/子）ごとの支払い額まで計算する。
    public static class ScoreCalculator
    {
        // 数え役満（翻数だけで役満相当とみなす）・三倍満・倍満・跳満になる最低翻数。
        private const int CountedYakumanMinHan = 13;
        private const int SanbaimanMinHan = 11;
        private const int BaimanMinHan = 8;
        private const int HanemanMinHan = 6;

        // 基本点 = 符 × 2^(2 + 翻数)。
        private const int BasePointsExponentOffset = 2;

        // 切り上げ満貫で満貫として扱う基本点の下限（4翻30符・3翻60符 = 1920点）。
        private const int KiriageManganMinBasePoints = 1920;

        public static ScoreResult Calculate(
            int han,
            int fu,
            bool winnerIsDealer,
            ScoringRule scoringRule,
            int yakumanMultiplier = 0
        )
        {
            bool isYakuman = yakumanMultiplier > 0;
            int basePoints = isYakuman
                ? scoringRule.YakumanBasePoints * yakumanMultiplier
                : CalculateBasePointsFromHanFu(han, fu, scoringRule);

            return new ScoreResult(
                han,
                fu,
                basePoints,
                winnerIsDealer,
                isYakuman,
                isYakuman ? yakumanMultiplier : 0,
                scoringRule.PointRoundingUnit
            );
        }

        private static int CalculateBasePointsFromHanFu(int han, int fu, ScoringRule scoringRule)
        {
            if (han >= CountedYakumanMinHan)
            {
                return scoringRule.YakumanBasePoints;
            }
            if (han >= SanbaimanMinHan)
            {
                return scoringRule.SanbaimanBasePoints;
            }
            if (han >= BaimanMinHan)
            {
                return scoringRule.BaimanBasePoints;
            }
            if (han >= HanemanMinHan)
            {
                return scoringRule.HanemanBasePoints;
            }

            int basePoints = fu * (1 << (BasePointsExponentOffset + han));
            bool isKiriageMangan =
                scoringRule.UseKiriageMangan && basePoints >= KiriageManganMinBasePoints;
            if (basePoints > scoringRule.ManganBasePoints || isKiriageMangan)
            {
                basePoints = scoringRule.ManganBasePoints;
            }
            return basePoints;
        }
    }
}
