using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.AgentBehavior.Components;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class AoEDirectionalCastingBehavior : AbstractAgentCastingBehavior
    {
        public AoEDirectionalCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.35f;
            TacticalBehavior = new DirectionalAoETacticalBehavior(agent, agent.GetComponent<WizardAIComponent>(), this);
        }

        public override void Execute()
        {
            var castingPosition = ((DirectionalAoETacticalBehavior) TacticalBehavior)?.CastingPosition;
            
            if (castingPosition.HasValue && Agent.Position.AsVec2.Distance(castingPosition.Value.AsVec2) > 6) return;

            base.Execute();
        }

        protected override float CalculateUtility(Target target)
        {
            if (!CommonAIStateFunctions.CanAgentMoveFreely(Agent) || Agent.GetAbility(AbilityIndex).IsOnCooldown())
                return 0.0f;

            return base.CalculateUtility(target);
        }
    }
}