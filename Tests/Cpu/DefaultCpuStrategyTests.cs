using System.Collections.Generic;
using NUnit.Framework;
using Zizo.Mahjong.Cpu;
using Zizo.Mahjong.Flow;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class DefaultCpuStrategyTests
    {
        private const int DefaultScore = 25000;
        private const int DefaultRemainingTileCount = 70;

        private static readonly MahjongRuleSet RuleSet = MahjongRuleSet.CreateStandardFourPlayer();
        private static readonly TileKind East = new TileKind(TileSuit.Wind, 1);

        private readonly DefaultCpuStrategy _strategy = new DefaultCpuStrategy();

        // 他家の情報を持たない、自分の手牌だけのビューを作る。
        private static PlayerView CreateView(
            string handNotation,
            IReadOnlyList<Meld> ownMelds = null,
            int score = DefaultScore,
            int remainingLiveTileCount = DefaultRemainingTileCount
        )
        {
            IReadOnlyList<Meld> melds = ownMelds ?? new List<Meld>();
            bool isMenzen = true;
            foreach (Meld meld in melds)
            {
                isMenzen &= meld.IsConcealed;
            }

            return new PlayerView(
                seatIndex: 0,
                concealedTiles: HandNotation.Parse(handNotation),
                ownMelds: melds,
                isMenzen: isMenzen,
                isRiichiDeclared: false,
                score: score,
                seatWind: East,
                roundWind: East,
                remainingLiveTileCount: remainingLiveTileCount,
                visibleDoraIndicators: new List<Tile>(),
                discardsBySeat: new Dictionary<int, IReadOnlyList<DiscardedTile>>(),
                meldsBySeat: new Dictionary<int, IReadOnlyList<Meld>> { [0] = melds },
                opponentRiichiDeclaredBySeat: new Dictionary<int, bool>()
            );
        }

        private static CallOpportunity CreatePonOpportunity(bool canRon = false)
        {
            return new CallOpportunity(
                seatIndex: 0,
                canRon: canRon,
                canPon: true,
                canOpenKan: false,
                chiPatterns: new List<(Tile First, Tile Second)>()
            );
        }

        [Test]
        public void ChooseDiscard_DiscardsIsolatedTileToReachTenpai()
        {
            // 123456789m 11p 22p + 7z: 7zを切ればシャンポン待ちの聴牌。
            PlayerView view = CreateView("123456789m1122p7z");

            Tile discard = _strategy.ChooseDiscard(view, RuleSet);

            Assert.AreEqual(HandNotation.Single("7z"), discard);
        }

        [Test]
        public void ShouldDeclareRiichi_MenzenTenpai_ReturnsTrue()
        {
            PlayerView view = CreateView("123456789m1122p");

            Assert.IsTrue(_strategy.ShouldDeclareRiichi(view, RuleSet));
        }

        [Test]
        public void ShouldDeclareRiichi_NotEnoughScoreForRiichiStick_ReturnsFalse()
        {
            PlayerView view = CreateView("123456789m1122p", score: 900);

            Assert.IsFalse(_strategy.ShouldDeclareRiichi(view, RuleSet));
        }

        [Test]
        public void ShouldDeclareRiichi_OpenHand_ReturnsFalse()
        {
            var melds = new List<Meld> { MeldFactory.Chi("123m") };
            PlayerView view = CreateView("456789m1122p", ownMelds: melds);

            Assert.IsFalse(_strategy.ShouldDeclareRiichi(view, RuleSet));
        }

        [Test]
        public void ShouldDeclareRiichi_NotTenpai_ReturnsFalse()
        {
            PlayerView view = CreateView("147m147p147s1234z");

            Assert.IsFalse(_strategy.ShouldDeclareRiichi(view, RuleSet));
        }

        [Test]
        public void DecideCall_RonAvailable_PrefersRonOverPon()
        {
            PlayerView view = CreateView("123456789m1122p");

            CallDecision decision = _strategy.DecideCall(
                view,
                RuleSet,
                CreatePonOpportunity(canRon: true),
                HandNotation.Single("1p")
            );

            Assert.AreEqual(CallDecisionType.Ron, decision.Type);
        }

        [Test]
        public void DecideCall_PonImprovesShanten_Pons()
        {
            // 123m 456p + 白の対子 + 浮き牌: 白をポンすると面子が1つ増えて向聴数が進む。
            PlayerView view = CreateView("123m456p147s2355z");

            CallDecision decision = _strategy.DecideCall(
                view,
                RuleSet,
                CreatePonOpportunity(),
                HandNotation.Single("5z")
            );

            Assert.AreEqual(CallDecisionType.Pon, decision.Type);
        }

        [Test]
        public void DecideCall_PonDoesNotImproveShanten_Passes()
        {
            // 既に聴牌（3sの辺張待ち）。白をポンしても向聴数は変わらない。
            PlayerView view = CreateView("123456m789p55z12s");

            CallDecision decision = _strategy.DecideCall(
                view,
                RuleSet,
                CreatePonOpportunity(),
                HandNotation.Single("5z")
            );

            Assert.AreEqual(CallDecisionType.None, decision.Type);
        }

        [Test]
        public void DecideCall_ChiImprovesShanten_ChoosesChiPattern()
        {
            // 23m 456789p 88s 9s 白白 の1向聴。上家の4mをチーして9sを切れば、88sと白のシャンポン待ちで聴牌になる。
            PlayerView view = CreateView("23m456789p889s55z");
            Tile discarded = HandNotation.Single("4m");
            var opportunity = new CallOpportunity(
                seatIndex: 0,
                canRon: false,
                canPon: false,
                canOpenKan: false,
                chiPatterns: new List<(Tile First, Tile Second)>
                {
                    (HandNotation.Single("2m"), HandNotation.Single("3m")),
                }
            );

            CallDecision decision = _strategy.DecideCall(view, RuleSet, opportunity, discarded);

            Assert.AreEqual(CallDecisionType.Chi, decision.Type);
            Assert.AreEqual(HandNotation.Single("2m"), decision.ChiPattern.Value.First);
            Assert.AreEqual(HandNotation.Single("3m"), decision.ChiPattern.Value.Second);
        }

        [Test]
        public void PlayerView_Create_ExposesOnlyPublicInformationOfOpponents()
        {
            RoundController controller = RoundTestHelper.CreateController();
            controller.Discard(1, HandNotation.Single("7m"), isRiichiDeclaration: true);

            PlayerView view = PlayerView.Create(controller, seatIndex: 0);

            CollectionAssert.AreEqual(
                controller.Players[0].Hand.ConcealedTiles,
                view.ConcealedTiles
            );
            Assert.IsFalse(view.OpponentRiichiDeclaredBySeat.ContainsKey(0));
            Assert.IsTrue(view.OpponentRiichiDeclaredBySeat[1]);
            Assert.AreEqual(1, view.DiscardsBySeat[1].Count);
            Assert.AreEqual(controller.Wall.DoraIndicators.Count, view.VisibleDoraIndicators.Count);
        }
    }
}
