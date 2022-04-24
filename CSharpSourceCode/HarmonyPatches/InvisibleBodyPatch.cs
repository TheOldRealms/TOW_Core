using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
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
            List<EquipmentElement> list = new List<EquipmentElement>();
            for(int i = (int)EquipmentIndex.ArmorItemBeginSlot; i > (int)EquipmentIndex.ArmorItemEndSlot; i++)
            {
                list.Add(equipment.GetEquipmentFromSlot((EquipmentIndex)i));
            }
            if(list.Count > 0)
            {
                if (list.Any(x => x.Item != null && x.Item.StringId.Contains("hideskin")))
                {
                    __result = SkinMask.NoneVisible;
                }
            }
        }
    }
}
