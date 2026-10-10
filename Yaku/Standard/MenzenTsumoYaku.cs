namespace Zizo.Mahjong.Yaku.Standard
{
    public class MenzenTsumoYaku : IYaku
    {
        public string Name => "門前清自摸和";
        public bool IsYakuman => false;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            if (context.IsMenzen && context.IsTsumo)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
