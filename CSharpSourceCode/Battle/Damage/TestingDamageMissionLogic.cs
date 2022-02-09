using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

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
            if (Input.IsKeyPressed(InputKey.P))
            {
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_gun_longrifle_001");
            }
            if (Input.IsKeyPressed(InputKey.N))
            {
                Blow b = new Blow();
                
                
                if (Mission.Current.PlayerTeam.IsAttacker)
                {
                    if(Mission.Teams.Defender.Leader.Health>0)
                        Mission.Teams.Defender.Leader.Die(b);
                }
                else
                {
                    if(Mission.Teams.Defender.Leader.Health>0)
                        Mission.Teams.Attacker.Leader.Die(b);
                }
            }
        }
    }
}