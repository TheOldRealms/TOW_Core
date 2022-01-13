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
        var character = agentBuildData.AgentCharacter as CharacterObject;
        if (character != null)
        {
            if (character.HeroObject != null && character.HeroObject.IsVampire())
            {
                Traverse.Create(agentBuildData).Property("AgentData").Field("_agentMonster").SetValue(_vampireMonster);
            }
            else if (character.IsVampire())
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
        Monster monster = null;
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(ModuleHelper.GetModuleFullPath("TOW_Core") + "ModuleData/tor_monsters.xml");
        var children = xmlDoc.ChildNodes.Item(1).ChildNodes;
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var id = child.Attributes.GetNamedItem("id").Value;
            if (id == name)
            {
                monster = new Monster();
                monster.Deserialize(MBObjectManager.Instance, child);
                break;
            }
        }
        return monster;
    }
}