using System.Data;

namespace TOW_Core.Battle.AI.Decision.ScoringFunction
{
    public abstract class AbstractFunction
    {
        public float Min = 0;
        public float Max = 0;
        public abstract float Evaluate(float x);
    }
}