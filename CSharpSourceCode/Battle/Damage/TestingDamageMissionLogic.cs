using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Damage
{
    /// <summary>Class <c>TestingDamageMissionLogic</c> A mission logic that can be used for debugging and testing.
    /// It should contain methods and mission logics  creating and deleting Agent entities in a mission on the fly and testing damage related features.
    ///</summary>
    public class TestingDamageMissionLogic: MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            if (Input.IsKeyPressed(InputKey.N))
            {
                Blow b = new Blow();

                if (Mission.Current.PlayerTeam.IsAttacker)
                {
                    Mission.Teams.Defender.Leader.Die(b);
                }
                else
                {
                    Mission.Teams.Attacker.Leader.Die(b);
                }
            }
        }
    }
}