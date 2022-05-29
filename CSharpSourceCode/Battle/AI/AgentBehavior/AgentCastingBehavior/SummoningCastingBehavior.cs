using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class SummoningCastingBehavior : AbstractAgentCastingBehavior
    {
        public SummoningCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
        }

        protected override Target UpdateTarget(Target target)
        {
            target.SelectedWorldPosition = Agent.Position + Agent.LookDirection * 2;
            return target;
        }
    }
}