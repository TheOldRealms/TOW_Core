using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Battle.TriggeredEffect;
using TOW_Core.Battle.TriggeredEffect.Scripts;

namespace TOW_Core.Battle.Grenades
{
    public class GrenadesMissionLogic : MissionLogic
    {
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            if (shooterAgent.WieldedWeapon.Item.Name.Contains("Grenade"))
            {
                Mission.Missile grenade = Mission.Missiles.FirstOrDefault(m => m.ShooterAgent == shooterAgent &&
                                                                               m.Weapon.Item.StringId.Contains("hand_grenade") &&
                                                                               !m.Entity.HasScriptOfType<HandGrenadeScript>());
                if (grenade != null) Activate(grenade);
            }
        }
        
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
        {
            if (affectorWeapon.Item != null && affectorWeapon.Item.Name.Contains("Grenade"))
            {
                Mission.Missile grenade = Mission.Current.Missiles.FirstOrDefault(m => m.Weapon.Item.Name.Contains("Grenade") && IsNearbyAgent(m, affectedAgent));
                if (grenade != null)
                {
                    GameEntity bouncedGrenade = grenade.Entity;
                    Vec3 position = new Vec3(affectorAgent.Position.X, affectorAgent.Position.Y, affectorAgent.Position.Z);
                    MatrixFrame frame = new MatrixFrame(Mat3.CreateMat3WithForward(Vec3.Zero), grenade.GetPosition());
                    bouncedGrenade.SetGlobalFrame(frame);
                }
            }
        }
        
        private void Activate(Mission.Missile grenade)
        {
            GameEntity grenadeEntity = grenade.Entity;
            grenadeEntity.CreateAndAddScriptComponent("HandGrenadeScript");
            HandGrenadeScript grenadeScript = grenadeEntity.GetFirstScriptOfType<HandGrenadeScript>();
            grenadeScript.SetShooterAgent(grenade.ShooterAgent);
            grenadeScript.SetTriggeredEffect(TriggeredEffectManager.CreateNew("grenade_explosion"));
            grenadeEntity.CallScriptCallbacks();
        }
       
        private bool IsNearbyAgent(Mission.Missile missile, Agent agent)
        {
            return Math.Abs(missile.GetPosition().X - agent.Position.X) < 0.8f && Math.Abs(missile.GetPosition().Y - agent.Position.Y) < 0.8f;
        }
    }
}
