using System;
using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Flow;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Cpu
{
    // 向聴数と受け入れ枚数（見えている牌から逆算した残り枚数）だけを根拠にする、最もシンプルなCPU戦略。
    // イカサマは行わず、`PlayerView`経由で渡される公開情報だけを使って判断する。
    public class DefaultCpuStrategy : ICpuStrategy
    {
        // 自分の後にもう一巡ツモが回ってくる程度の残り枚数が無ければリーチしない。
        private const int MinimumRemainingTileCountToRiichi = 4;

        public Tile ChooseDiscard(PlayerView view, MahjongRuleSet ruleSet)
        {
            List<TileKind> allKinds = GetAllTileKinds(ruleSet);
            DiscardEvaluation best = FindBestDiscard(
                view.ConcealedTiles,
                view.OwnMelds.Count,
                allKinds,
                ruleSet,
                view
            );
            return best.Discard;
        }

        public bool ShouldDeclareRiichi(PlayerView view, MahjongRuleSet ruleSet)
        {
            if (!view.IsMenzen || view.IsRiichiDeclared)
            {
                return false;
            }

            List<TileKind> allKinds = GetAllTileKinds(ruleSet);
            int shanten = ShantenCalculator
                .Calculate(view.ConcealedTiles, view.OwnMelds.Count, allKinds)
                .Shanten;

            return shanten == 0
                && view.Score >= ruleSet.ScoringRule.RiichiStickValue
                && view.RemainingLiveTileCount >= MinimumRemainingTileCountToRiichi;
        }

        public CallDecision DecideCall(
            PlayerView view,
            MahjongRuleSet ruleSet,
            CallOpportunity opportunity,
            Tile discardedTile
        )
        {
            if (opportunity.CanRon)
            {
                return CallDecision.Ron();
            }

            List<TileKind> allKinds = GetAllTileKinds(ruleSet);
            int currentShanten = ShantenCalculator
                .Calculate(view.ConcealedTiles, view.OwnMelds.Count, allKinds)
                .Shanten;

            if (opportunity.CanChi)
            {
                (Tile First, Tile Second)? bestPattern = null;
                int bestShantenAfterChi = int.MaxValue;
                foreach ((Tile First, Tile Second) pattern in opportunity.ChiPatterns)
                {
                    var afterCall = new List<Tile>(view.ConcealedTiles);
                    RemoveOneInstance(afterCall, pattern.First);
                    RemoveOneInstance(afterCall, pattern.Second);
                    int shantenAfter = FindBestDiscard(
                        afterCall,
                        view.OwnMelds.Count + 1,
                        allKinds,
                        ruleSet,
                        null
                    ).Shanten;
                    if (shantenAfter < bestShantenAfterChi)
                    {
                        bestShantenAfterChi = shantenAfter;
                        bestPattern = pattern;
                    }
                }
                if (bestPattern.HasValue && bestShantenAfterChi < currentShanten)
                {
                    return CallDecision.Chi(bestPattern.Value);
                }
            }

            if (opportunity.CanPon)
            {
                var afterCall = new List<Tile>(view.ConcealedTiles);
                RemoveMatchingKind(afterCall, discardedTile.Kind, 2);
                int shantenAfter = FindBestDiscard(
                    afterCall,
                    view.OwnMelds.Count + 1,
                    allKinds,
                    ruleSet,
                    null
                ).Shanten;
                if (shantenAfter < currentShanten)
                {
                    return CallDecision.Pon();
                }
            }

            // カンは向聴数を進めないため（暗刻が槓子に変わるだけ）、このシンプルな戦略では見送る。
            return CallDecision.None;
        }

        private readonly struct DiscardEvaluation
        {
            public Tile Discard { get; }
            public int Shanten { get; }
            public int UkeireCount { get; }

            public DiscardEvaluation(Tile discard, int shanten, int ukeireCount)
            {
                Discard = discard;
                Shanten = shanten;
                UkeireCount = ukeireCount;
            }
        }

        private static DiscardEvaluation FindBestDiscard(
            IReadOnlyList<Tile> tiles,
            int meldCount,
            List<TileKind> allKinds,
            MahjongRuleSet ruleSet,
            PlayerView viewForVisibility
        )
        {
            DiscardEvaluation? best = null;
            foreach (Tile candidate in DistinctByKind(tiles))
            {
                var remaining = new List<Tile>(tiles);
                RemoveOneInstance(remaining, candidate);
                ShantenResult result = ShantenCalculator.Calculate(remaining, meldCount, allKinds);
                int ukeireCount =
                    viewForVisibility == null
                        ? 0
                        : CountUkeireTiles(
                            remaining,
                            result.UsefulTileKinds,
                            ruleSet,
                            viewForVisibility
                        );

                if (
                    best == null
                    || result.Shanten < best.Value.Shanten
                    || (
                        result.Shanten == best.Value.Shanten && ukeireCount > best.Value.UkeireCount
                    )
                )
                {
                    best = new DiscardEvaluation(candidate, result.Shanten, ukeireCount);
                }
            }
            return best ?? new DiscardEvaluation(tiles[0], int.MaxValue, 0);
        }

        private static int CountUkeireTiles(
            List<Tile> handAfterDiscard,
            IReadOnlyList<TileKind> usefulKinds,
            MahjongRuleSet ruleSet,
            PlayerView view
        )
        {
            int total = 0;
            foreach (TileKind kind in usefulKinds)
            {
                int totalInSet = ruleSet
                    .TileSet.Entries.Where(entry => entry.Kind.Equals(kind))
                    .Sum(entry => entry.Count);
                int visible = CountVisible(kind, handAfterDiscard, view);
                total += Math.Max(0, totalInSet - visible);
            }
            return total;
        }

        private static int CountVisible(TileKind kind, List<Tile> ownHand, PlayerView view)
        {
            int count = ownHand.Count(tile => tile.Kind.Equals(kind));
            // MeldsBySeatには自分の鳴き面子も含まれるため、OwnMeldsは別途数えない。
            foreach (IReadOnlyList<Meld> melds in view.MeldsBySeat.Values)
            {
                count += melds.Sum(meld => meld.Tiles.Count(tile => tile.Kind.Equals(kind)));
            }
            // 鳴かれた捨て牌は鳴いた人の面子側で数えているため、二重に数えない。
            foreach (IReadOnlyList<DiscardedTile> discards in view.DiscardsBySeat.Values)
            {
                count += discards.Count(discard =>
                    !discard.WasCalled && discard.Tile.Kind.Equals(kind)
                );
            }
            count += view.VisibleDoraIndicators.Count(tile => tile.Kind.Equals(kind));
            return count;
        }

        private static IEnumerable<Tile> DistinctByKind(IReadOnlyList<Tile> tiles)
        {
            return tiles.GroupBy(tile => tile.Kind).Select(group => group.First());
        }

        // 赤ドラフラグまで一致する牌を優先して1枚取り除き、無ければ同じ種類の牌を取り除く。
        private static void RemoveOneInstance(List<Tile> tiles, Tile toRemove)
        {
            int index = TileSearch.FindExactIndex(tiles, toRemove);
            if (index < 0)
            {
                index = tiles.FindIndex(tile => tile.Kind.Equals(toRemove.Kind));
            }
            if (index >= 0)
            {
                tiles.RemoveAt(index);
            }
        }

        private static void RemoveMatchingKind(List<Tile> tiles, TileKind kind, int count)
        {
            for (int removedCount = 0; removedCount < count; removedCount++)
            {
                int index = tiles.FindIndex(tile => tile.Kind.Equals(kind));
                if (index >= 0)
                {
                    tiles.RemoveAt(index);
                }
            }
        }

        private static List<TileKind> GetAllTileKinds(MahjongRuleSet ruleSet)
        {
            return ruleSet.TileSet.GetDistinctKinds();
        }
    }
}
