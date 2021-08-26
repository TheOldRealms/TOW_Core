using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;

namespace TOW_Core.Battle.AI.Decision
{
    public static class DecisionManager
    {
        public static (IAgentBehavior, Target) EvaluateCastingBehaviors(List<IAgentBehavior> behaviors)
        {
            return Highest(behaviors
                .SelectMany(behavior => behavior.CalculateUtility())
                .ToDictionary(e => e.Key, e => e.Value));
        }

        private static (IAgentBehavior, Target) Highest(Dictionary<(IAgentBehavior, Target), float> utilityValues)
        {
            return utilityValues
                .MaxBy(entry => entry.Value)
                .Key;
        }

        public static double WeightedRandom(List<IAgentBehavior> behaviors)
        {
            throw new NotImplementedException("Weighted random not implemented!");
        }
    }
}