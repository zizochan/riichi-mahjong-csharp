using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Yaku.Standard
{
    // 面子分解の`HandGroup`（面前のみ）と鳴き面子`Meld`の両方を、役判定で扱いやすい共通の形にまとめたもの。
    internal readonly struct AtomicGroup
    {
        public bool IsSequence { get; }
        public TileSuit Suit { get; }

        // 刻子ならその数字、順子なら最小の数字。
        public int Rank { get; }

        public AtomicGroup(bool isSequence, TileSuit suit, int rank)
        {
            IsSequence = isSequence;
            Suit = suit;
            Rank = rank;
        }
    }
}
