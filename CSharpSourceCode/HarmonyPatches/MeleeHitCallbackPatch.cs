using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Dismemberment;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch(typeof(Mission))]
    public static class MeleeHitCallbackPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("MeleeHitCallback")]
        public static void Prefix(ref AttackCollisionData collisionData, Agent attacker, Agent victim)
        {
            Dismemberment.AttackCollision = collisionData;
            bool isPDV = collisionData.VictimHitBodyPart == BoneBodyPartType.Head && collisionData.StrikeType == 0 && collisionData.DamageType == 0 && (attacker.AttackDirection == Agent.UsageDirection.AttackLeft || attacker.AttackDirection == Agent.UsageDirection.AttackRight);
            if (isPDV)
                Dismemberment.AddPDV(victim, attacker);
        }
    }
}
