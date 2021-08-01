using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TOW_Core.Battle.AI.Behavior.CastingBehavior;

namespace TOW_Core.Battle.AI.Decision.ScoringFunction
{
    public static class ScoringAxis
    {
        public static float CalculateGeometricMean(List<Axis> axes)
        {
            var aggregate = axes
                .Select(axis => axis.Evaluate())
                .Aggregate((a, x) => a * x);
            return (float)Math.Pow(aggregate, 1.0 / axes.Count);
        }

        public static double CalculateArithmeticMean(List<Axis> axes)
        {
            var aggregate = axes
                .Select(axis => axis.Evaluate())
                .Aggregate((a, x) => a + x);
            return aggregate / axes.Count;
        }
    }
}