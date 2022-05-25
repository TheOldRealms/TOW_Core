using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace TOW_Core.CampaignSupport
{
    public class TORCaptivityCampaignBehaviour : CampaignBehaviorBase
    {
        private readonly float _maximumDaysInCaptivity = 10;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        private void OnDailyTick(Hero hero)
        {
            if (hero.IsPrisoner && hero.PartyBelongedToAsPrisoner != null && 
                hero != Hero.MainHero &&
                hero.PartyBelongedToAsPrisoner.Owner != null &&
                hero.PartyBelongedToAsPrisoner.Owner.Clan != Clan.PlayerClan &&
                hero.CaptivityStartTime != null)
            {
                var time = CampaignTime.Now;
                var duration = time - hero.CaptivityStartTime;
                if(duration.ToDays > _maximumDaysInCaptivity) EndCaptivityAction.ApplyByEscape(hero, null);
            }
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
