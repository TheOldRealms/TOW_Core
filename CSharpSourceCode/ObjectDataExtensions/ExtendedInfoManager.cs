using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities;
using TOW_Core.Battle.ObjectDataExtensions;
using TOW_Core.Battle.Voices;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Manages the information we extend the TW classes with  (i.e. Hero, MobileParty, (Base)CharacterObject) 
    /// with all relevant TOW Information  for Parties and Characters in game during Campaign gameplay.
    ///
    /// Makes use of TaleWorlds methods in order to Save and load data.
    /// </summary>
    public class ExtendedInfoManager: CampaignBehaviorBase
    {
        private MobilePartyExtendedInfo _mainPartyInfo;
        private Dictionary<string, MobilePartyExtendedInfo> _partyInfos = new Dictionary<string, MobilePartyExtendedInfo>();
        private static Dictionary<string, CharacterExtendedInfo> _characterInfos = new Dictionary<string, CharacterExtendedInfo>();
        private Dictionary<string, HeroExtendedInfo> _heroInfos = new Dictionary<string, HeroExtendedInfo>();
        private static ExtendedInfoManager _instance = new ExtendedInfoManager();

        public static ExtendedInfoManager Instance => _instance;

        private ExtendedInfoManager() {}
        
        public override void RegisterEvents()
        {
            //Game Saving Events
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionStart);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUpEnd);

            //Tick events
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, FillWindsOfMagic);

            //Parties created and destroyed
            CampaignEvents.MobilePartyCreated.AddNonSerializedListener(this, RegisterParty);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, DeregisterParty);

            //heroes
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
        }

        private void FillWindsOfMagic()
        {
            foreach (var entry in _heroInfos)
            {
                if (entry.Value.AllAttributes.Contains("SpellCaster"))
                {
                    entry.Value.MaxWindsOfMagic = Math.Max(entry.Value.MaxWindsOfMagic, 30);
                    entry.Value.CurrentWindsOfMagic += entry.Value.WindsOfMagicRechargeRate;
                    entry.Value.CurrentWindsOfMagic = Math.Min(entry.Value.CurrentWindsOfMagic, entry.Value.MaxWindsOfMagic);
                }
            }
        }

        private void OnHeroCreated(Hero arg1, bool arg2)
        {
            if (!_heroInfos.ContainsKey(arg1.StringId))
            {
                var info = new HeroExtendedInfo(arg1.CharacterObject);
                _heroInfos.Add(arg1.StringId, info);
            }
        }

        private void OnHeroKilled(Hero arg1, Hero arg2, KillCharacterAction.KillCharacterActionDetail arg3, bool arg4)
        {
            if (_heroInfos.ContainsKey(arg1.StringId))
            {
                _heroInfos.Remove(arg1.StringId);
            }
        }

        public MobilePartyExtendedInfo GetPlayerPartyInfo()
        {
            return _mainPartyInfo;
        }
        
        internal static CharacterExtendedInfo GetCharacterInfoFor(string id)
        {
            if(_characterInfos.Count < 1)
            {
                TryLoadCharacters(out _characterInfos);
            }
            return _characterInfos.ContainsKey(id) ? _characterInfos[id] : null;
        }

        private void OnSessionStart(CampaignGameStarter obj)
        {
            TryLoadCharacters(out _characterInfos);
            _mainPartyInfo = MobileParty.MainParty.GetInfo();
        }

        public static void Load()
        {
            TryLoadCharacters(out _characterInfos);
        }

        private static void TryLoadCharacters(out Dictionary<string, CharacterExtendedInfo> infos)
        {
            //construct character info for all CharacterObject templates loaded by the game.
            //this can be safely reconstructed at each session start without the need to save/load.
            Dictionary<string, CharacterExtendedInfo> unitlist = new Dictionary<string, CharacterExtendedInfo>();
            infos = unitlist;
            try
            {
                var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_extendedunitproperties.xml");
                if (File.Exists(path))
                {
                    var ser = new XmlSerializer(typeof(List<CharacterExtendedInfo>));
                    var list = ser.Deserialize(File.OpenRead(path)) as List<CharacterExtendedInfo>;
                    foreach (var item in list)
                    {
                        if (!infos.ContainsKey(item.CharacterStringId))
                        {
                            infos.Add(item.CharacterStringId, item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TOWCommon.Log(e.ToString(), LogLevel.Error);
                throw e; //TODO handle this more gracefully.
            }
        }

        public  MobilePartyExtendedInfo GetPartyInfoFor(string id)
        {
            return _partyInfos.ContainsKey(id) ? _partyInfos[id] : null;
        }

        public HeroExtendedInfo GetHeroInfoFor(string id)
        {
            return _heroInfos.ContainsKey(id) ? _heroInfos[id] : null;
        }

        private void EnterPartyIntoDictionary(MobileParty party)
        {

            if (_partyInfos.ContainsKey(party.Party.Id))
            {
                return;
            }
            
            MobilePartyExtendedInfo partyInfo = new MobilePartyExtendedInfo();
            partyInfo.PartyBaseId = party.Party.Id;
            partyInfo.PartyBase = party.Party;

            if (party.IsBandit)
            {
                partyInfo.PartyType = PartyType.BanditParty;
            }
            
            if (party.LeaderHero != null || party.IsMainParty)       //initialize LordParties
            {   
                Hero Leader = party.LeaderHero;
                partyInfo.Leader = Leader;
                partyInfo.LeaderInfo = party.LeaderHero.GetExtendedInfo();
                partyInfo.PartyType = PartyType.LordParty;
            }
            
            if (party.IsCaravan || party.IsVillager || party.IsMilitia)         //regular parties
            {
                partyInfo.PartyType= PartyType.Regular;
            }
            
            _partyInfos.Add(partyInfo.PartyBaseId, partyInfo);
        }
        
        public void RegisterParty(MobileParty party)
        {
            if (!_partyInfos.ContainsKey(party.Party.Id.ToString()))
            {
                EnterPartyIntoDictionary(party);
            }
            else
            {
                //TOWCommon.Say("Already added"); 
            }
            
        }

        public void DeregisterParty(MobileParty party, PartyBase partyBase)
        {
            if(_partyInfos.ContainsKey(party.Party.Id))
            {
                _partyInfos.Remove(party.Party.ToString());
            }
        }
        
        private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter campaignGameStarter)
        {
            TryLoadCharacters(out _characterInfos);
            InitializeHeroes();
            InitializeParties();
        }

        private void InitializeHeroes()
        {
            foreach(var hero in Hero.AllAliveHeroes)
            {
                if (!_heroInfos.ContainsKey(hero.StringId))
                {
                    var info = new HeroExtendedInfo(hero.CharacterObject);
                    _heroInfos.Add(hero.StringId, info);
                }
            }
        }
        
        private void InitializeParties()
        {
            var parties = Campaign.Current.MobileParties;

            foreach (MobileParty party in parties)
            {
                if (_partyInfos.ContainsKey(party.Party.Id))
                {
                    continue;
                }
                EnterPartyIntoDictionary(party);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_partyInfos", ref _partyInfos);
            dataStore.SyncData("_heroInfos", ref _heroInfos);
        }
        
    }
    
        public class BattleAttributesArgs : EventArgs
        {
            public List<MobilePartyExtendedInfo> activeParties;
        }
}