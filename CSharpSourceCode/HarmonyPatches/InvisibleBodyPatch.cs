using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class InvisibleBodyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemObject), "UsingFacegenScaling", MethodType.Getter)]
        public static void Postfix(ref bool __result, ItemObject __instance)
        {
            __result = false;
        }
 
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MBEquipmentMissionExtensions), "GetSkinMeshesMask")]
        public static void Postfix(Equipment equipment, ref SkinMask __result)
        {
            var elem = equipment.GetEquipmentFromSlot(EquipmentIndex.Body);
            if(elem.Item != null && elem.Item.StringId != null)
            {
                if (elem.Item.StringId.Contains("hideskin"))
                {
                    __result = SkinMask.NoneVisible;
                }
            }
        }
    }
}
