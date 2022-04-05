using System.Text;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class SelectMultiTargetCastingBehavior : AbstractAgentCastingBehavior
    {
        public SelectMultiTargetCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
        }

        protected override Target UpdateTarget(Target target)
        {
            target.Agent = CurrentTarget.Formation.GetMedianAgent(true, false, CurrentTarget.Formation.CurrentPosition);
            target.SelectedWorldPosition = target.Agent.Position;
            
            return target;
        }
    }
}