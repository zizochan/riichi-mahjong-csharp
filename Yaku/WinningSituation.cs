using System.Collections.Generic;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku
{
    // 面子の組み方に依存しない、和了時の状況（誰の何巡目のどんな和了か）をまとめたもの。
    public class WinningSituation
    {
        public IReadOnlyList<Meld> Melds { get; }
        public Tile WinningTile { get; }
        public bool IsTsumo { get; }
        public bool IsRiichi { get; }
        public bool IsDoubleRiichi { get; }
        public bool IsIppatsu { get; }
        public bool IsHaitei { get; }
        public bool IsHoutei { get; }
        public bool IsRinshan { get; }
        public bool IsChankan { get; }
        public bool IsFirstUninterruptedTurnWin { get; }
        public TileKind SeatWind { get; }
        public TileKind RoundWind { get; }

        // 親の自風は常に東になるため、自風から親かどうかを判定できる。
        public bool IsDealer => SeatWind.Equals(TileKind.East);

        public WinningSituation(
            IReadOnlyList<Meld> melds,
            Tile winningTile,
            bool isTsumo,
            TileKind seatWind,
            TileKind roundWind,
            bool isRiichi = false,
            bool isDoubleRiichi = false,
            bool isIppatsu = false,
            bool isHaitei = false,
            bool isHoutei = false,
            bool isRinshan = false,
            bool isChankan = false,
            bool isFirstUninterruptedTurnWin = false
        )
        {
            Melds = melds;
            WinningTile = winningTile;
            IsTsumo = isTsumo;
            SeatWind = seatWind;
            RoundWind = roundWind;
            IsRiichi = isRiichi;
            IsDoubleRiichi = isDoubleRiichi;
            IsIppatsu = isIppatsu;
            IsHaitei = isHaitei;
            IsHoutei = isHoutei;
            IsRinshan = isRinshan;
            IsChankan = isChankan;
            IsFirstUninterruptedTurnWin = isFirstUninterruptedTurnWin;
        }
    }
}
