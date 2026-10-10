namespace Zizo.Mahjong.Generation
{
    // どの役を狙って構築したか。「役を先に決めて機械的に組み立てる」方式の識別子。
    // 平和は和了牌の選び方（両面待ちの位置）まで制御する必要があるため、現状は対象外。
    public enum WinningHandArchetype
    {
        Chiitoitsu,
        Tanyao,
        Yakuhai,
        Toitoi,
        Honitsu,
        Chinitsu,
    }
}
