using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Reads out ExtendedInfoManager during Combat
    /// </summary>
    public class ExtendedInfoMissionLogic : MissionLogic
    {

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            foreach(var agent in Mission.Agents)
            {
                var hero = agent.GetHero();
                if (hero != null && hero.IsSpellCaster())
                {
                    var info = hero.GetExtendedInfo();
                    if(info != null)
                    {
                        info.CurrentWindsOfMagic += dt;
                        info.CurrentWindsOfMagic = Math.Min(info.MaxWindsOfMagic, info.CurrentWindsOfMagic);
                    }
                }
            }
        }
    }
}