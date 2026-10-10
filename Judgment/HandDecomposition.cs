using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Judgment
{
    // 手牌の面前部分を「4面子(-鳴き面子分)+雀頭」に分解した1通りの結果。
    public class HandDecomposition
    {
        public IReadOnlyList<HandGroup> Sets { get; }
        public TileKind PairKind { get; }

        public HandDecomposition(IReadOnlyList<HandGroup> sets, TileKind pairKind)
        {
            Sets = sets;
            PairKind = pairKind;
        }
    }
}
