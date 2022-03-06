using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Sound
{
    public class CustomVoicesMissionBehavior : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        private Queue<Agent> allAgents = new Queue<Agent>();
        private bool _voicesAssigned = false;

        public override void OnAgentCreated(Agent agent)
        {
            _voicesAssigned = false;
            allAgents.Enqueue(agent);
        }

        public override void OnMissionTick(float dt)
        {
            if (!_voicesAssigned && Mission.Current.CurrentState.Equals(Mission.State.Continuing))
            {
                while(allAgents.Count > 0)
                {
                    var agent = allAgents.Dequeue();
                    string voiceName = agent.Character?.GetCustomVoiceClassName();
                    if (voiceName != null && voiceName != "none")
                    {
                        agent.SetAgentVoiceByClassName(voiceName);
                    }
                }
                _voicesAssigned = true;
            }
        }
    }
}
