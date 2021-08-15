using System;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Decision.ScoringFunction
{
    public class Linear : AbstractFunction
    {
        public override float Evaluate(float x)
        {
            return Math.Max(Min, x) / Max;
        }
    }
}