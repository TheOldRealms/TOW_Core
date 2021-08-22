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
    }
}
