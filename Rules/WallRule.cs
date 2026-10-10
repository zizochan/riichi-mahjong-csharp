using System;

namespace Zizo.Mahjong.Rules
{
    // 王牌（ワンパイ）まわりのルール。王牌は先頭から「嶺上牌 → 裏ドラ表示牌」、末尾から「ドラ表示牌」の順に使う。
    public class WallRule
    {
        public int DeadWallTileCount { get; }
        public int InitialDoraIndicatorCount { get; }
        public int MaxKanCount { get; }

        public WallRule(
            int deadWallTileCount = 14,
            int initialDoraIndicatorCount = 1,
            int maxKanCount = 4
        )
        {
            if (initialDoraIndicatorCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDoraIndicatorCount));
            }
            if (maxKanCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxKanCount));
            }

            // 嶺上牌(MaxKanCount枚) + 表ドラ表示牌・裏ドラ表示牌(それぞれ最大 初期枚数+MaxKanCount 枚)が収まる必要がある。
            int requiredDeadWallTileCount =
                maxKanCount + (2 * (initialDoraIndicatorCount + maxKanCount));
            if (deadWallTileCount < requiredDeadWallTileCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deadWallTileCount),
                    deadWallTileCount,
                    $"王牌は少なくとも{requiredDeadWallTileCount}枚必要です。"
                );
            }

            DeadWallTileCount = deadWallTileCount;
            InitialDoraIndicatorCount = initialDoraIndicatorCount;
            MaxKanCount = maxKanCount;
        }
    }
}
