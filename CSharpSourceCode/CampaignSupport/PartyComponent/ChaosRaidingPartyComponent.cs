using System.ComponentModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TOW_Core.CampaignSupport.QuestBattleLocation;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignSupport.PartyComponent
{
    public class ChaosRaidingPartyComponent : TaleWorlds.CampaignSystem.PartyComponent
    {
        private readonly Settlement _portal;
        private readonly QuestBattleComponent settlementComponent;


        private ChaosRaidingPartyComponent(Settlement portal, QuestBattleComponent questBattleSettlementComponent)
        {
            this._portal = portal;
            this.settlementComponent = questBattleSettlementComponent;
        }

        private void InitializeChaosRaidingParty(int partySize)
        {
            PartyTemplateObject villagerPartyTemplate = this.Village.Settlement.Culture.VillagerPartyTemplate;
            this.Party.MobileParty.HomeSettlement = this.Village.Settlement;
            this.Party.Owner = this.Village.Settlement.OwnerClan.Leader;
            this.Party.MobileParty.Aggressiveness = 0.0f;
            if (this.Village.Bound?.Town?.Governor != null && this.Village.Bound.Town.Governor.GetPerkValue(DefaultPerks.Scouting.VillageNetwork))
                villagerPartySize = MathF.Round((float) villagerPartySize * (float) (1.0 + (double) DefaultPerks.Scouting.VillageNetwork.SecondaryBonus * 0.00999999977648258));
            if ((double) villagerPartySize > (double) this.Village.Hearth)
                villagerPartySize = (int) this.Village.Hearth;
            this.Village.Hearth -= (float) ((villagerPartySize + 1) / 2);
            this.Party.MobileParty.InitializeMobileParty(villagerPartyTemplate, this.Village.Owner.Settlement.Position2D, 1f, troopNumberLimit: villagerPartySize);
            this.Party.Visuals.SetMapIconAsDirty();
            this.Party.MobileParty.InitializePartyTrade(0);
            float num = 10000f;
            ItemObject itemObject1 = (ItemObject) null;
            foreach (ItemObject itemObject2 in Items.All)
            {
                if (itemObject2.ItemCategory == DefaultItemCategories.PackAnimal && (double) itemObject2.Value < (double) num && itemObject2.Value > 40)
                {
                    itemObject1 = itemObject2;
                    num = (float) itemObject2.Value;
                }
            }

            if (itemObject1 == null)
                return;
            int amount = (int) (0.5 * (double) villagerPartySize);
            this.MobileParty.ItemRoster.Add(new ItemRosterElement(itemObject1, amount));
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
                if (this._cachedName == null)
                {
                    this._cachedName = GameTexts.FindText("str_villagers_of_VILLAGE_NAME");
                    this._cachedName.SetTextVariable("VILLAGE_NAME", this.Village.Name);
                }

                return this._cachedName;
            }
        }


        protected override void OnInitialize() => this.settlementComponent.RaidingParties.Add(this);

        protected override void OnFinalize() => this.settlementComponent.RaidingParties.Remove(this);
    }
}