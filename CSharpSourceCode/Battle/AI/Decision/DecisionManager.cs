using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;

namespace TOW_Core.Battle.AI.Decision
{
    public static class DecisionManager
    {
        public static TacticalBehaviorOption EvaluateCastingBehaviors(List<IAgentBehavior> behaviors)
        {
            return behaviors
                .SelectMany(behavior => behavior.CalculateUtility())
                .MaxBy(option => option.Target.UtilityValue);
        }
    }
}