using HarmonyLib;
using TaleWorlds.MountAndBlade;

[HarmonyPatch]
public static class TestPatch
{
    private static float limit = 140;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Mission), "ComputeBlowMagnitudeMissile")]
    public static void ComputeBlowMagnitudeMissilePostfix(ref float baseMagnitude)
    {
        baseMagnitude = baseMagnitude <= limit ? baseMagnitude : limit;
    }
}