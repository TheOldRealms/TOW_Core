using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.SupportMissionLogic
{
    public class PowerfulSingleAgentTrackerMissionLogic : MissionLogic
    {
        private static readonly float EvalInterval = 5;
        private static Dictionary<Team, List<Agent>> _mostPowerfulAgentsByTeam;
        private float _dtSinceLastOccasional;


        public PowerfulSingleAgentTrackerMissionLogic()
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
            //TODO: Need to track which ones are buffed etc.
            Mission.Teams.ToList()
                .ForEach(team => { _mostPowerfulAgentsByTeam[team] = Mission.AttackerTeam.ActiveAgents.OrderByDescending(x => x.Character.GetBattlePower()).Take(5).ToList(); });
        }

        public static Agent ProvideAgentForTeam(Team team)
        {
            var agents = _mostPowerfulAgentsByTeam[team];
            if (agents.IsEmpty())
                return agents[0];
            return null;
        }

        public override void OnAgentDeleted(Agent affectedAgent)
        {
            _mostPowerfulAgentsByTeam.Values.ToList().ForEach(agents => agents.RemoveIfExists(affectedAgent));
        }
    }
}