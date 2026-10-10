using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    internal static class TerminalHonorAnalysis
    {
        // 雀頭・すべての面子（面前/鳴き問わず）が、幺九牌（老頭牌+字牌、allowHonor=falseなら老頭牌のみ）
        // のいずれかを含んでいるかどうか。混全帯幺九/純全帯幺九の判定に使う。
        public static bool AllGroupsTouchTerminalOrHonor(WinningContext context, bool allowHonor)
        {
            if (context.Decomposition == null)
            {
                return false;
            }

            if (!KindTouchesTerminalOrHonor(context.PairKind, allowHonor))
            {
                return false;
            }

            foreach (AtomicGroup group in ContextGroupExtractor.Extract(context))
            {
                if (group.IsSequence)
                {
                    // 順子の最小の数字が1(123)か7(789)なら老頭牌を含む。
                    bool touchesTerminal =
                        group.Rank == TileKind.MinRank
                        || group.Rank == TileKind.MaxSequenceStartRank;
                    if (!touchesTerminal)
                    {
                        return false;
                    }
                }
                else if (
                    !KindTouchesTerminalOrHonor(new TileKind(group.Suit, group.Rank), allowHonor)
                )
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasAnySequence(WinningContext context)
        {
            return ContextGroupExtractor.Extract(context).Any(group => group.IsSequence);
        }

        private static bool KindTouchesTerminalOrHonor(TileKind kind, bool allowHonor)
        {
            return kind.IsHonor ? allowHonor : kind.IsTerminal;
        }
    }
}
