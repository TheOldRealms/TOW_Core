﻿using HarmonyLib;
using System;
using TaleWorlds.MountAndBlade.GauntletUI;

namespace TOW_Core.HarmonyPatches
{
    //The purpose of this patch is to restrict the use of the vanilla loading screens. There are 13 of them, while TOW only has as many as totalnum private field has value for.
    //Instead of a patch, we could obviously create 13 assets with the same image, but that would unnecessarily bloat the filesize.
    //Once we have enough 2D art to override all 13 of them, this patch can be safely discarded from the codebase.
    [HarmonyPatch]
    public static class LoadingScreenTempPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoadingWindowViewModel), "SetTotalGenericImageCount")]
        public static void PostFix(ref int ____totalGenericImageCount)
        {
            ____totalGenericImageCount = 7;
        }
    }
}
