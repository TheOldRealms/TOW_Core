using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public class ConserveWindsCastingBehavior : AgentCastingBehavior
    {
        public ConserveWindsCastingBehavior(Agent agent, AbilityTemplate abilityTemplate, int abilityIndex) : base(agent, abilityTemplate, abilityIndex)
        {
        }

        public override void Execute()
        {
            //Do nothing. I am hoping that we will add some sort of "Channeling" which allows us to restore magic over time later on.
        }
    }
}