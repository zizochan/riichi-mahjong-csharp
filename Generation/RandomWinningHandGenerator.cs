using System;
using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Scoring;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;

namespace Zizo.Mahjong.Generation
{
    // 点数計算の練習問題などに使える、役が確定した和了手牌をランダムに生成する。
    // 「先に役を決めて、それが必ず成立する牌の組み合わせを機械的に組み立てる」構成的生成器。
    // 完全ランダムに面子を組んでから役判定してリトライする方式は、通常形が無役になりやすく
    // 収束が遅いため採用しない。対象役ごとの専用構築関数を呼べば、その役の成立は構築時点で
    // （WinningTileの選び方に依存しないため）ほぼ確定する。IsCompleteHand/EvaluateBestでの
    // 再チェックは、実装ミス以外では発火しないはずの防御的な安全弁。
    public static class RandomWinningHandGenerator
    {
        private const int DefaultMaxAttempts = 20;
        private const int RequiredHandTileCount = 14;
        private const int PairTileCount = 2;
        private const int TripletTileCount = 3;
        private const int MeldCountInRegularHand = 4;
        private const int ChiitoitsuPairKindCount = 7;
        private const int TanyaoMinRank = TileKind.MinRank + 1;
        private const int TanyaoMaxRank = TileKind.MaxNumberRank - 1;
        private const int TanyaoMaxSequenceStartRank = TileKind.MaxSequenceStartRank - 1;

        // 和了者は常に子として点数を計算する。役判定上の自風・場風はどちらも東に固定する
        // （東の刻子は常にダブ東として扱われる）。
        private const bool WinnerIsDealer = false;

        // チーするか・赤牌にするかの抽選に使う乱数の分解能（確率を0.1%刻みで扱う）
        private const int ProbabilityResolution = 1000;

        // 自風=場風=東固定のため、役牌になりうる刻子は三元牌3種と東のみ（南/西/北は役牌にならない）。
        private static readonly TileKind[] YakuhaiCandidateKinds =
        {
            new TileKind(TileSuit.Dragon, 1),
            new TileKind(TileSuit.Dragon, 2),
            new TileKind(TileSuit.Dragon, 3),
            TileKind.East,
        };

        private static readonly WinningHandArchetype[] TargetArchetypes =
        {
            WinningHandArchetype.Chiitoitsu,
            WinningHandArchetype.Tanyao,
            WinningHandArchetype.Yakuhai,
            WinningHandArchetype.Toitoi,
            WinningHandArchetype.Honitsu,
            WinningHandArchetype.Chinitsu,
        };

