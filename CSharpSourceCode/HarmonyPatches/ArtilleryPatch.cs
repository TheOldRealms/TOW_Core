using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class ArtilleryPatch
    {
        [HarmonyPatch(typeof(RangedSiegeWeaponAi.ThreatSeeker), "GetTarget")]
        [HarmonyPrefix]
        public static bool GetTargetPatch(ref Threat __result, RangedSiegeWeaponAi.ThreatSeeker __instance)
        {
            var formation = GetUnemployedEnemyFormations(__instance.Weapon.Side).MaxBy(x => x.CountOfUnits);
            __result = new Threat { Formation = formation, ThreatValue = 1f };
            return false;
        }

        private static IEnumerable<Formation> GetUnemployedEnemyFormations(BattleSideEnum side)
        {
            return from f in (from t in Mission.Current.Teams
                              where t.Side.GetOppositeSide() == side
                              select t).SelectMany((Team t) => t.FormationsIncludingSpecial)
                   where f.CountOfUnits > 0
                   select f;
        }
    }
}
