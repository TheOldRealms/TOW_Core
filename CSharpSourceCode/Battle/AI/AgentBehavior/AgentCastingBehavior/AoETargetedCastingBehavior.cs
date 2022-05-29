using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class AoETargetedCastingBehavior : MissileCastingBehavior
    {
        public AoETargetedCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }

        protected override bool HaveLineOfSightToTarget(Target target)
        {
            return true;
        }

    }
}