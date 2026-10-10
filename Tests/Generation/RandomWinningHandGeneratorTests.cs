using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Generation;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    // ランダム生成のため、シードを固定した乱数で複数回生成し、全ての手が満たすべき性質を検証する。
    public class RandomWinningHandGeneratorTests
    {
        private const int RandomSeed = 12345;
        private const int GenerateCount = 200;
        private const int RequiredHandTileCount = 14;
        private const int ChiTileCount = 3;

        // 標準牌構成では赤牌は各数牌の5に1枚ずつ
        private const int RedDoraRank = 5;
        private const int MaxRedDoraCountPerKind = 1;

        private static readonly WinningHandArchetype[] ChiAllowedArchetypes =
        {
            WinningHandArchetype.Tanyao,
            WinningHandArchetype.Yakuhai,
            WinningHandArchetype.Honitsu,
            WinningHandArchetype.Chinitsu,
        };

        // アーキタイプごとに、構築時点で成立が確定しているはずの役。
        private static readonly Dictionary<WinningHandArchetype, string> TargetYakuNames =
            new Dictionary<WinningHandArchetype, string>
            {
                [WinningHandArchetype.Chiitoitsu] = "七対子",
                [WinningHandArchetype.Tanyao] = "断幺九",
                [WinningHandArchetype.Yakuhai] = "役牌",
                [WinningHandArchetype.Toitoi] = "対々和",
                [WinningHandArchetype.Honitsu] = "混一色",
                [WinningHandArchetype.Chinitsu] = "清一色",
            };

        private static List<GeneratedWinningHand> GenerateHands(
            float chiProbability,
            float redDoraProbability = 0f
        )
        {
            MahjongRuleSet ruleSet = MahjongRuleSet.CreateStandardFourPlayer();
            var randomSource = new DefaultRandomSource(RandomSeed);
            var hands = new List<GeneratedWinningHand>();
            for (int generateIndex = 0; generateIndex < GenerateCount; generateIndex++)
            {
                hands.Add(
                    RandomWinningHandGenerator.Generate(
                        ruleSet,
                        randomSource,
                        chiProbability,
                        redDoraProbability
                    )
                );
            }
            return hands;
        }

        private static bool HasChi(GeneratedWinningHand hand)
        {
            return hand.Situation.Melds.Any(meld => meld.Type == MeldType.Chi);
        }

        [Test]
        public void Generate_ChiProbabilityZero_NeverContainsChi()
        {
            List<GeneratedWinningHand> hands = GenerateHands(chiProbability: 0f);

            Assert.IsFalse(hands.Any(HasChi));
        }

        [Test]
        public void Generate_ChiProbabilityOne_ContainsChiHands()
        {
            List<GeneratedWinningHand> hands = GenerateHands(chiProbability: 1f);

            Assert.IsTrue(hands.Any(HasChi));
        }

        [Test]
        public void Generate_ChiHand_IsValidOpenWinningHand()
        {
            List<GeneratedWinningHand> chiHands = GenerateHands(chiProbability: 1f)
                .Where(HasChi)
                .ToList();

            foreach (GeneratedWinningHand hand in chiHands)
            {
                CollectionAssert.Contains(ChiAllowedArchetypes, hand.Archetype);
                Assert.AreEqual(1, hand.Situation.Melds.Count);
                Assert.AreEqual(RequiredHandTileCount, hand.AllTiles.Count);
                Assert.AreEqual(
                    RequiredHandTileCount - ChiTileCount,
                    hand.YakuResult.Context.ConcealedTiles.Count
                );
                Assert.IsTrue(hand.YakuResult.HasYaku);
                Assert.IsFalse(hand.YakuResult.Context.IsMenzen);

                // チーは同じスートの連続した3枚で構成される
                List<Tile> chiTiles = hand
                    .Situation.Melds[0]
                    .Tiles.OrderBy(tile => tile.Rank)
                    .ToList();
                Assert.AreEqual(ChiTileCount, chiTiles.Count);
                Assert.IsTrue(chiTiles.All(tile => tile.Kind.IsNumberTile));
                Assert.IsTrue(chiTiles.All(tile => tile.Suit == chiTiles[0].Suit));
                Assert.AreEqual(chiTiles[0].Rank + 1, chiTiles[1].Rank);
                Assert.AreEqual(chiTiles[0].Rank + 2, chiTiles[2].Rank);

                // 和了牌は鳴いた面子ではなく手の内の牌から選ばれる
                CollectionAssert.Contains(hand.YakuResult.Context.ConcealedTiles, hand.WinningTile);
            }
        }

        [Test]
        public void Generate_RedDoraProbabilityZero_NeverContainsRedDora()
        {
            List<GeneratedWinningHand> hands = GenerateHands(
                chiProbability: 1f,
                redDoraProbability: 0f
            );

            Assert.IsFalse(hands.Any(hand => hand.AllTiles.Any(tile => tile.IsRedDora)));
            Assert.IsTrue(hands.All(hand => hand.RedDoraCount == 0));
        }

        [Test]
        public void Generate_RedDoraProbabilityOne_ContainsRedDoraHands()
        {
            List<GeneratedWinningHand> hands = GenerateHands(
                chiProbability: 1f,
                redDoraProbability: 1f
            );

            Assert.IsTrue(hands.Any(hand => hand.RedDoraCount > 0));
        }

        [Test]
        public void Generate_RedDoraHand_IsConsistentWithTilesAndScore()
        {
            List<GeneratedWinningHand> hands = GenerateHands(
                chiProbability: 1f,
                redDoraProbability: 1f
            );

            foreach (GeneratedWinningHand hand in hands)
            {
                List<Tile> redDoraTiles = hand.AllTiles.Where(tile => tile.IsRedDora).ToList();

                // 赤牌は数牌の5のみで、牌構成の枚数（各1枚）を超えない
                Assert.IsTrue(redDoraTiles.All(tile => tile.Kind.IsNumberTile));
                Assert.IsTrue(redDoraTiles.All(tile => tile.Rank == RedDoraRank));
                Assert.IsTrue(
                    redDoraTiles
                        .GroupBy(tile => tile.Kind)
                        .All(group => group.Count() <= MaxRedDoraCountPerKind)
                );

                // 14枚全体の赤牌は、手の内の牌と鳴いた面子の牌の赤牌と一致する
                int concealedRedDoraCount = hand.YakuResult.Context.ConcealedTiles.Count(tile =>
                    tile.IsRedDora
                );
                int meldRedDoraCount = hand
                    .Situation.Melds.SelectMany(meld => meld.Tiles)
                    .Count(tile => tile.IsRedDora);
                Assert.AreEqual(redDoraTiles.Count, concealedRedDoraCount + meldRedDoraCount);

                // 役満以外は赤ドラが翻数に加算され、役満では加算されない
                int expectedRedDoraCount = hand.YakuResult.HasYakuman ? 0 : redDoraTiles.Count;
                Assert.AreEqual(expectedRedDoraCount, hand.RedDoraCount);
                if (!hand.YakuResult.HasYakuman)
                {
                    Assert.AreEqual(hand.YakuResult.TotalHan + hand.RedDoraCount, hand.Score.Han);
                }
            }
        }

        [Test]
        public void Generate_SameSeed_ProducesSameHand()
        {
            MahjongRuleSet ruleSet = MahjongRuleSet.CreateStandardFourPlayer();

            GeneratedWinningHand first = RandomWinningHandGenerator.Generate(
                ruleSet,
                new DefaultRandomSource(RandomSeed),
                chiProbability: 0.5f,
                redDoraProbability: 0.5f
            );
            GeneratedWinningHand second = RandomWinningHandGenerator.Generate(
                ruleSet,
                new DefaultRandomSource(RandomSeed),
                chiProbability: 0.5f,
                redDoraProbability: 0.5f
            );

            CollectionAssert.AreEqual(
                first.AllTiles.Select(tile => (tile.Kind, tile.IsRedDora)),
                second.AllTiles.Select(tile => (tile.Kind, tile.IsRedDora))
            );
            Assert.AreEqual(first.WinningTile, second.WinningTile);
            Assert.AreEqual(first.Score.RonPayment, second.Score.RonPayment);
        }

        [Test]
        public void Generate_ManyHands_CoversEveryArchetypeWithItsTargetYaku()
        {
            List<GeneratedWinningHand> hands = GenerateHands(chiProbability: 0.3f);

            foreach (
                WinningHandArchetype archetype in (WinningHandArchetype[])
                    Enum.GetValues(typeof(WinningHandArchetype))
            )
            {
                Assert.IsTrue(
                    hands.Any(hand => hand.Archetype == archetype),
                    $"{archetype}の手が1つも生成されていません。"
                );
            }

            foreach (GeneratedWinningHand hand in hands)
            {
                string targetYakuName = TargetYakuNames[hand.Archetype];
                Assert.IsTrue(
                    hand.YakuResult.HasYakuman
                        || hand.YakuResult.MatchedYaku.Any(entry =>
                            entry.Yaku.Name == targetYakuName
                        ),
                    $"{hand.Archetype}として生成した手に「{targetYakuName}」が成立していません。"
                );
            }
        }
    }
}
