using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public static class AgentCastingBehaviorMapping
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>> BehaviorByType =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>>
            {
                {AbilityEffectType.MovingProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DynamicProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.CenteredStaticAOE, (agent, abilityTemplate, abilityIndex) => new CenteredStaticAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.TargetedStaticAOE, (agent, abilityTemplate, abilityIndex) => new TargetedStaticAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.RandomMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DirectionalMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
            };

        public static readonly Dictionary<Type, List<Axis>> UtilityByType =
            new Dictionary<Type, List<Axis>>
            {
                {typeof(CenteredStaticAoEAgentCastingBehavior), CreateMovingProjectileAxis()},
                {typeof(ConserveWindsAgentCastingBehavior), CreateMovingProjectileAxis()},
                {typeof(DirectionalMovingAoEAgentCastingBehavior), CreateMovingProjectileAxis()},
                {typeof(MovingProjectileAgentCastingBehavior), CreateMovingProjectileAxis()},
                {typeof(TargetedStaticAoEAgentCastingBehavior), CreateMovingProjectileAxis()},
            };


        public static List<Axis> CreateMovingProjectileAxis()
        {
            return new List<Axis>();
        }
    }
}