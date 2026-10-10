using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Judgment
{
    public static class ActionValidator
    {
        public static bool CanChi(
            Hand hand,
            Tile discardedTile,
            int discarderSeatIndex,
            int ownSeatIndex,
            int playerCount
        )
        {
            int leftNeighborSeatIndex = (ownSeatIndex - 1 + playerCount) % playerCount;
            if (discarderSeatIndex != leftNeighborSeatIndex)
            {
                return false;
            }
            return GetChiCompletionPatterns(hand, discardedTile).Count > 0;
        }

        // 鳴いた牌と組み合わせて順子を作れる、手牌側の2枚の組み合わせを全て返す。
        public static List<(Tile First, Tile Second)> GetChiCompletionPatterns(
            Hand hand,
            Tile discardedTile
        )
        {
            var patterns = new List<(Tile First, Tile Second)>();
            if (!discardedTile.Kind.IsNumberTile)
            {
                return patterns;
            }

            TileSuit suit = discardedTile.Suit;
            int rank = discardedTile.Rank;

            void TryAddPattern(int rankA, int rankB)
            {
                if (rankA < TileKind.MinRank || rankB > TileKind.MaxNumberRank)
                {
                    return;
                }
                Tile? tileA = FindConcealedTileByKind(hand, new TileKind(suit, rankA));
                Tile? tileB = FindConcealedTileByKind(hand, new TileKind(suit, rankB));
                if (tileA.HasValue && tileB.HasValue)
                {
                    patterns.Add((tileA.Value, tileB.Value));
                }
            }

            TryAddPattern(rank - 2, rank - 1);
            TryAddPattern(rank - 1, rank + 1);
            TryAddPattern(rank + 1, rank + 2);

            return patterns;
        }

        public static bool CanPon(Hand hand, Tile discardedTile)
        {
            return hand.ConcealedTiles.Count(tile => tile.Kind.Equals(discardedTile.Kind)) >= 2;
        }

        public static bool CanOpenKan(Hand hand, Tile discardedTile)
        {
            return hand.ConcealedTiles.Count(tile => tile.Kind.Equals(discardedTile.Kind)) >= 3;
        }

        public static List<TileKind> GetClosedKanCandidates(Hand hand)
        {
            return hand
                .ConcealedTiles.GroupBy(tile => tile.Kind)
                .Where(group => group.Count() >= 4)
                .Select(group => group.Key)
                .ToList();
        }

        public static bool CanAddedKan(PlayerState playerState, TileKind kind)
        {
            bool hasExistingPon = playerState.Melds.Any(meld =>
                meld.Type == MeldType.Pon && meld.Tiles[0].Kind.Equals(kind)
            );
            bool hasFourthTileInHand = playerState.Hand.ConcealedTiles.Any(tile =>
                tile.Kind.Equals(kind)
            );
            return hasExistingPon && hasFourthTileInHand;
        }

        public static bool CanRiichi(
            PlayerState playerState,
            int shanten,
            int remainingLiveWallTileCount,
            int minimumScoreToDeclare = 1000,
            int minimumRemainingTileCount = 4
        )
        {
            return playerState.IsMenzen
                && !playerState.IsRiichiDeclared
                && shanten == 0
                && playerState.Score >= minimumScoreToDeclare
                && remainingLiveWallTileCount >= minimumRemainingTileCount;
        }

        // 自分の捨て牌の中に自分の待ち牌があるかどうか（永続的なフリテン）。
        // 「一度でも見逃すとその局の間ずっとロン不可」になる同巡フリテン等はRoundController側の状態として別途管理する。
        public static bool IsFuriten(
            PlayerState playerState,
            IReadOnlyList<TileKind> waitingTileKinds
        )
        {
            if (waitingTileKinds.Count == 0)
            {
                return false;
            }
            return playerState.DiscardPile.Discards.Any(discarded =>
                waitingTileKinds.Any(kind => kind.Equals(discarded.Tile.Kind))
            );
        }

        private static Tile? FindConcealedTileByKind(Hand hand, TileKind kind)
        {
            foreach (Tile tile in hand.ConcealedTiles)
            {
                if (tile.Kind.Equals(kind))
                {
                    return tile;
                }
            }
            return null;
        }
    }
}
