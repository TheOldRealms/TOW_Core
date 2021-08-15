using System;

namespace TOW_Core.Battle.AI.Decision.Scoring
{
    public class Linear : Scoring.ScoringFunction
    {
        public override float Evaluate(float x)
        {
            return Math.Max(Min, x) / Max;
        }
    }
}