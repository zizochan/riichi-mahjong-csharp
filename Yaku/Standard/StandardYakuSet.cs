using System.Collections.Generic;

namespace Zizo.Mahjong.Yaku.Standard
{
    // 標準的なリーチ麻雀の役一式。役を追加・削除したい場合は、このリストを元に
    // 独自のリストを作ってMahjongRuleSetに渡せばよい（既存の役クラスを変更する必要はない）。
    public static class StandardYakuSet
    {
        public static List<IYaku> CreateAll()
        {
            return new List<IYaku>
            {
                // 通常役
                new RiichiYaku(),
                new DoubleRiichiYaku(),
                new IppatsuYaku(),
                new MenzenTsumoYaku(),
                new PinfuYaku(),
                new TanyaoYaku(),
                new IipeikouYaku(),
                new RyanpeikouYaku(),
                new YakuhaiYaku(),
                new ToitoiYaku(),
                new SanankouYaku(),
                new ChiitoitsuYaku(),
                new ChantaYaku(),
                new JunchanYaku(),
                new IttsuYaku(),
                new SanshokuDoujunYaku(),
                new SanshokuDoukouYaku(),
                new SankantsuYaku(),
                new HonroutouYaku(),
                new ShousangenYaku(),
                new HonitsuYaku(),
                new ChinitsuYaku(),
                new RinshanKaihouYaku(),
                new ChankanYaku(),
                new HaiteiRaoyueYaku(),
                new HouteiRaoyuiYaku(),
                // 役満
                new TenhouYaku(),
                new ChiihouYaku(),
                new RenhouYaku(),
                new KokushiMusouYaku(),
                new KokushiMusouThirteenWaitYaku(),
                new SuuankouYaku(),
                new SuuankouTankiYaku(),
                new DaisangenYaku(),
                new ShousuushiYaku(),
                new DaisuushiYaku(),
                new TsuuiisouYaku(),
                new RyuuiisouYaku(),
                new ChinroutouYaku(),
                new SuukantsuYaku(),
                new ChuurenPoutouYaku(),
                new ChuurenPoutouPureYaku(),
            };
        }
    }
}
