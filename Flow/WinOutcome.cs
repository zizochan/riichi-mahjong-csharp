using Zizo.Mahjong.Scoring;
using Zizo.Mahjong.Yaku;

namespace Zizo.Mahjong.Flow
{
    public class WinOutcome
    {
        public int WinnerSeatIndex { get; }
        public bool IsTsumo { get; }
        public YakuEvaluationResult YakuResult { get; }
        public int Fu { get; }
        public int DoraCount { get; }
        public int UraDoraCount { get; }
        public int RedDoraCount { get; }

        // ドラを含めた合計翻数（役満の場合は「何倍役満か」）。
        public int TotalHan => Score.Han;
        public ScoreResult Score { get; }
        public int TotalGain { get; }

        public WinOutcome(
            int winnerSeatIndex,
            bool isTsumo,
            YakuEvaluationResult yakuResult,
            int fu,
            int doraCount,
            int uraDoraCount,
            int redDoraCount,
            ScoreResult score,
            int totalGain
        )
        {
            WinnerSeatIndex = winnerSeatIndex;
            IsTsumo = isTsumo;
            YakuResult = yakuResult;
            Fu = fu;
            DoraCount = doraCount;
            UraDoraCount = uraDoraCount;
            RedDoraCount = redDoraCount;
            Score = score;
            TotalGain = totalGain;
        }
    }
}
