using System;
using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Scoring;
using Zizo.Mahjong.Tiles;
using Zizo.Mahjong.Yaku;

namespace Zizo.Mahjong.Flow
{
    // 1局分の進行（配牌〜打牌〜鳴き〜和了/流局）を管理する。半荘・東風戦などの局をまたぐ進行や
    // 親のローテーションはこのクラスの責務外で、呼び出し側が局ごとにRoundControllerを作り直す。
    // 誰がいつ何をするか（手番の順序・鳴きの優先順位など）は呼び出し側が制御し、このクラスは
    // 個々の操作の実行と、その時点で可能な選択肢の問い合わせに答える。
    public class RoundController
    {
        private const int ConcealedHandTileCount = 13;
        private const int KyuushuKyuuhaiMinKindCount = 9;

        // 鳴きで手牌から抜き出す枚数（捨て牌1枚と合わせて面子になる）。
        private const int PonTakenTileCount = 2;
        private const int OpenKanTakenTileCount = 3;
        private const int ClosedKanTileCount = 4;

        public MahjongRuleSet RuleSet { get; }
        public Wall Wall { get; }
        public IReadOnlyList<PlayerState> Players { get; }
        public int DealerSeatIndex { get; }
        public TileKind RoundWind { get; }
        public int HonbaCount { get; private set; }
        public int RiichiStickCount { get; private set; }
        public int CurrentTurnSeatIndex => _turnManager.CurrentPlayerSeatIndex;
        public bool IsExhaustiveDraw => Wall.RemainingLiveTileCount == 0;

        private readonly TurnManager _turnManager;
        private readonly List<TileKind> _allTileKinds;
        private readonly Dictionary<int, int> _riichiDeclarationDiscardIndex =
            new Dictionary<int, int>();
        private readonly Dictionary<int, bool> _isDoubleRiichi = new Dictionary<int, bool>();

        // 鳴き・カンで第一巡が中断されたか（天和/地和/人和・ダブルリーチ・九種九牌の判定に使う）。
        private bool _isFirstTurnInterrupted;

        // 最後に鳴き・カンが発生した時点の打牌数-1（一発の消滅判定に使う）。
        private int _lastInterruptionDiscardIndex = -1;
        private int _discardCount;

        public RoundController(
            MahjongRuleSet ruleSet,
            TileKind roundWind,
            int dealerSeatIndex,
            int initialScorePerPlayer,
            int honbaCount = 0,
            int riichiStickCount = 0,
            IRandomSource randomSource = null
        )
        {
            RuleSet = ruleSet ?? throw new ArgumentNullException(nameof(ruleSet));
            if (dealerSeatIndex < 0 || dealerSeatIndex >= ruleSet.PlayerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dealerSeatIndex),
                    dealerSeatIndex,
                    null
                );
            }

            RoundWind = roundWind;
            DealerSeatIndex = dealerSeatIndex;
            HonbaCount = honbaCount;
            RiichiStickCount = riichiStickCount;
            Wall = new Wall(ruleSet.TileSet, ruleSet.WallRule, randomSource);
            _allTileKinds = ruleSet.TileSet.GetDistinctKinds();

            var players = new List<PlayerState>();
            for (int seat = 0; seat < ruleSet.PlayerCount; seat++)
            {
                var initialTiles = new List<Tile>();
                for (int tileIndex = 0; tileIndex < ConcealedHandTileCount; tileIndex++)
                {
                    initialTiles.Add(Wall.DrawFromLiveWall());
                }
                players.Add(new PlayerState(seat, new Hand(initialTiles), initialScorePerPlayer));
            }
            Players = players;

