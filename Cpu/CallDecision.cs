using Zizo.Mahjong.Tiles;

namespace Zizo.Mahjong.Cpu
{
    public class CallDecision
    {
        public static readonly CallDecision None = new CallDecision(CallDecisionType.None, null);

        public CallDecisionType Type { get; }
        public (Tile First, Tile Second)? ChiPattern { get; }

        private CallDecision(CallDecisionType type, (Tile First, Tile Second)? chiPattern)
        {
            Type = type;
            ChiPattern = chiPattern;
        }

        public static CallDecision Ron()
        {
            return new CallDecision(CallDecisionType.Ron, null);
        }

        public static CallDecision Pon()
        {
            return new CallDecision(CallDecisionType.Pon, null);
        }

        public static CallDecision OpenKan()
        {
            return new CallDecision(CallDecisionType.OpenKan, null);
        }

        public static CallDecision Chi((Tile First, Tile Second) pattern)
        {
            return new CallDecision(CallDecisionType.Chi, pattern);
        }
    }
}
