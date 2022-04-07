using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class SelectMultiTargetCastingBehavior : AoETargetedCastingBehavior
    {
        public SelectMultiTargetCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
        }
    }
}