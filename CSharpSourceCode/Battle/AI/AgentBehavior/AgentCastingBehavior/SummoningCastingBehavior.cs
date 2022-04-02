using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class SummoningCastingBehavior : AbstractAgentCastingBehavior
    {
        public SummoningCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
        }

        public override void Execute()
        {
            Agent.SelectAbility(AbilityIndex);
            CastSpellAtCurrentTarget();
        }
        
    }
}