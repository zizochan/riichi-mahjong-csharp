using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class DaisangenYaku : IYaku
    {
        private const int RequiredDragonTripletCount = 3;

        public string Name => "大三元";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            int dragonTripletCount = ContextGroupExtractor.CountTriplets(context, TileSuit.Dragon);

            if (dragonTripletCount != RequiredDragonTripletCount)
            {
                return false;
            }

            han = 1;
            return true;
        }
    }
}
