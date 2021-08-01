using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Behavior.CastingBehavior;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.Decision.CastingDecision
{
    public static class CastingDecisionManager
    {
        public static AgentCastingBehavior ChooseCastingBehavior(Agent agent, WizardAIComponent component)
        {
            var targetFormation = ChooseTargetFormation(agent, component.CurrentCastingBehavior?.TargetFormation);
            var chosenCastingBehavior = DecideCastingBehavior(agent, component, targetFormation);
            chosenCastingBehavior.TargetFormation = targetFormation;

            return chosenCastingBehavior;
        }

        private static AgentCastingBehavior DecideCastingBehavior(Agent agent, WizardAIComponent component, Formation targetFormation)
        {
            if (agent.Position.AsVec2.Distance(targetFormation.CurrentPosition) < 40)
            {
                var agentCastingBehavior = component.AvailableCastingBehaviors.Find(behavior => behavior.GetType() == typeof(DirectionalMovingAoECastingBehavior) && !agent.GetAbility(behavior.AbilityIndex).IsOnCooldown());
                if (agentCastingBehavior != null) return agentCastingBehavior;
            }

            return component.AvailableCastingBehaviors[0];
        }

        private static Formation ChooseTargetFormation(Agent agent, Formation targetFormation)
        {
            var formation = agent?.Formation?.QuerySystem?.ClosestEnemyFormation?.Formation;
            if (!(formation != null && (targetFormation == null || !formation.HasPlayer || formation.Distance < targetFormation.Distance && formation.Distance < 15 || targetFormation.GetFormationPower() < 15)))
            {
                formation = targetFormation;
            }

            return formation;
        }
    }
}