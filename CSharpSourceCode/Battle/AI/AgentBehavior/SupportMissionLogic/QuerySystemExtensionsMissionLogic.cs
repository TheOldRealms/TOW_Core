using TaleWorlds.MountAndBlade;
using System.Collections.Generic;
using System.Linq;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.SupportMissionLogic
{
    public class QuerySystemExtensionsMissionLogic : MissionLogic
    {
        private static readonly float EvalInterval = 5;
        private static Dictionary<Team, List<Agent>> _mostPowerfulAgentsByTeam;

        private float _dtSinceLastOccasional;

        public QuerySystemExtensionsMissionLogic()
        {
            _mostPowerfulAgentsByTeam = new Dictionary<Team, List<Agent>>();
        }

        public override void OnMissionTick(float dt)
        {
            _dtSinceLastOccasional += dt;
            if (_dtSinceLastOccasional >= EvalInterval) TickOccasionally();
        }

        private void TickOccasionally()
        {
            MostPowerfulAgentsByTeam();
        }

        private void MostPowerfulAgentsByTeam()
        {
            //TODO: Need to track which ones are buffed etc.
            Mission.Teams.ToList()
                .ForEach(team => { _mostPowerfulAgentsByTeam[team] = Mission.AttackerTeam.ActiveAgents.OrderByDescending(x => x.Character.GetBattlePower()).Take(5).ToList(); });
        }

        public static List<Agent> GetMostPowerfulAgentsByTeam(Team team)
        {
            return _mostPowerfulAgentsByTeam[team];
        }

        public override void OnAgentDeleted(Agent affectedAgent)
        {
            _mostPowerfulAgentsByTeam.Values.ToList().ForEach(agents => agents.RemoveIfExists(affectedAgent));
        }
    }
}