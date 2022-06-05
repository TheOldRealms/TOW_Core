using HarmonyLib;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using TOW_Core.CampaignSupport.QuestBattleLocation;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    internal class SettlementPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Settlement), "Deserialize")]
        public static void DeserializePostfix(MBObjectManager objectManager, XmlNode node, Settlement __instance)
        {
            if (__instance.GetComponent<QuestBattleComponent>() != null)
            {
                Clan clan = objectManager.ReadObjectReferenceFromXml<Clan>("owner", node);
                if (clan != null)
                {
                    __instance.GetComponent<QuestBattleComponent>().SetClan(clan);
                }
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Settlement))]
        [HarmonyPatch("OwnerClan", MethodType.Getter)]
        public static bool OwnerClanPrefix(ref Clan __result, Settlement __instance)
        {
            if (__instance.GetComponent<QuestBattleComponent>() != null)
            {
                __result = __instance.GetComponent<QuestBattleComponent>().OwnerClan;
                return false;
            }
            return true;
        }
    }
}
