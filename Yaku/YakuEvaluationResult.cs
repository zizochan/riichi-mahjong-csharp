using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;

namespace Zizo.Mahjong.Yaku
{
    public class YakuEvaluationResult
    {
        public IReadOnlyList<(IYaku Yaku, int Han)> MatchedYaku { get; }
        public WinningContext Context { get; }
        public HandDecomposition Decomposition => Context?.Decomposition;

        public bool HasYakuman => MatchedYaku.Any(matched => matched.Yaku.IsYakuman);
        public bool HasYaku => MatchedYaku.Count > 0;

        // 点数計算に使われる役。役満が1つでも成立していれば通常役は無視して役満のみとする（実際の役満ルールに合わせる）。
        public IEnumerable<(IYaku Yaku, int Han)> ScoringYaku =>
            HasYakuman ? MatchedYaku.Where(matched => matched.Yaku.IsYakuman) : MatchedYaku;

        public int TotalHan => ScoringYaku.Sum(matched => matched.Han);

        public YakuEvaluationResult(
            IReadOnlyList<(IYaku Yaku, int Han)> matchedYaku,
            WinningContext context
        )
        {
            MatchedYaku = matchedYaku;
            Context = context;
        }
    }
}
