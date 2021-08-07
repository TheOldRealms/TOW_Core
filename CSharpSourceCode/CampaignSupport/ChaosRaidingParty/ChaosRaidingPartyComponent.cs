using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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

        [SaveableProperty(3)] public Settlement Target { get; set; }

        [CachedData] private TextObject _cachedName;

        private ChaosRaidingPartyComponent(Settlement portal, QuestBattleComponent questBattleSettlementComponent, bool patrol)
        {
            this.Portal = portal;
            this.Patrol = patrol;
        }

        private void InitializeChaosRaidingParty(MobileParty mobileParty, int partySize)
        {
            PartyTemplateObject chaosPartyTemplate = Campaign.Current.ObjectManager.GetObject<PartyTemplateObject>("chaos_cultists");
            mobileParty.Party.MobileParty.InitializeMobileParty(chaosPartyTemplate, Portal.Position2D, 1f, troopNumberLimit: partySize);
            mobileParty.ActualClan = Clan.All.ToList().Find(clan => clan.Name.ToString() == "Chaos Warriors");
            mobileParty.Party.Owner = mobileParty.ActualClan.Leader;
            mobileParty.HomeSettlement = Portal;
            mobileParty.Aggressiveness = 2.0f;
            mobileParty.Party.Visuals.SetMapIconAsDirty();
            mobileParty.ItemRoster.Add(new ItemRosterElement(DefaultItems.Meat, MBRandom.RandomInt(partySize * 10, partySize * 20)));
            mobileParty.SetPartyUsedByQuest(true);
        }

        public static MobileParty CreateChaosRaidingParty(
            string stringId,
            Settlement portal,
            QuestBattleComponent component,
            int partySize)
        {
            return MobileParty.CreateParty(stringId,
                new ChaosRaidingPartyComponent(portal, component, false),
                mobileParty => ((ChaosRaidingPartyComponent) mobileParty.PartyComponent).InitializeChaosRaidingParty(mobileParty, partySize));
        }

        public static MobileParty CreateChaosPatrolParty(
            string stringId,
            Settlement portal,
            QuestBattleComponent component,
            int partySize)
        {
            return MobileParty.CreateParty(stringId,
                new ChaosRaidingPartyComponent(portal, component, true),
                mobileParty => ((ChaosRaidingPartyComponent) mobileParty.PartyComponent).InitializeChaosRaidingParty(mobileParty, partySize));
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