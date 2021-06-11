using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities;
using System.Timers;

namespace TOW_Core.Battle.AttributeSystem.CustomAgentComponents
{
    public class UndeadMoraleAgentComponent : AgentComponent
    {
        private float _crumbleThreshold = 15f;
        private float _timeElapsed = 0;

        private CommonAIComponent _moraleComponent;

        public UndeadMoraleAgentComponent(Agent agent) : base(agent) { }

        public override void Initialize()
        {
            base.Initialize();
            _moraleComponent = Agent.GetComponent<CommonAIComponent>();
        }

        public override void OnTickAsAI(float dt)
        {
            base.OnTickAsAI(dt);
            _timeElapsed += dt;
            if(_timeElapsed >= 0.5)
            {
                _timeElapsed = 0;
                try
                {
                    if (Agent.IsActive() || Agent.IsRetreating())
                    {
                        if (_moraleComponent.Morale < _crumbleThreshold)
                        {
                            ApplyCrumble();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TOWCommon.Log("Attempted to apply crumbling to agent. Error: " + ex.Message, NLog.LogLevel.Error);
                }
            }
        }

        private void ApplyCrumble()
        {
            Agent.ApplyStatusEffect("crumble");
        }
    }
} 
