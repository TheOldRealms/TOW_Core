using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Grenades
{
    public class GrenadesMissionLogic : MissionLogic
    {
        public GrenadesMissionLogic()
        {
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(InputKey.L))
                EquipGrenades();
        }
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            ActivateIfGrenade();
        }
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, damage, affectorWeapon);
            if (affectorWeapon.Item != null && affectorWeapon.Item.Name.Contains("Grenade"))
            {
                Mission.Missile grenade = Mission.Current.Missiles.ToList().First(m => m.Weapon.Item.Name.Contains("Grenade"));
                if (grenade != null)
                {
                    GameEntity bouncedGrenade = grenade.Entity;
                    Vec3 position = new Vec3(affectorAgent.Position.X, affectorAgent.Position.Y, affectorAgent.Position.Z);
                    MatrixFrame frame = new MatrixFrame(Mat3.CreateMat3WithForward(Vec3.Zero), grenade.GetPosition());
                    bouncedGrenade.SetGlobalFrame(frame);
                }
            }
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
