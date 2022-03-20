using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.Components;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class AoEAdjacentCastingBehavior : AbstractAgentCastingBehavior
    {
        public AoEAdjacentCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            TacticalBehavior = new AdjacentAoETacticalBehavior(agent, agent.GetComponent<WizardAIComponent>(), this);
        }

        public override void Execute()
        {
            var castingPosition = ((AdjacentAoETacticalBehavior) TacticalBehavior)?.CastingPosition;
            if (castingPosition.HasValue && Agent.Position.AsVec2.Distance(castingPosition.Value.AsVec2) > 5) return;

            base.Execute();
        }
        
        public override bool IsPositional()
        {
            return true;
        }
    }
}