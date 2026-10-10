using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    // シャッフルを行わない(=定義順のまま)ランダム源。Wallの山構築は末尾から順にFisher-Yatesで
    // スワップするため、Next(exclusiveMax)が常に「自分自身の位置(exclusiveMax - 1)」を返せば
    // 実質何も入れ替わらず、TileSetDefinition.BuildTiles()の生成順がそのまま山の順序になる。
    // RoundController経由のテストで配牌内容を決定的にするために使う。
    public class SequentialRandomSource : IRandomSource
    {
        public int Next(int exclusiveMax)
        {
            return exclusiveMax - 1;
        }
    }
}
