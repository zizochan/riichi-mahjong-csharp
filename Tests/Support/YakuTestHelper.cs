using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;
using Zizo.Mahjong.Yaku.Standard;

namespace Zizo.Mahjong.Tests
{
    // 役判定テスト用の共通ヘルパー。手牌記法と和了状況から YakuEvaluator.EvaluateBest を呼び、
    // 特定の役が(期待する翻数で)成立しているかを検証する。
    public static class YakuTestHelper
    {
        public static readonly TileKind East = new TileKind(TileSuit.Wind, 1);
        public static readonly TileKind South = new TileKind(TileSuit.Wind, 2);

        public static readonly IReadOnlyList<IYaku> EnabledYaku = StandardYakuSet.CreateAll();

        // 鳴き面子を除いた手の内の牌（和了牌を含む）と、和了状況を指定して役を判定する。
        // 状況のフラグは WinningSituation のコンストラクタ引数と同じ意味を持つ。
        public static YakuEvaluationResult Evaluate(
            string concealedHandNotation,
            string winningTileNotation,
            bool isTsumo = false,
            IReadOnlyList<Meld> melds = null,
            TileKind? seatWind = null,
            TileKind? roundWind = null,
            bool isRiichi = false,
            bool isDoubleRiichi = false,
            bool isIppatsu = false,
            bool isHaitei = false,
            bool isHoutei = false,
            bool isRinshan = false,
            bool isChankan = false,
            bool isFirstUninterruptedTurnWin = false
        )
        {
            IReadOnlyList<Meld> actualMelds = melds ?? new List<Meld>();
            var situation = new WinningSituation(
                melds: actualMelds,
                winningTile: HandNotation.Single(winningTileNotation),
                isTsumo: isTsumo,
                seatWind: seatWind ?? East,
                roundWind: roundWind ?? East,
                isRiichi: isRiichi,
                isDoubleRiichi: isDoubleRiichi,
                isIppatsu: isIppatsu,
                isHaitei: isHaitei,
                isHoutei: isHoutei,
                isRinshan: isRinshan,
                isChankan: isChankan,
                isFirstUninterruptedTurnWin: isFirstUninterruptedTurnWin
            );
            return YakuEvaluator.EvaluateBest(
                HandNotation.Parse(concealedHandNotation),
                actualMelds.Count,
                situation,
                EnabledYaku
            );
        }

        public static void AssertYakuMatched(
            YakuEvaluationResult result,
            string yakuName,
            int expectedHan
        )
        {
            Assert.IsTrue(result.HasYaku, "役が1つも成立していません。");
            (IYaku Yaku, int Han) matched = result.MatchedYaku.FirstOrDefault(entry =>
                entry.Yaku.Name == yakuName
            );
            Assert.IsNotNull(matched.Yaku, $"「{yakuName}」が成立していません。");
            Assert.AreEqual(expectedHan, matched.Han, $"「{yakuName}」の翻数が想定と異なります。");
        }

        public static void AssertYakuNotMatched(YakuEvaluationResult result, string yakuName)
        {
            Assert.IsFalse(
                result.MatchedYaku.Any(entry => entry.Yaku.Name == yakuName),
                $"「{yakuName}」が成立してはいけません。"
            );
        }
    }
}
