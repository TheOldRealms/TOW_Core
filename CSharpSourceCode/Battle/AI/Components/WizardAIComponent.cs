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

        private float _dtSinceLastOccasional = (float) TOWMath.GetRandomDouble(0, EvalInterval); //Randomly distribute ticks
        private readonly IAgentBehavior _currentTacticalBehavior;
        private AbstractAgentCastingBehavior _currentCastingBehavior;

        public Mat3 SpellTargetRotation = Mat3.Identity;
        public List<IAgentBehavior> AvailableCastingBehaviors { get; }

        public WizardAIComponent(Agent agent) : base(agent)
        {
            var toRemove = agent.Components.OfType<HumanAIComponent>().ToList();
            foreach (var item in toRemove) // This is intentional. Components is read-only
                agent.RemoveComponent(item);

            _currentTacticalBehavior = new KeepSafeAgentTacticalBehavior(agent, this);
            AvailableCastingBehaviors = new List<IAgentBehavior>(PrepareCastingBehaviors(agent, this));
        }


        public override void OnTickAsAI(float dt)
        {
            _dtSinceLastOccasional += dt;
            if (_dtSinceLastOccasional >= EvalInterval) TickOccasionally();

            _currentTacticalBehavior.Execute();
            _currentCastingBehavior?.Execute();

            base.OnTickAsAI(dt);
        }

        private void TickOccasionally()
        {
            _dtSinceLastOccasional = 0;

            var newBehavior = DecisionManager.DecideCastingBehavior(AvailableCastingBehaviors);
            if (newBehavior.Item1 != _currentCastingBehavior) _currentCastingBehavior?.Terminate();

            _currentCastingBehavior = newBehavior.Item1 as AbstractAgentCastingBehavior;
            if (_currentCastingBehavior != null)
            {
                _currentCastingBehavior.Target = newBehavior.Item2;
                TOWCommon.Say(_currentCastingBehavior?.GetType().Name);
            }
        }


        private static List<AbstractAgentCastingBehavior> PrepareCastingBehaviors(Agent agent, WizardAIComponent component)
        {
            var castingBehaviors = new List<AbstractAgentCastingBehavior>();
            var index = 0;
            foreach (var knownAbilityTemplate in agent.GetComponent<AbilityComponent>().GetKnownAbilityTemplates())
            {
                castingBehaviors.Add(AgentCastingBehaviorMapping.BehaviorByType.GetValueOrDefault(knownAbilityTemplate.AbilityEffectType, AgentCastingBehaviorMapping.BehaviorByType[AbilityEffectType.MovingProjectile])
                    .Invoke(agent, index, knownAbilityTemplate));
                index++;
            }

            if (!IsCustomBattle()) castingBehaviors.Add(new ConserveWindsAgentCastingBehavior(agent, null, index));

            return castingBehaviors;
        }

        private static bool IsCustomBattle()
        {
            return false; //TODO: Not sure how to check this. Dont need magic conservation if not in campaign.
        }
    }
}