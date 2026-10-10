using System;
using System.Collections.Generic;

namespace Zizo.Mahjong.Tiles
{
    // Wind の Rank は 1=東 2=南 3=西 4=北、Dragon の Rank は 1=白 2=發 3=中 を表す。
    public readonly struct TileKind : IEquatable<TileKind>
    {
        public const int MinRank = 1;
        public const int MaxNumberRank = 9;
        public const int MaxWindRank = 4;
        public const int MaxDragonRank = 3;

        // 順子（連続する3つの数字）の最小の数字として取りうる最大値（789の7）。
        public const int MaxSequenceStartRank = MaxNumberRank - 2;

        public static IReadOnlyList<TileSuit> NumberSuits { get; } =
            new[] { TileSuit.Man, TileSuit.Pin, TileSuit.Sou };

        // 東。親の自風・東場の場風になる。
        public static TileKind East { get; } = new TileKind(TileSuit.Wind, MinRank);

        public TileSuit Suit { get; }
        public int Rank { get; }

        // 頻繁に生成される値型のため、Rankの範囲チェックは行わない（IsValidで確認できる）。
        public TileKind(TileSuit suit, int rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public bool IsNumberTile => Suit is TileSuit.Man or TileSuit.Pin or TileSuit.Sou;

        // 字牌（風牌・三元牌）。
        public bool IsHonor => !IsNumberTile;

        // 老頭牌（数牌の1・9）。
        public bool IsTerminal => IsNumberTile && (Rank == MinRank || Rank == MaxNumberRank);

        // 幺九牌（老頭牌+字牌）。
        public bool IsTerminalOrHonor => IsTerminal || IsHonor;

        // Rankがスートごとの有効範囲（数牌1〜9、風牌1〜4、三元牌1〜3）に収まっているか。
        public bool IsValid => Rank >= MinRank && Rank <= GetMaxRank(Suit);

        public static int GetMaxRank(TileSuit suit)
        {
            return suit switch
            {
                TileSuit.Wind => MaxWindRank,
                TileSuit.Dragon => MaxDragonRank,
                _ => MaxNumberRank,
            };
        }

        public bool Equals(TileKind other)
        {
            return Suit == other.Suit && Rank == other.Rank;
        }

        public override bool Equals(object obj)
        {
            return obj is TileKind other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Suit, Rank);
        }

        public override string ToString()
        {
            return $"{Suit}{Rank}";
        }
    }
}
