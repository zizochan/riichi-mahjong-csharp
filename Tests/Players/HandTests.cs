using System;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Players;

namespace Zizo.Mahjong.Tests
{
    public class HandTests
    {
        [Test]
        public void Discard_DistinguishesRedFiveFromNormalFive()
        {
            var hand = new Hand(HandNotation.Parse("05m"));

            hand.Discard(HandNotation.Single("5m"));

            Assert.AreEqual(1, hand.ConcealedTiles.Count);
            Assert.IsTrue(hand.ConcealedTiles[0].IsRedDora);
        }

        [Test]
        public void Discard_TileNotInHand_Throws()
        {
            var hand = new Hand(HandNotation.Parse("123m"));

            Assert.Throws<InvalidOperationException>(() => hand.Discard(HandNotation.Single("0m")));
        }

        [Test]
        public void RemoveForMeld_RemovesEachTile()
        {
            var hand = new Hand(HandNotation.Parse("1123m"));

            hand.RemoveForMeld(HandNotation.Parse("11m"));

            CollectionAssert.AreEqual(HandNotation.Parse("23m"), hand.ConcealedTiles.ToList());
        }
    }
}
