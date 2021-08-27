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
        private readonly Func<Target, float> _parameterFunction;
        private readonly float _range;

        public Axis(float minInput, float maxInput, Func<float, float> function, Func<Target, float> parameterFunction)
        {
            _min = minInput;
            _max = maxInput;
            _range = maxInput - minInput;
            _function = function;
            _parameterFunction = parameterFunction;
        }

        public float Evaluate(Target target)
        {
            var x = _parameterFunction.Invoke(target);
            var range = (Math.Max(_min, Math.Min(_max, x)) - _min) / _range;
            var invoke = _function.Invoke(range);
            return invoke;
        }
    }

    public static class AxisExtensions
    {
        public static float GeometricMean(this List<Axis> axes, Target target)
        {
            var aggregate = axes
                .Select(axis => axis.Evaluate(target))
                .Aggregate((a, x) => a * x);
            return (float) Math.Pow(aggregate, 1.0 / axes.Count);
        }

        public static double ArithmeticMean(this List<Axis> axes, Target target)
        {
            var aggregate = axes
                .Select(axis => axis.Evaluate(target))
                .Aggregate((a, x) => a + x);
            return aggregate / axes.Count;
        }
    }
}