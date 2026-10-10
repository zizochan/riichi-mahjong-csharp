using System;

namespace Zizo.Mahjong.Rules
{
    public class ScoringRule
    {
        public int FuRoundingUnit { get; }
        public int PointRoundingUnit { get; }

        // 満貫・跳満・倍満・三倍満・役満の基本点（子のロン時の点数はこれらから算出する）。
        public int ManganBasePoints { get; }
        public int HanemanBasePoints { get; }
        public int BaimanBasePoints { get; }
        public int SanbaimanBasePoints { get; }
        public int YakumanBasePoints { get; }

        // true の場合、4翻30符・3翻60符など満貫超の点数計算結果を満貫に切り上げる（切り上げ満貫）。
        public bool UseKiriageMangan { get; }

        // 積み棒1本あたりの点数（ロンは放銃者からまとめて、ツモは各家から個別に支払われるため別で持つ）。
        public int RonHonbaValue { get; }
        public int TsumoHonbaValuePerOpponent { get; }
        public int RiichiStickValue { get; }

        // 荒牌平局（流局）時に、テンパイ者とノーテン者の間でやり取りされる合計点（テンパイ料）。
        public int ExhaustiveDrawPenaltyTotal { get; }

        public ScoringRule(
            int fuRoundingUnit = 10,
            int pointRoundingUnit = 100,
            int manganBasePoints = 2000,
            int hanemanBasePoints = 3000,
            int baimanBasePoints = 4000,
            int sanbaimanBasePoints = 6000,
            int yakumanBasePoints = 8000,
            bool useKiriageMangan = false,
            int ronHonbaValue = 300,
            int tsumoHonbaValuePerOpponent = 100,
            int riichiStickValue = 1000,
            int exhaustiveDrawPenaltyTotal = 3000
        )
        {
            // 切り上げ単位は割り算に使うため、0以下だと計算が破綻する。
            ThrowIfNotPositive(fuRoundingUnit, nameof(fuRoundingUnit));
            ThrowIfNotPositive(pointRoundingUnit, nameof(pointRoundingUnit));

            FuRoundingUnit = fuRoundingUnit;
            PointRoundingUnit = pointRoundingUnit;
            ManganBasePoints = manganBasePoints;
            HanemanBasePoints = hanemanBasePoints;
            BaimanBasePoints = baimanBasePoints;
            SanbaimanBasePoints = sanbaimanBasePoints;
            YakumanBasePoints = yakumanBasePoints;
            UseKiriageMangan = useKiriageMangan;
            RonHonbaValue = ronHonbaValue;
            TsumoHonbaValuePerOpponent = tsumoHonbaValuePerOpponent;
            RiichiStickValue = riichiStickValue;
            ExhaustiveDrawPenaltyTotal = exhaustiveDrawPenaltyTotal;
        }

        private static void ThrowIfNotPositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "1以上で指定してください。"
                );
            }
        }
    }
}
