using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zizo.Mahjong.Judgment;
using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Tests
{
    public class HandDecomposerTests
    {
        [Test]
        public void EnumerateDecompositions_AmbiguousShape_ReturnsBothInterpretations()
        {
            // 111222333m は「3つの刻子」とも「123m×3の順子」とも分解できる。
            List<HandDecomposition> decompositions = HandDecomposer.EnumerateDecompositions(
                HandNotation.Parse("111222333m456p77s")
            );

            Assert.AreEqual(2, decompositions.Count);
            Assert.IsTrue(
                decompositions.Any(decomposition =>
                    decomposition.Sets.Count(set => set.Shape == SetShape.Triplet) == 3
                )
            );
            Assert.IsTrue(
                decompositions.Any(decomposition =>
                    decomposition.Sets.All(set => set.Shape == SetShape.Sequence)
                )
            );
            Assert.IsTrue(
                decompositions.All(decomposition =>
                    decomposition.PairKind.Equals(new TileKind(TileSuit.Sou, 7))
                )
            );
        }

        [Test]
        public void EnumerateDecompositions_HonorPair_UsesHonorAsPair()
        {
            List<HandDecomposition> decompositions = HandDecomposer.EnumerateDecompositions(
                HandNotation.Parse("123456789m11155z")
            );

            Assert.AreEqual(1, decompositions.Count);
            Assert.AreEqual(new TileKind(TileSuit.Dragon, 1), decompositions[0].PairKind);
            Assert.AreEqual(4, decompositions[0].Sets.Count);
        }

        [Test]
        public void EnumerateDecompositions_IncompleteHand_ReturnsEmpty()
        {
            List<HandDecomposition> decompositions = HandDecomposer.EnumerateDecompositions(
                HandNotation.Parse("123456789m11p12s5z")
            );

            Assert.IsEmpty(decompositions);
        }

        [Test]
        public void EnumerateDecompositions_SevenPairs_IsNotRegularShape()
        {
            List<HandDecomposition> decompositions = HandDecomposer.EnumerateDecompositions(
                HandNotation.Parse("1133557799m2244p")
            );

            Assert.IsEmpty(decompositions);
        }

        [Test]
        public void EnumerateDecompositions_WithCalledMelds_DecomposesRemainingTilesOnly()
        {
            // 鳴き面子2つを除いた残り8枚(2面子+雀頭)だけを渡す。
            List<HandDecomposition> decompositions = HandDecomposer.EnumerateDecompositions(
                HandNotation.Parse("234m777p99s")
            );

            Assert.AreEqual(1, decompositions.Count);
            Assert.AreEqual(2, decompositions[0].Sets.Count);
        }
    }
}
