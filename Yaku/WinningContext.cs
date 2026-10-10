using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku
{
    // 役判定1回分の入力。通常形はDecompositionを持ち、七対子・国士無双はDecompositionを持たず
    // 専用フラグ（IsChiitoitsuShape/IsKokushiShape）で表す（形が違いすぎて共通の面子分解に乗らないため）。
    public class WinningContext
    {
        public IReadOnlyList<Tile> ConcealedTiles { get; }
        public HandDecomposition Decomposition { get; }
        public bool IsChiitoitsuShape { get; }
        public bool IsKokushiShape { get; }
        public WinningSituation Situation { get; }

        public IReadOnlyList<Tile> AllTiles { get; }

        public IReadOnlyList<Meld> Melds => Situation.Melds;
        public Tile WinningTile => Situation.WinningTile;
        public bool IsTsumo => Situation.IsTsumo;

        // 面前（門前）かどうか。暗槓は他家の牌を使わないため面前を崩さない。
        public bool IsMenzen => Melds.All(meld => meld.IsConcealed);
        public IReadOnlyList<HandGroup> Sets => Decomposition?.Sets ?? new List<HandGroup>();
        public TileKind PairKind => Decomposition?.PairKind ?? default;

        public WinningContext(
            IReadOnlyList<Tile> concealedTiles,
            WinningSituation situation,
            HandDecomposition decomposition = null,
            bool isChiitoitsuShape = false,
            bool isKokushiShape = false
        )
        {
            ConcealedTiles = concealedTiles;
            Situation = situation;
            Decomposition = decomposition;
            IsChiitoitsuShape = isChiitoitsuShape;
            IsKokushiShape = isKokushiShape;

            var allTiles = new List<Tile>(concealedTiles);
            foreach (Meld meld in situation.Melds)
            {
                allTiles.AddRange(meld.Tiles);
            }
            AllTiles = allTiles;
        }

        // ロン和了で、和了牌によって完成した（=明刻扱いになる）刻子かどうか。
        // 和了牌を順子や雀頭の一部とも解釈できる場合は、点数が高くなる「暗刻のまま」の解釈を採用する。
        public bool IsTripletCompletedByRon(HandGroup set)
        {
            if (IsTsumo || set.Shape != SetShape.Triplet || !set.Kinds[0].Equals(WinningTile.Kind))
            {
                return false;
            }

            bool winningTileFitsElsewhere =
                PairKind.Equals(WinningTile.Kind)
                || Sets.Any(other =>
                    other.Shape == SetShape.Sequence && other.Kinds.Contains(WinningTile.Kind)
                );
            return !winningTileFitsElsewhere;
        }

        public bool IsSetKindYakuhai(TileKind kind)
        {
            return kind.Suit == TileSuit.Dragon
                || kind.Equals(Situation.SeatWind)
                || kind.Equals(Situation.RoundWind);
        }

        // 和了牌を加える前の面前の手牌（聴牌形）。待ちの形に依存する役（十三面待ち・九面待ち）の判定に使う。
        // ConcealedTilesに和了牌が含まれていない場合はnullを返す。
        public List<Tile> GetConcealedTilesBeforeWinningTile()
        {
            int winningTileIndex = TileSearch.FindExactIndex(ConcealedTiles, WinningTile);
            if (winningTileIndex < 0)
            {
                return null;
            }

            var tilesBeforeWin = new List<Tile>(ConcealedTiles);
            tilesBeforeWin.RemoveAt(winningTileIndex);
            return tilesBeforeWin;
        }

        // 国士無双の十三面待ちかどうか。和了牌を除いた13枚が「13種類すべて1枚ずつ」の状態
        // （＝どの種類が和了牌でもよかった状態）であれば十三面待ち。
        public bool IsKokushiThirteenWait()
        {
            if (!IsKokushiShape)
            {
                return false;
            }

            List<Tile> tilesBeforeWin = GetConcealedTilesBeforeWinningTile();
            return tilesBeforeWin != null
                && tilesBeforeWin.Select(tile => tile.Kind).Distinct().Count()
                    == tilesBeforeWin.Count;
        }
    }
}
