namespace Zizo.Mahjong.Yaku
{
    public interface IYaku
    {
        string Name { get; }
        bool IsYakuman { get; }

        // 成立する場合はtrueを返し、翻数（役満の場合は「何倍役満か」、通常役なら翻数）をhanに入れる。
        bool TryEvaluate(WinningContext context, out int han);
    }
}
