﻿using TaleWorlds.Core;
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
            CheckForSlowMotionTime();
            //KillEnemyLeader();
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