        // chiProbability: 順子を含みうるアーキタイプ（断幺九・役牌・混一色・清一色）で、
        // 順子の1つをチー（鳴いた面子）にする確率（0〜1）。0fなら常に門前で生成する
        // redDoraProbability: 手牌の中の赤牌になりうる牌（標準では数牌の5）を、1枚ごとに赤牌にする確率（0〜1）。
        // 牌構成（TileSetDefinition）の赤牌枚数を上限とする。0fなら赤牌を含めない
        public static GeneratedWinningHand Generate(
            MahjongRuleSet ruleSet,
            IRandomSource randomSource,
            float chiProbability = 0f,
            float redDoraProbability = 0f,
            int maxAttempts = DefaultMaxAttempts
        )
        {
            for (int attemptIndex = 0; attemptIndex < maxAttempts; attemptIndex++)
            {
                WinningHandArchetype archetype = TargetArchetypes[
                    randomSource.Next(TargetArchetypes.Length)
                ];

                bool shouldChi = RollProbability(chiProbability, randomSource);
                (List<Tile> concealedTiles, IReadOnlyList<Meld> builtMelds) = BuildHandTiles(
                    archetype,
                    ruleSet,
                    shouldChi,
                    randomSource
                );
                if (concealedTiles == null || concealedTiles.Count != RequiredHandTileCount)
                {
                    continue; // 牌のプールが偶然足りなかった場合。全体を作り直す。
                }

                // 鳴き（ポン等）があれば、その面子分の牌は手牌としての完成判定・役判定の対象から除く。
                // 赤牌は、Equalsが赤牌を区別しないRemoveMeldTilesで赤牌が意図せず入れ替わらないよう、取り除いた後に差し替える。
                (List<Tile> scoringConcealedTiles, IReadOnlyList<Meld> melds) = ApplyRedDora(
                    RemoveMeldTiles(concealedTiles, builtMelds),
                    builtMelds,
                    ruleSet,
                    redDoraProbability,
                    randomSource
                );
                List<Tile> allTiles = scoringConcealedTiles
                    .Concat(melds.SelectMany(meld => meld.Tiles))
                    .ToList();

                if (
                    !ShantenCalculator.IsCompleteHand(
                        scoringConcealedTiles,
                        calledMeldCount: melds.Count
                    )
                )
                {
                    continue; // 構築ロジックが正しければ常にtrueになるはずの防御的チェック。
                }

                // 和了牌は鳴いた面子以外（実際に手の内にある牌）からのみ選ぶ。
                Tile winningTile = scoringConcealedTiles[
                    randomSource.Next(scoringConcealedTiles.Count)
                ];
                // ツモ/ロンで刻子の暗刻扱いが変わる（例: 四暗刻はロンで刻子側を完成させると不成立になる）ため、
                // 常に片方に固定せずランダムに決める。
                bool isTsumo = randomSource.Next(2) == 0;
                var situation = new WinningSituation(
                    melds: melds,
                    winningTile: winningTile,
                    isTsumo: isTsumo,
                    seatWind: TileKind.East,
                    roundWind: TileKind.East
                );

                YakuEvaluationResult yakuResult = YakuEvaluator.EvaluateBest(
                    scoringConcealedTiles,
                    calledMeldCount: melds.Count,
                    situation,
                    ruleSet.EnabledYaku
                );
                if (!yakuResult.HasYaku)
                {
                    continue; // ruleSet.EnabledYakuで対象役が外れている場合のみ発生しうる防御的チェック。
                }

                int fu = FuCalculator.Calculate(yakuResult.Context, ruleSet.ScoringRule);
                // 赤ドラは役ではなく加算翻のため、役満のときは加算しない（RoundControllerと同じ扱い）
                int redDoraCount = yakuResult.HasYakuman
                    ? 0
                    : DoraCalculator.CountRedDora(allTiles);
                int yakumanMultiplier = yakuResult.HasYakuman ? yakuResult.TotalHan : 0;
                ScoreResult score = ScoreCalculator.Calculate(
                    yakuResult.TotalHan + redDoraCount,
                    fu,
                    WinnerIsDealer,
                    ruleSet.ScoringRule,
                    yakumanMultiplier
                );

                return new GeneratedWinningHand(
                    allTiles,
                    winningTile,
                    archetype,
                    situation,
                    yakuResult,
                    fu,
                    redDoraCount,
                    score
                );
            }

            throw new InvalidOperationException(
                $"和了手牌の生成に{maxAttempts}回失敗しました。TileSetDefinitionの牌数構成やEnabledYakuを確認してください。"
            );
        }

        // shouldChiは、順子を含みうるアーキタイプでのみ使う（七対子・対々和はチーと両立しないため無視する）。
        private static (List<Tile> Tiles, IReadOnlyList<Meld> Melds) BuildHandTiles(
            WinningHandArchetype archetype,
            MahjongRuleSet ruleSet,
            bool shouldChi,
            IRandomSource randomSource
        )
        {
            return archetype switch
            {
                WinningHandArchetype.Chiitoitsu => (
                    TryBuildChiitoitsuHand(ruleSet, randomSource),
                    Array.Empty<Meld>()
                ),
                WinningHandArchetype.Tanyao => TryBuildTanyaoHand(ruleSet, shouldChi, randomSource),
                WinningHandArchetype.Yakuhai => TryBuildYakuhaiHand(
                    ruleSet,
                    shouldChi,
                    randomSource
                ),
                WinningHandArchetype.Toitoi => TryBuildToitoiHand(ruleSet, randomSource),
                WinningHandArchetype.Honitsu => TryBuildHonitsuHand(
                    ruleSet,
                    shouldChi,
                    randomSource
                ),
                WinningHandArchetype.Chinitsu => TryBuildChinitsuHand(
                    ruleSet,
                    shouldChi,
                    randomSource
                ),
                _ => (null, Array.Empty<Meld>()),
            };
        }

