namespace Zizo.Mahjong.Scoring
{
    internal static class PointRounding
    {
        // valueをunitの倍数に切り上げる（符の10符単位・点数の100点単位の切り上げに使う）。
        public static int RoundUp(int value, int unit)
        {
            return ((value + unit - 1) / unit) * unit;
        }
    }
}
