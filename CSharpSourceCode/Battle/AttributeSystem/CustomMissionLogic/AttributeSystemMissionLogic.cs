using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AttributeSystem.CustomAgentComponents;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomMissionLogic
{
    class AttributeSystemMissionLogic : MissionLogic
    {
        public AttributeSystemMissionLogic()
        {
        }

        public override void OnAgentCreated(Agent agent)
        {
            if (agent.IsUndead())
            {
                agent.AddComponent(new UndeadMoraleAgentComponent(agent));
            }
        }
    }
}
