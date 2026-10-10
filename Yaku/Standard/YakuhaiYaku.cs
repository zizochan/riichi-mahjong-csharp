using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    // 三元牌・自風・場風の刻子（暗刻/明刻/槓）。ダブ東などは自風と場風の両方に一致するため2翻になる。
    public class YakuhaiYaku : IYaku
    {
        public string Name => "役牌";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            foreach (TileKind kind in GetTripletKinds(context))
            {
                if (kind.Suit == TileSuit.Dragon)
                {
                    han += 1;
                }
                if (kind.Equals(context.Situation.SeatWind))
                {
                    han += 1;
                }
                if (kind.Equals(context.Situation.RoundWind))
                {
                    han += 1;
                }
            }
            return han > 0;
        }

        private static IEnumerable<TileKind> GetTripletKinds(WinningContext context)
        {
            if (context.Decomposition != null)
            {
                foreach (HandGroup set in context.Sets.Where(set => set.Shape == SetShape.Triplet))
                {
                    yield return set.Kinds[0];
                }
            }

            foreach (Meld meld in context.Melds.Where(meld => meld.IsTripletLike))
            {
                yield return meld.Tiles[0].Kind;
            }
        }
    }
}
