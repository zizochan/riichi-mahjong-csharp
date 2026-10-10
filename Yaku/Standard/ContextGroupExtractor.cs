using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    internal static class ContextGroupExtractor
    {
        public static List<AtomicGroup> Extract(WinningContext context)
        {
            var groups = new List<AtomicGroup>();

            // 七対子・国士無双の形には面子が無い（Setsは空になる）。
            foreach (HandGroup set in context.Sets)
            {
                groups.Add(CreateGroup(set.Shape == SetShape.Sequence, set.Kinds));
            }

            foreach (Meld meld in context.Melds)
            {
                groups.Add(CreateGroup(!meld.IsTripletLike, meld.Tiles.Select(tile => tile.Kind)));
            }

            return groups;
        }

        // 指定したスートの刻子（鳴き・槓子を含む）の数。三元牌・風牌の役の判定に使う。
        public static int CountTriplets(WinningContext context, TileSuit suit)
        {
            return Extract(context).Count(group => !group.IsSequence && group.Suit == suit);
        }

        // 萬子・筒子・索子のすべてに、指定した形（順子/刻子）・数字の面子があるか。三色同順・三色同刻の判定に使う。
        public static bool ExistsInAllNumberSuits(
            IReadOnlyList<AtomicGroup> groups,
            bool isSequence,
            int rank
        )
        {
            return TileKind.NumberSuits.All(suit =>
                groups.Any(group =>
                    group.IsSequence == isSequence && group.Suit == suit && group.Rank == rank
                )
            );
        }

        private static AtomicGroup CreateGroup(bool isSequence, IEnumerable<TileKind> kinds)
        {
            List<TileKind> kindList = kinds.ToList();
            int lowestRank = kindList.Min(kind => kind.Rank);
            return new AtomicGroup(isSequence, kindList[0].Suit, lowestRank);
        }
    }
}
