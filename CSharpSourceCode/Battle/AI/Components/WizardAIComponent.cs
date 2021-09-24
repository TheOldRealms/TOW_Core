using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Components
{
    public class WizardAIComponent : HumanAIComponent
    {
        private static readonly float EvalInterval = 1;

        private List<IAgentBehavior> _availableCastingBehaviors;
        private float _dtSinceLastOccasional = (float) TOWMath.GetRandomDouble(0, EvalInterval); //Randomly distribute ticks
        private readonly IAgentBehavior _currentTacticalBehavior;
        public AbstractAgentCastingBehavior CurrentCastingBehavior;

        public Mat3 SpellTargetRotation = Mat3.Identity;

        public List<IAgentBehavior> AvailableCastingBehaviors => _availableCastingBehaviors ?? (_availableCastingBehaviors = new List<IAgentBehavior>(PrepareCastingBehaviors(Agent)));

        public WizardAIComponent(Agent agent) : base(agent)
        {
            var toRemove = agent.Components.OfType<HumanAIComponent>().ToList();
            foreach (var item in toRemove) // This is intentional. Components is read-only
                agent.RemoveComponent(item);

            _currentTacticalBehavior = new KeepSafeAgentTacticalBehavior(agent, this);
        }


        public override void OnTickAsAI(float dt)
        {
            _dtSinceLastOccasional += dt;
            if (_dtSinceLastOccasional >= EvalInterval) TickOccasionally();

            _currentTacticalBehavior.Execute();
            CurrentCastingBehavior?.Execute();

            base.OnTickAsAI(dt);
        }

        private void TickOccasionally()
        {
            _dtSinceLastOccasional = 0;
            CurrentCastingBehavior = DetermineBehavior(AvailableCastingBehaviors, CurrentCastingBehavior);
        }

        private AbstractAgentCastingBehavior DetermineBehavior(List<IAgentBehavior> availableCastingBehaviors, AbstractAgentCastingBehavior current)
        {
            var (newBehavior, target) = DecisionManager.EvaluateCastingBehaviors(availableCastingBehaviors);
            if (newBehavior != current) current?.Terminate();

            var returnBehavior = newBehavior as AbstractAgentCastingBehavior;
            if (returnBehavior != null)
            {
                returnBehavior.CurrentTarget = target;
            }

            return returnBehavior;
        }

        private static List<AbstractAgentCastingBehavior> PrepareCastingBehaviors(Agent agent)
        {
            var castingBehaviors = new List<AbstractAgentCastingBehavior>();
            var index = 0;
            foreach (var knownAbilityTemplate in agent.GetComponent<AbilityComponent>().GetKnownAbilityTemplates())
            {
                castingBehaviors.Add(AgentCastingBehaviorMapping.BehaviorByType.GetValueOrDefault(knownAbilityTemplate.AbilityEffectType, AgentCastingBehaviorMapping.BehaviorByType[AbilityEffectType.MovingProjectile])
                    .Invoke(agent, index, knownAbilityTemplate));
                index++;
            }

            castingBehaviors.Add(new PreserveWindsAgentCastingBehavior(agent, new AbilityTemplate {AbilityTargetType = AbilityTargetType.Self}, index));

            return castingBehaviors;
        }
    }
}