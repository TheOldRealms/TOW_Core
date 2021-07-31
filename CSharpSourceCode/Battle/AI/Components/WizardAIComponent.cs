using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Behavior;
using TOW_Core.Battle.AI.Behavior.CastingBehavior;
using TOW_Core.Battle.AI.Behavior.TacticalBehavior;
using TOW_Core.Battle.AI.Decision.CastingDecision;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Components
{
    public class WizardAIComponent : HumanAIComponent
    {
        private const float EvalInterval = 1;
        private float _dtSinceLastOccasional = EvalInterval;
        private AgentCombatBehavior _tacticalBehavior;

        public Mat3 SpellTargetRotation = Mat3.Identity;
        public AgentCastingBehavior CurrentCastingBehavior;
        public List<AgentCastingBehavior> AvailableCastingBehaviors { get; }

        public WizardAIComponent(Agent agent) : base(agent)
        {
            var toRemove = agent.Components.OfType<HumanAIComponent>().ToList();
            foreach (var item in toRemove) // This is intentional. Components is read-only
                agent.RemoveComponent(item);

            _tacticalBehavior = new KeepSafeTacticalBehavior(agent, this);
            AvailableCastingBehaviors = PrepareCastingBehaviors(agent, this);
        }


        public override void OnTickAsAI(float dt)
        {
            _dtSinceLastOccasional += dt;
            if (_dtSinceLastOccasional >= EvalInterval) TickOccasionally();

            _tacticalBehavior.ApplyBehaviorParams();
            CurrentCastingBehavior?.Execute();

            base.OnTickAsAI(dt);
        }

        private void TickOccasionally()
        {
            _dtSinceLastOccasional = 0;
            CurrentCastingBehavior = CastingDecisionManager.ChooseCastingBehavior(Agent, this);
            Agent.SelectAbility(CurrentCastingBehavior.AbilityIndex);
        }


        private static List<AgentCastingBehavior> PrepareCastingBehaviors(Agent agent, WizardAIComponent component)
        {
            var castingBehaviors = new List<AgentCastingBehavior>();
            var index = 0;
            foreach (var knownAbilityTemplate in agent.GetComponent<AbilityComponent>().GetKnownAbilityTemplates())
            {
                castingBehaviors.Add(AgentCastingBehavior.CastingBehaviorsByAbilityEffect[knownAbilityTemplate.AbilityEffectType].Invoke(agent, index, knownAbilityTemplate));
                index++;
            }

            return castingBehaviors;
        }
    }
}