using System.Data;

namespace TOW_Core.Battle.AI.Decision.ScoringFunction
{
    public abstract class AbstractFunction
    {
        public abstract float Evaluate(float x);
    }
}