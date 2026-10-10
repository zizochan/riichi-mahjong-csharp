using Zizo.Mahjong.Flow;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Cpu
{
    public interface ICpuStrategy
    {
        // 手牌14枚（ツモ直後）から捨てる1枚を選ぶ。
        Tile ChooseDiscard(PlayerView view, MahjongRuleSet ruleSet);

        // 現在の手牌（13枚、聴牌時）でリーチを宣言するかどうか。
        bool ShouldDeclareRiichi(PlayerView view, MahjongRuleSet ruleSet);

        // 他家の捨て牌に対して、鳴く/ロンする/見送るを判断する。
        CallDecision DecideCall(
            PlayerView view,
            MahjongRuleSet ruleSet,
            CallOpportunity opportunity,
            Tile discardedTile
        );
    }
}
