using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class SettlementNotableController : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.CheckEmpireSettlements));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void CheckEmpireSettlements()
        {
            empireSettlements = Campaign.Current.Settlements.Where(s => IsEmpireSettlement(s)).ToArray();

            foreach (var settlement in empireSettlements)
            {
                foreach (var hero in settlement.Notables.ToArray())
                {
                    if (hero.IsNotableVampire())
                    {
                        ReplaceNotableVampire(hero, settlement);
                        areThereKilledVampires = true;
                    }
                }
            }
            if (areThereKilledVampires)
            {
                TOWCommon.Say("The Inquisition carried out a cleanup");
            }
            areThereKilledVampires = false;
        }

        private bool IsEmpireSettlement(Settlement settlement)
        {
            return (settlement.IsVillage || settlement.IsTown) &&
                   (settlement.MapFaction.Name.Contains("Stirland") ||
                    settlement.MapFaction.Name.Contains("Averland") ||
                    settlement.MapFaction.Name.Contains("The Moot"));
        }

        private void ReplaceNotableVampire(Hero vampire, Settlement settlement)
        {
            Occupation occupation = vampire.CharacterObject.Occupation;
            KillCharacterAction.ApplyByRemove(vampire, false);
            Hero newHero;

            do
            {
                newHero = HeroCreator.CreateHeroAtOccupation(occupation, settlement);
                if (newHero.IsNotableVampire())
                {
                    KillCharacterAction.ApplyByRemove(newHero, true);
                }
            }
            while (newHero.IsDead);
        }

        private bool areThereKilledVampires;

        private Settlement[] empireSettlements;
    }
}
