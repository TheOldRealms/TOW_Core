using HarmonyLib;
using MountAndBlade.CampaignBehaviors;
using SandBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation.OptionsStage;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities;
using TOW_Core.CampaignSupport.MapBar;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

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
            //{"SPCultures", "tow_cultures.xml" }
        };

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MBObjectManager), "LoadXML")]
        public static bool ForceLoadCertainTypes(string id, MBObjectManager __instance)
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecruitmentCampaignBehavior), "FindRandomMercenaryTroop")]
        public static void OverrideMercenaryTypes(CharacterObject mercenaryTroop, ref float __result, ref CharacterObject ____selectedTroop)
        {
            if (____selectedTroop == null) return;
            if (!____selectedTroop.IsTOWTemplate())
            {
                ____selectedTroop = CharacterObject.All.GetRandomElementWithPredicate(x => x.IsTOWTemplate() && x.StringId.StartsWith("tor_dog_"));
            }
            __result = 1;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CaravanPartyComponent), "InitializeCaravanOnCreation")]
        public static bool CaravanLeaderOverride(MobileParty mobileParty, Hero caravanLeader, ItemRoster caravanItems, int troopToBeGiven, CaravanPartyComponent __instance)
        {
            __instance.MobileParty.Aggressiveness = 0f;

            if (troopToBeGiven == 0)
            {
                float num;
                if (MBRandom.RandomFloat < 0.67f)
                {
                    num = (1f - MBRandom.RandomFloat * MBRandom.RandomFloat) * 0.5f + 0.5f;
                }
                else
                {
                    num = 1f;
                }
                int num2 = (int)((float)mobileParty.Party.PartySizeLimit * num);
                if (num2 >= 10)
                {
                    num2--;
                }
                troopToBeGiven = num2;
            }
            mobileParty.InitializeMobilePartyAtPosition(__instance.Settlement.Culture.CaravanPartyTemplate, __instance.Settlement.GatePosition, troopToBeGiven);
            if (caravanLeader != null)
            {
                mobileParty.MemberRoster.AddToCounts(caravanLeader.CharacterObject, 1, true, 0, 0, true, -1);
            }
            else
            {
                CharacterObject character2 = __instance.Settlement.Culture.CaravanMaster;
                mobileParty.MemberRoster.AddToCounts(character2, 1, true, 0, 0, true, -1);
            }
            mobileParty.Party.Visuals.SetMapIconAsDirty();
            mobileParty.InitializePartyTrade(10000 + ((__instance.Owner.Clan == Clan.PlayerClan) ? 5000 : 0));
            if (caravanItems != null)
            {
                mobileParty.ItemRoster.Add(caravanItems);
                return false;
            }
            float num3 = 10000f;
            ItemObject itemObject = null;
            foreach (ItemObject itemObject2 in TaleWorlds.CampaignSystem.Items.All)
            {
                if (itemObject2.ItemCategory == DefaultItemCategories.PackAnimal && !itemObject2.NotMerchandise && (float)itemObject2.Value < num3)
                {
                    itemObject = itemObject2;
                    num3 = (float)itemObject2.Value;
                }
            }
            if (itemObject != null)
            {
                mobileParty.ItemRoster.Add(new ItemRosterElement(itemObject, (int)((float)mobileParty.MemberRoster.TotalManCount * 0.5f), null));
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingdom), "InitialHomeLand", MethodType.Getter)]
        public static void InitialHomeLandFix(ref Settlement __result, Kingdom __instance)
        {
            if (__result == null)
            {
                __result = Settlement.All.FirstOrDefault((Settlement x) => x.IsTown && x.MapFaction == __instance.MapFaction);
                if (__result == null)
                {
                    __result = Settlement.All.FirstOrDefault((Settlement x) => x.IsTown);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterCreationOptionsStageVM), MethodType.Constructor,
            typeof(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation), typeof(Action), typeof(TextObject),
            typeof(Action), typeof(TextObject), typeof(int), typeof(int), typeof(int), typeof(Action<int>))]
        public static void RemoveLifeDeathCycleOptionFromCharacterCreation(CharacterCreationOptionsStageVM __instance)
        {
            var option = __instance.OptionsController.Options.First(o => o.Identifier == "IsLifeDeathCycleEnabled");
            __instance.OptionsController.Options.Remove(option);
            __instance.RefreshValues();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignOptionsVM), MethodType.Constructor, typeof(Action))]
        public static void RemoveLifeDeathCycleFromCampaignMenu(CampaignOptionsVM __instance)
        {
            var option = __instance.OptionsController.Options.First(o => o.Identifier == "IsLifeDeathCycleEnabled");
            __instance.OptionsController.Options.Remove(option);
            __instance.RefreshValues();
        }
        
        [HarmonyPatch(typeof(MapScene), "Load")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int truthOccurance = -1;
            bool truthFlag = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr && instruction.OperandIs("Main_map"))
                {
                    instruction.operand = "modded_main_map";
                }
                else if (instruction.opcode == OpCodes.Ldloca_S)
                {
                    truthOccurance++;
                    truthFlag = true;
                }
                else if (instruction.opcode == OpCodes.Stfld)
                {
                    truthFlag = false;
                }
                else if (instruction.opcode == OpCodes.Ldc_I4_0 && truthFlag && (truthOccurance == 1 || truthOccurance == 3))
                {
                    instruction.opcode = OpCodes.Ldc_I4_1;
                }
                yield return instruction;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapScene), "GetMapBorders")]
        public static void CustomBorders(MapScene __instance, ref Vec2 minimumPosition, ref Vec2 maximumPosition, ref float maximumHeight)
        {
            minimumPosition = new Vec2(1200, 600);
            maximumPosition = new Vec2(1750, 1500);
            maximumHeight = 350;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameSceneDataManager), "LoadSPBattleScenes", argumentTypes: typeof(XmlDocument))]
        public static void LoadSinglePlayerBattleScenes(GameSceneDataManager __instance, ref XmlDocument doc)
        {
            var path = System.IO.Path.Combine(BasePath.Name, "Modules/TOR_Environment/ModuleData/tow_singleplayerbattlescenes.xml");
            if (File.Exists(path))
            {
                XmlDocument moredoc = new XmlDocument();
                moredoc.Load(path);
                doc = moredoc;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(KingdomDecisionProposalBehavior), "ConsiderPeace")]
        public static bool ConsiderPeacePatch(ref bool __result, Clan clan, Clan otherClan, Kingdom kingdom, IFaction otherFaction, out MakePeaceKingdomDecision decision)
        {
            if (!kingdom.Name.Contains("Sylvania") && !otherFaction.Name.Contains("Sylvania"))
            {
                decision = null;
                return true;
            }
            else
            {
                decision = new MakePeaceKingdomDecision(clan, otherFaction, 0, false);
                __result = false;
                return false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapVM), MethodType.Constructor, typeof(INavigationHandler), typeof(IMapStateHandler), typeof(MapBarShortcuts), typeof(Action))]
        public static void ReplaceMapVM(MapVM __instance)
        {
            __instance.MapInfo = new TorMapInfoVM();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapVM), "OnRefresh")]
        public static void RefreshExtraProperties(MapVM __instance)
        {
            var info = __instance.MapInfo as TorMapInfoVM;
            if (info != null) info.RefreshExtraProperties();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CampaignAgentComponent), "OwnerParty", MethodType.Getter)]
        public static bool PatchOwnerPartyForSummons(Agent ___Agent, ref PartyBase __result)
        {
            if (___Agent.Origin is SummonedAgentOrigin)
            {
                var origin = ___Agent.Origin as SummonedAgentOrigin;
                __result = origin.OwnerParty;
                return false;
            }
            else return true;
        }
    }
}