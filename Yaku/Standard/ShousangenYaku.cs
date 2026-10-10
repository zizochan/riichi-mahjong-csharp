using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class ShousangenYaku : IYaku
    {
        private const int RequiredDragonTripletCount = 2;

        public string Name => "小三元";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            int dragonTripletCount = ContextGroupExtractor.CountTriplets(context, TileSuit.Dragon);

            bool pairIsDragon =
                context.Decomposition != null && context.PairKind.Suit == TileSuit.Dragon;

            // 三元牌の刻子が3つ揃う場合は大三元(役満)側でのみ成立させる。
            if (dragonTripletCount != RequiredDragonTripletCount || !pairIsDragon)
            {
                return false;
            }

            han = 2;
            return true;
        }
    }
}
