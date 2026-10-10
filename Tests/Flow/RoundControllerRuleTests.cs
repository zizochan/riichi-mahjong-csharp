using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Flow;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;
using Zizo.Mahjong.Yaku.Standard;
using static Zizo.Mahjong.Tests.RoundTestHelper;

namespace Zizo.Mahjong.Tests
{
    // RoundControllerの進行中の状態（立直・巡目・鳴き・山の残り）によって変わるルール判定を検証する。
    // 配牌はRoundTestHelper.ReplaceHandで任意の手牌に差し替える。
    public class RoundControllerRuleTests
    {
        // 234567m 345p 99p 67s: 5s/8sの両面待ちで聴牌している13枚（平和形）。
        private const string PinfuTenpaiHand = "234567m34599p67s";
        private const string NotenHand = "147m147p147s1234z";

        private static bool HasYaku(YakuEvaluationResult result, string yakuName)
        {
            return result.MatchedYaku.Any(entry => entry.Yaku.Name == yakuName);
        }

        // --- 立直・一発・裏ドラ ---

        [Test]
        public void TryWin_RiichiWinner_RevealsUraDoraIndicators()
        {
            RoundController controller = CreateController();
            // 第一打牌での立直はダブル立直になるため、2打牌目で立直する。
            ReplaceHand(controller, 0, PinfuTenpaiHand + "12z");
            controller.Discard(0, HandNotation.Single("1z"));
            controller.Discard(0, HandNotation.Single("2z"), isRiichiDeclaration: true);

            WinOutcome outcome = controller.TryWin(
                0,
                HandNotation.Single("8s"),
                isTsumo: false,
                discarderSeatIndex: 1
            );

            Assert.IsNotNull(outcome);
            Assert.AreEqual(
                controller.Wall.DoraIndicators.Count,
                controller.Wall.UraDoraIndicators.Count
            );
            Assert.IsTrue(HasYaku(outcome.YakuResult, "立直"));
            Assert.IsTrue(HasYaku(outcome.YakuResult, "一発"));
        }

        [Test]
        public void TryWin_NonRiichiWinner_DoesNotRevealUraDora()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, PinfuTenpaiHand);

            WinOutcome outcome = controller.TryWin(
                0,
                HandNotation.Single("8s"),
                isTsumo: false,
                discarderSeatIndex: 1
            );

