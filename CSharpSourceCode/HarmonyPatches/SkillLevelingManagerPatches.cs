using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public class SkillLevelingManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillLevelingManager), "OnSurgeryApplied")]
        public static bool OnSurgeryAppliedPrefix(ref MobileParty party)
        {
            if (party == null)
            {
                return false;
            }
            return true;
        }
    }
}