        private static bool RollProbability(float probability, IRandomSource randomSource)
        {
            if (probability <= 0f)
            {
                return false; // 確率0のときは乱数を消費しない（既存の生成結果の再現性を保つため）
            }
            return randomSource.Next(ProbabilityResolution) < probability * ProbabilityResolution;
        }

        // 手の内の牌・鳴いた面子の牌それぞれについて、赤牌になりうる牌を確率で赤牌に差し替える。
        // 牌構成の赤牌枚数（TileSetEntry.RedDoraCount）を超えて赤牌にはしない。
        // 確率0のときは乱数を消費せず、そのまま返す（既存の生成結果の再現性を保つため）
        private static (List<Tile> ConcealedTiles, IReadOnlyList<Meld> Melds) ApplyRedDora(
            List<Tile> concealedTiles,
            IReadOnlyList<Meld> melds,
            MahjongRuleSet ruleSet,
            float redDoraProbability,
            IRandomSource randomSource
        )
        {
            if (redDoraProbability <= 0f)
            {
                return (concealedTiles, melds);
            }

            Dictionary<TileKind, int> remainingRedDoraCounts = ruleSet.TileSet.Entries.ToDictionary(
                entry => entry.Kind,
                entry => entry.RedDoraCount
            );

            Tile ToRedDoraByChance(Tile tile)
            {
                if (
                    !remainingRedDoraCounts.TryGetValue(tile.Kind, out int remainingCount)
                    || remainingCount <= 0
                    || !RollProbability(redDoraProbability, randomSource)
                )
                {
                    return tile;
                }
                remainingRedDoraCounts[tile.Kind] = remainingCount - 1;
                return new Tile(tile.Kind, isRedDora: true);
            }

            List<Tile> redDoraAppliedConcealedTiles = concealedTiles
                .Select(ToRedDoraByChance)
                .ToList();
            List<Meld> redDoraAppliedMelds = melds
                .Select(meld => new Meld(
                    meld.Type,
                    meld.Tiles.Select(ToRedDoraByChance).ToList(),
                    meld.CalledTile,
                    meld.SourceSeatIndex
                ))
                .ToList();
            return (redDoraAppliedConcealedTiles, redDoraAppliedMelds);
        }

        // 構築済みの面子から順子を1つランダムに選び、チーした面子として返す。順子が無ければ鳴きなし。
        // 和了形として組み上がった面子の1つを鳴いた扱いにするだけなので、残りの牌も必ず和了形のままになる。
        // 対象アーキタイプの役（断幺九・役牌・混一色・清一色）はいずれも鳴いても成立する（喰いタンあり）。
        private static IReadOnlyList<Meld> CreateChiMelds(
            List<List<TileKind>> meldKindGroups,
            bool shouldChi,
            IRandomSource randomSource
        )
        {
            if (!shouldChi)
            {
                return Array.Empty<Meld>();
            }

            List<List<TileKind>> sequenceKindGroups = meldKindGroups
                .Where(meldKinds => !meldKinds[0].Equals(meldKinds[1]))
                .ToList();
            if (sequenceKindGroups.Count == 0)
            {
                return Array.Empty<Meld>();
            }

            List<TileKind> chiKinds = sequenceKindGroups[
                randomSource.Next(sequenceKindGroups.Count)
            ];
            var chiMeld = new Meld(MeldType.Chi, chiKinds.Select(kind => new Tile(kind)));
            return new List<Meld> { chiMeld };
        }

        // 鳴き（ポン等）がある場合、その面子の構成牌を全体の牌リストから取り除き、
        // 手牌としての完成判定・役判定に渡す「面前部分のみ」の牌リストを作る。
        private static List<Tile> RemoveMeldTiles(List<Tile> allTiles, IReadOnlyList<Meld> melds)
        {
            if (melds.Count == 0)
            {
                return allTiles;
            }

            var remainingTiles = new List<Tile>(allTiles);
            foreach (Meld meld in melds)
            {
                foreach (Tile meldTile in meld.Tiles)
                {
                    remainingTiles.Remove(meldTile);
                }
            }
            return remainingTiles;
        }

