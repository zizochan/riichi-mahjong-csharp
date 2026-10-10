using System;
using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Judgment
{
    // 向聴数（あと何回牌を交換すれば聴牌になるか）と、それを進める有効牌を計算する。
    // 通常形・七対子・国士無双のうち最も向聴数が小さいものを採用する。
    public static class ShantenCalculator
    {
        private const int KokushiRequiredKindCount = 13;
        private const int ChiitoitsuRequiredPairCount = 7;
        private const int RegularHandBlockCount = 5;
        private const int MaxRegularShanten = 8;
        private const int CompleteShanten = -1;

        public static ShantenResult Calculate(
            IReadOnlyList<Tile> concealedTiles,
            int calledMeldCount,
            IReadOnlyList<TileKind> allTileKinds
        )
        {
            int shanten = CalculateBestShanten(concealedTiles, calledMeldCount);

            var usefulTileKinds = new List<TileKind>();
            foreach (TileKind kind in allTileKinds.Distinct())
            {
                var tryTiles = new List<Tile>(concealedTiles) { new Tile(kind) };
                if (CalculateBestShanten(tryTiles, calledMeldCount) < shanten)
                {
                    usefulTileKinds.Add(kind);
                }
            }

            return new ShantenResult(shanten, usefulTileKinds);
        }

        public static bool IsCompleteHand(IReadOnlyList<Tile> concealedTiles, int calledMeldCount)
        {
            return CalculateBestShanten(concealedTiles, calledMeldCount) == CompleteShanten;
        }

        private static int CalculateBestShanten(
            IReadOnlyList<Tile> concealedTiles,
            int calledMeldCount
        )
        {
            int regularShanten = CalculateRegularShanten(concealedTiles, calledMeldCount);

            if (calledMeldCount > 0)
            {
                // 七対子・国士無双は鳴きを一切許さない。
                return regularShanten;
            }

            int chiitoitsuShanten = CalculateChiitoitsuShanten(concealedTiles);
            int kokushiShanten = CalculateKokushiShanten(concealedTiles);
            return Math.Min(regularShanten, Math.Min(chiitoitsuShanten, kokushiShanten));
        }

        // --- 通常形（4面子+1雀頭） ---

        private static int CalculateRegularShanten(
            IReadOnlyList<Tile> concealedTiles,
            int calledMeldCount
        )
        {
            Dictionary<TileKind, int> countsByKind = TileCounter.CountByKind(concealedTiles);

            var groupResults = new List<List<BlockCombo>>();

            foreach (TileSuit suit in TileKind.NumberSuits)
            {
                int[] rankCounts = TileCounter.BuildRankCounts(countsByKind, suit);
                if (rankCounts.Any(count => count > 0))
                {
                    groupResults.Add(DecomposeNumberSuit(rankCounts));
                }
            }

            foreach ((TileKind kind, int count) in countsByKind)
            {
                if (kind.IsHonor)
                {
                    groupResults.Add(DecomposeHonorKind(count));
                }
            }

            List<BlockCombo> combined = CombineGroups(groupResults);

            int bestShanten = int.MaxValue;
            foreach (BlockCombo combo in combined)
            {
                int shanten = ComputeShantenFromBlocks(
                    combo.Sets + calledMeldCount,
                    combo.Partials,
                    combo.HasPair
                );
                bestShanten = Math.Min(bestShanten, shanten);
            }

            return bestShanten == int.MaxValue
                ? ComputeShantenFromBlocks(calledMeldCount, 0, false)
                : bestShanten;
        }

        // 通常形の向聴数 = 8 - 2×面子数 - ターツ数(対子含む)。ただし面子+ターツは「4面子+1雀頭」の5ブロックまでしか
        // 数えず、5ブロック揃っていても雀頭候補(対子)が無い場合は1向聴分不足する。
        private static int ComputeShantenFromBlocks(int sets, int partials, bool hasPair)
        {
            int blocks = sets + partials;
            if (blocks > RegularHandBlockCount)
            {
                partials -= blocks - RegularHandBlockCount;
                blocks = RegularHandBlockCount;
            }

            int shanten = MaxRegularShanten - (2 * sets) - partials;
            if (blocks == RegularHandBlockCount && !hasPair)
            {
                shanten += 1;
            }

            return shanten;
        }

        private readonly struct BlockCombo : IEquatable<BlockCombo>
        {
            public int Sets { get; }
            public int Partials { get; }
            public bool HasPair { get; }

            public BlockCombo(int sets, int partials, bool hasPair)
            {
                Sets = sets;
                Partials = partials;
                HasPair = hasPair;
            }

            public bool Equals(BlockCombo other)
            {
                return Sets == other.Sets && Partials == other.Partials && HasPair == other.HasPair;
            }

            public override bool Equals(object obj) => obj is BlockCombo other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Sets, Partials, HasPair);
        }

        private static List<BlockCombo> DecomposeNumberSuit(int[] rankCounts)
        {
            var results = new HashSet<BlockCombo>();
            RecurseNumberSuit((int[])rankCounts.Clone(), 0, 0, 0, false, results);
            return PruneFrontier(results);
        }

        private static void RecurseNumberSuit(
            int[] counts,
            int rankIndex,
            int sets,
            int partials,
            bool hasPair,
            HashSet<BlockCombo> results
        )
        {
            if (rankIndex >= counts.Length)
            {
                results.Add(new BlockCombo(sets, partials, hasPair));
                return;
            }

            if (counts[rankIndex] == 0)
            {
                RecurseNumberSuit(counts, rankIndex + 1, sets, partials, hasPair, results);
                return;
            }

            // 刻子（同じ数字3枚）
            if (counts[rankIndex] >= 3)
            {
                counts[rankIndex] -= 3;
                RecurseNumberSuit(counts, rankIndex, sets + 1, partials, hasPair, results);
                counts[rankIndex] += 3;
            }

            // 順子（連続する3つの数字）
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
                RecurseNumberSuit(counts, rankIndex, sets + 1, partials, hasPair, results);
                counts[rankIndex]++;
                counts[rankIndex + 1]++;
                counts[rankIndex + 2]++;
            }

            // 対子（雀頭候補）
            if (counts[rankIndex] >= 2)
            {
                counts[rankIndex] -= 2;
                RecurseNumberSuit(counts, rankIndex, sets, partials + 1, true, results);
                counts[rankIndex] += 2;
            }

            // 両面/辺張（連続する2つの数字）
            if (
                rankIndex + 1 < counts.Length
                && counts[rankIndex] >= 1
                && counts[rankIndex + 1] >= 1
            )
            {
                counts[rankIndex]--;
                counts[rankIndex + 1]--;
                RecurseNumberSuit(counts, rankIndex, sets, partials + 1, hasPair, results);
                counts[rankIndex]++;
                counts[rankIndex + 1]++;
            }

            // 嵌張（1つ飛ばした2つの数字）
            if (
                rankIndex + 2 < counts.Length
                && counts[rankIndex] >= 1
                && counts[rankIndex + 2] >= 1
            )
            {
                counts[rankIndex]--;
                counts[rankIndex + 2]--;
                RecurseNumberSuit(counts, rankIndex, sets, partials + 1, hasPair, results);
                counts[rankIndex]++;
                counts[rankIndex + 2]++;
            }

            // 残りは孤立牌として捨てて次の数字へ進む
            RecurseNumberSuit(counts, rankIndex + 1, sets, partials, hasPair, results);
        }

        private static List<BlockCombo> DecomposeHonorKind(int count)
        {
            var results = new List<BlockCombo> { new BlockCombo(0, 0, false) };
            if (count >= 2)
            {
                results.Add(new BlockCombo(0, 1, true));
            }
            if (count >= 3)
            {
                results.Add(new BlockCombo(1, 0, false));
            }
            return results;
        }

        private static List<BlockCombo> CombineGroups(List<List<BlockCombo>> groups)
        {
            var combined = new HashSet<BlockCombo> { new BlockCombo(0, 0, false) };
            foreach (List<BlockCombo> group in groups)
            {
                var next = new HashSet<BlockCombo>();
                foreach (BlockCombo accumulated in combined)
                {
                    foreach (BlockCombo candidate in group)
                    {
                        next.Add(
                            new BlockCombo(
                                accumulated.Sets + candidate.Sets,
                                accumulated.Partials + candidate.Partials,
                                accumulated.HasPair || candidate.HasPair
                            )
                        );
                    }
                }
                combined = new HashSet<BlockCombo>(PruneFrontier(next));
            }
            return combined.ToList();
        }

        // パレート最適でない組み合わせ（Sets・Partials・HasPairの全てで他の組み合わせに劣る）を除外し、
        // グループ結合時の組み合わせ爆発を防ぐ。
        private static List<BlockCombo> PruneFrontier(IEnumerable<BlockCombo> combos)
        {
            List<BlockCombo> list = combos.Distinct().ToList();
            var frontier = new List<BlockCombo>();
            foreach (BlockCombo candidate in list)
            {
                bool isDominated = list.Any(other =>
                    !other.Equals(candidate) && Dominates(other, candidate)
                );
                if (!isDominated)
                {
                    frontier.Add(candidate);
                }
            }
            return frontier;
        }

        private static bool Dominates(BlockCombo a, BlockCombo b)
        {
            bool hasPairAtLeastAsGood = a.HasPair || !b.HasPair;
            bool geAll = a.Sets >= b.Sets && a.Partials >= b.Partials && hasPairAtLeastAsGood;
            bool gtAny = a.Sets > b.Sets || a.Partials > b.Partials || (a.HasPair && !b.HasPair);
            return geAll && gtAny;
        }

        // --- 七対子（7種の対子） ---

        private static int CalculateChiitoitsuShanten(IReadOnlyList<Tile> concealedTiles)
        {
            Dictionary<TileKind, int> countsByKind = TileCounter.CountByKind(concealedTiles);

            int pairKindCount = countsByKind.Values.Count(count => count >= 2);
            pairKindCount = Math.Min(pairKindCount, ChiitoitsuRequiredPairCount);
            int distinctKindCount = countsByKind.Count;

            return (ChiitoitsuRequiredPairCount - 1)
                - pairKindCount
                + Math.Max(0, ChiitoitsuRequiredPairCount - distinctKindCount);
        }

        // --- 国士無双（数牌の1・9すべてと風牌・三元牌すべて） ---

        private static int CalculateKokushiShanten(IReadOnlyList<Tile> concealedTiles)
        {
            Dictionary<TileKind, int> kokushiKindCounts = TileCounter.CountByKind(
                concealedTiles.Where(tile => tile.Kind.IsTerminalOrHonor)
            );

            int distinctKindCount = kokushiKindCounts.Count;
            bool hasPair = kokushiKindCounts.Values.Any(count => count >= 2);

            return KokushiRequiredKindCount - distinctKindCount - (hasPair ? 1 : 0);
        }
    }
}
