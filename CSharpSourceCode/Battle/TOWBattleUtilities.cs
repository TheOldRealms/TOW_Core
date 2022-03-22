using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.Damage;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle
{
    public static class TOWBattleUtilities
    {
        public static void DamageAgents(IEnumerable<Agent> agents, int minDamage, int maxDamage = -1, Agent damager = null, TargetType targetType = TargetType.All, string spellID="",DamageType damageType=DamageType.Physical, bool hasShockWave = false, Vec3 impactPosition = new Vec3())
        {
            if (agents != null)
            {
                foreach (var agent in agents)
                {
                    if (spellID != "" && damager != null)
                        SpellBlowInfoManager.EnqueueSpellInfo(agent.Index, damager.Index, spellID, damageType);

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
        }

        public static void HealAgents(IEnumerable<Agent> agents, int minHeal, int maxHeal = -1, Agent healer = null, TargetType targetType = TargetType.Friendly)
        {
            if(agents != null)
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
        }

        public static void ApplyStatusEffectToAgents(IEnumerable<Agent> agents, string effectId, Agent applierAgent, TargetType targetType = TargetType.All)
        {
            if(agents != null)
            {
                foreach (var agent in agents)
                {
                    agent.ApplyStatusEffect(effectId, applierAgent);
                }
            }
        }
    }
}