            _turnManager = new TurnManager(ruleSet.PlayerCount, dealerSeatIndex);
        }

        public TileKind GetSeatWind(int seatIndex)
        {
            int offset = GetTurnOffset(seatIndex);
            return new TileKind(TileSuit.Wind, TileKind.MinRank + offset);
        }

        private int GetTurnOffset(int seatIndex)
        {
            return (seatIndex - DealerSeatIndex + RuleSet.PlayerCount) % RuleSet.PlayerCount;
        }

        public Tile DrawForCurrentPlayer()
        {
            Tile tile = Wall.DrawFromLiveWall();
            Players[CurrentTurnSeatIndex].Hand.Draw(tile);
            return tile;
        }

        public DiscardedTile Discard(int seatIndex, Tile tile, bool isRiichiDeclaration = false)
        {
            PlayerState player = Players[seatIndex];
            player.Hand.Discard(tile);
            DiscardedTile discarded = player.DiscardPile.Add(tile, isRiichiDeclaration);

            if (isRiichiDeclaration)
            {
                bool isDoubleRiichi =
                    !_isFirstTurnInterrupted && _discardCount == GetTurnOffset(seatIndex);
                player.DeclareRiichi();
                player.ApplyScoreDelta(-RuleSet.ScoringRule.RiichiStickValue);
                RiichiStickCount++;
                _riichiDeclarationDiscardIndex[seatIndex] = _discardCount;
                _isDoubleRiichi[seatIndex] = isDoubleRiichi;
            }

            _discardCount++;
            return discarded;
        }

        public void AdvanceTurnToNext()
        {
            _turnManager.AdvanceToNextPlayer();
        }

        public List<CallOpportunity> GetCallOpportunities(
            int discarderSeatIndex,
            Tile discardedTile
        )
        {
            var opportunities = new List<CallOpportunity>();
            for (int seat = 0; seat < RuleSet.PlayerCount; seat++)
            {
                if (seat == discarderSeatIndex)
                {
                    continue;
                }

                PlayerState player = Players[seat];
                Hand hand = player.Hand;

                // 立直後は手牌を変えられず、河底牌（山の最後の打牌）はロン以外で鳴けない。
                bool canCallForMeld = !player.IsRiichiDeclared && Wall.RemainingLiveTileCount > 0;
                bool canChi =
                    canCallForMeld
                    && ActionValidator.CanChi(
                        hand,
                        discardedTile,
                        discarderSeatIndex,
                        seat,
                        RuleSet.PlayerCount
                    );
                var chiPatterns = canChi
                    ? ActionValidator.GetChiCompletionPatterns(hand, discardedTile)
                    : new List<(Tile First, Tile Second)>();
                bool canPon = canCallForMeld && ActionValidator.CanPon(hand, discardedTile);
                bool canOpenKan =
                    canCallForMeld
                    && Wall.CanDrawFromDeadWall
                    && ActionValidator.CanOpenKan(hand, discardedTile);
                bool canRon = CanDeclareRon(seat, discardedTile);

                var opportunity = new CallOpportunity(
                    seat,
                    canRon,
                    canPon,
                    canOpenKan,
                    chiPatterns
                );
                if (opportunity.HasAnyOption)
                {
                    opportunities.Add(opportunity);
                }
            }
            return opportunities;
        }

        public bool CanDeclareRon(int seatIndex, Tile winningTile)
        {
            return TryEvaluateRon(seatIndex, winningTile, isChankan: false) != null;
        }

        public List<int> GetChankanRonSeats(int kanDeclaringSeatIndex, TileKind kind)
        {
            Tile robbedTile = Players[kanDeclaringSeatIndex]
                .Hand.ConcealedTiles.First(tile => tile.Kind.Equals(kind));
            var winners = new List<int>();
            for (int seat = 0; seat < RuleSet.PlayerCount; seat++)
            {
                if (seat == kanDeclaringSeatIndex)
                {
                    continue;
                }
                if (TryEvaluateRon(seat, robbedTile, isChankan: true) != null)
                {
                    winners.Add(seat);
                }
            }
            return winners;
        }

        private YakuEvaluationResult TryEvaluateRon(int seatIndex, Tile winningTile, bool isChankan)
        {
            PlayerState player = Players[seatIndex];
            var trialTiles = new List<Tile>(player.Hand.ConcealedTiles) { winningTile };
            if (!WinningHandDetector.IsComplete(trialTiles, player.Melds.Count))
            {
                return null;
            }

            List<TileKind> waitingTileKinds = WinningHandDetector
                .GetWaitingTileKinds(player.Hand.ConcealedTiles, player.Melds.Count, _allTileKinds)
                .ToList();
            if (ActionValidator.IsFuriten(player, waitingTileKinds))
            {
                return null;
            }

            YakuEvaluationResult result = EvaluateYaku(
                seatIndex,
                winningTile,
                isTsumo: false,
                isChankan: isChankan
            );
            return result.HasYaku ? result : null;
        }

        public YakuEvaluationResult EvaluateYaku(
            int seatIndex,
            Tile winningTile,
            bool isTsumo,
            bool isRinshan = false,
            bool isChankan = false
        )
        {
            PlayerState player = Players[seatIndex];
            List<Tile> tilesForEvaluation = isTsumo
                ? new List<Tile>(player.Hand.ConcealedTiles)
                : new List<Tile>(player.Hand.ConcealedTiles) { winningTile };

            WinningSituation situation = BuildSituation(
                seatIndex,
                winningTile,
                isTsumo,
                isRinshan,
                isChankan
            );
            return YakuEvaluator.EvaluateBest(
                tilesForEvaluation,
                player.Melds.Count,
                situation,
                RuleSet.EnabledYaku
            );
        }

        private WinningSituation BuildSituation(
            int seatIndex,
            Tile winningTile,
            bool isTsumo,
            bool isRinshan,
            bool isChankan
        )
        {
            PlayerState player = Players[seatIndex];
            bool isDoubleRiichi =
                _isDoubleRiichi.TryGetValue(seatIndex, out bool doubleRiichi) && doubleRiichi;

            return new WinningSituation(
                melds: player.Melds,
                winningTile: winningTile,
                isTsumo: isTsumo,
                seatWind: GetSeatWind(seatIndex),
                roundWind: RoundWind,
                isRiichi: player.IsRiichiDeclared,
                isDoubleRiichi: isDoubleRiichi,
                isIppatsu: player.IsRiichiDeclared && IsIppatsuEligible(seatIndex),
                // 嶺上牌でのツモ和了は、山が尽きていても海底摸月ではなく嶺上開花として扱う。
                isHaitei: isTsumo && !isRinshan && Wall.RemainingLiveTileCount == 0,
                isHoutei: !isTsumo && Wall.RemainingLiveTileCount == 0,
                isRinshan: isRinshan,
                isChankan: isChankan,
                isFirstUninterruptedTurnWin: IsFirstUninterruptedTurnWin(seatIndex, isTsumo)
            );
        }

        private bool IsIppatsuEligible(int seatIndex)
        {
            if (
                !_riichiDeclarationDiscardIndex.TryGetValue(
                    seatIndex,
                    out int declaredAtDiscardCount
                )
            )
            {
                return false;
            }
            if (_lastInterruptionDiscardIndex >= declaredAtDiscardCount)
            {
                return false;
            }
            // 立直宣言牌の後、他家全員の打牌(PlayerCount - 1枚)を経て自分の次のツモまでが一発の対象。
            // 宣言牌自身を含めた打牌数がPlayerCountを超える(=自分が次の打牌をした)時点で消滅する。
            return (_discardCount - declaredAtDiscardCount) <= RuleSet.PlayerCount;
        }

        // 天和/地和/人和の判定。鳴き・カンが一切無く、和了者がまだ一度も打牌していないことを条件にする。
        // ツモ和了（天和/地和）は自分の最初のツモ、ロン和了（人和）は自分の最初のツモより前の打牌が対象になる。
        private bool IsFirstUninterruptedTurnWin(int seatIndex, bool isTsumo)
        {
            if (_isFirstTurnInterrupted)
            {
                return false;
            }
            int turnOffset = GetTurnOffset(seatIndex);
            return isTsumo ? _discardCount == turnOffset : _discardCount <= turnOffset;
        }

        public WinOutcome TryWin(
            int seatIndex,
            Tile winningTile,
            bool isTsumo,
            int? discarderSeatIndex = null,
            bool isRinshan = false,
            bool isChankan = false
        )
        {
            PlayerState winner = Players[seatIndex];
            YakuEvaluationResult yakuResult = EvaluateYaku(
                seatIndex,
                winningTile,
                isTsumo,
                isRinshan,
                isChankan
            );
            if (!yakuResult.HasYaku)
            {
                return null;
            }

            WinningContext context = yakuResult.Context;
            int fu = FuCalculator.Calculate(context, RuleSet.ScoringRule);

            (int doraCount, int uraDoraCount, int redDoraCount) = CountDoraForWin(
                yakuResult,
                winner
            );

            int totalHan = yakuResult.HasYakuman
                ? yakuResult.TotalHan
                : yakuResult.TotalHan + doraCount + uraDoraCount + redDoraCount;
            int yakumanMultiplier = yakuResult.HasYakuman ? yakuResult.TotalHan : 0;
            bool winnerIsDealer = seatIndex == DealerSeatIndex;

            ScoreResult scoreResult = ScoreCalculator.Calculate(
                totalHan,
                fu,
                winnerIsDealer,
                RuleSet.ScoringRule,
                yakumanMultiplier
            );

            int totalGain;
            if (isTsumo)
            {
                List<PlayerState> otherPlayers = Players
                    .Where(player => player.SeatIndex != seatIndex)
                    .ToList();
                totalGain = ScoreManager.ApplyTsumoResult(
                    winner,
                    otherPlayers,
                    Players[DealerSeatIndex],
                    scoreResult,
                    HonbaCount,
                    RiichiStickCount,
                    RuleSet.ScoringRule
                );
            }
            else
            {
                if (discarderSeatIndex == null)
                {
                    throw new InvalidOperationException("ロンには放銃者の指定が必要です。");
                }
                PlayerState discarder = Players[discarderSeatIndex.Value];
                totalGain = ScoreManager.ApplyRonResult(
                    winner,
                    discarder,
                    scoreResult,
                    HonbaCount,
                    RiichiStickCount,
                    RuleSet.ScoringRule
                );
                winner.Hand.Draw(winningTile);
            }

            RiichiStickCount = 0;

            return new WinOutcome(
                seatIndex,
                isTsumo,
                yakuResult,
                fu,
                doraCount,
                uraDoraCount,
                redDoraCount,
                scoreResult,
                totalGain
            );
        }

        // 役満ではドラを数えない（WinOutcomeの各ドラ枚数も0になる）。裏ドラは立直者の和了時のみめくる。
        private (int Dora, int UraDora, int RedDora) CountDoraForWin(
            YakuEvaluationResult yakuResult,
            PlayerState winner
        )
        {
            if (yakuResult.HasYakuman)
            {
                return (0, 0, 0);
            }

            IReadOnlyList<Tile> allTiles = yakuResult.Context.AllTiles;
            int doraCount = DoraCalculator.CountDora(allTiles, Wall.DoraIndicators);
            int redDoraCount = DoraCalculator.CountRedDora(allTiles);
            int uraDoraCount = 0;
            if (winner.IsRiichiDeclared)
            {
                Wall.RevealUraDoraIndicatorsToMatchDora();
                uraDoraCount = DoraCalculator.CountDora(allTiles, Wall.UraDoraIndicators);
            }
            return (doraCount, uraDoraCount, redDoraCount);
        }

        // 九種九牌。第一巡の自分のツモ直後（鳴き・カンが無い状態）に、幺九牌が9種類以上あれば宣言できる。
        public bool CanDeclareKyuushuKyuuhai(int seatIndex)
        {
            if (_isFirstTurnInterrupted || _discardCount != GetTurnOffset(seatIndex))
            {
                return false;
            }

            int terminalOrHonorKindCount = Players[seatIndex]
                .Hand.ConcealedTiles.Where(tile => tile.Kind.IsTerminalOrHonor)
                .Select(tile => tile.Kind)
                .Distinct()
                .Count();
            return terminalOrHonorKindCount >= KyuushuKyuuhaiMinKindCount;
        }

        public Dictionary<int, int> SettleExhaustiveDraw()
        {
            var tenpaiSeats = Players
                .Where(player =>
                    WinningHandDetector.IsTenpai(
                        player.Hand.ConcealedTiles,
                        player.Melds.Count,
                        _allTileKinds
                    )
                )
                .Select(player => player.SeatIndex)
                .ToList();
            var notenSeats = Players
                .Select(player => player.SeatIndex)
                .Except(tenpaiSeats)
                .ToList();

            var deltas = Players.ToDictionary(player => player.SeatIndex, _ => 0);
            if (tenpaiSeats.Count == 0 || notenSeats.Count == 0)
            {
                return deltas;
            }

            int pot = RuleSet.ScoringRule.ExhaustiveDrawPenaltyTotal;
            int receivePerTenpai = pot / tenpaiSeats.Count;
            int payPerNoten = pot / notenSeats.Count;

            foreach (int seat in tenpaiSeats)
            {
                Players[seat].ApplyScoreDelta(receivePerTenpai);
                deltas[seat] = receivePerTenpai;
            }
            foreach (int seat in notenSeats)
            {
                Players[seat].ApplyScoreDelta(-payPerNoten);
                deltas[seat] = -payPerNoten;
            }
            return deltas;
        }

        public Tile ExecuteChi(
            int seatIndex,
            int discarderSeatIndex,
            Tile discardedTile,
            (Tile First, Tile Second) pattern
        )
        {
            PlayerState player = Players[seatIndex];
            player.Hand.RemoveForMeld(new[] { pattern.First, pattern.Second });
            var meldTiles = new List<Tile> { pattern.First, pattern.Second, discardedTile };
            player.AddMeld(new Meld(MeldType.Chi, meldTiles, discardedTile, discarderSeatIndex));

            MarkLastDiscardAsCalled(discarderSeatIndex);
            _turnManager.JumpToPlayer(seatIndex);
            return discardedTile;
        }

        public void ExecutePon(int seatIndex, int discarderSeatIndex, Tile discardedTile)
        {
            PlayerState player = Players[seatIndex];
            List<Tile> meldTiles = TakeConcealedTilesOfKind(
                player,
                discardedTile.Kind,
                PonTakenTileCount
            );
            meldTiles.Add(discardedTile);
            player.AddMeld(new Meld(MeldType.Pon, meldTiles, discardedTile, discarderSeatIndex));

            MarkLastDiscardAsCalled(discarderSeatIndex);
            _turnManager.JumpToPlayer(seatIndex);
        }

        public Tile ExecuteOpenKan(int seatIndex, int discarderSeatIndex, Tile discardedTile)
        {
            PlayerState player = Players[seatIndex];
            List<Tile> meldTiles = TakeConcealedTilesOfKind(
                player,
                discardedTile.Kind,
                OpenKanTakenTileCount
            );
            meldTiles.Add(discardedTile);
            player.AddMeld(
                new Meld(MeldType.OpenKan, meldTiles, discardedTile, discarderSeatIndex)
            );

            MarkLastDiscardAsCalled(discarderSeatIndex);
            _turnManager.JumpToPlayer(seatIndex);
            return DrawRinshanTileAfterKan(player);
        }

        public Tile ExecuteClosedKan(int seatIndex, TileKind kind)
        {
            PlayerState player = Players[seatIndex];
            List<Tile> meldTiles = TakeConcealedTilesOfKind(player, kind, ClosedKanTileCount);
            player.AddMeld(new Meld(MeldType.ClosedKan, meldTiles));

            MarkTurnInterrupted();
            return DrawRinshanTileAfterKan(player);
        }

        public Tile ExecuteAddedKan(int seatIndex, TileKind kind)
        {
            PlayerState player = Players[seatIndex];
            Tile fourthTile = player.Hand.ConcealedTiles.First(tile => tile.Kind.Equals(kind));
            player.Hand.RemoveForMeld(new[] { fourthTile });

            if (!player.TryUpgradePonToAddedKan(kind, fourthTile))
            {
                throw new InvalidOperationException("対応するポンが見つかりません。");
            }

            MarkTurnInterrupted();
            return DrawRinshanTileAfterKan(player);
        }

        // 手牌から指定した種類の牌を指定枚数だけ抜き出す（鳴き面子の構成牌にするため）。
        private static List<Tile> TakeConcealedTilesOfKind(
            PlayerState player,
            TileKind kind,
            int count
        )
        {
            List<Tile> tiles = player
                .Hand.ConcealedTiles.Where(tile => tile.Kind.Equals(kind))
                .Take(count)
                .ToList();
            if (tiles.Count < count)
            {
                throw new InvalidOperationException($"手牌に{kind}が{count}枚ありません。");
            }
            player.Hand.RemoveForMeld(tiles);
            return tiles;
        }

        // カンの後は嶺上牌をツモり、新ドラ表示牌をめくる。
        private Tile DrawRinshanTileAfterKan(PlayerState player)
        {
            Tile rinshanTile = Wall.DrawFromDeadWall();
            player.Hand.Draw(rinshanTile);
            Wall.RevealNextDoraIndicator();
            return rinshanTile;
        }

        private void MarkLastDiscardAsCalled(int discarderSeatIndex)
        {
            IReadOnlyList<DiscardedTile> discards = Players[discarderSeatIndex]
                .DiscardPile
                .Discards;
            discards[discards.Count - 1].MarkAsCalled();
            MarkTurnInterrupted();
        }

        // 鳴き・カン（暗槓含む）は第一巡と、全員の一発を消滅させる。
        private void MarkTurnInterrupted()
        {
            _isFirstTurnInterrupted = true;
            _lastInterruptionDiscardIndex = _discardCount - 1;
        }
    }
}
