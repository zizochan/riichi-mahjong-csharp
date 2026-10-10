using System;
using System.Collections.Generic;
using Zizo.Mahjong.Rules;

namespace Zizo.Mahjong.Tiles
{
    public class Wall
    {
        private readonly WallRule _wallRule;
        private readonly List<Tile> _liveTiles;
        private readonly List<Tile> _deadWallTiles;
        private readonly List<Tile> _doraIndicators = new List<Tile>();
        private readonly List<Tile> _uraDoraIndicators = new List<Tile>();
        private int _rinshanDrawnCount;

        public int RemainingLiveTileCount => _liveTiles.Count;

        // 嶺上牌をまだツモれるか（カンの回数がWallRule.MaxKanCountに達していないか）。
        public bool CanDrawFromDeadWall => _rinshanDrawnCount < _wallRule.MaxKanCount;
        public IReadOnlyList<Tile> DoraIndicators => _doraIndicators;
        public IReadOnlyList<Tile> UraDoraIndicators => _uraDoraIndicators;

        public Wall(
            TileSetDefinition tileSetDefinition,
            WallRule wallRule,
            IRandomSource randomSource = null
        )
        {
            if (tileSetDefinition == null)
            {
                throw new ArgumentNullException(nameof(tileSetDefinition));
            }
            _wallRule = wallRule ?? throw new ArgumentNullException(nameof(wallRule));

            IRandomSource random = randomSource ?? new DefaultRandomSource();
            List<Tile> shuffledTiles = Shuffle(tileSetDefinition.BuildTiles(), random);
            if (shuffledTiles.Count < wallRule.DeadWallTileCount)
            {
                throw new ArgumentException(
                    "牌の総数が王牌の枚数より少ないため、山を構築できません。"
                );
            }

            int deadWallStartIndex = shuffledTiles.Count - wallRule.DeadWallTileCount;
            _liveTiles = shuffledTiles.GetRange(0, deadWallStartIndex);
            _deadWallTiles = shuffledTiles.GetRange(deadWallStartIndex, wallRule.DeadWallTileCount);

            for (
                int indicatorIndex = 0;
                indicatorIndex < wallRule.InitialDoraIndicatorCount;
                indicatorIndex++
            )
            {
                RevealNextDoraIndicator();
            }
        }

        public Tile DrawFromLiveWall()
        {
            if (_liveTiles.Count == 0)
            {
                throw new InvalidOperationException("山に残り牌がありません。");
            }

            Tile tile = _liveTiles[0];
            _liveTiles.RemoveAt(0);
            return tile;
        }

        public Tile DrawFromDeadWall()
        {
            if (!CanDrawFromDeadWall)
            {
                throw new InvalidOperationException("嶺上牌はこれ以上ツモれません。");
            }

            Tile tile = _deadWallTiles[_rinshanDrawnCount];
            _rinshanDrawnCount++;

            // カンで王牌が1枚減った分を、生牌山の末尾を王牌側に繰り込むことで補充する（実物理と同じ挙動）。
            if (_liveTiles.Count > 0)
            {
                _liveTiles.RemoveAt(_liveTiles.Count - 1);
            }

            return tile;
        }

        public void RevealNextDoraIndicator()
        {
            int revealedCount = _doraIndicators.Count;
            if (revealedCount >= MaxDoraIndicatorCount)
            {
                throw new InvalidOperationException("これ以上ドラ表示牌をめくれません。");
            }

            int indicatorIndex = _deadWallTiles.Count - 1 - revealedCount;
            _doraIndicators.Add(_deadWallTiles[indicatorIndex]);
        }

        // 裏ドラは立直で和了した場合のみ、表ドラと同じ枚数をめくる。王牌のうち嶺上牌・表ドラ表示牌の
        // どちらにも使われない区間を裏ドラ表示牌用に割り当てる。
        public void RevealNextUraDoraIndicator()
        {
            int revealedCount = _uraDoraIndicators.Count;
            if (revealedCount >= MaxDoraIndicatorCount)
            {
                throw new InvalidOperationException("これ以上裏ドラ表示牌をめくれません。");
            }

            int uraDoraStartIndex = _wallRule.MaxKanCount;
            _uraDoraIndicators.Add(_deadWallTiles[uraDoraStartIndex + revealedCount]);
        }

        // 立直者の和了時に、表ドラ表示牌と同じ枚数になるまで裏ドラ表示牌をめくる。
        public void RevealUraDoraIndicatorsToMatchDora()
        {
            while (_uraDoraIndicators.Count < _doraIndicators.Count)
            {
                RevealNextUraDoraIndicator();
            }
        }

        // 初期ドラ表示牌 + カンのたびに増えるドラ表示牌の最大数（裏ドラも同数が上限）。
        private int MaxDoraIndicatorCount =>
            _wallRule.InitialDoraIndicatorCount + _wallRule.MaxKanCount;

        private static List<Tile> Shuffle(List<Tile> tiles, IRandomSource random)
        {
            for (int index = tiles.Count - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                (tiles[index], tiles[swapIndex]) = (tiles[swapIndex], tiles[index]);
            }
            return tiles;
        }
    }
}
