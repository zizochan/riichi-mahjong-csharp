using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class DaisuushiYaku : IYaku
    {
        private const int RequiredWindTripletCount = 4;

        public string Name => "大四喜";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            int windTripletCount = ContextGroupExtractor.CountTriplets(context, TileSuit.Wind);

            if (windTripletCount != RequiredWindTripletCount)
            {
                return false;
            }

            han = 2; // ダブル役満
            return true;
        }
    }
}
