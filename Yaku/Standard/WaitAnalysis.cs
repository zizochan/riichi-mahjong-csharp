using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    internal static class WaitAnalysis
    {
        // 和了牌の種類を含む面前の順子。和了牌がどの面子で使われたかは面子分解が保持していないため、
        // 呼び出し側はこの候補のうち点数が高くなる解釈を採用する（高点法）。
        public static IEnumerable<HandGroup> GetSequencesContainingWinningTile(
            WinningContext context
        )
        {
            return context.Sets.Where(set =>
                set.Shape == SetShape.Sequence && set.Kinds.Contains(context.WinningTile.Kind)
            );
        }

        // 和了牌でこの順子を完成させた形が両面待ちと解釈できるか（嵌張・辺張でないか）。
        public static bool IsRyanmenCompletion(HandGroup sequence, TileKind winningKind)
        {
            int lowestRank = sequence.Kinds.Min(kind => kind.Rank);
            int positionInSequence = winningKind.Rank - lowestRank;

            bool isKanchan = positionInSequence == 1;
            bool isPenchanOfSevenEightNine =
                positionInSequence == 0 && lowestRank == TileKind.MaxSequenceStartRank;
            bool isPenchanOfOneTwoThree = positionInSequence == 2 && lowestRank == TileKind.MinRank;
            return !isKanchan && !isPenchanOfSevenEightNine && !isPenchanOfOneTwoThree;
        }
    }
}
