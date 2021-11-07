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
    public static bool SpawnAgentPrefixprivate(AgentBuildData agentBuildData)
    {
        var character = agentBuildData.AgentCharacter;
        if (character != null && character.IsHero && (character as CharacterObject).IsVampire())
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(ModuleHelper.GetModuleFullPath("TOW_Core") + "ModuleData/tor_monsters.xml");
            foreach (var node in xmlDoc.ChildNodes.Item(1).ChildNodes)
            {
                XmlNode xmlNode = (XmlNode)node;
                var monster = new Monster();
                monster.Deserialize(MBObjectManager.Instance, xmlNode);
                Traverse.Create(agentBuildData).Property("AgentData").Field("_agentMonster").SetValue(monster);
                break;
            }
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mission), "FallDamageCallback")]
    public static bool FallDamageCallbackPrefix(ref AttackCollisionData collisionData, Blow b, Agent attacker, Agent victim)
    {
        if (victim.IsHuman && victim.Character != null)
        {
            var character = victim.Character as CharacterObject;
            if (character.IsHero && character.HeroObject.IsVampire())
            {
                return false;
            }
        }
        return true;
    }
}