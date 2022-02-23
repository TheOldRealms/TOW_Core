using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Sound
{
    public class CustomAgentSoundMissionLogic: MissionLogic
    {
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            var comp = new AgentSoundComponent(agent);
            agent.AddComponent(comp);
        }
    }
}