using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class WordOfPainScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            var agent = triggeredAgents.FirstOrDefault();
            if (agent != null)
            {
                EquipmentIndex index1 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                EquipmentIndex index2 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
                if (index1 != EquipmentIndex.None)
                {
                    agent.DropItem(index1);
                }
                if (index2 != EquipmentIndex.None)
                {
                    agent.DropItem(index2);
                }
                if (!agent.HasMount)
                {
                    var frame = agent.Frame;
                    frame.Advance(10);
                    agent.Retreat(new WorldPosition(Mission.Current.Scene, frame.origin));
                    agent.SetActionChannel(0, ActionIndexCache.Create("act_death_by_fire1"), true);
                }
            }
        }
    }
}
