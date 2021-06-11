using HarmonyLib;
using System;
using TaleWorlds.MountAndBlade.GauntletUI;

namespace TOW_Core.HarmonyPatches
{
    //The purpose of this patch is to restrict the use of the vanilla loading screens. There are 13 of them, while TOW only has 2 right now.
    //Instead of a patch, we could obviously create 13 assets with the same image, but that would unnecessarily bloat the filesize.
    //Once we have enough 2D art to override all 13 of them, this patch can be safely discarded from the codebase.
    [HarmonyPatch]
    public static class LoadingScreenTempPatch
    {
        private static int nextnum = 1;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoadingWindowViewModel), "SetNextGenericImage")]
        public static void PostFix(ref LoadingWindowViewModel __instance, int ____currentImage)
        {
            if(____currentImage > 2)
            {
                __instance.LoadingImageName = "loading_" + nextnum.ToString("00");
                nextnum = nextnum == 1 ? 2 : 1;
            }
        }
    }
}
