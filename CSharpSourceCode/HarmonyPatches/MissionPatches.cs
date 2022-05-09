using HarmonyLib;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

[HarmonyPatch]
public static class MissionPatches
{
    private static readonly Monster _vampireMonster = GetCustomMonster("vampire");

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mission), "SpawnAgent")]
    public static bool SpawnAgentPrefix(AgentBuildData agentBuildData, Mission __instance)
    {
        var character = agentBuildData.AgentCharacter;
        if (character != null)
        {
            if (character.IsVampire())
            {
                Traverse.Create(agentBuildData).Property("AgentData").Field("_agentMonster").SetValue(_vampireMonster);
                Traverse.Create(agentBuildData).Property("AgentData").Property("AgentAge").SetValue(19);
                Traverse.Create(agentBuildData).Property("AgentData").Property("AgeOverriden").SetValue(true);
            }
            else if (character.IsHero && Game.Current.GameType is Campaign)
            {
                var hero = ((CharacterObject)character).HeroObject;
                if (hero.IsVampire())
                {
                    Traverse.Create(agentBuildData).Property("AgentData").Field("_agentMonster").SetValue(_vampireMonster);
                    Traverse.Create(agentBuildData).Property("AgentData").Property("AgentAge").SetValue(19);
                    Traverse.Create(agentBuildData).Property("AgentData").Property("AgeOverriden").SetValue(true);
                }
            }
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mission), "FallDamageCallback")]
    public static bool FallDamageCallbackPrefix(Agent victim)
    {
        if (victim.IsVampire())
        {
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MissionAgentSpawnLogic), "IsSideDepleted")]
    public static void IsSideDepletedPostfix(BattleSideEnum side, ref bool __result)
    {
        if(__result == true)
        {
            var teams = Mission.Current.Teams.Where(x => x.Side == side).ToList();
            foreach(var team in teams)
            {
                if(team.ActiveAgents.Any(x=>x.Origin is SummonedAgentOrigin))
                {
                    __result = false;
                    return;
                }
            }
        }
    }

    private static Monster GetCustomMonster(string name)
    {
        return MBObjectManager.Instance.GetObject<Monster>(name);
    }
}