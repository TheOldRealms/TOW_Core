using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

[HarmonyPatch]
public static class TestPatch
{
    private static float limit = 140;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Mission), "ComputeBlowMagnitudeMissile")]
    public static void MissionPostfix(ItemObject weaponItem, ref float baseMagnitude)
    {
        baseMagnitude = baseMagnitude <= limit ? baseMagnitude : limit;
    }
}