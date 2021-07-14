using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Voices
{
    public class CustomVoicesMissionBehavior : MissionBehaviour
    {
        public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

        private List<Agent> allAgents = new List<Agent>();
        private bool _voicesAssigned = false;

        public override void OnAgentCreated(Agent agent)
        {
            _voicesAssigned = false;
            base.OnAgentCreated(agent);
            allAgents.Add(agent);
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!_voicesAssigned && Mission.Current.CurrentState.Equals(Mission.State.Continuing))
            {
                foreach (Agent agent in allAgents)
                {
                    string voiceName = agent.Character?.GetCustomVoiceClassName();
                    if (voiceName != null)
                    {
                        agent.SetAgentVoiceByClassName(voiceName);
                    }
                }

                _voicesAssigned = true;
            }
        }
    }
}
