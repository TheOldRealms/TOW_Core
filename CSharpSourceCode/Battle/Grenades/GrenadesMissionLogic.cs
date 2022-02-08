using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.TriggeredEffect;
using TOW_Core.Battle.TriggeredEffect.Scripts;

namespace TOW_Core.Battle.Grenades
{
    public class GrenadesMissionLogic : MissionLogic
    {
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            if (shooterAgent.WieldedWeapon.Item.Type == ItemObject.ItemTypeEnum.Thrown)
            {
                Mission.Missile grenade = Mission.Missiles.FirstOrDefault(m => m.ShooterAgent == shooterAgent &&
                                                                               m.Weapon.Item.StringId.Contains("hand_grenade") &&
                                                                               !m.Entity.HasScriptOfType<HandGrenadeScript>());
                if (grenade != null)
                {
                    AddHandGrenadeScipt(grenade, "grenade_explosion");
                    //if (grenade.Weapon.CurrentUsageItem.WeaponFlags.HasFlag(WeaponFlags.Burning))
                    //{
                    //    AddHandGrenadeScipt(grenade, "incediary_grenade_explosion");
                    //}
                }
            }
        }

        private void AddHandGrenadeScipt(Mission.Missile grenade, string triggeredEffectName)
        {
            GameEntity grenadeEntity = grenade.Entity;
            grenadeEntity.CreateAndAddScriptComponent("HandGrenadeScript");
            HandGrenadeScript grenadeScript = grenadeEntity.GetFirstScriptOfType<HandGrenadeScript>();
            grenadeScript.SetShooterAgent(grenade.ShooterAgent);
            grenadeScript.SetTriggeredEffect(TriggeredEffectManager.CreateNew(triggeredEffectName));
            grenadeScript.SetMissile(grenade);
            grenadeEntity.CallScriptCallbacks();
        }
    }
}
