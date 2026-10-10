using System.Collections.Generic;
using NUnit.Framework;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Scoring;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class ScoreManagerTests
    {
        private const int InitialScore = 25000;
        private static readonly ScoringRule ScoringRule = new ScoringRule();

        private static List<PlayerState> CreatePlayers(int playerCount = 4)
        {
            var players = new List<PlayerState>();
            for (int seatIndex = 0; seatIndex < playerCount; seatIndex++)
            {
                players.Add(new PlayerState(seatIndex, new Hand(new List<Tile>()), InitialScore));
            }
            return players;
        }

        [Test]
        public void ApplyRonResult_WithHonbaAndRiichiSticks_TransfersToWinner()
        {
            List<PlayerState> players = CreatePlayers();
            ScoreResult score = ScoreCalculator.Calculate(
                han: 4,
                fu: 30,
                winnerIsDealer: false,
                ScoringRule
            );

            int totalGain = ScoreManager.ApplyRonResult(
                winner: players[1],
                discarder: players[2],
                score,
                honbaCount: 2,
                riichiStickCount: 1,
                ScoringRule
            );

            // 7700 + 積み棒300×2 + 供託1000。供託分は放銃者ではなく場から支払われる。
            Assert.AreEqual(9300, totalGain);
            Assert.AreEqual(InitialScore + 9300, players[1].Score);
            Assert.AreEqual(InitialScore - 8300, players[2].Score);
            Assert.AreEqual(InitialScore, players[0].Score);
            Assert.AreEqual(InitialScore, players[3].Score);
        }

        [Test]
        public void ApplyTsumoResult_NonDealerWinner_DealerPaysMore()
        {
            List<PlayerState> players = CreatePlayers();
            PlayerState winner = players[1];
            PlayerState dealer = players[0];
            var others = new List<PlayerState> { players[0], players[2], players[3] };
            ScoreResult score = ScoreCalculator.Calculate(
                han: 4,
                fu: 30,
                winnerIsDealer: false,
                ScoringRule
            );

            int totalGain = ScoreManager.ApplyTsumoResult(
                winner,
                others,
                dealer,
                score,
                honbaCount: 1,
                riichiStickCount: 0,
                ScoringRule
            );

            // 親3900+100、子2000+100×2。
            Assert.AreEqual(8200, totalGain);
            Assert.AreEqual(InitialScore + 8200, winner.Score);
            Assert.AreEqual(InitialScore - 4000, dealer.Score);
            Assert.AreEqual(InitialScore - 2100, players[2].Score);
            Assert.AreEqual(InitialScore - 2100, players[3].Score);
        }

        [Test]
        public void ApplyTsumoResult_IsZeroSumExceptRiichiSticks()
        {
            List<PlayerState> players = CreatePlayers();
            var others = new List<PlayerState> { players[1], players[2], players[3] };
            ScoreResult score = ScoreCalculator.Calculate(
                han: 3,
                fu: 30,
                winnerIsDealer: true,
                ScoringRule
            );

            ScoreManager.ApplyTsumoResult(
                players[0],
                others,
                dealerPlayer: players[0],
                score,
                honbaCount: 0,
                riichiStickCount: 2,
                ScoringRule
            );

            int totalScore = 0;
            foreach (PlayerState player in players)
            {
                totalScore += player.Score;
            }
            Assert.AreEqual((InitialScore * 4) + (2 * ScoringRule.RiichiStickValue), totalScore);
            Assert.AreEqual(InitialScore - 2000, players[1].Score);
        }
    }
}