        // --- アーキタイプ別の構築（構築できればその役の成立が確定する） ---

        private static List<Tile> TryBuildChiitoitsuHand(
            MahjongRuleSet ruleSet,
            IRandomSource randomSource
        )
        {
            Dictionary<TileKind, int> remainingCounts = BuildRemainingCounts(ruleSet);
            List<TileKind> shuffledKinds = ShuffleCopy(GetAllKinds(ruleSet), randomSource);

            var chosenPairKinds = new List<TileKind>();
            foreach (TileKind candidate in shuffledKinds)
            {
                if (chosenPairKinds.Count == ChiitoitsuPairKindCount)
                {
                    break;
                }
                if (remainingCounts[candidate] >= PairTileCount)
                {
                    remainingCounts[candidate] -= PairTileCount;
                    chosenPairKinds.Add(candidate);
                }
            }

            if (chosenPairKinds.Count != ChiitoitsuPairKindCount)
            {
                return null;
            }

            var tiles = new List<Tile>();
            foreach (TileKind kind in chosenPairKinds)
            {
                tiles.Add(new Tile(kind));
                tiles.Add(new Tile(kind));
            }
            return tiles;
        }

        private static (List<Tile> Tiles, IReadOnlyList<Meld> Melds) TryBuildTanyaoHand(
            MahjongRuleSet ruleSet,
            bool shouldChi,
            IRandomSource randomSource
        )
        {
            Dictionary<TileKind, int> remainingCounts = BuildRemainingCounts(ruleSet);
            List<TileKind> candidateKinds = TileKind
                .NumberSuits.SelectMany(suit =>
                    GetNumberKindsInRankRange(suit, TanyaoMinRank, TanyaoMaxRank)
                )
                .ToList();
            List<(TileSuit Suit, int StartRank)> sequenceStarts = GetSequenceStartCandidates(
                TileKind.NumberSuits,
                TanyaoMinRank,
                TanyaoMaxSequenceStartRank
            );

            bool success = TryBuildPairAndMelds(
                remainingCounts,
                candidateKinds,
                candidateKinds,
                sequenceStarts,
                allowSequence: true,
                meldCount: MeldCountInRegularHand,
                randomSource,
                out TileKind pairKind,
                out List<List<TileKind>> meldKindGroups
            );
            if (!success)
            {
                return (null, Array.Empty<Meld>());
            }

            return (
                ToTiles(pairKind, meldKindGroups),
                CreateChiMelds(meldKindGroups, shouldChi, randomSource)
            );
        }

        private static (List<Tile> Tiles, IReadOnlyList<Meld> Melds) TryBuildYakuhaiHand(
            MahjongRuleSet ruleSet,
            bool shouldChi,
            IRandomSource randomSource
        )
        {
            Dictionary<TileKind, int> remainingCounts = BuildRemainingCounts(ruleSet);
            if (
                !TryPickAndConsumeSingleKind(
                    remainingCounts,
                    YakuhaiCandidateKinds,
                    TripletTileCount,
                    randomSource,
                    out TileKind yakuhaiKind
                )
            )
            {
                return (null, Array.Empty<Meld>());
            }

            List<TileKind> allKinds = GetAllKinds(ruleSet);
            List<(TileSuit Suit, int StartRank)> sequenceStarts = GetSequenceStartCandidates(
                TileKind.NumberSuits,
                TileKind.MinRank,
                TileKind.MaxSequenceStartRank
            );

            bool success = TryBuildPairAndMelds(
                remainingCounts,
                allKinds,
                allKinds,
                sequenceStarts,
                allowSequence: true,
                meldCount: MeldCountInRegularHand - 1,
                randomSource,
                out TileKind pairKind,
                out List<List<TileKind>> otherMeldKindGroups
            );
            if (!success)
            {
                return (null, Array.Empty<Meld>());
            }

            List<Tile> tiles = ToTiles(pairKind, otherMeldKindGroups);
            tiles.Add(new Tile(yakuhaiKind));
            tiles.Add(new Tile(yakuhaiKind));
            tiles.Add(new Tile(yakuhaiKind));
            return (tiles, CreateChiMelds(otherMeldKindGroups, shouldChi, randomSource));
        }

