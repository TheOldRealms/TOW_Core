using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public static class CastingBehaviorMapping
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AgentCastingBehavior>> BehaviorByType =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AgentCastingBehavior>>
            {
                {AbilityEffectType.MovingProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DynamicProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.CenteredStaticAOE, (agent, abilityTemplate, abilityIndex) => new CenteredStaticAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.TargetedStaticAOE, (agent, abilityTemplate, abilityIndex) => new TargetedStaticAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.RandomMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DirectionalMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
            };

        public static readonly Dictionary<Type, List<Axis>> UtilityByType =
            new Dictionary<Type, List<Axis>>
            {
                {typeof(CenteredStaticAoECastingBehavior), CreateMovingProjectileAxis()},
                {typeof(ConserveWindsCastingBehavior), CreateMovingProjectileAxis()},
                {typeof(DirectionalMovingAoECastingBehavior), CreateMovingProjectileAxis()},
                {typeof(MovingProjectileCastingBehavior), CreateMovingProjectileAxis()},
                {typeof(TargetedStaticAoECastingBehavior), CreateMovingProjectileAxis()},
            };


        public static List<Axis> CreateMovingProjectileAxis()
        {
            return new List<Axis>();
        }
    }
}