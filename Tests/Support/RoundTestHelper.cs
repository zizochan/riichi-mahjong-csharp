using System.Collections.Generic;
using System.Linq;
using Zizo.Mahjong.Flow;
using Zizo.Mahjong.Players;
using Zizo.Mahjong.Rules;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    // RoundControllerを使ったテスト用のヘルパー。
    public static class RoundTestHelper
    {
        public const int InitialScore = 25000;

        public static RoundController CreateController(
            int dealerSeatIndex = 0,
            MahjongRuleSet ruleSet = null
        )
        {
            return new RoundController(
                ruleSet ?? MahjongRuleSet.CreateStandardFourPlayer(),
                roundWind: new TileKind(TileSuit.Wind, 1),
                dealerSeatIndex: dealerSeatIndex,
                initialScorePerPlayer: InitialScore,
                randomSource: new SequentialRandomSource()
            );
        }

        // 配牌を任意の手牌に差し替える。山の中身との整合性（同じ牌が5枚以上になる等）は考慮しないため、
        // 判定ロジックの検証専用に使う。
        public static void ReplaceHand(RoundController controller, int seatIndex, string notation)
        {
            Hand hand = controller.Players[seatIndex].Hand;
            List<Tile> currentTiles = hand.ConcealedTiles.ToList();
            hand.RemoveForMeld(currentTiles);
            foreach (Tile tile in HandNotation.Parse(notation))
            {
                hand.Draw(tile);
            }
        }

        // 生牌山（王牌以外）を最後の1枚まで引き切る。海底・河底の状況を作るために使う。
        public static void DrainLiveWall(RoundController controller)
        {
            while (controller.Wall.RemainingLiveTileCount > 0)
            {
                controller.Wall.DrawFromLiveWall();
            }
        }
    }
}
