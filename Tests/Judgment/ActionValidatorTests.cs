using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class ActionValidatorTests
    {
        private static readonly IReadOnlyList<TileKind> AllTileKinds = TileSetDefinition
            .CreateStandard()
            .Entries.Select(entry => entry.Kind)
            .Distinct()
            .ToList();

        [Test]
        public void CanChi_FromLeftNeighbor_ReturnsTrue()
        {
            var hand = new Hand(HandNotation.Parse("2456m"));

            // 4人麻雀で自席2の上家は席1。
            bool canChi = ActionValidator.CanChi(
                hand,
                HandNotation.Single("3m"),
                discarderSeatIndex: 1,
                ownSeatIndex: 2,
                playerCount: 4
            );

            Assert.IsTrue(canChi);
        }

        [Test]
        public void CanChi_FromNonLeftNeighbor_ReturnsFalse()
        {
            var hand = new Hand(HandNotation.Parse("2456m"));

            bool canChi = ActionValidator.CanChi(
                hand,
                HandNotation.Single("3m"),
                discarderSeatIndex: 0,
                ownSeatIndex: 2,
                playerCount: 4
            );

            Assert.IsFalse(canChi);
        }

        [Test]
        public void GetChiCompletionPatterns_ReturnsEveryValidCombination()
        {
            var hand = new Hand(HandNotation.Parse("2456m"));

            List<(Tile First, Tile Second)> patterns = ActionValidator.GetChiCompletionPatterns(
                hand,
                HandNotation.Single("3m")
            );

            // 2-4(カンチャン)と4-5(ペンチャン側)の2通りが成立し、1-2は1mが無いため成立しない。
            Assert.AreEqual(2, patterns.Count);
        }

        [Test]
        public void GetChiCompletionPatterns_NonNumberTile_ReturnsEmpty()
        {
            var hand = new Hand(HandNotation.Parse("2456m"));

            List<(Tile First, Tile Second)> patterns = ActionValidator.GetChiCompletionPatterns(
                hand,
                HandNotation.Single("1z")
            );

            Assert.IsEmpty(patterns);
        }

        [Test]
        public void CanPon_TwoMatchingTilesInHand_ReturnsTrue()
        {
            var hand = new Hand(HandNotation.Parse("55m123p"));

            Assert.IsTrue(ActionValidator.CanPon(hand, HandNotation.Single("5m")));
        }

        [Test]
        public void CanPon_OnlyOneMatchingTile_ReturnsFalse()
        {
            var hand = new Hand(HandNotation.Parse("5m123p"));

            Assert.IsFalse(ActionValidator.CanPon(hand, HandNotation.Single("5m")));
        }

        [Test]
        public void CanOpenKan_ThreeMatchingTiles_ReturnsTrue()
        {
            var hand = new Hand(HandNotation.Parse("555m123p"));

            Assert.IsTrue(ActionValidator.CanOpenKan(hand, HandNotation.Single("5m")));
        }

        [Test]
        public void CanOpenKan_OnlyTwoMatchingTiles_ReturnsFalse()
        {
            var hand = new Hand(HandNotation.Parse("55m123p"));

            Assert.IsFalse(ActionValidator.CanOpenKan(hand, HandNotation.Single("5m")));
        }

        [Test]
        public void CanAddedKan_ExistingPonAndFourthTileInHand_ReturnsTrue()
        {
            var player = new PlayerState(0, new Hand(HandNotation.Parse("5m123p")), 25000);
            player.AddMeld(MeldFactory.Pon("555m"));

            Assert.IsTrue(ActionValidator.CanAddedKan(player, new TileKind(TileSuit.Man, 5)));
        }

        [Test]
        public void CanAddedKan_NoFourthTileInHand_ReturnsFalse()
        {
            var player = new PlayerState(0, new Hand(HandNotation.Parse("123p")), 25000);
            player.AddMeld(MeldFactory.Pon("555m"));

            Assert.IsFalse(ActionValidator.CanAddedKan(player, new TileKind(TileSuit.Man, 5)));
        }

        [Test]
        public void CanAddedKan_NoMatchingPon_ReturnsFalse()
        {
            var player = new PlayerState(0, new Hand(HandNotation.Parse("5m123p")), 25000);
            player.AddMeld(MeldFactory.Chi("456m"));

            Assert.IsFalse(ActionValidator.CanAddedKan(player, new TileKind(TileSuit.Man, 5)));
        }

        [Test]
        public void GetClosedKanCandidates_FourMatchingTiles_ReturnsThatKind()
        {
            var hand = new Hand(HandNotation.Parse("5555m123p"));

            List<TileKind> candidates = ActionValidator.GetClosedKanCandidates(hand);

            CollectionAssert.Contains(candidates, new TileKind(TileSuit.Man, 5));
        }

        [Test]
        public void CanRiichi_MenzenTenpaiWithEnoughScoreAndWall_ReturnsTrue()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                25000
            );

            Assert.IsTrue(
                ActionValidator.CanRiichi(playerState, shanten: 0, remainingLiveWallTileCount: 10)
            );
        }

        [Test]
        public void CanRiichi_NotTenpai_ReturnsFalse()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                25000
            );

            Assert.IsFalse(
                ActionValidator.CanRiichi(playerState, shanten: 1, remainingLiveWallTileCount: 10)
            );
        }

        [Test]
        public void CanRiichi_ScoreBelowMinimum_ReturnsFalse()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                500
            );

            Assert.IsFalse(
                ActionValidator.CanRiichi(playerState, shanten: 0, remainingLiveWallTileCount: 10)
            );
        }

        [Test]
        public void CanRiichi_NotEnoughRemainingWall_ReturnsFalse()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                25000
            );

            Assert.IsFalse(
                ActionValidator.CanRiichi(playerState, shanten: 0, remainingLiveWallTileCount: 3)
            );
        }

        [Test]
        public void CanRiichi_AlreadyDeclared_ReturnsFalse()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                25000
            );
            playerState.DeclareRiichi();

            Assert.IsFalse(
                ActionValidator.CanRiichi(playerState, shanten: 0, remainingLiveWallTileCount: 10)
            );
        }

        [Test]
        public void IsFuriten_DiscardedAWaitingTile_ReturnsTrue()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                25000
            );
            playerState.DiscardPile.Add(HandNotation.Single("1p"));

            var waits = new List<TileKind>
            {
                new TileKind(TileSuit.Pin, 1),
                new TileKind(TileSuit.Sou, 2),
            };

            Assert.IsTrue(ActionValidator.IsFuriten(playerState, waits));
        }

        [Test]
        public void IsFuriten_DiscardsDoNotContainWaits_ReturnsFalse()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                25000
            );
            playerState.DiscardPile.Add(HandNotation.Single("9p"));

            var waits = new List<TileKind>
            {
                new TileKind(TileSuit.Pin, 1),
                new TileKind(TileSuit.Sou, 2),
            };

            Assert.IsFalse(ActionValidator.IsFuriten(playerState, waits));
        }

        [Test]
        public void IsFuriten_NoWaits_ReturnsFalse()
        {
            var playerState = new PlayerState(
                0,
                new Hand(HandNotation.Parse("123456789m11p22s")),
                25000
            );
            playerState.DiscardPile.Add(HandNotation.Single("1p"));

            Assert.IsFalse(ActionValidator.IsFuriten(playerState, new List<TileKind>()));
        }
    }
}
