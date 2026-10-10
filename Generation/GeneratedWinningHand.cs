using System.Collections.Generic;
using Zizo.Mahjong.Scoring;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;

namespace Zizo.Mahjong.Generation
{
    // ランダム生成された、役の確定した14枚の和了手牌1つ分。
    // AllTilesは鳴いた面子の牌も含む14枚全体（赤牌の有無も含めて手の内・鳴いた面子と一致する）。
    // 手の内の牌だけが必要な場合はYakuResult.Context.ConcealedTiles、鳴いた面子はSituation.Meldsを参照する。
    public class GeneratedWinningHand
    {
        public IReadOnlyList<Tile> AllTiles { get; }
        public Tile WinningTile { get; }
        public WinningHandArchetype Archetype { get; }
        public WinningSituation Situation { get; }
        public YakuEvaluationResult YakuResult { get; }
        public int Fu { get; }

        // 点数計算で加算した赤ドラの枚数（役満のときは加算しないため0）
        public int RedDoraCount { get; }
        public ScoreResult Score { get; }

        public GeneratedWinningHand(
            IReadOnlyList<Tile> allTiles,
            Tile winningTile,
            WinningHandArchetype archetype,
            WinningSituation situation,
            YakuEvaluationResult yakuResult,
            int fu,
            int redDoraCount,
            ScoreResult score
        )
        {
            AllTiles = allTiles;
            WinningTile = winningTile;
            Archetype = archetype;
            Situation = situation;
            YakuResult = yakuResult;
            Fu = fu;
            RedDoraCount = redDoraCount;
            Score = score;
        }
    }
}
