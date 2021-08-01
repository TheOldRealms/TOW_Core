using System;
using System.Collections.Generic;
using System.Linq;

namespace TOW_Core.Battle.AI.Decision.ScoringFunction
{
    public static class ScoreCalculator
    {
        public static double CalculateGeometricAverage(List<Axis> axes)
        {
            var aggregate = axes
                .Select(axis => axis.Evaluate())
                .Aggregate((a, x) => a * x);
            return Math.Pow(aggregate, 1.0 / axes.Count);
        }
    }
}