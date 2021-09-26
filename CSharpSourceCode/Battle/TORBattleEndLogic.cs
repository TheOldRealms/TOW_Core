using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle
{
    public class TORBattleEndLogic : BattleEndLogic
    {
        private IMissionAgentSpawnLogic spawnLogic;

        public override bool MissionEnded(ref MissionResult missionResult)
        {
            bool result = false;

            if (this.DefenderSidePulledBack)
            {
                missionResult = MissionResult.CreateDefenderPushedBack();
                result = true;
            }
            else if (this.IsEnemySideRetreating)
            {
                missionResult = MissionResult.CreateSuccessful(base.Mission);
                result = true;
            }
            else if (base.Mission.Teams.PlayerEnemy.ActiveAgents.Count == 0)
            {
                missionResult = MissionResult.CreateSuccessful(base.Mission);
                result = true;
            }
            else if (Agent.Main == null || Agent.Main.State != AgentState.Active)
            {

                if (base.Mission.Teams.Player.ActiveAgents.Count == 0)
                {
                    missionResult = MissionResult.CreateDefeated(base.Mission);
                    result = true;
                }
            }
            if (result)
            {
                this.spawnLogic.StopSpawner();
            }
            return result;
        }

        public override void OnBehaviourInitialize()
        {
            var beh = Mission.Current.GetMissionBehaviour<IMissionAgentSpawnLogic>();
            if (beh != null)
            {
                spawnLogic = beh;
                Traverse.Create(this).Field("_missionAgentSpawnLogic").SetValue(beh);
            }
            Traverse.Create(this).Field("_checkRetreatingTimer").SetValue(new BasicTimer(MBCommon.TimeType.Mission));
        }
    }
}
