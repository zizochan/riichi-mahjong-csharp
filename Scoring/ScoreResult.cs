namespace Zizo.Mahjong.Scoring
{
    public class ScoreResult
    {
        // 基本点に掛ける倍率（親のロン=6倍、子のロン=4倍、ツモは親払い=2倍・子払い=1倍）。
        private const int DealerRonMultiplier = 6;
        private const int NonDealerRonMultiplier = 4;
        private const int DealerShareMultiplier = 2;
        private const int NonDealerShareMultiplier = 1;

        public int Han { get; }
        public int Fu { get; }
        public int BasePoints { get; }
        public bool WinnerIsDealer { get; }
        public bool IsYakuman { get; }
        public int YakumanMultiplier { get; }

        // ロン時に、放銃した1人が支払う点数。
        public int RonPayment { get; }

        // ツモ時に、和了者が子の場合に親が支払う点数。
        public int TsumoPaymentFromDealer { get; }

        // ツモ時に、子（和了者が親なら他家全員、和了者が子ならもう一人の子）が支払う点数。
        public int TsumoPaymentFromNonDealer { get; }

        public ScoreResult(
            int han,
            int fu,
            int basePoints,
            bool winnerIsDealer,
            bool isYakuman,
            int yakumanMultiplier,
            int pointRoundingUnit
        )
        {
            Han = han;
            Fu = fu;
            BasePoints = basePoints;
            WinnerIsDealer = winnerIsDealer;
            IsYakuman = isYakuman;
            YakumanMultiplier = yakumanMultiplier;

            int ronMultiplier = winnerIsDealer ? DealerRonMultiplier : NonDealerRonMultiplier;
            RonPayment = PointRounding.RoundUp(basePoints * ronMultiplier, pointRoundingUnit);
            TsumoPaymentFromDealer = PointRounding.RoundUp(
                basePoints * DealerShareMultiplier,
                pointRoundingUnit
            );
            // 親のツモ和了では子が親の取り分(2倍)を支払う。
            int nonDealerMultiplier = winnerIsDealer
                ? DealerShareMultiplier
                : NonDealerShareMultiplier;
            TsumoPaymentFromNonDealer = PointRounding.RoundUp(
                basePoints * nonDealerMultiplier,
                pointRoundingUnit
            );
        }

        // ツモ和了で和了者が総取りする点数の合計（積み棒・供託リーチ棒は含まない）。
        public int GetTotalTsumoPoints(int otherPlayerCount)
        {
            return WinnerIsDealer
                ? TsumoPaymentFromNonDealer * otherPlayerCount
                : TsumoPaymentFromDealer + (TsumoPaymentFromNonDealer * (otherPlayerCount - 1));
        }
    }
}
