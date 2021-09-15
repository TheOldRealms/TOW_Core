using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOW_Core.CampaignSupport;
using TOW_Core.Utilities;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class MainMenuPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Module), "GetInitialStateOptions")]
        public static void MainMenuSkipStoryMode(ref IEnumerable<InitialStateOption> __result)
        {
            List<InitialStateOption> newlist = new List<InitialStateOption>();
            newlist = __result.Where(x => x.Id != "StoryModeNewGame" && x.Id != "SandBoxNewGame").ToList();
            var towOption = new InitialStateOption("TOWNewgame", new TextObject("Enter the Old World"), 3, OnCLick, IsDisabledAndReason);
            var towOption2 = new InitialStateOption("TOWForceLoad", new TextObject("Run this first"), 4, OnForceClick, IsDisabledAndReason);
            newlist.Add(towOption);
            newlist.Add(towOption2);
            newlist.Sort((x, y) => x.OrderIndex.CompareTo(y.OrderIndex));
            __result = newlist;
        }

        private static void OnForceClick()
        {
            DisplayWindow();
        }

        private static void DisplayWindow()
        {
            var data = new InquiryData(
                "Important warning",
                "This will load a scene with all the unique troops and NPCs present in our mod. The purpose of this is to compile the local shader cache on your PC.\n" +
                "THIS WILL TAKE A LONG TIME!!!" +
                "Our users report anything between 20 and 70 minutes." +
                "This is ensures that you won't need to compile the shaders individually during normal gameplay, as it can cause issues with stability." +
                "This is meant to reduce the number of UI portrait generation crashes and also eliminate the long battle loading times during normal gameplay.",
                true,
                false,
                "Understood",
                null,
                BuildShaderCache,
                null
                );
            InformationManager.ShowInquiry(data);
        }

        private static void BuildShaderCache()
        {
            MBGameManager.StartNewGame(new ShaderGameManager());
        }

        private static void OnCLick()
        {
            MBGameManager.StartNewGame(new TowCampaignGameManager());
        }

        private static (bool, TextObject) IsDisabledAndReason()
        {
            TextObject coreContentDisabledReason = new TextObject("{=V8BXjyYq}Disabled during installation.", null);
            return new ValueTuple<bool, TextObject>(Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason);
        }

    }
}
