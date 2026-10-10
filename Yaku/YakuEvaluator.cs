using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku
{
    // 和了形の面子の組み方（複数通りありうる）を全て試し、最も翻数が高くなる組み合わせを採用する。
    public static class YakuEvaluator
    {
        private const int CompleteHandTileCount = 14;
        private const int ChiitoitsuPairCount = 7;
        private const int KokushiKindCount = 13;

        public static YakuEvaluationResult EvaluateBest(
            IReadOnlyList<Tile> concealedTiles,
            int calledMeldCount,
            WinningSituation situation,
            IReadOnlyList<IYaku> enabledYaku
        )
        {
            var candidates = new List<YakuEvaluationResult>();

            if (calledMeldCount == 0)
            {
                if (IsChiitoitsuComplete(concealedTiles))
                {
                    var context = new WinningContext(
                        concealedTiles,
                        situation,
                        isChiitoitsuShape: true
                    );
                    candidates.Add(EvaluateContext(context, enabledYaku));
                }

                if (IsKokushiComplete(concealedTiles))
                {
                    var context = new WinningContext(
                        concealedTiles,
                        situation,
                        isKokushiShape: true
                    );
                    candidates.Add(EvaluateContext(context, enabledYaku));
                }
            }

            foreach (
                HandDecomposition decomposition in HandDecomposer.EnumerateDecompositions(
                    concealedTiles
                )
            )
            {
                var context = new WinningContext(
                    concealedTiles,
                    situation,
                    decomposition: decomposition
                );
                candidates.Add(EvaluateContext(context, enabledYaku));
            }

            if (candidates.Count == 0)
            {
                return new YakuEvaluationResult(new List<(IYaku, int)>(), null);
            }

            // 役満候補は倍率としてのHanを持つため、通常役の翻数と同じ物差しでは比較できない。
            // まず役満の有無で優先し、その中でTotalHanが最大のものを採用する。
            return candidates
                .OrderByDescending(candidate => candidate.HasYakuman)
                .ThenByDescending(candidate => candidate.TotalHan)
                .First();
        }

        private static YakuEvaluationResult EvaluateContext(
            WinningContext context,
            IReadOnlyList<IYaku> enabledYaku
        )
        {
            var matched = new List<(IYaku Yaku, int Han)>();
            foreach (IYaku yaku in enabledYaku)
            {
                if (yaku.TryEvaluate(context, out int han))
                {
                    matched.Add((yaku, han));
                }
            }
            return new YakuEvaluationResult(matched, context);
        }

        private static bool IsChiitoitsuComplete(IReadOnlyList<Tile> concealedTiles)
        {
            if (concealedTiles.Count != CompleteHandTileCount)
            {
                return false;
            }
            List<int> countsByKind = concealedTiles
                .GroupBy(tile => tile.Kind)
                .Select(group => group.Count())
                .ToList();
            return countsByKind.Count == ChiitoitsuPairCount
                && countsByKind.All(count => count == 2);
        }

        private static bool IsKokushiComplete(IReadOnlyList<Tile> concealedTiles)
        {
            if (concealedTiles.Count != CompleteHandTileCount)
            {
                return false;
            }

            if (concealedTiles.Any(tile => !tile.Kind.IsTerminalOrHonor))
            {
                return false;
            }

            Dictionary<TileKind, int> countsByKind = concealedTiles
                .GroupBy(tile => tile.Kind)
                .ToDictionary(group => group.Key, group => group.Count());

            return countsByKind.Count == KokushiKindCount
                && countsByKind.Values.Count(count => count == 2) == 1
                && countsByKind.Values.All(count => count == 1 || count == 2);
        }
    }
}
