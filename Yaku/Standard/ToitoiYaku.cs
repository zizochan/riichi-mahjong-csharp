using System.Linq;
using Zizo.Mahjong.Judgment;

namespace Zizo.Mahjong.Yaku.Standard
{
    public class ToitoiYaku : IYaku
    {
        public string Name => "対々和";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            han = 0;
            if (context.Decomposition == null)
            {
                return false;
            }

            bool allConcealedSetsAreTriplets = context.Sets.All(set =>
                set.Shape == SetShape.Triplet
            );
            bool allMeldsAreTripletLike = context.Melds.All(meld => meld.IsTripletLike);
            if (!allConcealedSetsAreTriplets || !allMeldsAreTripletLike)
            {
                return false;
            }

            han = 2;
            return true;
        }
    }
}
