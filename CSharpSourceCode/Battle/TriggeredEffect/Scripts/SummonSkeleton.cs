using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class SummonSkeleton : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent)
        {
            SpawnAgent(triggeredByAgent, position);
        }

        private void SpawnAgent(Agent caster, Vec3 position)
        {
            IAgentOriginBase troopOrigin = caster.Origin;
            BasicCharacterObject troopCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("tow_skeleton_recruit");
            Team casterTeam = Mission.GetAgentTeam(troopOrigin, true);
            MatrixFrame frame = new MatrixFrame(Mat3.Identity, position);
            AgentBuildData buildData = new AgentBuildData(troopCharacter).
                Team(casterTeam).
                Banner(troopOrigin.Banner).
                ClothingColor1(casterTeam.Color).
                ClothingColor2(casterTeam.Color2).
                TroopOrigin(troopOrigin).
                CivilianEquipment(Mission.Current.DoesMissionRequireCivilianEquipment);

            if (!troopCharacter.IsPlayerCharacter)
                buildData.IsReinforcement(true).SpawnOnInitialPoint(true);

            buildData.InitialFrame(frame);
            Agent troop = Mission.Current.SpawnAgent(buildData, false, 1);
            troop.SetWatchState(Agent.WatchState.Alarmed);
        }
    }
}
