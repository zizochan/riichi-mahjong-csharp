using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class IttsuYaku : IYaku
    {
        // 123・456・789の順子の最小の数字。
        private static readonly int[] RequiredLowestRanks =
        {
            TileKind.MinRank,
            TileKind.MinRank + 3,
            TileKind.MaxSequenceStartRank,
        };

        public string Name => "一気通貫";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            List<AtomicGroup> groups = ContextGroupExtractor.Extract(context);

            foreach (TileSuit suit in TileKind.NumberSuits)
            {
                var lowestRanksInSuit = new HashSet<int>(
                    groups
                        .Where(group => group.IsSequence && group.Suit == suit)
                        .Select(group => group.Rank)
                );

                if (RequiredLowestRanks.All(lowestRanksInSuit.Contains))
                {
                    han = context.IsMenzen ? 2 : 1;
                    return true;
                }
            }

            return false;
        }
    }
}
