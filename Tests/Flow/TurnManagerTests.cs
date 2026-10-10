using NUnit.Framework;
using Zizo.Mahjong.Flow;

namespace Zizo.Mahjong.Tests
{
    public class TurnManagerTests
    {
        [Test]
        public void AdvanceToNextPlayer_WrapsAroundAfterLastSeat()
        {
            var turnManager = new TurnManager(playerCount: 3, startingSeatIndex: 2);

            int nextSeat = turnManager.AdvanceToNextPlayer();

            Assert.AreEqual(0, nextSeat);
            Assert.AreEqual(0, turnManager.CurrentPlayerSeatIndex);
        }

        [Test]
        public void JumpToPlayer_SetsCurrentSeat()
        {
            var turnManager = new TurnManager(playerCount: 4, startingSeatIndex: 0);

            turnManager.JumpToPlayer(2);
            turnManager.AdvanceToNextPlayer();

            Assert.AreEqual(3, turnManager.CurrentPlayerSeatIndex);
        }
    }
}
