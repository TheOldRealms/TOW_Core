using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class PreserveWindsAgentCastingBehavior : AbstractAgentCastingBehavior
    {
        public PreserveWindsAgentCastingBehavior(Agent agent, AbilityTemplate abilityTemplate, int abilityIndex) : base(agent, abilityTemplate, abilityIndex)
        {
        }

        public override void Terminate()
        {
        }

        public override bool IsPositional()
        {
            return false;
        }

        public override void Execute()
        {
            //Do nothing. I am hoping that we will add some sort of "Channeling" which allows us to restore magic over time later on.
        }

        protected override float UtilityFunction(Target target)
        {
            return 0.4f;
        }
    }
}