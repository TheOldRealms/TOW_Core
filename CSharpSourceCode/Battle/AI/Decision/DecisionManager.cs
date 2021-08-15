using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Decision
{
    public static class DecisionManager
    {
        public static IAgentBehavior DecideCastingBehavior(List<IAgentBehavior> objects)
        {
            objects.ForEach(behavior => behavior.CalculateUtility());
            return Highest(objects);
        }
        
        public static IAgentBehavior Highest(List<IAgentBehavior> behaviors)
        {
            return behaviors.MaxBy(behavior => behavior.GetLatestScore());
        }

        public static double WeightedRandom(List<IAgentBehavior> behaviors)
        {
            throw new NotImplementedException("Weighted random not implemented!");
        }
    }
}