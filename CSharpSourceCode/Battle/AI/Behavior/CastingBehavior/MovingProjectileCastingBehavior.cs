using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Components;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public class MovingProjectileCastingBehavior:AgentCastingBehavior
    {
        public MovingProjectileCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }
    }
}