using HarmonyLib;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

[HarmonyPatch]
public static class MissionPatches
{
    private static readonly Monster _vampireMonster = GetCustomMonster("vampire");

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mission), "SpawnAgent")]
    public static bool SpawnAgentPrefix(AgentBuildData agentBuildData)
    {
        var character = agentBuildData.AgentCharacter;
        if (character != null)
        {
            if (character.IsVampire())
            {
                Traverse.Create(agentBuildData).Property("AgentData").Field("_agentMonster").SetValue(_vampireMonster);
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

    private static Monster GetCustomMonster(string name)
    {
        return MBObjectManager.Instance.GetObject<Monster>(name);
    }
}