        private static (List<Tile> Tiles, IReadOnlyList<Meld> Melds) TryBuildToitoiHand(
            MahjongRuleSet ruleSet,
            IRandomSource randomSource
        )
        {
            Dictionary<TileKind, int> remainingCounts = BuildRemainingCounts(ruleSet);
            List<TileKind> allKinds = GetAllKinds(ruleSet);

            var tripletKinds = new List<TileKind>();
            for (int meldIndex = 0; meldIndex < MeldCountInRegularHand; meldIndex++)
            {
                if (
                    !TryPickAndConsumeSingleKind(
                        remainingCounts,
                        allKinds,
                        TripletTileCount,
                        randomSource,
                        out TileKind tripletKind
                    )
                )
                {
                    return (null, Array.Empty<Meld>());
                }
                tripletKinds.Add(tripletKind);
            }

            if (
                !TryPickAndConsumeSingleKind(
                    remainingCounts,
                    allKinds,
                    PairTileCount,
                    randomSource,
                    out TileKind pairKind
                )
            )
            {
                return (null, Array.Empty<Meld>());
            }

            var tiles = new List<Tile> { new Tile(pairKind), new Tile(pairKind) };
            foreach (TileKind tripletKind in tripletKinds)
            {
                tiles.Add(new Tile(tripletKind));
                tiles.Add(new Tile(tripletKind));
                tiles.Add(new Tile(tripletKind));
            }

            // 4つの刻子を全部暗刻のままにすると、和了牌の選ばれ方次第で四暗刻（役満）が偶発的に
            // 成立してしまう。対々和として安定して出現させるため、1つは必ずポン（明刻）にする。
            TileKind ponnedTripletKind = tripletKinds[0];
            var toitoiMeld = new Meld(
                MeldType.Pon,
                new[]
                {
                    new Tile(ponnedTripletKind),
                    new Tile(ponnedTripletKind),
                    new Tile(ponnedTripletKind),
                }
            );

            return (tiles, new List<Meld> { toitoiMeld });
        }

        private static (List<Tile> Tiles, IReadOnlyList<Meld> Melds) TryBuildHonitsuHand(
            MahjongRuleSet ruleSet,
            bool shouldChi,
            IRandomSource randomSource
        )
        {
            Dictionary<TileKind, int> remainingCounts = BuildRemainingCounts(ruleSet);
            TileSuit chosenSuit = TileKind.NumberSuits[
                randomSource.Next(TileKind.NumberSuits.Count)
            ];

            // 混一色の成立には字牌が最低1つ必要（無いと清一色側の判定に流れてしまう）。
            List<TileKind> honorKinds = GetAllKinds(ruleSet)
                .Where(kind => !kind.IsNumberTile)
                .ToList();
            if (
                !TryPickAndConsumeSingleKind(
                    remainingCounts,
                    honorKinds,
                    TripletTileCount,
                    randomSource,
                    out TileKind honorTripletKind
                )
            )
            {
                return (null, Array.Empty<Meld>());
            }

            List<TileKind> suitKinds = GetNumberKindsInRankRange(
                chosenSuit,
                TileKind.MinRank,
                TileKind.MaxNumberRank
            );
            List<(TileSuit Suit, int StartRank)> sequenceStarts = GetSequenceStartCandidates(
                new[] { chosenSuit },
                TileKind.MinRank,
                TileKind.MaxSequenceStartRank
            );

            bool success = TryBuildPairAndMelds(
                remainingCounts,
                suitKinds,
                suitKinds,
                sequenceStarts,
                allowSequence: true,
                meldCount: MeldCountInRegularHand - 1,
                randomSource,
                out TileKind pairKind,
                out List<List<TileKind>> otherMeldKindGroups
            );
            if (!success)
            {
                return (null, Array.Empty<Meld>());
            }

            List<Tile> tiles = ToTiles(pairKind, otherMeldKindGroups);
            tiles.Add(new Tile(honorTripletKind));
            tiles.Add(new Tile(honorTripletKind));
            tiles.Add(new Tile(honorTripletKind));
            return (tiles, CreateChiMelds(otherMeldKindGroups, shouldChi, randomSource));
        }

