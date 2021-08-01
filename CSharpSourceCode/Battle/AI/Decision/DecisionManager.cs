using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TOW_Core.Battle.AI.Behavior.CastingBehavior;
using TOW_Core.Battle.AI.Decision.ScoringFunction;

namespace TOW_Core.Battle.AI.Decision
{
    public static class DecisionManager
    {
        public static IUtilityObject DecideCastingBehavior(List<IUtilityObject> objects)
        {
            objects.ForEach(behavior => behavior.CalculateUtility());
            return Highest(objects);
        }
        
        public static IUtilityObject Highest(List<IUtilityObject> behaviors)
        {
            return behaviors.MaxBy(behavior => behavior.GetLatestScore());
        }

        public static double WeightedRandom(List<IUtilityObject> behaviors)
        {
            throw new NotImplementedException("Weighted random not implemented!");
        }
    }
}