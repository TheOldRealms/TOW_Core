using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;
using TaleWorlds.Localization;
using SandBox;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using System.Reflection.Emit;
using TaleWorlds.Engine;

//Need a way to somehow skip loading of vanilla xmls in the following categories:
//Settlements, Clans, Kingdoms, Heroes

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CampaignPatches
    {
        private static readonly Dictionary<string, string> _typesToForce = new Dictionary<string, string>() 
        {
            {"Settlements", "tow_settlements.xml"},
            {"Heroes", "tow_heroes.xml"},
            {"Kingdoms", "tow_kingdoms.xml"},
            {"Factions", "tow_clans.xml"},
        };

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MBObjectManager), "LoadXML")]
        public static bool Prefix(string id, MBObjectManager __instance)
        {
            if (_typesToForce.ContainsKey(id))
            {
                try
                {
                    var path = System.IO.Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/" + _typesToForce[id]);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    __instance.LoadXml(doc, null);
                    return false;
                }
                catch (Exception)
                {
                    TOWCommon.Log("Error in MBObjectManagerPatch, tried to force load TOW specific xml but failed.", NLog.LogLevel.Error);
                    return true;
                }
            }
            else return true;
        }

        //Only spawn wanderers for empire and khuzait, the rest crashes because there are no settlements for the other cultures.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior), "CreateCompanion")]
        public static bool Prefix2(CharacterObject companionTemplate)
        {
            if (companionTemplate == null || !companionTemplate.Culture.IsMainCulture)
            {
                return false;
            }
            else return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingdom), "InitialHomeLand", MethodType.Getter)]
        public static void Postfix(ref Settlement __result, Kingdom __instance)
        {
            if(__result == null)
            {
                __result = Settlement.All.FirstOrDefault((Settlement x) => x.IsTown && x.MapFaction == __instance.MapFaction);
                if (__result == null)
                {
                    __result = Settlement.All.FirstOrDefault((Settlement x) => x.IsTown);
                }
            }
        }

        //Ideally this should not need a harmony patch, but somehow removing this on gamestart in submodule.cs still makes it run on NewGameStart event.
        //This behaviour contains hardcoded hero / lord references that are not present because we skip loading the vanilla files.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BackstoryCampaignBehavior), "RegisterEvents")]
        public static bool Prefix3()
        {
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Module), "GetInitialStateOptions")]
        public static void Postfix2(ref IEnumerable<InitialStateOption> __result)
        {
            List<InitialStateOption> newlist = new List<InitialStateOption>();
            newlist = __result.Where(x => x.Id != "StoryModeNewGame" && x.Id != "SandBoxNewGame").ToList();
            var towOption = new InitialStateOption("TOWNewgame", new TextObject("Enter the Old World"),3,OnCLick,IsDisabledAndReason);
            newlist.Add(towOption);
            newlist.Sort((x, y) => x.OrderIndex.CompareTo(y.OrderIndex));
            __result = newlist;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DefaultMapDistanceModel), "LoadCacheFromFile")]
        public static bool Prefix4(ref System.IO.BinaryReader reader)
        {
            reader = null;
            return true;
        }

        private static void OnCLick()
        {
            MBGameManager.StartNewGame(new SandBoxGameManager());
        }

        private static (bool, TextObject) IsDisabledAndReason()
        {
            TextObject coreContentDisabledReason = new TextObject("{=V8BXjyYq}Disabled during installation.", null);
            return new ValueTuple<bool, TextObject>(Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapScene), "Load")]
        public static bool Prefix5(MapScene __instance, ref Scene ____scene, ref MBAgentRendererSceneController ____agentRendererSceneController)
        {
            Debug.Print("Creating map scene", 0, Debug.DebugColor.White, 17592186044416UL);
            ____scene = Scene.CreateNewScene(false);
            ____scene.SetName("MapScene");
            ____scene.SetClothSimulationState(true);
            ____agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(____scene, 4096);
            ____scene.SetOcclusionMode(true);
            SceneInitializationData initData = new SceneInitializationData(true);
            initData.UsePhysicsMaterials = false;
            initData.EnableFloraPhysics = false;
            initData.UseTerrainMeshBlending = false;
            Debug.Print("reading map scene", 0, Debug.DebugColor.White, 17592186044416UL);
            ____scene.Read("modded_main_map", initData, "");
            TaleWorlds.Engine.Utilities.SetAllocationAlwaysValidScene(____scene);
            ____scene.DisableStaticShadows(true);
            ____scene.InvalidateTerrainPhysicsMaterials();
            MBMapScene.LoadAtmosphereData(____scene);
            __instance.DisableUnwalkableNavigationMeshes();
            MBMapScene.ValidateTerrainSoundIds();
            ____scene.OptimizeScene(true, false);
            Debug.Print("Ticking map scene for first initialization", 0, Debug.DebugColor.White, 17592186044416UL);
            ____scene.Tick(0.1f);
            return false;
        }
    }
}
