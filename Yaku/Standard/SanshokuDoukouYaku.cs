using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class SanshokuDoukouYaku : IYaku
    {
        public string Name => "三色同刻";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            List<AtomicGroup> groups = ContextGroupExtractor.Extract(context);

            for (int rank = TileKind.MinRank; rank <= TileKind.MaxNumberRank; rank++)
            {
                if (ContextGroupExtractor.ExistsInAllNumberSuits(groups, isSequence: false, rank))
                {
                    han = 2;
                    return true;
                }
            }

            return false;
        }
    }
}
