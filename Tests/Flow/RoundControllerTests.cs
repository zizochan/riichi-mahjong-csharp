using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Flow;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;

namespace Zizo.Mahjong.Tests
{
    // SequentialRandomSourceでシャッフルを無効化し、TileSetDefinition.CreateStandard()の生成順が
    // そのまま山の順序になることを利用して、配牌内容を完全に決定的にした上でRoundControllerの
    // 進行(配牌・打牌・鳴き・和了判定・流局精算)を検証する。
    public class RoundControllerTests
    {
        private static RoundController CreateController(int dealerSeatIndex = 0)
        {
            MahjongRuleSet ruleSet = MahjongRuleSet.CreateStandardFourPlayer();
            return new RoundController(
                ruleSet,
                roundWind: new TileKind(TileSuit.Wind, 1),
                dealerSeatIndex: dealerSeatIndex,
                initialScorePerPlayer: 25000,
                randomSource: new SequentialRandomSource()
            );
        }

        [Test]
        public void Constructor_DealsThirteenTilesPerPlayerInWallOrder()
        {
            RoundController controller = CreateController();

            // 山は定義順(萬子1→9,筒子...)のまま消費されるため、席0は1m~4mの13枚になる。
            List<Tile> expectedSeat0Hand = HandNotation.Parse("1111222233334m");
            CollectionAssert.AreEquivalent(
                expectedSeat0Hand,
                controller.Players[0].Hand.ConcealedTiles
            );

            foreach (PlayerState player in controller.Players)
            {
                Assert.AreEqual(13, player.Hand.ConcealedTiles.Count);
            }
        }

        [Test]
        public void GetSeatWind_IsRelativeToDealerSeat()
        {
            RoundController controller = CreateController(dealerSeatIndex: 0);

            Assert.AreEqual(new TileKind(TileSuit.Wind, 1), controller.GetSeatWind(0)); // 東
            Assert.AreEqual(new TileKind(TileSuit.Wind, 2), controller.GetSeatWind(1)); // 南
            Assert.AreEqual(new TileKind(TileSuit.Wind, 3), controller.GetSeatWind(2)); // 西
            Assert.AreEqual(new TileKind(TileSuit.Wind, 4), controller.GetSeatWind(3)); // 北
        }

        [Test]
        public void Discard_RemovesFromHandAndAppendsToDiscardPile()
        {
            RoundController controller = CreateController();
            Tile drawn = controller.DrawForCurrentPlayer();

            Assert.AreEqual(14, controller.Players[0].Hand.ConcealedTiles.Count);

            controller.Discard(0, drawn);

            Assert.AreEqual(13, controller.Players[0].Hand.ConcealedTiles.Count);
            Assert.AreEqual(1, controller.Players[0].DiscardPile.Discards.Count);
            Assert.AreEqual(drawn, controller.Players[0].DiscardPile.Discards[0].Tile);
        }

        [Test]
        public void Discard_WithRiichiDeclaration_DeductsStickAndFlagsPlayer()
        {
            RoundController controller = CreateController();
            Tile drawn = controller.DrawForCurrentPlayer();
            int scoreBeforeRiichi = controller.Players[0].Score;

            controller.Discard(0, drawn, isRiichiDeclaration: true);

            Assert.IsTrue(controller.Players[0].IsRiichiDeclared);
            Assert.AreEqual(scoreBeforeRiichi - 1000, controller.Players[0].Score);
            Assert.AreEqual(1, controller.RiichiStickCount);
        }

        [Test]
        public void AdvanceTurnToNext_CyclesThroughAllSeats()
        {
            RoundController controller = CreateController();

            Assert.AreEqual(0, controller.CurrentTurnSeatIndex);
            controller.AdvanceTurnToNext();
            Assert.AreEqual(1, controller.CurrentTurnSeatIndex);
            controller.AdvanceTurnToNext();
            controller.AdvanceTurnToNext();
            controller.AdvanceTurnToNext();
            Assert.AreEqual(0, controller.CurrentTurnSeatIndex);
        }

        [Test]
        public void ExecutePon_MovesTilesIntoMeldAndJumpsTurnToCaller()
        {
            RoundController controller = CreateController();

            // 席0の配牌には4m、席1の配牌には4mが3枚含まれる(決定的な配牌による)。
            Tile discarded = HandNotation.Single("4m");
            controller.Discard(0, discarded);

            List<CallOpportunity> opportunities = controller.GetCallOpportunities(0, discarded);
            CallOpportunity opportunityForSeat1 = opportunities.First(o => o.SeatIndex == 1);
            Assert.IsTrue(opportunityForSeat1.CanPon);

            int handCountBeforePon = controller.Players[1].Hand.ConcealedTiles.Count;
            controller.ExecutePon(1, 0, discarded);

            Assert.AreEqual(
                handCountBeforePon - 2,
                controller.Players[1].Hand.ConcealedTiles.Count
            );
            Assert.AreEqual(1, controller.Players[1].Melds.Count);
            Assert.AreEqual(MeldType.Pon, controller.Players[1].Melds[0].Type);
            Assert.AreEqual(1, controller.CurrentTurnSeatIndex);
        }

        [Test]
        public void TryWin_HandWithoutAnyYaku_ReturnsNull()
        {
            RoundController controller = CreateController();
            Tile drawn = controller.DrawForCurrentPlayer();

            YakuEvaluationResult yakuResult = controller.EvaluateYaku(0, drawn, isTsumo: true);
            Assert.IsFalse(yakuResult.HasYaku);

            WinOutcome outcome = controller.TryWin(0, drawn, isTsumo: true);
            Assert.IsNull(outcome);
        }

        [Test]
        public void SettleExhaustiveDraw_TransfersAreZeroSumAcrossAllPlayers()
        {
            RoundController controller = CreateController();

            Dictionary<int, int> deltas = controller.SettleExhaustiveDraw();

            Assert.AreEqual(controller.Players.Count, deltas.Count);
            Assert.AreEqual(0, deltas.Values.Sum());
        }

        [Test]
        public void CanDeclareKyuushuKyuuhai_HandWithFewerThanNineTerminalOrHonorKinds_ReturnsFalse()
        {
            RoundController controller = CreateController();

            // 決定的な初期配牌(1m~4mのみ)には幺九牌が1種類(1m)しかなく、九種九牌の条件を満たさない。
            Assert.IsFalse(controller.CanDeclareKyuushuKyuuhai(0));
        }
    }
}
