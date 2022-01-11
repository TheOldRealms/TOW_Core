using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Timer = System.Timers.Timer;

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
                    frame.Advance(100);
                    var vec = frame.origin.AsVec2;
                    agent.SetMovementDirection(in vec);
                    agent.Controller = Agent.ControllerType.None;

                    var action = agent.GetCurrentAction(0);
                    if (action.Name != "act_death_by_fire1")
                    {
                        agent.SetActionChannel(0, ActionIndexCache.Create("act_death_by_fire1"), true);
                    }
                }
            }

            endTimer = new Timer(9000);
            endTimer.AutoReset = false;
            endTimer.Elapsed += (s, e) =>
            {
                var enemy = triggeredAgents.FirstOrDefault();
                if (enemy != null && enemy.Health > 0)
                {
                    enemy.SetActionChannel(0, ActionIndexCache.Create("act_strike_fall_back_back_rise_continue"), true, actionSpeed: 0.5f);
                    enemy.Controller = Agent.ControllerType.AI;
                }
            };
            endTimer.Start();
        }

        private Timer endTimer;
    }
}
