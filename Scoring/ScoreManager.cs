using System.Collections.Generic;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;

namespace Zizo.Mahjong.Scoring
{
    // ロン/ツモの結果を実際の`PlayerState.Score`の増減に反映する。積み棒・供託リーチ棒もここで清算する。
    public static class ScoreManager
    {
        // 戻り値は和了者の獲得点合計（積み棒・供託リーチ棒を含む）。
        public static int ApplyRonResult(
            PlayerState winner,
            PlayerState discarder,
            ScoreResult scoreResult,
            int honbaCount,
            int riichiStickCount,
            ScoringRule scoringRule
        )
        {
            int honbaBonus = honbaCount * scoringRule.RonHonbaValue;
            int totalGain =
                scoreResult.RonPayment
                + honbaBonus
                + (riichiStickCount * scoringRule.RiichiStickValue);

            winner.ApplyScoreDelta(totalGain);
            discarder.ApplyScoreDelta(-(scoreResult.RonPayment + honbaBonus));

            return totalGain;
        }

        // dealerPlayer: その局の親（和了者自身が親の場合はwinnerと同じ参照で構わない）。
        public static int ApplyTsumoResult(
            PlayerState winner,
            IReadOnlyList<PlayerState> otherPlayers,
            PlayerState dealerPlayer,
            ScoreResult scoreResult,
            int honbaCount,
            int riichiStickCount,
            ScoringRule scoringRule
        )
        {
            int totalGain = riichiStickCount * scoringRule.RiichiStickValue;
            int honbaSharePerOpponent = honbaCount * scoringRule.TsumoHonbaValuePerOpponent;

            foreach (PlayerState opponent in otherPlayers)
            {
                bool opponentIsDealer = ReferenceEquals(opponent, dealerPlayer);
                int basePayment = opponentIsDealer
                    ? scoreResult.TsumoPaymentFromDealer
                    : scoreResult.TsumoPaymentFromNonDealer;
                int payment = basePayment + honbaSharePerOpponent;

                opponent.ApplyScoreDelta(-payment);
                totalGain += payment;
            }

            winner.ApplyScoreDelta(totalGain);
            return totalGain;
        }
    }
}
