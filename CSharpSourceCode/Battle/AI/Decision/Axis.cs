using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Decision
{
    public class Axis
    {
        private float _min;
        private float _max;

        private readonly Func<float, float> _function;
        private readonly Func<Agent, Target, float> _parameterFunction;

        public Axis(float minInput, float maxInput, Func<float, float> function, Func<Agent, Target, float> parameterFunction)
        {
            _min = minInput;
            _max = maxInput;
            _function = function;
            _parameterFunction = parameterFunction;
        }

        public float Evaluate(Agent agent, Target target)
        {
            var x = _parameterFunction.Invoke(agent, target);
            return _function.Invoke(Math.Max(_min, Math.Min(_max, x)) / _max);
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