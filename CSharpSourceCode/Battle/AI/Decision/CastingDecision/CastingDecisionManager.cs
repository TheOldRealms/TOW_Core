using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Behavior.CastingBehavior;
using TOW_Core.Battle.AI.Components;

namespace TOW_Core.Battle.AI.Decision.CastingDecision
{
    public static class CastingDecisionManager
    {
        public static AgentCastingBehavior ChooseCastingBehavior(Agent agent, WizardAIComponent component)
        {
            var chosenCastingBehavior = component.AvailableCastingBehaviors[0];
            chosenCastingBehavior.TargetFormation = ChooseTargetFormation(agent, component.CurrentCastingBehavior?.TargetFormation);
            return chosenCastingBehavior;
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