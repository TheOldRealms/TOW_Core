using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TOW_Core.Utilities;

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
            empireSettlements = Campaign.Current.Settlements.Where(s => (s.IsVillage || s.IsTown) && s.Culture.Name.Contains("Empire")).ToArray();

            foreach (var settlement in empireSettlements)
            {
                foreach (var hero in settlement.Notables.ToArray())
                {
                    if (hero.IsNotable && hero.Age < 22)
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

        private void ReplaceNotableVampire(Hero vampire, Settlement settlement)
        {
            Occupation occupation = vampire.CharacterObject.Occupation;
            KillCharacterAction.ApplyByRemove(vampire, false);
            Hero newHero;

            do
            {
                newHero = HeroCreator.CreateHeroAtOccupation(occupation, settlement);
                if (newHero.Age < 22)
                {
                    KillCharacterAction.ApplyByRemove(newHero, false);
                }
            }
            while (newHero.IsAlive);
        }

        private bool areThereKilledVampires;

        private Settlement[] empireSettlements;
    }
}
