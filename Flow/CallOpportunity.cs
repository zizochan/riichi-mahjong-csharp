using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Flow
{
    // ある捨て牌1枚に対して、特定の1プレイヤーが取りうる選択肢をまとめたもの。
    public class CallOpportunity
    {
        public int SeatIndex { get; }
        public bool CanRon { get; }
        public bool CanPon { get; }
        public bool CanOpenKan { get; }
        public IReadOnlyList<(Tile First, Tile Second)> ChiPatterns { get; }

        public bool CanChi => ChiPatterns.Count > 0;
        public bool HasAnyOption => CanRon || CanPon || CanOpenKan || CanChi;

        public CallOpportunity(
            int seatIndex,
            bool canRon,
            bool canPon,
            bool canOpenKan,
            IReadOnlyList<(Tile First, Tile Second)> chiPatterns
        )
        {
            SeatIndex = seatIndex;
            CanRon = canRon;
            CanPon = canPon;
            CanOpenKan = canOpenKan;
            ChiPatterns = chiPatterns;
        }
    }
}
