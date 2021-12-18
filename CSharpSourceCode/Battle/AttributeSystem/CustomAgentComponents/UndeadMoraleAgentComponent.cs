using System;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomAgentComponents
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
            if (_moraleComponent != null) // Main agent has UndeadMoralComponent somehow
            {
                _timeElapsed += dt;
                if (_timeElapsed >= 0.5)
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
        }

        private void ApplyCrumble()
        {
            Agent.ApplyStatusEffect("crumble", Agent);
        }
    }
}
