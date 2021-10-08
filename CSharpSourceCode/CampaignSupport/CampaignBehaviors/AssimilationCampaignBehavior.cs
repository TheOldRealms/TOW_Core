using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace TOW_Core.CampaignSupport.CampaignBehaviors
{
    public class AssimilationCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(OnOwnerChanged));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(CheckForAssimilationComponent));
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (settlement.IsTown && settlement.Culture != newOwner.MapFaction.Culture)
            {
                if (settlement.IsCastle)
                {
                    settlement.Culture = newOwner.MapFaction.Culture;
                }

                if (settlement.Town.Villages.Count > 0 || settlement.IsTown)
                {
                    var component = settlement.GetComponent<AssimilationComponent>();
                    if (component == null)
                    {
                        settlement.AddComponent<AssimilationComponent>();
                    }
                    else
                    {
                        component.Reset();
                    }
                }
            }
        }

        private void CheckForAssimilationComponent(Settlement settlement)
        {
            var component = settlement.GetComponent<AssimilationComponent>();
            if (component != null)
            {
                if (!component.IsAssimilationComplete)
                {
                    component.Tick();
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
