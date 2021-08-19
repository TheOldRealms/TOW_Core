using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Decision
{
    public abstract class Axis
    {
        private readonly Scoring.ScoringFunction _function;
        private readonly Func<Agent, Target, float> _parameterFunction;

        protected Axis(Scoring.ScoringFunction function, Func<Agent, Target, float> parameterFunction)
        {
            _function = function;
            _parameterFunction = parameterFunction;
        }

        public float Evaluate(Agent agent, Target target)
        {
            return _function.Evaluate(_parameterFunction.Invoke(agent, target));
        }
    }

    public static class AxisExtensions
    {
        public static float GeometricMean(this List<Axis> axes, Agent agent, Target target)
        {
            var aggregate = axes
                .Select(axis => axis.Evaluate(agent, target))
                .Aggregate((a, x) => a * x);
            return (float) Math.Pow(aggregate, 1.0 / axes.Count);
        }

        public static double ArithmeticMean(this List<Axis> axes, Agent agent, Target target)
        {
            var aggregate = axes
                .Select(axis => axis.Evaluate(agent, target))
                .Aggregate((a, x) => a + x);
            return aggregate / axes.Count;
        }
    }
}