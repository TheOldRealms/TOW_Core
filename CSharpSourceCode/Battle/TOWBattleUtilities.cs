using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle
{
    public static class TOWBattleUtilities
    {
        public static void DamageAgentsInArea(Vec2 center, float radius, int minDamage, int maxDamage = -1, Agent damager = null, TargetType targetType = TargetType.All, bool hasShockWave = false)
        {
            var list = new List<Agent>();
            if (targetType == TargetType.Enemy && damager != null)
            {
                list = Mission.Current.GetNearbyEnemyAgents(center, radius, damager.Team).ToList();
            }
            else if(targetType == TargetType.All)
            {
                list = Mission.Current.GetNearbyAgents(center, radius).ToList();
            }
            foreach(var agent in list)
            {
                if(maxDamage < minDamage)
                {
                    agent.ApplyDamage(minDamage, damager, hasShockWave:hasShockWave);
                }
                else
                {
                    agent.ApplyDamage(TOW_Core.Utilities.TOWMath.GetRandomInt(minDamage, maxDamage), damager, hasShockWave: hasShockWave);
                }
            }
        }

        public static void ApplyStatusEffectToAgentsInArea(Vec2 center, float radius, string effectId, Agent damager = null, TargetType targetType = TargetType.All)
        {
            var list = new List<Agent>();
            if (targetType == TargetType.Enemy && damager != null)
            {
                list = Mission.Current.GetNearbyEnemyAgents(center, radius, damager.Team).ToList();
            }
            else if(targetType == TargetType.All)
            {
                list = Mission.Current.GetNearbyAgents(center, radius).ToList();
            }
            foreach (var agent in list)
            {
                agent.ApplyStatusEffect(effectId);
            }

        }
    }
}
