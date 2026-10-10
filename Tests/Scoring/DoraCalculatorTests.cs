using System.Collections.Generic;
using NUnit.Framework;
using Zizo.Mahjong.Scoring;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class DoraCalculatorTests
    {
        [Test]
        public void CountDora_IndicatorPointsToNextRank_CountsMatchingTilesInHand()
        {
            List<Tile> allTiles = HandNotation.Parse("44m123p");
            List<Tile> indicators = HandNotation.Parse("3m");

            Assert.AreEqual(2, DoraCalculator.CountDora(allTiles, indicators));
        }

        [Test]
        public void CountDora_NumberIndicatorAtNine_WrapsAroundToOne()
        {
            List<Tile> allTiles = HandNotation.Parse("1m123p");
            List<Tile> indicators = HandNotation.Parse("9m");

            Assert.AreEqual(1, DoraCalculator.CountDora(allTiles, indicators));
        }

        [Test]
        public void CountDora_WindIndicatorAtNorth_WrapsAroundToEast()
        {
            List<Tile> allTiles = HandNotation.Parse("1z123p");
            List<Tile> indicators = HandNotation.Parse("4z");

            Assert.AreEqual(1, DoraCalculator.CountDora(allTiles, indicators));
        }

        [Test]
        public void CountDora_DragonIndicatorAtRed_WrapsAroundToWhite()
        {
            List<Tile> allTiles = HandNotation.Parse("5z123p");
            List<Tile> indicators = HandNotation.Parse("7z");

            Assert.AreEqual(1, DoraCalculator.CountDora(allTiles, indicators));
        }

        [Test]
        public void CountRedDora_CountsRedFlaggedTilesRegardlessOfKind()
        {
            List<Tile> allTiles = new List<Tile>
            {
                HandNotation.Single("0m"),
                HandNotation.Single("0p"),
                HandNotation.Single("3s"),
            };

            Assert.AreEqual(2, DoraCalculator.CountRedDora(allTiles));
        }

        [Test]
        public void CountDora_MultipleIndicators_SumsEachIndicator()
        {
            // 3m→4m(2枚)、5z(白)→6z(發)(3枚)。
            List<Tile> allTiles = HandNotation.Parse("44m666z");
            List<Tile> indicators = HandNotation.Parse("3m5z");

            Assert.AreEqual(5, DoraCalculator.CountDora(allTiles, indicators));
        }

        [Test]
        public void CountDora_SameIndicatorTwice_CountsTwice()
        {
            List<Tile> allTiles = HandNotation.Parse("4m");
            List<Tile> indicators = HandNotation.Parse("33m");

            Assert.AreEqual(2, DoraCalculator.CountDora(allTiles, indicators));
        }
    }
}
