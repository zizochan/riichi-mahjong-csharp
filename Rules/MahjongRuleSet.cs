using System;
using System.Collections.Generic;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;
using Zizo.Mahjong.Yaku.Standard;

namespace Zizo.Mahjong.Rules
{
    // ルール全体の可変ポイントを束ねる設定。人数・山構成・役リストを差し替えることで
    // 三人麻雀や特殊な牌構成、役の追加/削除に対応できるようにする。
    public class MahjongRuleSet
    {
        public int PlayerCount { get; }
        public TileSetDefinition TileSet { get; }
        public IReadOnlyList<IYaku> EnabledYaku { get; }
        public WallRule WallRule { get; }
        public ScoringRule ScoringRule { get; }

        // 座席風（東南西北）を割り当てられる人数の範囲。
        public const int MinPlayerCount = 2;
        public const int MaxPlayerCount = 4;

        public MahjongRuleSet(
            int playerCount,
            TileSetDefinition tileSet,
            IReadOnlyList<IYaku> enabledYaku,
            WallRule wallRule,
            ScoringRule scoringRule
        )
        {
            if (playerCount < MinPlayerCount || playerCount > MaxPlayerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(playerCount),
                    playerCount,
                    $"人数は{MinPlayerCount}〜{MaxPlayerCount}人で指定してください。"
                );
            }

            PlayerCount = playerCount;
            TileSet = tileSet ?? throw new ArgumentNullException(nameof(tileSet));
            EnabledYaku = enabledYaku ?? throw new ArgumentNullException(nameof(enabledYaku));
            WallRule = wallRule ?? throw new ArgumentNullException(nameof(wallRule));
            ScoringRule = scoringRule ?? throw new ArgumentNullException(nameof(scoringRule));
        }

        public static MahjongRuleSet CreateStandardFourPlayer()
        {
            return new MahjongRuleSet(
                playerCount: 4,
                tileSet: TileSetDefinition.CreateStandard(),
                enabledYaku: StandardYakuSet.CreateAll(),
                wallRule: new WallRule(),
                scoringRule: new ScoringRule()
            );
        }
    }
}
