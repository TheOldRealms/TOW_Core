using System;
using System.Collections.Generic;
using System.Xml;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer;
using TaleWorlds.ObjectSystem;
using TOW_Core.CustomBattles;
using TOW_Core.Utilities;

namespace TOW_Core.HarmonyPatches
{
    //This is an insanely hacky class to mould Custom Battle to TOW needs. Does not effect other aspects of gameplay (campaign or else).
    [HarmonyPatch]
    public static class CustomBattlePatches
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBattleState.Helper), "GetDefaultTroopOfFormationForFaction")]
        public static void Postfix(ref BasicCharacterObject __result, BasicCultureObject culture, FormationClass formation)
        {
            var obj = CustomBattleTroopManager.GetTroopObjectFor(culture, formation);
            if (obj != null) __result = obj;
        }

        //Fill available characters
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBattleData), "Characters", MethodType.Getter)]
        public static void Postfix2(ref IEnumerable<BasicCharacterObject> __result)
        {
            var list = new List<BasicCharacterObject>();
            try
            {
                //Ideally this should not be hardcoded. Maybe create a custombattlelords xml template and load that?
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("emp_lord"));
                //list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("mannfred"));
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("vc_lord"));
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("wizard_lord"));
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("necromancer_lord"));
                //list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("krell")); 
            }
            catch (Exception e)
            {
                TOWCommon.Log(e.Message, NLog.LogLevel.Error);
            }
            if (list.Count > 1) __result = list;
        }

        //Fill available cultures
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBattleData), "Factions", MethodType.Getter)]
        public static void Postfix3(ref IEnumerable<BasicCultureObject> __result)
        {
            var list = new List<BasicCultureObject>();
            try
            {
                //Ideally this should not be hardcoded. Maybe create a custombattlecultures xml template and load that?
                list.Add(Game.Current.ObjectManager.GetObject<BasicCultureObject>("empire"));
                list.Add(Game.Current.ObjectManager.GetObject<BasicCultureObject>("khuzait"));
            }
            catch (Exception e)
            {
                TOWCommon.Log(e.Message, NLog.LogLevel.Error);
            }
            if (list.Count > 1) __result = list;
        }


        //This is prime example of Taleworlds hardcoding simple color strings. Need to override.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WidgetsMultiplayerHelper), "GetFactionColorCode")]
        public static void Postfix4(ref string __result, string lowercaseFactionCode, bool useSecondary)
        {
            if (useSecondary)
            {
                if (lowercaseFactionCode == "empire")
                {
                    __result = "#ED3F16FF";
                }
                if (lowercaseFactionCode == "khuzait")
                {
                    __result = "#ED3F16FF";
                }
            }
            else
            {
                if (lowercaseFactionCode == "empire")
                {
                    __result = "#F8F2F0FF";
                }
                if (lowercaseFactionCode == "khuzait")
                {
                    __result = "#2E2727FF";
                }
            }
        }
    }
}
