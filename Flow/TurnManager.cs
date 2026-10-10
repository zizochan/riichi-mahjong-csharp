namespace Zizo.Mahjong.Flow
{
    // 座席の順送りだけを管理する。PlayerCountに依存して汎用的に動くようにするための最小限のクラス。
    public class TurnManager
    {
        private readonly int _playerCount;

        public int CurrentPlayerSeatIndex { get; private set; }

        public TurnManager(int playerCount, int startingSeatIndex)
        {
            _playerCount = playerCount;
            CurrentPlayerSeatIndex = startingSeatIndex;
        }

        public int AdvanceToNextPlayer()
        {
            CurrentPlayerSeatIndex = (CurrentPlayerSeatIndex + 1) % _playerCount;
            return CurrentPlayerSeatIndex;
        }

        public void JumpToPlayer(int seatIndex)
        {
            CurrentPlayerSeatIndex = seatIndex;
        }
    }
}
