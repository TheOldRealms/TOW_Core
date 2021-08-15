﻿using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public class CenteredStaticAoECastingBehavior : AgentCastingBehavior
    {
        public CenteredStaticAoECastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }

        protected override float UtilityFunction()
        {
            if (Agent.GetAbility(AbilityIndex).IsOnCooldown())
            {
                return 0.0f;
            }

            return 0.5f;
        }
    }
}