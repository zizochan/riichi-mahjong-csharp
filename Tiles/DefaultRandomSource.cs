using System;

namespace Zizo.Mahjong.Tiles
{
    public sealed class DefaultRandomSource : IRandomSource
    {
        private readonly Random _random;

        public DefaultRandomSource()
        {
            _random = new Random();
        }

        public DefaultRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int exclusiveMax)
        {
            return _random.Next(exclusiveMax);
        }
    }
}
