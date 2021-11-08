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
    private static float limit = 140;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Mission), "ComputeBlowMagnitudeMissile")]
    public static void ComputeBlowMagnitudeMissilePostfix(ref float baseMagnitude)
    {
        baseMagnitude = baseMagnitude <= limit ? baseMagnitude : limit;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mission), "SpawnAgent")]
    public static bool SpawnAgentPrefix(AgentBuildData agentBuildData)
    {
        var character = agentBuildData.AgentCharacter as CharacterObject;
        if (character != null)
        {
            string name = null;
            if (character.HeroObject != null && character.HeroObject.IsVampire())
            {
                name = "vampire";
            }
            else if (character.IsVampire())
            {
                name = "vampire";
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                Monster monster = GetCustomMonster(name);
                if (monster != null)
                {
                    Traverse.Create(agentBuildData).Property("AgentData").Field("_agentMonster").SetValue(monster);
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