            Assert.IsNotNull(outcome);
            Assert.AreEqual(0, controller.Wall.UraDoraIndicators.Count);
            Assert.AreEqual(0, outcome.UraDoraCount);
        }

        [Test]
        public void TryWin_Ron_TransfersScoreAndClearsRiichiSticks()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, PinfuTenpaiHand + "1z");
            controller.Discard(0, HandNotation.Single("1z"), isRiichiDeclaration: true);
            int discarderScoreBefore = controller.Players[1].Score;

            WinOutcome outcome = controller.TryWin(
                0,
                HandNotation.Single("8s"),
                isTsumo: false,
                discarderSeatIndex: 1
            );

            Assert.AreEqual(
                discarderScoreBefore - outcome.Score.RonPayment,
                controller.Players[1].Score
            );
            Assert.AreEqual(0, controller.RiichiStickCount);
            Assert.AreEqual(14, controller.Players[0].Hand.ConcealedTiles.Count);
        }

        [Test]
        public void EvaluateYaku_ClosedKanAfterRiichi_CancelsIppatsu()
        {
            RoundController controller = CreateController();
            // 親(席0)の配牌は1111222233334m。打牌で4mを切っても1mの暗槓ができる。
            controller.Discard(0, HandNotation.Single("4m"));
            ReplaceHand(controller, 1, PinfuTenpaiHand + "1z");
            controller.Discard(1, HandNotation.Single("1z"), isRiichiDeclaration: true);

            YakuEvaluationResult beforeKan = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );
            controller.ExecuteClosedKan(0, new TileKind(TileSuit.Man, 1));
            YakuEvaluationResult afterKan = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            Assert.IsTrue(HasYaku(beforeKan, "一発"));
            Assert.IsFalse(HasYaku(afterKan, "一発"));
        }

        [Test]
        public void EvaluateYaku_TsumoOnFirstDrawAfterRiichi_IsIppatsu()
        {
            RoundController controller = CreateRiichiAndGoAround();
            controller.Players[1].Hand.Draw(HandNotation.Single("8s"));

            YakuEvaluationResult result = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: true
            );

            Assert.IsTrue(HasYaku(result, "一発"));
        }

        [Test]
        public void EvaluateYaku_RonOnLastDiscardBeforeOwnDraw_IsIppatsu()
        {
            RoundController controller = CreateRiichiAndGoAround();

            YakuEvaluationResult result = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            Assert.IsTrue(HasYaku(result, "一発"));
        }

        [Test]
        public void EvaluateYaku_RonAfterOwnNextDiscard_IsNotIppatsu()
        {
            RoundController controller = CreateRiichiAndGoAround();
            DiscardFiller(controller, 1);

            YakuEvaluationResult result = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            // 第一巡の立直のため両立直になる（一発が消えても立直系の役自体は残る）。
            Assert.IsTrue(HasYaku(result, "両立直"));
            Assert.IsFalse(HasYaku(result, "一発"));
        }

        [Test]
        public void EvaluateYaku_PonAfterRiichi_CancelsIppatsu()
        {
            RoundController controller = CreateController();
            DiscardFiller(controller, 0);
            ReplaceHand(controller, 1, PinfuTenpaiHand + "1z");
            controller.Discard(1, HandNotation.Single("1z"), isRiichiDeclaration: true);
            ReplaceHand(controller, 2, "1z");
            ReplaceHand(controller, 3, "11z");
            controller.Discard(2, HandNotation.Single("1z"));

            controller.ExecutePon(3, 2, HandNotation.Single("1z"));
            YakuEvaluationResult result = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            Assert.IsFalse(HasYaku(result, "一発"));
        }

        // 席0が打牌 → 席1が立直 → 席2・席3・席0が打牌し、席1の次のツモ直前まで一巡させる。
        private static RoundController CreateRiichiAndGoAround()
        {
            RoundController controller = CreateController();
            DiscardFiller(controller, 0);
            ReplaceHand(controller, 1, PinfuTenpaiHand + "1z");
            controller.Discard(1, HandNotation.Single("1z"), isRiichiDeclaration: true);
            DiscardFiller(controller, 2);
            DiscardFiller(controller, 3);
            DiscardFiller(controller, 0);
            return controller;
        }

        // 判定に影響しない字牌を1枚持たせてから捨てる（巡目を進めるためだけの打牌）。
        private static void DiscardFiller(RoundController controller, int seatIndex)
        {
            Tile filler = HandNotation.Single("4z");
            controller.Players[seatIndex].Hand.Draw(filler);
            controller.Discard(seatIndex, filler);
        }

        [Test]
        public void Discard_RiichiAfterCallOnFirstTurn_IsNotDoubleRiichi()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, "4z");
            ReplaceHand(controller, 2, "44z");
            controller.Discard(0, HandNotation.Single("4z"));
            controller.ExecutePon(2, 0, HandNotation.Single("4z"));
            ReplaceHand(controller, 1, PinfuTenpaiHand + "1z");

            controller.Discard(1, HandNotation.Single("1z"), isRiichiDeclaration: true);
            YakuEvaluationResult result = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            Assert.IsTrue(HasYaku(result, "立直"));
            Assert.IsFalse(HasYaku(result, "両立直"));
        }

        [Test]
        public void Discard_RiichiOnFirstUninterruptedTurn_IsDoubleRiichi()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, PinfuTenpaiHand + "1z");

            controller.Discard(0, HandNotation.Single("1z"), isRiichiDeclaration: true);
            YakuEvaluationResult result = controller.EvaluateYaku(
                0,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            Assert.IsTrue(HasYaku(result, "両立直"));
            Assert.IsFalse(HasYaku(result, "立直"));
        }

        // --- 鳴きの可否 ---

        [Test]
        public void GetCallOpportunities_RiichiPlayer_CannotPon()
        {
            RoundController controller = CreateController();
            // 席1の配牌(444555566667 7m)は4mを3枚持つ。リーチ後はポン・カンできない。
            controller.Discard(1, HandNotation.Single("7m"), isRiichiDeclaration: true);
            Tile discarded = HandNotation.Single("4m");
            controller.Discard(0, discarded);

            List<CallOpportunity> opportunities = controller.GetCallOpportunities(0, discarded);

            Assert.IsFalse(opportunities.Any(opportunity => opportunity.SeatIndex == 1));
        }

        [Test]
        public void GetCallOpportunities_LastDiscard_OnlyAllowsRon()
        {
            RoundController controller = CreateController();
            DrainLiveWall(controller);
            Tile discarded = HandNotation.Single("4m");
            controller.Discard(0, discarded);

            List<CallOpportunity> opportunities = controller.GetCallOpportunities(0, discarded);

            Assert.IsFalse(
                opportunities.Any(opportunity =>
                    opportunity.CanPon || opportunity.CanChi || opportunity.CanOpenKan
                )
            );
        }

        [Test]
        public void GetCallOpportunities_FuritenPlayer_CannotRon()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 1, PinfuTenpaiHand + "5s");
            // 待ち牌の5sを自分で捨てているためフリテン。
            controller.Discard(1, HandNotation.Single("5s"));
            Tile discarded = HandNotation.Single("8s");
            ReplaceHand(controller, 0, "8s");
            controller.Discard(0, discarded);

            Assert.IsFalse(controller.CanDeclareRon(1, discarded));
        }

        [Test]
        public void CanDeclareRon_CompleteHandWithoutYaku_ReturnsFalse()
        {
            RoundController controller = CreateController();
            // 1pのポンがあるため門前でなく、8sで和了しても役が無い。
            ReplaceHand(controller, 1, "234567m345p67s99p");
            controller.Players[1].AddMeld(MeldFactory.Pon("111p"));

            Assert.IsFalse(controller.CanDeclareRon(1, HandNotation.Single("8s")));
        }

        [Test]
        public void ExecutePon_NotEnoughMatchingTiles_Throws()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 1, "4z123m");

            Assert.Throws<InvalidOperationException>(() =>
                controller.ExecutePon(1, 0, HandNotation.Single("4z"))
            );
        }

        [Test]
        public void ExecuteOpenKan_DrawsReplacementTileRevealsNewDoraAndJumpsTurn()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, "4z");
            ReplaceHand(controller, 2, "444z123m");
            Tile discarded = HandNotation.Single("4z");
            controller.Discard(0, discarded);

            Tile rinshanTile = controller.ExecuteOpenKan(2, 0, discarded);

            Meld meld = controller.Players[2].Melds[0];
            Assert.AreEqual(MeldType.OpenKan, meld.Type);
            Assert.AreEqual(4, meld.Tiles.Count);
            CollectionAssert.AreEquivalent(
                HandNotation.Parse("123m").Append(rinshanTile).ToList(),
                controller.Players[2].Hand.ConcealedTiles.ToList()
            );
            Assert.AreEqual(2, controller.Wall.DoraIndicators.Count);
            Assert.AreEqual(2, controller.CurrentTurnSeatIndex);
            Assert.IsTrue(controller.Players[0].DiscardPile.Discards[0].WasCalled);
        }

        [Test]
        public void ExecuteChi_MovesTilesAndMarksDiscardAsCalled()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 1, "23m456789p11s");
            Tile discarded = HandNotation.Single("4m");
            controller.Discard(0, discarded);
            CallOpportunity opportunity = controller
                .GetCallOpportunities(0, discarded)
                .First(candidate => candidate.SeatIndex == 1);

            controller.ExecuteChi(1, 0, discarded, opportunity.ChiPatterns[0]);

            Assert.AreEqual(MeldType.Chi, controller.Players[1].Melds[0].Type);
            Assert.IsTrue(controller.Players[0].DiscardPile.Discards[0].WasCalled);
            Assert.AreEqual(1, controller.CurrentTurnSeatIndex);
            Assert.IsFalse(controller.Players[1].IsMenzen);
        }

        // --- 嶺上・海底 ---

        [Test]
        public void ExecuteClosedKan_DrawsReplacementTileAndRevealsNewDora()
        {
            RoundController controller = CreateController();
            int handCountBefore = controller.Players[0].Hand.ConcealedTiles.Count;

            Tile rinshanTile = controller.ExecuteClosedKan(0, new TileKind(TileSuit.Man, 1));

            Assert.AreEqual(
                handCountBefore - 4 + 1,
                controller.Players[0].Hand.ConcealedTiles.Count
            );
            CollectionAssert.Contains(
                controller.Players[0].Hand.ConcealedTiles.ToList(),
                rinshanTile
            );
            Assert.AreEqual(2, controller.Wall.DoraIndicators.Count);
            Assert.IsTrue(controller.Players[0].IsMenzen);
        }

        [Test]
        public void ExecuteAddedKan_UpgradesPon()
        {
            RoundController controller = CreateController();
            Tile discarded = HandNotation.Single("4m");
            controller.Discard(0, discarded);
            controller.ExecutePon(1, 0, discarded);
            // 席1は4mを3枚持っていたので、ポン後も1枚残っている。

            controller.ExecuteAddedKan(1, new TileKind(TileSuit.Man, 4));

            Assert.AreEqual(MeldType.AddedKan, controller.Players[1].Melds[0].Type);
        }

        [Test]
        public void GetChankanRonSeats_PlayerWaitingOnAddedKanTile_CanRob()
        {
            RoundController controller = CreateController();
            Tile discarded = HandNotation.Single("4m");
            controller.Discard(0, discarded);
            controller.ExecutePon(1, 0, discarded);
            ReplaceHand(controller, 2, "23m456789p11155s");

            List<int> robbers = controller.GetChankanRonSeats(1, new TileKind(TileSuit.Man, 4));

            CollectionAssert.AreEqual(new[] { 2 }, robbers);
        }

        [Test]
        public void EvaluateYaku_TsumoOnLastLiveTile_IsHaitei()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, PinfuTenpaiHand + "8s");
            DrainLiveWall(controller);

            YakuEvaluationResult result = controller.EvaluateYaku(
                0,
                HandNotation.Single("8s"),
                isTsumo: true
            );

            Assert.IsTrue(HasYaku(result, "海底摸月"));
        }

        [Test]
        public void EvaluateYaku_RinshanTsumoWithEmptyLiveWall_IsNotHaitei()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, PinfuTenpaiHand + "8s");
            DrainLiveWall(controller);

            YakuEvaluationResult result = controller.EvaluateYaku(
                0,
                HandNotation.Single("8s"),
                isTsumo: true,
                isRinshan: true
            );

            Assert.IsTrue(HasYaku(result, "嶺上開花"));
            Assert.IsFalse(HasYaku(result, "海底摸月"));
        }

        // --- 第一巡の役満 ---

        [Test]
        public void EvaluateYaku_DealerFirstTsumo_IsTenhou()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, PinfuTenpaiHand + "8s");

            YakuEvaluationResult result = controller.EvaluateYaku(
                0,
                HandNotation.Single("8s"),
                isTsumo: true
            );

            Assert.IsTrue(HasYaku(result, "天和"));
        }

        [Test]
        public void EvaluateYaku_DealerTsumoAfterClosedKan_IsNotTenhou()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, "1111z234567m99p678s");
            controller.ExecuteClosedKan(0, new TileKind(TileSuit.Wind, 1));
            ReplaceHand(controller, 0, "234567m99p678s");

            YakuEvaluationResult result = controller.EvaluateYaku(
                0,
                HandNotation.Single("8s"),
                isTsumo: true,
                isRinshan: true
            );

            Assert.IsFalse(HasYaku(result, "天和"));
        }

        [Test]
        public void EvaluateYaku_NonDealerRonOnFirstDiscard_IsRenhou()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 1, PinfuTenpaiHand);
            ReplaceHand(controller, 0, "8s");
            controller.Discard(0, HandNotation.Single("8s"));

            YakuEvaluationResult result = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            Assert.IsTrue(HasYaku(result, "人和"));
        }

        [Test]
        public void EvaluateYaku_NonDealerRonAfterOwnDiscard_IsNotRenhou()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, "1z");
            controller.Discard(0, HandNotation.Single("1z"));
            ReplaceHand(controller, 1, PinfuTenpaiHand + "2z");
            controller.Discard(1, HandNotation.Single("2z"));
            ReplaceHand(controller, 2, "8s");
            controller.Discard(2, HandNotation.Single("8s"));

            YakuEvaluationResult result = controller.EvaluateYaku(
                1,
                HandNotation.Single("8s"),
                isTsumo: false
            );

            Assert.IsFalse(HasYaku(result, "人和"));
        }

        [Test]
        public void CanDeclareKyuushuKyuuhai_NineTerminalOrHonorKindsOnFirstTurn_ReturnsTrue()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, "19m19p19s1234z258m");

            Assert.IsTrue(controller.CanDeclareKyuushuKyuuhai(0));
        }

        [Test]
        public void CanDeclareKyuushuKyuuhai_AfterCallOnFirstTurn_ReturnsFalse()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, "4z");
            ReplaceHand(controller, 2, "44z");
            controller.Discard(0, HandNotation.Single("4z"));
            controller.ExecutePon(2, 0, HandNotation.Single("4z"));
            ReplaceHand(controller, 1, "19m19p19s1234z258m");

            Assert.IsFalse(controller.CanDeclareKyuushuKyuuhai(1));
        }

        // --- 流局 ---

        [Test]
        public void SettleExhaustiveDraw_OneTenpaiThreeNoten_TenpaiReceivesFullPot()
        {
            RoundController controller = CreateController();
            ReplaceHand(controller, 0, PinfuTenpaiHand);
            for (int seatIndex = 1; seatIndex < controller.Players.Count; seatIndex++)
            {
                ReplaceHand(controller, seatIndex, NotenHand);
            }

            Dictionary<int, int> deltas = controller.SettleExhaustiveDraw();

            Assert.AreEqual(3000, deltas[0]);
            Assert.AreEqual(-1000, deltas[1]);
            Assert.AreEqual(-1000, deltas[2]);
            Assert.AreEqual(-1000, deltas[3]);
            Assert.AreEqual(InitialScore + 3000, controller.Players[0].Score);
        }

        [Test]
        public void SettleExhaustiveDraw_AllNoten_NoTransfer()
        {
            RoundController controller = CreateController();
            for (int seatIndex = 0; seatIndex < controller.Players.Count; seatIndex++)
            {
                ReplaceHand(controller, seatIndex, NotenHand);
            }

            Dictionary<int, int> deltas = controller.SettleExhaustiveDraw();

            Assert.IsTrue(deltas.Values.All(delta => delta == 0));
        }

        // --- 人数・牌構成の変更 ---

        [Test]
        public void Constructor_ThreePlayers_DealsAndRotatesForThreeSeats()
        {
            var ruleSet = new MahjongRuleSet(
                playerCount: 3,
                TileSetDefinition.CreateStandard(),
                StandardYakuSet.CreateAll(),
                new WallRule(),
                new ScoringRule()
            );

            RoundController controller = CreateController(dealerSeatIndex: 2, ruleSet: ruleSet);

            Assert.AreEqual(3, controller.Players.Count);
            Assert.AreEqual(136 - 14 - (13 * 3), controller.Wall.RemainingLiveTileCount);
            Assert.AreEqual(new TileKind(TileSuit.Wind, 1), controller.GetSeatWind(2));
            Assert.AreEqual(new TileKind(TileSuit.Wind, 2), controller.GetSeatWind(0));
            Assert.AreEqual(new TileKind(TileSuit.Wind, 3), controller.GetSeatWind(1));
            controller.AdvanceTurnToNext();
            Assert.AreEqual(0, controller.CurrentTurnSeatIndex);
        }

        [Test]
        public void Constructor_CustomTileSet_UsesCustomTileCount()
        {
            List<TileSetEntry> entries = TileSetDefinition.CreateStandard().Entries.ToList();
            var white = new TileKind(TileSuit.Dragon, 1);
            int whiteIndex = entries.FindIndex(entry => entry.Kind.Equals(white));
            entries[whiteIndex] = new TileSetEntry(white, count: 6);
            var ruleSet = new MahjongRuleSet(
                4,
                new TileSetDefinition(entries),
                StandardYakuSet.CreateAll(),
                new WallRule(),
                new ScoringRule()
            );

            RoundController controller = CreateController(ruleSet: ruleSet);

            Assert.AreEqual(138 - 14 - (13 * 4), controller.Wall.RemainingLiveTileCount);
        }
    }
}
