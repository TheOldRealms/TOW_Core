using System;

namespace TOW_Core.Battle.AI.Decision.ScoringFunction
{
    public class Axis
    {
        private AbstractFunction _function;
        private float _multiplier;
        public float X;

        public float Evaluate()
        {
            return _function.Evaluate(X);
        }
    }
}