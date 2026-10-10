using System;
using System.Collections.Generic;
using System.Linq;

namespace Zizo.Mahjong.Tiles
{
    // 山に含める牌の種類と枚数の定義。ここを差し替えるだけで「白を6枚にする」等の
    // カスタム牌構成に対応できるようにする（人数・役リストと並ぶ可変ポイントの一つ）。
    public class TileSetDefinition
    {
        private const int CopiesPerKind = 4;
        private const int RedDoraRank = 5;

        private readonly List<TileSetEntry> _entries;

        public IReadOnlyList<TileSetEntry> Entries => _entries;

        public TileSetDefinition(IEnumerable<TileSetEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }
            _entries = new List<TileSetEntry>(entries);
        }

        // 牌構成に含まれる牌の種類（重複なし）。待ち牌・有効牌の候補として使う。
        public List<TileKind> GetDistinctKinds()
        {
            return _entries.Select(entry => entry.Kind).Distinct().ToList();
        }

        public List<Tile> BuildTiles()
        {
            var tiles = new List<Tile>();
            foreach (TileSetEntry entry in _entries)
            {
                for (int tileIndex = 0; tileIndex < entry.Count; tileIndex++)
                {
                    bool isRedDora = tileIndex < entry.RedDoraCount;
                    tiles.Add(new Tile(entry.Kind, isRedDora));
                }
            }
            return tiles;
        }

        // 一般的な四人麻雀の標準136枚セット（各数牌4枚、風牌4種×4枚、三元牌3種×4枚、赤5が1枚ずつ）。
        public static TileSetDefinition CreateStandard(int redDoraCountPerNumberSuit = 1)
        {
            var entries = new List<TileSetEntry>();

            foreach (TileSuit suit in TileKind.NumberSuits)
            {
                for (int rank = TileKind.MinRank; rank <= TileKind.MaxNumberRank; rank++)
                {
                    int redDoraCount = rank == RedDoraRank ? redDoraCountPerNumberSuit : 0;
                    entries.Add(
                        new TileSetEntry(new TileKind(suit, rank), CopiesPerKind, redDoraCount)
                    );
                }
            }

            foreach (TileSuit suit in new[] { TileSuit.Wind, TileSuit.Dragon })
            {
                for (int rank = TileKind.MinRank; rank <= TileKind.GetMaxRank(suit); rank++)
                {
                    entries.Add(new TileSetEntry(new TileKind(suit, rank), CopiesPerKind));
                }
            }

            return new TileSetDefinition(entries);
        }
    }
}
