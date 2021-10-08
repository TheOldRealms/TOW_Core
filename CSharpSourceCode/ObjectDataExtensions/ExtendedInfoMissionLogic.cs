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
        private float _timeSinceLastTick = 0;
        private bool _tickWindsOfMagic;

        public override void OnMissionTick(float dt)
        {
            _timeSinceLastTick += dt;
            if (_timeSinceLastTick > 1)
            {
                _timeSinceLastTick = 0;
                _tickWindsOfMagic = true;

            }
            if (_tickWindsOfMagic)
            {
                foreach (var agent in Mission.Agents)
                {
                    var hero = agent.GetHero();
                    if (hero != null && hero.IsSpellCaster())
                    {
                        var info = hero.GetExtendedInfo();
                        if (info != null)
                        {
                            info.CurrentWindsOfMagic += info.WindsOfMagicRechargeRate;
                            info.CurrentWindsOfMagic = Math.Min(info.MaxWindsOfMagic, info.CurrentWindsOfMagic);
                        }
                    }
                }
                _tickWindsOfMagic = false;
            }
        }
    }
}