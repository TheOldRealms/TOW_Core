using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public static class CastingBehaviorUtils
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AgentCastingBehavior>> BehaviorByType =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AgentCastingBehavior>>
            {
                {AbilityEffectType.MovingProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.CenteredStaticAOE, (agent, abilityTemplate, abilityIndex) => new CenteredStaticAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DirectionalMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
            };

    }
}