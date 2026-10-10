using NUnit.Framework;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class PlayerStateTests
    {
        private static PlayerState CreatePlayer(string handNotation = "")
        {
            return new PlayerState(0, new Hand(HandNotation.Parse(handNotation)), 25000);
        }

        [Test]
        public void IsMenzen_WithOnlyClosedKan_IsTrue()
        {
            PlayerState player = CreatePlayer();

            player.AddMeld(MeldFactory.ClosedKan("1111z"));

            Assert.IsTrue(player.IsMenzen);
        }

        [Test]
        public void IsMenzen_WithPon_IsFalse()
        {
            PlayerState player = CreatePlayer();

            player.AddMeld(MeldFactory.Pon("555z"));

            Assert.IsFalse(player.IsMenzen);
        }

        [Test]
        public void TryUpgradePonToAddedKan_ExistingPon_BecomesAddedKanKeepingSource()
        {
            PlayerState player = CreatePlayer();
            player.AddMeld(MeldFactory.Pon("555z"));

            bool upgraded = player.TryUpgradePonToAddedKan(
                new TileKind(TileSuit.Dragon, 1),
                HandNotation.Single("5z")
            );

            Assert.IsTrue(upgraded);
            Meld meld = player.Melds[0];
            Assert.AreEqual(MeldType.AddedKan, meld.Type);
            Assert.AreEqual(4, meld.Tiles.Count);
            Assert.IsTrue(meld.IsKan);
            Assert.IsNotNull(meld.SourceSeatIndex);
        }

        [Test]
        public void TryUpgradePonToAddedKan_NoMatchingPon_ReturnsFalse()
        {
            PlayerState player = CreatePlayer();
            player.AddMeld(MeldFactory.Pon("555z"));

            bool upgraded = player.TryUpgradePonToAddedKan(
                new TileKind(TileSuit.Dragon, 2),
                HandNotation.Single("6z")
            );

            Assert.IsFalse(upgraded);
            Assert.AreEqual(MeldType.Pon, player.Melds[0].Type);
        }

        [Test]
        public void ApplyScoreDelta_AddsToScore()
        {
            PlayerState player = CreatePlayer();

            player.ApplyScoreDelta(-1000);

            Assert.AreEqual(24000, player.Score);
        }
    }
}
