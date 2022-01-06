using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Damage
{
    public class TestDamageMissionLogic: MissionLogic
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