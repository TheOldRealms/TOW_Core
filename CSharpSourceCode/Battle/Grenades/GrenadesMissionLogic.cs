using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.Battle.Grenades
{
    public class GrenadesMissionLogic : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(InputKey.P))
                EquipGrenades();
        }
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            ActivateIfGrenade();
        }
        private void ActivateIfGrenade()
        {
            Mission.Missile missile = Mission.Current.Missiles.ToArray()[0];
            if (missile.Weapon.Item.Name.Contains("Grenade"))
            {
                GameEntity grenade = missile.Entity;
                grenade.CreateAndAddScriptComponent("GrenadeScript");
                GrenadeScript grenadeScript = grenade.GetFirstScriptOfType<GrenadeScript>();
                grenadeScript.SetShooterAgent(missile.ShooterAgent);
                grenade.CallScriptCallbacks();
            }
        }
        private void EquipGrenades()
        {
            ItemObject itemObject = MBObjectManager.Instance.GetObject<ItemObject>("dwarf_hand_grenade");
            MissionWeapon grenades = new MissionWeapon(itemObject, null, Banner.CreateRandomBanner());
            Agent.Main.EquipWeaponToExtraSlotAndWield(ref grenades);
        }
    }
}
