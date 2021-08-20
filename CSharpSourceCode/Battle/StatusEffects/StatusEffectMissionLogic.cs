﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectMissionLogic : MissionLogic
    {
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if (agent.IsHuman)
            {
                StatusEffectComponent effectComponent = new StatusEffectComponent(agent);
                agent.AddComponent(effectComponent);
            }
        }
        
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            foreach(var agent in Mission.Current.Agents)
            {
                if (agent.GetComponent<StatusEffectComponent>() != null)
                {
                    if (agent.IsActive() && agent.Health > 1f)
                    {
                        var comp = agent.GetComponent<StatusEffectComponent>();
                        comp.OnTick(dt);
                    }
                }
            }
        }

        public override MissionBehaviourType BehaviourType { get; }
    }
}