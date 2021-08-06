using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TOW_Core.CampaignSupport.QuestBattleLocation;

namespace TOW_Core.CampaignSupport.ChaosRaidingParty
{
    public class ChaosRaidingPartyComponent : PartyComponent 
    {
        [SaveableProperty(1)] public Settlement Portal { get; private set; }

        [SaveableProperty(2)] public bool Patrol { get; private set; }

        [CachedData] private TextObject _cachedName;

        private ChaosRaidingPartyComponent(Settlement portal, QuestBattleComponent questBattleSettlementComponent, bool patrol)
        {
            this.Portal = portal;
            this.Patrol = patrol;
        }

        private void InitializeChaosRaidingParty(int partySize)
        {
            PartyTemplateObject chaosPartyTemplate = Campaign.Current.ObjectManager.GetObject<PartyTemplateObject>("chaos_cultists");
            Party.MobileParty.ActualClan = Clan.All.ToList().Find(clan => clan.Name.ToString() == "Chaos Warriors");
            //      Party.Owner = Party.MobileParty.ActualClan.Leader;
            Party.MobileParty.HomeSettlement = Portal;
            Party.MobileParty.Aggressiveness = 2.0f;

            //      if (this.Village.Bound?.Town?.Governor != null && this.Village.Bound.Town.Governor.GetPerkValue(DefaultPerks.Scouting.VillageNetwork))
            ///           villagerPartySize = MathF.Round((float) villagerPartySize * (float) (1.0 + (double) DefaultPerks.Scouting.VillageNetwork.SecondaryBonus * 0.00999999977648258));
            //      if ((double) villagerPartySize > (double) this.Village.Hearth)
            //          villagerPartySize = (int) this.Village.Hearth;
            //       this.Village.Hearth -= (float) ((villagerPartySize + 1) / 2);

            Party.MobileParty.Party.MobileParty.InitializeMobileParty(chaosPartyTemplate, Portal.Position2D, 1f, troopNumberLimit: partySize);
            Party.Visuals.SetMapIconAsDirty();
            Party.MobileParty.InitializePartyTrade(0);
        }

        public static MobileParty CreateChaosRaidingParty(
            string stringId,
            Settlement portal,
            QuestBattleComponent component,
            int partySize)
        {
            return MobileParty.CreateParty(stringId,
                new ChaosRaidingPartyComponent(portal, component, false),
                mobileParty => ((ChaosRaidingPartyComponent) mobileParty.PartyComponent).InitializeChaosRaidingParty(partySize));
        }

        public static MobileParty CreateChaosPatrolParty(
            string stringId,
            Settlement portal,
            QuestBattleComponent component,
            int partySize)
        {
            return MobileParty.CreateParty(stringId,
                new ChaosRaidingPartyComponent(portal, component, true),
                mobileParty => ((ChaosRaidingPartyComponent) mobileParty.PartyComponent).InitializeChaosRaidingParty(partySize));
        }

        public override TextObject Name
        {
            get
            {
                if (_cachedName == null && !Patrol)
                {
                    _cachedName = new TextObject("Chaos Raiders");
                }

                if (_cachedName == null && Patrol)
                {
                    _cachedName = new TextObject("Chaos Warrior Patrol");
                }

                return _cachedName;
            }
        }


        protected override void OnInitialize()
        {
            if (Patrol)
            {
                ((QuestBattleComponent) Portal.GetComponent(typeof(QuestBattleComponent))).PatrolParties.Add(this);
            }
            else
            {
                ((QuestBattleComponent) Portal.GetComponent(typeof(QuestBattleComponent))).RaidingParties.Add(this);
            }
        }

        protected override void OnFinalize()
        {
            if (Patrol)
            {
                ((QuestBattleComponent) Portal.GetComponent(typeof(QuestBattleComponent))).PatrolParties.Remove(this);
            }
            else
            {
                ((QuestBattleComponent) Portal.GetComponent(typeof(QuestBattleComponent))).RaidingParties.Remove(this);
            }
        }
    }

    public class ChaosRaidingPartySaveDefiner : SaveableTypeDefiner
    {
        public ChaosRaidingPartySaveDefiner() : base(2_543_135)
        {
        }

        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(ChaosRaidingPartyComponent), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(List<ChaosRaidingPartyComponent>));
        }
    }
}