using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class TargetedStaticAoEAgentCastingBehavior : AbstractAgentCastingBehavior
    {
        public TargetedStaticAoEAgentCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }

        public override void Terminate()
        {
        }

        public override bool IsPositional()
        {
            return false;
        }
    }
}