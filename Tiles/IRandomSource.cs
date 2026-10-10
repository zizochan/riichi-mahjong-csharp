namespace Zizo.Mahjong.Tiles
{
    // 山のシャッフルに使う乱数源。テストで結果を固定したい場合に差し替えられるようにインターフェース化する。
    public interface IRandomSource
    {
        int Next(int exclusiveMax);
    }
}
