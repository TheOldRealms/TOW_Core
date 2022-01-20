using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle
{
    public static class TOWBattleUtilities
    {
        public static void DamageAgents(IEnumerable<Agent> agents, int minDamage, int maxDamage = -1, Agent damager = null, TargetType targetType = TargetType.All, bool hasShockWave = false, Vec3 impactPosition = new Vec3())
        {
            foreach (var agent in agents)
            {
                if (maxDamage < minDamage)
                {
                    agent.ApplyDamage(minDamage, damager, doBlow: true, hasShockWave: hasShockWave, impactPosition: impactPosition);
                }
                else
                {
                    agent.ApplyDamage(TOWMath.GetRandomInt(minDamage, maxDamage), damager, doBlow: true, hasShockWave: hasShockWave, impactPosition: impactPosition);
                }
            }
        }

        public static void HealAgents(IEnumerable<Agent> agents, int minHeal, int maxHeal = -1, Agent healer = null, TargetType targetType = TargetType.Friendly)
        {
            foreach (var agent in agents)
            {
                if (maxHeal < minHeal)
                {
                    agent.Heal(minHeal);
                }
                else
                {
                    agent.Heal(TOWMath.GetRandomInt(minHeal, maxHeal));
                }
            }
        }

        public static void ApplyStatusEffectToAgents(IEnumerable<Agent> agents, string effectId, Agent applierAgent, TargetType targetType = TargetType.All)
        {
            foreach (var agent in agents)
            {
                agent.ApplyStatusEffect(effectId, applierAgent);
            }
        }
    }
}
