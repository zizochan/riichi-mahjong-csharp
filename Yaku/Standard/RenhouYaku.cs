namespace Zizo.Mahjong.Yaku.Standard
{
    // 人和。子の第一巡、他家の打牌をロンして和了した場合の役（ルールにより役満/跳満扱いなど扱いが分かれる）。
    // ここでは役満相当として実装しておき、採用しないルールでは`MahjongRuleSet.EnabledYaku`から外せばよい。
    public class RenhouYaku : IYaku
    {
        public string Name => "人和";
        public bool IsYakuman => true;

        public bool TryEvaluate(WinningContext context, out int han)
        {
            bool isDealer = context.Situation.IsDealer;
            if (!isDealer && !context.IsTsumo && context.Situation.IsFirstUninterruptedTurnWin)
            {
                han = 1;
                return true;
            }
            han = 0;
            return false;
        }
    }
}
