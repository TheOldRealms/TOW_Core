using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class CenteredStaticAoEAgentCastingBehavior : AbstractAgentCastingBehavior
    {
        public CenteredStaticAoEAgentCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }
        
        protected override float UtilityFunction(Target target)
        {
            if (Agent.GetAbility(AbilityIndex).IsOnCooldown())
            {
                return 0.0f;
            }

            return 0.5f;
        }
    }
}