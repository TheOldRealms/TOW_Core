using System;
using HarmonyLib;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TOW_Core.CharacterCreation;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CharacterCreationPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterCreationCultureStageVM), "SortCultureList")]
        public static bool Prefix(MBBindingList<CharacterCreationCultureVM> listToWorkOn)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SandBoxGameManager), "LaunchSandboxCharacterCreation")]
        public static bool Prefix2(SandBoxGameManager __instance)
        {
            CharacterCreationState gameState = Game.Current.GameStateManager.CreateState<CharacterCreationState>(new object[]
            {
                new TOWCharacterCreationContent()
            });
            Game.Current.GameStateManager.CleanAndPushState(gameState, 0);
            return false;
        }


        //TODO: get rid of this ASAP. need to create specific culture traits. This is only text this way.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterCreationCultureVM), MethodType.Constructor, new Type[] { typeof(CultureObject), typeof(Action<CharacterCreationCultureVM>) })]
        public static void Postfix(CultureObject culture, CharacterCreationCultureVM __instance)
        {
            if (culture.StringId == "empire")
            {
                __instance.PositiveEffectText = "+20% party size, access to knightly orders, 20% more xp from training for lower tier units";
            }
            else if (culture.StringId == "khuzait")
            {
                __instance.PositiveEffectText = "+75% party size, access to necromancy, access to blood knight orders";
            }
        }
    }
}
