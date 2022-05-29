using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.AgentBehavior.SupportMissionLogic
{
    public static class FormationQuerySystemExtensions
    {
        public static void DispersednessVector(this FormationQuerySystem formation)
        {
        }

        public static Agent ProvideMostPowerfulAgentForTeam(this Team team)
        {
            var agents = QuerySystemExtensionsMissionLogic.GetMostPowerfulAgentsByTeam(team);
            if (!agents.IsEmpty())
                return agents[0];
            return null;
        }
    }
}