        private static (List<Tile> Tiles, IReadOnlyList<Meld> Melds) TryBuildChinitsuHand(
            MahjongRuleSet ruleSet,
            bool shouldChi,
            IRandomSource randomSource
        )
        {
            Dictionary<TileKind, int> remainingCounts = BuildRemainingCounts(ruleSet);
            TileSuit chosenSuit = TileKind.NumberSuits[
                randomSource.Next(TileKind.NumberSuits.Count)
            ];

            List<TileKind> suitKinds = GetNumberKindsInRankRange(
                chosenSuit,
                TileKind.MinRank,
                TileKind.MaxNumberRank
            );
            List<(TileSuit Suit, int StartRank)> sequenceStarts = GetSequenceStartCandidates(
                new[] { chosenSuit },
                TileKind.MinRank,
                TileKind.MaxSequenceStartRank
            );

            bool success = TryBuildPairAndMelds(
                remainingCounts,
                suitKinds,
                suitKinds,
                sequenceStarts,
                allowSequence: true,
                meldCount: MeldCountInRegularHand,
                randomSource,
                out TileKind pairKind,
                out List<List<TileKind>> meldKindGroups
            );
            if (!success)
            {
                return (null, Array.Empty<Meld>());
            }

            return (
                ToTiles(pairKind, meldKindGroups),
                CreateChiMelds(meldKindGroups, shouldChi, randomSource)
            );
        }

        // --- 通常形の共通部品（雀頭1つ + 面子N個。刻子/順子はここで牌の消費と整合性を管理する） ---

        private static bool TryBuildPairAndMelds(
            Dictionary<TileKind, int> remainingCounts,
            IReadOnlyList<TileKind> pairCandidateKinds,
            IReadOnlyList<TileKind> tripletCandidateKinds,
            IReadOnlyList<(TileSuit Suit, int StartRank)> sequenceCandidateStarts,
            bool allowSequence,
            int meldCount,
            IRandomSource randomSource,
            out TileKind pairKind,
            out List<List<TileKind>> meldKindGroups
        )
        {
            meldKindGroups = new List<List<TileKind>>();

            if (
                !TryPickAndConsumeSingleKind(
                    remainingCounts,
                    pairCandidateKinds,
                    PairTileCount,
                    randomSource,
                    out pairKind
                )
            )
            {
                return false;
            }

            for (int meldIndex = 0; meldIndex < meldCount; meldIndex++)
            {
                if (
                    !TryBuildMeld(
                        remainingCounts,
                        tripletCandidateKinds,
                        sequenceCandidateStarts,
                        allowSequence,
                        randomSource,
                        out List<TileKind> meldKinds
                    )
                )
                {
                    return false;
                }
                meldKindGroups.Add(meldKinds);
            }
            return true;
        }

        private static bool TryBuildMeld(
            Dictionary<TileKind, int> remainingCounts,
            IReadOnlyList<TileKind> tripletCandidateKinds,
            IReadOnlyList<(TileSuit Suit, int StartRank)> sequenceCandidateStarts,
            bool allowSequence,
            IRandomSource randomSource,
            out List<TileKind> meldKinds
        )
        {
            bool preferSequence =
                allowSequence && sequenceCandidateStarts.Count > 0 && randomSource.Next(2) == 0;

            if (
                preferSequence
                && TryPickAndConsumeSequence(
                    remainingCounts,
                    sequenceCandidateStarts,
                    randomSource,
                    out meldKinds
                )
            )
            {
                return true;
            }

            if (
                TryPickAndConsumeSingleKind(
                    remainingCounts,
                    tripletCandidateKinds,
                    TripletTileCount,
                    randomSource,
                    out TileKind tripletKind
                )
            )
            {
                meldKinds = new List<TileKind> { tripletKind, tripletKind, tripletKind };
                return true;
            }

            // 刻子が引けなかった場合の最後の手段として順子側も試す（まだ試していない場合のみ）。
            if (
                allowSequence
                && !preferSequence
                && TryPickAndConsumeSequence(
                    remainingCounts,
                    sequenceCandidateStarts,
                    randomSource,
                    out meldKinds
                )
            )
            {
                return true;
            }

            meldKinds = null;
            return false;
        }

        // --- 牌のプール管理（残り枚数の消費）を行う最下層のプリミティブ ---

