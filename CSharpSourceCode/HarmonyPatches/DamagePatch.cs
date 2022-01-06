using System.Windows.Forms.VisualStyles;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;
using TaleWorlds.TwoDimension;
using TOW_Core.Battle.Damage;
using TOW_Core.Battle.StatusEffects;
using TOW_Core.Items;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class DamagePatch
    {
        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(Agent), "HandleBlow")]
        public static bool PreHandleBlow(ref Blow b, ref Agent __instance)
        {
            //check for new Damage system, will change that 



            return true;

        }*/
    }
}