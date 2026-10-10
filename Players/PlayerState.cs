using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Players
{
    public class PlayerState
    {
        private readonly List<Meld> _melds = new List<Meld>();

        public int SeatIndex { get; }
        public Hand Hand { get; }
        public DiscardPile DiscardPile { get; } = new DiscardPile();
        public IReadOnlyList<Meld> Melds => _melds;
        public int Score { get; private set; }
        public bool IsRiichiDeclared { get; private set; }

        // 面前（メンゼン）かどうか。暗槓は他家の牌を使わないため面前を崩さない。
        public bool IsMenzen => _melds.All(meld => meld.IsConcealed);

        public PlayerState(int seatIndex, Hand hand, int initialScore)
        {
            SeatIndex = seatIndex;
            Hand = hand;
            Score = initialScore;
        }

        public void AddMeld(Meld meld)
        {
            _melds.Add(meld);
        }

        public void DeclareRiichi()
        {
            IsRiichiDeclared = true;
        }

        public void ApplyScoreDelta(int delta)
        {
            Score += delta;
        }

        // 加槓（既存のポンに4枚目を足して槓にする）。対応するポンが無ければfalseを返す。
        public bool TryUpgradePonToAddedKan(TileKind kind, Tile fourthTile)
        {
            int index = _melds.FindIndex(meld =>
                meld.Type == MeldType.Pon && meld.Tiles[0].Kind.Equals(kind)
            );
            if (index < 0)
            {
                return false;
            }

            Meld existing = _melds[index];
            var tiles = new List<Tile>(existing.Tiles) { fourthTile };
            _melds[index] = new Meld(
                MeldType.AddedKan,
                tiles,
                existing.CalledTile,
                existing.SourceSeatIndex
            );
            return true;
        }
    }
}
