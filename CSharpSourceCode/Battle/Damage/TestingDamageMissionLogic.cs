using System;
using System.Diagnostics;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Damage
{
    /// <summary>Class <c>TestingDamageMissionLogic</c> A mission logic that can be used for debugging and testing.
    /// It should contain methods and mission logics  creating and deleting Agent entities in a mission on the fly and testing damage related features.
    ///</summary>
    public class TestingDamageMissionLogic : MissionLogic
    {
        private static float _slowMotionEndTime;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Debugger.IsAttached)
            {
                CheckForSlowMotionTime();
                //KillEnemyLeader();
                GetSomeFireArms();
            }
        }

        private void GetSomeFireArms()
        {
            if (Input.IsKeyPressed(InputKey.P))
            {
                TOWDebug.EquipWeapon(Agent.Main, "tor_neutral_weapon_ammo_musket_ball", EquipmentIndex.Weapon4);
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_gun_longrifle_004", EquipmentIndex.Weapon3);
            }
            if (Input.IsKeyPressed(InputKey.O))
            {
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_ammo_grenade", EquipmentIndex.Weapon4);
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_ammo_shrapnel", EquipmentIndex.Weapon3);
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_gun_blunderbuss_001", EquipmentIndex.Weapon2);
            }
        }

        public static void EnableSlowMotion(float time)
        {
            _slowMotionEndTime = Mission.Current.CurrentTime + time;
            Mission.Current.Scene.SlowMotionMode = true;
        }

        private void CheckForSlowMotionTime()
        {
            if (Mission.Scene.SlowMotionMode && _slowMotionEndTime <= Mission.CurrentTime)
            {
                Mission.Scene.SlowMotionMode = false;
            }
        }

        private void KillEnemyLeader()
        {
            if (Input.IsKeyPressed(InputKey.N))
            {
                Blow b = new Blow();
                Mission.PlayerEnemyTeam.Leader.Die(b);
            }
        }
    }
}