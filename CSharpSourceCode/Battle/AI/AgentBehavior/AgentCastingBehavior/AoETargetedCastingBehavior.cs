using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class AoETargetedCastingBehavior : AbstractAgentCastingBehavior
    {
        public AoETargetedCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }


        public override void Execute()
        {
            if (AbilityTemplate.AbilityTargetType == AbilityTargetType.Self)
            {
                Agent.SelectAbility(AbilityIndex);
                CastSpellAtAgent(Agent);
            }
            else
            {
                base.Execute();
            }
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