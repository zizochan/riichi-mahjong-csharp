using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;
using Zizo.Mahjong.Yaku.Standard;

namespace Zizo.Mahjong.Scoring
{
    // 符（点数計算の基礎となる値）を計算する。YakuEvaluatorが選んだ面子分解と役の組み合わせに対して呼ぶことを想定する。
    public static class FuCalculator
    {
        private const int BaseFu = 20;
        private const int MenzenRonFu = 10;
        private const int TsumoFu = 2;
        private const int ChiitoitsuFu = 25;
        private const int OpenPinfuShapeFu = 30;
        private const int PinfuTsumoFu = 20;
        private const int PinfuRonFu = 30;
        private const int YakuhaiPairFu = 2;
        private const int NonRyanmenWaitFu = 2;

        // 刻子の符は「明刻・中張牌=2符」を基準に、幺九牌・暗刻でそれぞれ2倍、槓子で4倍になる。
        private const int OpenSimpleTripletFu = 2;
        private const int TerminalOrHonorMultiplier = 2;
        private const int ConcealedMultiplier = 2;
        private const int KanMultiplier = 4;

        private static readonly PinfuYaku Pinfu = new PinfuYaku();

        public static int Calculate(WinningContext context, ScoringRule scoringRule)
        {
            if (context.IsChiitoitsuShape)
            {
                return ChiitoitsuFu;
            }
            if (context.Decomposition == null)
            {
                // 国士無双など、そもそも符を使わない役満の形。呼び出し側は符無し役満として扱う想定。
                return 0;
            }

            // 平和は「ロン30符/ツモ20符」の固定値になる（待ち・雀頭・面子による加符を一切見ない）。
            if (Pinfu.TryEvaluate(context, out _))
            {
                return context.IsTsumo ? PinfuTsumoFu : PinfuRonFu;
            }

            int fu = BaseFu;
            if (context.IsMenzen && !context.IsTsumo)
            {
                fu += MenzenRonFu;
            }
            if (context.IsTsumo)
            {
                fu += TsumoFu;
            }

            fu += CalculatePairFu(context);
            fu += CalculateWaitFu(context);
            fu += CalculateConcealedSetsFu(context);
            fu += CalculateMeldsFu(context);

            // 鳴いた手で加符が一切無い形（喰い平和形のロン）は20符にならず、30符として扱う。
            if (!context.IsMenzen && fu == BaseFu)
            {
                return OpenPinfuShapeFu;
            }

            return PointRounding.RoundUp(fu, scoringRule.FuRoundingUnit);
        }

        private static int CalculatePairFu(WinningContext context)
        {
            TileKind pairKind = context.PairKind;
            int fu = 0;
            if (pairKind.Suit == TileSuit.Dragon)
            {
                fu += YakuhaiPairFu;
            }
            if (pairKind.Equals(context.Situation.SeatWind))
            {
                fu += YakuhaiPairFu;
            }
            if (pairKind.Equals(context.Situation.RoundWind))
            {
                fu += YakuhaiPairFu;
            }
            return fu;
        }

        // 和了牌が複数の面子に当てはまる場合は、符が高くなる解釈（単騎・嵌張・辺張）を優先する（高点法）。
        private static int CalculateWaitFu(WinningContext context)
        {
            if (context.WinningTile.Kind.Equals(context.PairKind))
            {
                return NonRyanmenWaitFu; // 単騎待ち
            }

            bool canBeNonRyanmen = WaitAnalysis
                .GetSequencesContainingWinningTile(context)
                .Any(sequence =>
                    !WaitAnalysis.IsRyanmenCompletion(sequence, context.WinningTile.Kind)
                );

            // 両面待ち、または刻子側の完成（シャンポン）は待ちとしての加符が無い。
            return canBeNonRyanmen ? NonRyanmenWaitFu : 0;
        }

        private static int CalculateConcealedSetsFu(WinningContext context)
        {
            // ロンでこの刻子を完成させた場合、その面子だけ明刻扱いになる（暗刻より符が低い）。
            return context
                .Sets.Where(set => set.Shape == SetShape.Triplet)
                .Sum(set =>
                    CalculateTripletFu(
                        set.Kinds[0],
                        isConcealed: !context.IsTripletCompletedByRon(set),
                        isKan: false
                    )
                );
        }

        private static int CalculateMeldsFu(WinningContext context)
        {
            return context
                .Melds.Where(meld => meld.IsTripletLike)
                .Sum(meld => CalculateTripletFu(meld.Tiles[0].Kind, meld.IsConcealed, meld.IsKan));
        }

        private static int CalculateTripletFu(TileKind kind, bool isConcealed, bool isKan)
        {
            int fu = OpenSimpleTripletFu;
            if (kind.IsTerminalOrHonor)
            {
                fu *= TerminalOrHonorMultiplier;
            }
            if (isConcealed)
            {
                fu *= ConcealedMultiplier;
            }
            if (isKan)
            {
                fu *= KanMultiplier;
            }
            return fu;
        }
    }
}
