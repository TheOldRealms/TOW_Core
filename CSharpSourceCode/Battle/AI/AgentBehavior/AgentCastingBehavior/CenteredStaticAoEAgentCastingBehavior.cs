using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.Components;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class CenteredStaticAoEAgentCastingBehavior : AbstractAgentCastingBehavior
    {
        public CenteredStaticAoEAgentCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            TacticalBehavior = new DirectionalAoETacticalBehavior(agent, agent.GetComponent<WizardAIComponent>(), this);
        }

        public override void Execute()
        {
            var castingPosition = ((AdjacentAoETacticalBehavior) TacticalBehavior)?.CastingPosition;
            if (castingPosition.HasValue && Agent.Position.AsVec2.Distance(castingPosition.Value.AsVec2) > 5) return;

            base.Execute();
        }

        public override void Terminate()
        {
        }

        protected override bool HaveLineOfSightToAgent(Agent targetAgent)
        {
            return true;
        }

        public override bool IsPositional()
        {
            return true;
        }
    }
}