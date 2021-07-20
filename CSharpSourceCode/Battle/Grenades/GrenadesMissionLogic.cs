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
        private uint currentMissileId = 0;

        public GrenadesMissionLogic()
        {

        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(InputKey.L))
                DebugOnlyEquipGrenades();
        }
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            if (shooterAgent.WieldedWeapon.Item.Name.Contains("Grenade"))
            {
                shooterAgent.WieldedWeapon.Item.Id = new MBGUID(currentMissileId);
                Mission.Missile grenade = Mission.Current.Missiles.ToList().FirstOrDefault(m => m.Weapon.Item.Id.InternalValue == currentMissileId && m.Weapon.Item.Name.Contains("Grenade"));
                currentMissileId++;
                ActivateIfGrenade(grenade);
            }
        }
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, damage, affectorWeapon);
            if (affectorWeapon.Item != null && affectorWeapon.Item.Name.Contains("Grenade"))
            {
                MissionWeapon weapon = affectorWeapon;
                Mission.Missile grenade = Mission.Current.Missiles.ToList().FirstOrDefault(m => m.Weapon.Item.Id == weapon.Item.Id && m.Weapon.Item.Name.Contains("Grenade"));
                if (grenade != null)
                {
                    GameEntity bouncedGrenade = grenade.Entity;
                    Vec3 position = new Vec3(affectorAgent.Position.X, affectorAgent.Position.Y, affectorAgent.Position.Z);
                    MatrixFrame frame = new MatrixFrame(Mat3.CreateMat3WithForward(Vec3.Zero), grenade.GetPosition());
                    bouncedGrenade.SetGlobalFrame(frame);
                }
            }
        }
        private void ActivateIfGrenade(Mission.Missile grenade)
        {
            GameEntity grenadeEntity = grenade.Entity;
            grenadeEntity.CreateAndAddScriptComponent("GrenadeScript");
            GrenadeScript grenadeScript = grenadeEntity.GetFirstScriptOfType<GrenadeScript>();
            grenadeScript.SetShooterAgent(grenade.ShooterAgent);
            grenadeEntity.CallScriptCallbacks();
        }

        private void DebugOnlyEquipGrenades()
        {
            ItemObject itemObject = MBObjectManager.Instance.GetObject<ItemObject>("dwarf_hand_grenade");
            MissionWeapon grenades = new MissionWeapon(itemObject, null, Banner.CreateRandomBanner());
            Agent.Main.EquipWeaponToExtraSlotAndWield(ref grenades);
        }
    }
}
