using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Judgment
{
    // 和了形の手牌（鳴き面子を除いた面前部分）を、実際の面子+雀頭の組み合わせとして列挙する。
    // 平和・三色同順・一盃口など、面子の組み方そのものに依存する役の判定に使う
    // （ShantenCalculatorは枚数の組み合わせしか見ないため、こちらは実際の牌種の並びまで復元する）。
    public static class HandDecomposer
    {
        public static List<HandDecomposition> EnumerateDecompositions(
            IReadOnlyList<Tile> concealedTiles
        )
        {
            Dictionary<TileKind, int> countsByKind = TileCounter.CountByKind(concealedTiles);
            List<TileKind> honorKinds = countsByKind.Keys.Where(kind => kind.IsHonor).ToList();

            var results = new List<HandDecomposition>();

            foreach (TileSuit pairSuitCandidate in TileKind.NumberSuits)
            {
                int[] pairSuitCounts = TileCounter.BuildRankCounts(countsByKind, pairSuitCandidate);
                if (pairSuitCounts.All(count => count == 0))
                {
                    continue;
                }

                List<(TileKind PairKind, List<HandGroup> Sets)> pairSuitOptions =
                    DecomposeSuitWithPair(pairSuitCandidate, pairSuitCounts);
                if (pairSuitOptions.Count == 0)
                {
                    continue;
                }

                if (
                    !TryDecomposeRemainingAsSetsOnly(
                        countsByKind,
                        honorKinds,
                        excludedSuit: pairSuitCandidate,
                        out List<List<HandGroup>> remainingCombinations
                    )
                )
                {
                    continue;
                }

                foreach ((TileKind pairKind, List<HandGroup> pairSuitSets) in pairSuitOptions)
                foreach (List<HandGroup> remainingSets in remainingCombinations)
                {
                    var allSets = new List<HandGroup>(pairSuitSets);
                    allSets.AddRange(remainingSets);
                    results.Add(new HandDecomposition(allSets, pairKind));
                }
            }

            foreach (TileKind pairHonorCandidate in honorKinds)
            {
                if (countsByKind[pairHonorCandidate] != 2)
                {
                    continue;
                }

                if (
                    !TryDecomposeRemainingAsSetsOnly(
                        countsByKind,
                        honorKinds.Where(kind => !kind.Equals(pairHonorCandidate)).ToList(),
                        excludedSuit: null,
                        out List<List<HandGroup>> remainingCombinations
                    )
                )
                {
                    continue;
                }

                foreach (List<HandGroup> remainingSets in remainingCombinations)
                {
                    results.Add(new HandDecomposition(remainingSets, pairHonorCandidate));
                }
            }

            return results;
        }

        private static bool TryDecomposeRemainingAsSetsOnly(
            Dictionary<TileKind, int> countsByKind,
            List<TileKind> honorKinds,
            TileSuit? excludedSuit,
            out List<List<HandGroup>> combinations
        )
        {
            var optionsPerGroup = new List<List<List<HandGroup>>>();

            foreach (TileSuit suit in TileKind.NumberSuits)
            {
                if (excludedSuit.HasValue && suit == excludedSuit.Value)
                {
                    continue;
                }

                int[] counts = TileCounter.BuildRankCounts(countsByKind, suit);
                List<List<HandGroup>> options = DecomposeSuitSetsOnly(suit, counts);
                if (options.Count == 0)
                {
                    combinations = new List<List<HandGroup>>();
                    return false;
                }
                optionsPerGroup.Add(options);
            }

            foreach (TileKind honorKind in honorKinds)
            {
                List<List<HandGroup>> options = DecomposeHonorSetsOnly(
                    honorKind,
                    countsByKind[honorKind]
                );
                if (options.Count == 0)
                {
                    combinations = new List<List<HandGroup>>();
                    return false;
                }
                optionsPerGroup.Add(options);
            }

            combinations = CartesianCombine(optionsPerGroup);
            return true;
        }

        // 雀頭を使わず、面子だけで消費しきれる組み方を列挙する。
        private static List<List<HandGroup>> DecomposeSuitSetsOnly(TileSuit suit, int[] counts)
        {
            // 雀頭を「使用済み」として探索を始めれば、面子だけの組み方のみが残る。
            return EnumerateSuitDecompositions(suit, counts, isPairAlreadyUsed: true)
                .Select(result => result.Sets)
                .ToList();
        }

        // 雀頭1つ+面子で消費しきれる組み方を、雀頭の種類とともに列挙する。
        private static List<(TileKind PairKind, List<HandGroup> Sets)> DecomposeSuitWithPair(
            TileSuit suit,
            int[] counts
        )
        {
            return EnumerateSuitDecompositions(suit, counts, isPairAlreadyUsed: false);
        }

        private static List<(TileKind PairKind, List<HandGroup> Sets)> EnumerateSuitDecompositions(
            TileSuit suit,
            int[] counts,
            bool isPairAlreadyUsed
        )
        {
            var results = new List<(TileKind, List<HandGroup>)>();
            RecurseWithPair(
                suit,
                (int[])counts.Clone(),
                0,
                isPairAlreadyUsed,
                default,
                new List<HandGroup>(),
                results
            );
            return results;
        }

        private static void RecurseWithPair(
            TileSuit suit,
            int[] counts,
            int rankIndex,
            bool pairUsed,
            TileKind pairKind,
            List<HandGroup> current,
            List<(TileKind, List<HandGroup>)> results
        )
        {
            while (rankIndex < counts.Length && counts[rankIndex] == 0)
            {
                rankIndex++;
            }
            if (rankIndex >= counts.Length)
            {
                if (pairUsed)
                {
                    results.Add((pairKind, new List<HandGroup>(current)));
                }
                return;
            }

            if (counts[rankIndex] >= 3)
            {
                counts[rankIndex] -= 3;
                current.Add(BuildTriplet(suit, rankIndex + TileKind.MinRank));
                RecurseWithPair(suit, counts, rankIndex, pairUsed, pairKind, current, results);
                current.RemoveAt(current.Count - 1);
                counts[rankIndex] += 3;
            }

            if (
                rankIndex + 2 < counts.Length
                && counts[rankIndex] >= 1
                && counts[rankIndex + 1] >= 1
                && counts[rankIndex + 2] >= 1
            )
            {
                counts[rankIndex]--;
                counts[rankIndex + 1]--;
                counts[rankIndex + 2]--;
                current.Add(BuildSequence(suit, rankIndex + TileKind.MinRank));
                RecurseWithPair(suit, counts, rankIndex, pairUsed, pairKind, current, results);
                current.RemoveAt(current.Count - 1);
                counts[rankIndex]++;
                counts[rankIndex + 1]++;
                counts[rankIndex + 2]++;
            }

            if (!pairUsed && counts[rankIndex] >= 2)
            {
                counts[rankIndex] -= 2;
                RecurseWithPair(
                    suit,
                    counts,
                    rankIndex,
                    true,
                    new TileKind(suit, rankIndex + TileKind.MinRank),
                    current,
                    results
                );
                counts[rankIndex] += 2;
            }
        }

        private static List<List<HandGroup>> DecomposeHonorSetsOnly(TileKind kind, int count)
        {
            if (count == 0)
            {
                return new List<List<HandGroup>> { new List<HandGroup>() };
            }
            if (count == 3)
            {
                return new List<List<HandGroup>>
                {
                    new List<HandGroup>
                    {
                        new HandGroup(SetShape.Triplet, new[] { kind, kind, kind }),
                    },
                };
            }
            return new List<List<HandGroup>>();
        }

        private static List<List<HandGroup>> CartesianCombine(
            List<List<List<HandGroup>>> groupOptions
        )
        {
            var accumulated = new List<List<HandGroup>> { new List<HandGroup>() };
            foreach (List<List<HandGroup>> options in groupOptions)
            {
                var next = new List<List<HandGroup>>();
                foreach (List<HandGroup> partial in accumulated)
                foreach (List<HandGroup> option in options)
                {
                    var combined = new List<HandGroup>(partial);
                    combined.AddRange(option);
                    next.Add(combined);
                }
                accumulated = next;
            }
            return accumulated;
        }

        private static HandGroup BuildTriplet(TileSuit suit, int rank)
        {
            var kind = new TileKind(suit, rank);
            return new HandGroup(SetShape.Triplet, new[] { kind, kind, kind });
        }

        private static HandGroup BuildSequence(TileSuit suit, int rank)
        {
            return new HandGroup(
                SetShape.Sequence,
                new[]
                {
                    new TileKind(suit, rank),
                    new TileKind(suit, rank + 1),
                    new TileKind(suit, rank + 2),
                }
            );
        }
    }
}
