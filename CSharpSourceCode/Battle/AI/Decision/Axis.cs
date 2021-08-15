using System;
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
}