        private static bool TryPickAndConsumeSingleKind(
            Dictionary<TileKind, int> remainingCounts,
            IReadOnlyList<TileKind> candidateKinds,
            int copiesNeeded,
            IRandomSource randomSource,
            out TileKind chosenKind
        )
        {
            List<TileKind> shuffledCandidates = ShuffleCopy(candidateKinds, randomSource);
            foreach (TileKind candidate in shuffledCandidates)
            {
                if (
                    remainingCounts.TryGetValue(candidate, out int available)
                    && available >= copiesNeeded
                )
                {
                    remainingCounts[candidate] = available - copiesNeeded;
                    chosenKind = candidate;
                    return true;
                }
            }
            chosenKind = default;
            return false;
        }

        private static bool TryPickAndConsumeSequence(
            Dictionary<TileKind, int> remainingCounts,
            IReadOnlyList<(TileSuit Suit, int StartRank)> candidateStarts,
            IRandomSource randomSource,
            out List<TileKind> chosenKinds
        )
        {
            List<(TileSuit Suit, int StartRank)> shuffledCandidates = ShuffleCopy(
                candidateStarts,
                randomSource
            );
            foreach ((TileSuit suit, int startRank) in shuffledCandidates)
            {
                var kinds = new List<TileKind>
                {
                    new TileKind(suit, startRank),
                    new TileKind(suit, startRank + 1),
                    new TileKind(suit, startRank + 2),
                };
                bool allAvailable = kinds.All(kind =>
                    remainingCounts.TryGetValue(kind, out int available) && available >= 1
                );
                if (!allAvailable)
                {
                    continue;
                }
                foreach (TileKind kind in kinds)
                {
                    remainingCounts[kind] -= 1;
                }
                chosenKinds = kinds;
                return true;
            }
            chosenKinds = null;
            return false;
        }

        // Fisher-Yatesシャッフル。候補の中から「使える1つ」を探す走査順をランダム化するために使う
        // （先頭からrandom.Next()で当たりを引き直す方式は、外れを引き続けた時の挙動が読みにくいため）。
        private static List<T> ShuffleCopy<T>(IReadOnlyList<T> source, IRandomSource randomSource)
        {
            var shuffled = new List<T>(source);
            for (int index = shuffled.Count - 1; index > 0; index--)
            {
                int swapIndex = randomSource.Next(index + 1);
                (shuffled[index], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[index]);
            }
            return shuffled;
        }

        private static Dictionary<TileKind, int> BuildRemainingCounts(MahjongRuleSet ruleSet)
        {
            var remainingCounts = new Dictionary<TileKind, int>();
            foreach (TileSetEntry entry in ruleSet.TileSet.Entries)
            {
                remainingCounts[entry.Kind] = entry.Count;
            }
            return remainingCounts;
        }

        private static List<TileKind> GetAllKinds(MahjongRuleSet ruleSet)
        {
            return ruleSet.TileSet.Entries.Select(entry => entry.Kind).ToList();
        }

        private static List<TileKind> GetNumberKindsInRankRange(
            TileSuit suit,
            int minRank,
            int maxRank
        )
        {
            var kinds = new List<TileKind>();
            for (int rank = minRank; rank <= maxRank; rank++)
            {
                kinds.Add(new TileKind(suit, rank));
            }
            return kinds;
        }

        private static List<(TileSuit Suit, int StartRank)> GetSequenceStartCandidates(
            IEnumerable<TileSuit> suits,
            int minStartRank,
            int maxStartRank
        )
        {
            var candidates = new List<(TileSuit Suit, int StartRank)>();
            foreach (TileSuit suit in suits)
            {
                for (int startRank = minStartRank; startRank <= maxStartRank; startRank++)
                {
                    candidates.Add((suit, startRank));
                }
            }
            return candidates;
        }

        private static List<Tile> ToTiles(TileKind pairKind, List<List<TileKind>> meldKindGroups)
        {
            var tiles = new List<Tile> { new Tile(pairKind), new Tile(pairKind) };
            foreach (List<TileKind> meldKinds in meldKindGroups)
            {
                foreach (TileKind kind in meldKinds)
                {
                    tiles.Add(new Tile(kind));
                }
            }
            return tiles;
        }
    }
}
