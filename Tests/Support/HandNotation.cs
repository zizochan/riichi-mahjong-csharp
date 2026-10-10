using System;
using System.Collections.Generic;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    // 手牌を「123m456p789s11z」のような一般的な麻雀記法から組み立てるテスト用ヘルパー。
    // m=萬子 p=筒子 s=索子 z=字牌(1-4=東南西北 5-7=白發中)。0は赤5を表す(例: 0m=赤五萬)。
    // Tile/TileKindを直接組み立てる冗長な記述を避け、テストケースの追加・変更をしやすくするために用意する。
    public static class HandNotation
    {
        public static List<Tile> Parse(string notation)
        {
            var tiles = new List<Tile>();
            var pendingDigits = new List<int>();

            foreach (char character in notation)
            {
                if (char.IsDigit(character))
                {
                    pendingDigits.Add(character - '0');
                    continue;
                }

                foreach (int digit in pendingDigits)
                {
                    tiles.Add(CreateTile(character, digit));
                }
                pendingDigits.Clear();
            }

            if (pendingDigits.Count > 0)
            {
                throw new ArgumentException(
                    $"サフィックス(m/p/s/z)のない数字が残っています: {notation}",
                    nameof(notation)
                );
            }

            return tiles;
        }

        public static Tile Single(string notation)
        {
            List<Tile> tiles = Parse(notation);
            if (tiles.Count != 1)
            {
                throw new ArgumentException(
                    $"牌1枚を表す記法ではありません: {notation}",
                    nameof(notation)
                );
            }
            return tiles[0];
        }

        private static Tile CreateTile(char suffix, int digit)
        {
            if (suffix == 'z')
            {
                if (digit is >= 1 and <= 4)
                {
                    return new Tile(TileSuit.Wind, digit);
                }
                if (digit is >= 5 and <= 7)
                {
                    return new Tile(TileSuit.Dragon, digit - 4);
                }
                throw new ArgumentException($"字牌の番号は1〜7である必要があります: {digit}z");
            }

            TileSuit suit = suffix switch
            {
                'm' => TileSuit.Man,
                'p' => TileSuit.Pin,
                's' => TileSuit.Sou,
                _ => throw new ArgumentException($"不明な牌種別サフィックスです: {suffix}"),
            };

            return digit == 0 ? new Tile(suit, 5, isRedDora: true) : new Tile(suit, digit);
        }
    }
}
