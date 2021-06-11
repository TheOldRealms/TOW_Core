using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class UndeadBleedParticlePatch
    {
        /**
         * In DecideAgentHitParticles, the "victim" agent's hit particles are set. Usually this is sweat or blood particles.
         * This prefix allows you to set the particles for Undead units specifically.
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mission), "DecideAgentHitParticles")]
        public static bool Prefix(Agent victim)
        {
            if(victim != null && victim.IsUndead())
            {
                return false;
            }
            return true;
        }
    }
}
