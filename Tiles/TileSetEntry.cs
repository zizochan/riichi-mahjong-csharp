using System;

namespace Zizo.Mahjong.Tiles
{
    public class TileSetEntry
    {
        public TileKind Kind { get; }
        public int Count { get; }
        public int RedDoraCount { get; }

        public TileSetEntry(TileKind kind, int count, int redDoraCount = 0)
        {
            if (!kind.IsValid)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(kind),
                    kind,
                    "牌の種類の数字がスートの有効範囲外です。"
                );
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    count,
                    "枚数は0以上で指定してください。"
                );
            }
            if (redDoraCount < 0 || redDoraCount > count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(redDoraCount),
                    redDoraCount,
                    "赤ドラの枚数は0以上かつ枚数(count)以下で指定してください。"
                );
            }

            Kind = kind;
            Count = count;
            RedDoraCount = redDoraCount;
        }
    }
}
