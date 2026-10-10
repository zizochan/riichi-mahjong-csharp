using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class ShousuushiYaku : IYaku
    {
        private const int RequiredWindTripletCount = 3;

        public string Name => "小四喜";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            int windTripletCount = ContextGroupExtractor.CountTriplets(context, TileSuit.Wind);
            bool pairIsWind =
                context.Decomposition != null && context.PairKind.Suit == TileSuit.Wind;

            if (windTripletCount != RequiredWindTripletCount || !pairIsWind)
            {
                return false;
            }

            han = 1;
            return true;
        }
    }
}
