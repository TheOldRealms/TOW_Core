using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Behavior.CastingBehavior;
using TOW_Core.Battle.AI.Behavior.TacticalBehavior;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Battle.AI.Decision.ScoringFunction;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Components
{
    public class WizardAIComponent : HumanAIComponent
    {
        private const float EvalInterval = 1;
        private readonly List<IUtilityObject> _utilityObjects;

        private float _dtSinceLastOccasional = EvalInterval;
        private AgentCombatBehavior _tacticalBehavior;
        private AgentCastingBehavior _currentCastingBehavior;

        public Mat3 SpellTargetRotation = Mat3.Identity;
        public List<AgentCastingBehavior> AvailableCastingBehaviors { get; }

        public WizardAIComponent(Agent agent) : base(agent)
        {
            var toRemove = agent.Components.OfType<HumanAIComponent>().ToList();
            foreach (var item in toRemove) // This is intentional. Components is read-only
                agent.RemoveComponent(item);

            _tacticalBehavior = new KeepSafeTacticalBehavior(agent, this);
            AvailableCastingBehaviors = PrepareCastingBehaviors(agent, this);
            _utilityObjects = new List<IUtilityObject>(AvailableCastingBehaviors);
        }


        public override void OnTickAsAI(float dt)
        {
            if (!Mission.Current.IsFriendlyMission)
            {
                _dtSinceLastOccasional += dt;
                if (_dtSinceLastOccasional >= EvalInterval) TickOccasionally();

                _tacticalBehavior.ApplyBehaviorParams();
                _currentCastingBehavior?.Execute();
            }

            base.OnTickAsAI(dt);
        }

        private void TickOccasionally()
        {
            _dtSinceLastOccasional = 0;

            var newBehavior = DecisionManager.DecideCastingBehavior(_utilityObjects);
            if (newBehavior != _currentCastingBehavior) _currentCastingBehavior?.Terminate();

            _currentCastingBehavior = newBehavior as AgentCastingBehavior;
        }


        private static List<AgentCastingBehavior> PrepareCastingBehaviors(Agent agent, WizardAIComponent component)
        {
            var castingBehaviors = new List<AgentCastingBehavior>();
            var index = 0;
            foreach (var knownAbilityTemplate in agent.GetComponent<AbilityComponent>().GetKnownAbilityTemplates())
            {
                castingBehaviors.Add(CastingBehaviorMapping.BehaviorByType.GetValueOrDefault(knownAbilityTemplate.AbilityEffectType, CastingBehaviorMapping.BehaviorByType[AbilityEffectType.MovingProjectile])
                    .Invoke(agent, index, knownAbilityTemplate));
                index++;
            }

            if (!IsCustomBattle()) castingBehaviors.Add(new ConserveWindsCastingBehavior(agent, null, index));

            return castingBehaviors;
        }

        private static bool IsCustomBattle()
        {
            return false; //TODO: Not sure how to check this. Dont need magic conservation if not in campaign.
        }
    }
}