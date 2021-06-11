using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectMissionLogic : MissionLogic
    {
        private Dictionary<string, StatusEffect> _presentEffects = new Dictionary<string, StatusEffect>();

        public EventHandler<OnTickArgs> NotifyStatusEffectTickObservers;

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            StatusEffectComponent effectComponent = new StatusEffectComponent(agent);
            NotifyStatusEffectTickObservers += effectComponent.OnTick;
            agent.AddComponent(effectComponent);
            if(!_presentEffects.ContainsKey("crumble") && agent.IsUndead())
            {
                _presentEffects.Add("crumble", StatusEffectManager.GetStatusEffect("crumble"));
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            OnTickArgs arguments = new OnTickArgs() { deltatime = dt };
            NotifyStatusEffectTickObservers?.Invoke(this, arguments);
        }

        public override MissionBehaviourType BehaviourType { get; }
    }



    public class OnTickArgs : EventArgs
    {
        public float deltatime;
    }
}