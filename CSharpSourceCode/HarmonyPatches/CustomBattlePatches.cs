using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;

namespace TOW_Core.HarmonyPatches
{
    //This is an insanely hacky class to mould Custom Battle to TOW needs. Does not effect other aspects of gameplay (campaign or else).
    [HarmonyPatch]
    public static class CustomBattlePatches
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBattleHelper), "GetDefaultTroopOfFormationForFaction")]
        public static void Postfix(ref BasicCharacterObject __result, BasicCultureObject culture)
        {
            switch (culture.GetCultureCode())
            {
                case CultureCode.Empire:
                    __result = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("tor_empire_recruit");
                    break;
                case CultureCode.Khuzait:
                    __result = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("tor_vc_skeleton_recruit");
                    break;
                default:
                    __result = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("tor_empire_recruit");
                    break;
            }
        }

        //Fill available characters
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBattleData), "Characters", MethodType.Getter)]
        public static void Postfix2(ref IEnumerable<BasicCharacterObject> __result)
        {
            var list = new List<BasicCharacterObject>();
            try
            {
                //Ideally this should not be hardcoded. 
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("tor_emp_lord"));
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("tor_vc_lord"));
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("tor_wizard_lord"));
                list.Add(Game.Current.ObjectManager.GetObject<BasicCharacterObject>("tor_necromancer_lord"));
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
                list.Add(Game.Current.ObjectManager.GetObject<BasicCultureObject>("chaos_culture"));
            }
            catch (Exception e)
            {
                TOWCommon.Log(e.Message, NLog.LogLevel.Error);
            }
            if (list.Count > 1) __result = list;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ArmyCompositionItemVM), "IsValidUnitItem")]
        public static bool Prefix(ref ArmyCompositionItemVM __instance, BasicCharacterObject o, ref bool __result, BasicCultureObject ____culture, ArmyCompositionItemVM.CompositionType ____type)
        {
            if (o != null && o.StringId.StartsWith("tor_") && o.Culture.StringId == ____culture.StringId && o.DefaultFormationClass == GetFormationFor(____type))
            {
                __result = true;
            }
            else __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomGame), "LoadCustomBattleScenes")]
        public static void Postfix5(ref CustomGame __instance, ref XmlDocument doc)
        {
            var path = Path.Combine(BasePath.Name, "Modules/TOR_Environment/ModuleData/tow_custombattlescenes.xml");
            if (File.Exists(path))
            {
                XmlDocument moredoc = new XmlDocument();
                moredoc.Load(path);
                doc = MBObjectManager.MergeTwoXmls(doc, moredoc);
            }
        }

        private static FormationClass GetFormationFor(ArmyCompositionItemVM.CompositionType type)
        {
            switch (type)
            {
                case ArmyCompositionItemVM.CompositionType.MeleeInfantry:
                    return FormationClass.Infantry;
                case ArmyCompositionItemVM.CompositionType.RangedInfantry:
                    return FormationClass.Ranged;
                case ArmyCompositionItemVM.CompositionType.MeleeCavalry:
                    return FormationClass.Cavalry;
                case ArmyCompositionItemVM.CompositionType.RangedCavalry:
                    return FormationClass.HorseArcher;
                default:
                    return FormationClass.Infantry;
            }
        }
    }
}
