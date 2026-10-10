using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Players
{
    public class Meld
    {
        public MeldType Type { get; }
        public IReadOnlyList<Tile> Tiles { get; }

        // 暗槓（ClosedKan）は他家から取った牌が無いためnull。
        public Tile? CalledTile { get; }
        public int? SourceSeatIndex { get; }

        public Meld(
            MeldType type,
            IEnumerable<Tile> tiles,
            Tile? calledTile = null,
            int? sourceSeatIndex = null
        )
        {
            Type = type;
            Tiles = new List<Tile>(tiles);
            CalledTile = calledTile;
            SourceSeatIndex = sourceSeatIndex;
        }

        public bool IsConcealed => Type == MeldType.ClosedKan;

        public bool IsKan => Type is MeldType.OpenKan or MeldType.ClosedKan or MeldType.AddedKan;

        // 刻子として扱える面子（ポン・各種カン）。チー以外はすべて同じ牌種で構成される。
        public bool IsTripletLike => Type != MeldType.Chi;
    }
}
