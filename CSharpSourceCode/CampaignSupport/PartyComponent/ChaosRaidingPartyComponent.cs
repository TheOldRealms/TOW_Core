using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOW_Core.CampaignSupport.QuestBattleLocation;

namespace TOW_Core.CampaignSupport.PartyComponent
{
    public class ChaosRaidingPartyComponent : TaleWorlds.CampaignSystem.PartyComponent
    {
        private readonly Settlement _portal;
        private readonly QuestBattleComponent settlementComponent;
        private TextObject _cachedName;


        private ChaosRaidingPartyComponent(Settlement portal, QuestBattleComponent questBattleSettlementComponent)
        {
            _portal = portal;
            settlementComponent = questBattleSettlementComponent;
        }

        private void InitializeChaosRaidingParty(int partySize)
        {
            //   PartyTemplateObject villagerPartyTemplate = _portal.Culture.VillagerPartyTemplate;
            PartyTemplateObject chaosPartyTemplate = Campaign.Current.ObjectManager.GetObject<PartyTemplateObject>("chaos_raiders_template");
            Party.MobileParty.HomeSettlement = _portal;
            Party.MobileParty.ActualClan = _portal.OwnerClan;
            Party.Owner = _portal.OwnerClan.Leader;
            Party.MobileParty.Aggressiveness = 4.0f;
            //      if (this.Village.Bound?.Town?.Governor != null && this.Village.Bound.Town.Governor.GetPerkValue(DefaultPerks.Scouting.VillageNetwork))
            ///           villagerPartySize = MathF.Round((float) villagerPartySize * (float) (1.0 + (double) DefaultPerks.Scouting.VillageNetwork.SecondaryBonus * 0.00999999977648258));
            //      if ((double) villagerPartySize > (double) this.Village.Hearth)
            //          villagerPartySize = (int) this.Village.Hearth;
            //       this.Village.Hearth -= (float) ((villagerPartySize + 1) / 2);
            Party.MobileParty.InitializeMobileParty(chaosPartyTemplate, _portal.Position2D, 1f, troopNumberLimit: partySize);
            Party.Visuals.SetMapIconAsDirty();
            Party.MobileParty.InitializePartyTrade(0);
            //   float num = 10000f;
            //    ItemObject itemObject1 = (ItemObject) null;
            //      foreach (ItemObject itemObject2 in Items.All)
            //      {
            //         if (itemObject2.ItemCategory == DefaultItemCategories.PackAnimal && (double) itemObject2.Value < (double) num && itemObject2.Value > 40)
            //  {
            //             itemObject1 = itemObject2;
            //           num = (float) itemObject2.Value;
            //          }
            //   }

            ////      if (itemObject1 == null)
            //       int amount = (int) (0.5 * (double) villagerPartySize);
            //     this.MobileParty.ItemRoster.Add(new ItemRosterElement(itemObject1, amount));
        }

        public static MobileParty CreateChaosRaidingParty(
            string stringId,
            Settlement portal,
            QuestBattleComponent component,
            int partySize)
        {
            return MobileParty.CreateParty(stringId,
                new ChaosRaidingPartyComponent(portal, component),
                mobileParty => ((ChaosRaidingPartyComponent) mobileParty.PartyComponent).InitializeChaosRaidingParty(partySize));
        }

        public override TextObject Name
        {
            get
            {
                if (_cachedName == null)
                {
                    _cachedName = GameTexts.FindText("str_villagers_of_VILLAGE_NAME");
                    _cachedName.SetTextVariable("VILLAGE_NAME", _portal.Name);
                }

                return _cachedName;
            }
        }


        protected override void OnInitialize() => settlementComponent.RaidingParties.Add(this);

        protected override void OnFinalize() => settlementComponent.RaidingParties.Remove(this);
    }
}