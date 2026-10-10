using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class SanshokuDoujunYaku : IYaku
    {
        public string Name => "三色同順";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            List<AtomicGroup> groups = ContextGroupExtractor.Extract(context);

            for (
                int lowestRank = TileKind.MinRank;
                lowestRank <= TileKind.MaxSequenceStartRank;
                lowestRank++
            )
            {
                if (
                    ContextGroupExtractor.ExistsInAllNumberSuits(
                        groups,
                        isSequence: true,
                        lowestRank
                    )
                )
                {
                    han = context.IsMenzen ? 2 : 1;
                    return true;
                }
            }

            return false;
        }
    }
}
