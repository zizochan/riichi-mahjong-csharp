using System;

namespace Zizo.Mahjong.Tiles
{
    public readonly struct Tile : IEquatable<Tile>
    {
        public TileKind Kind { get; }
        public bool IsRedDora { get; }

        public TileSuit Suit => Kind.Suit;
        public int Rank => Kind.Rank;

        public Tile(TileKind kind, bool isRedDora = false)
        {
            Kind = kind;
            IsRedDora = isRedDora;
        }

        public Tile(TileSuit suit, int rank, bool isRedDora = false)
            : this(new TileKind(suit, rank), isRedDora) { }

        // 赤牌かどうかは点数計算にのみ関わるため、手牌の形を揃える上での同一判定（Equals）には含めない。
        public bool Equals(Tile other)
        {
            return Kind.Equals(other.Kind);
        }

        public override bool Equals(object obj)
        {
            return obj is Tile other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Kind.GetHashCode();
        }

        public override string ToString()
        {
            return IsRedDora ? $"{Kind}(赤)" : Kind.ToString();
        }
    }
}
