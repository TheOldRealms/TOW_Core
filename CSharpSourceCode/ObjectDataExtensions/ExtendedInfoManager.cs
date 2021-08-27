using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
        private MapEvent _currentPlayerEvent;
        private Dictionary<string, MobilePartyExtendedInfo> _partyInfos = new Dictionary<string, MobilePartyExtendedInfo>();
        private Dictionary<string, CharacterExtendedInfo> _characterInfos = new Dictionary<string, CharacterExtendedInfo>();
        private Dictionary<string, HeroExtendedInfo> _heroInfos = new Dictionary<string, HeroExtendedInfo>();
        private static Dictionary<string, CharacterExtendedInfo> _customBattleInfos = new Dictionary<string, CharacterExtendedInfo>();

        public ExtendedInfoManager() {}
        
        public override void RegisterEvents()
        {
            //Game Saving Events
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionStart);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUpEnd);

            //Tick events
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, FillWindsOfMagic);

            //Events and Battles
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, EventCreated);

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
        
        internal CharacterExtendedInfo GetCharacterInfoFor(string id)
        {
            if(_characterInfos.Count < 1)
            {
                TryLoadCharacters();
            }
            return _characterInfos.ContainsKey(id) ? _characterInfos[id] : null;
        }

        private void OnSessionStart(CampaignGameStarter obj)
        {
            TryLoadCharacters();
            _mainPartyInfo = MobileParty.MainParty.GetInfo();
        }

        internal static CharacterExtendedInfo GetCharacterInfoForStatic(string id)
        {
            if(_customBattleInfos.Count < 1)
            {
                TryLoadCharactersStatic();
            }
            CharacterExtendedInfo info = null;
            _customBattleInfos.TryGetValue(id, out info);
            return info;
        }

        private static void TryLoadCharactersStatic()
        {
            var characters = new List<string>();
            characters.AddRange(AttributeManager.GetAllCharacterIds());
            foreach(var character in AbilityManager.GetAllCharacterIds())
            {
                if (!characters.Contains(character)) characters.Add(character);
            }
            foreach (var character in characters)
            {
                if (!_customBattleInfos.ContainsKey(character))
                {
                    var info = new CharacterExtendedInfo();
                    info.CharacterStringId = character;
                    var attributes = AttributeManager.GetAttributesFor(character);
                    if (attributes != null) info.CharacterAttributes = attributes;
                    var abilities = AbilityManager.GetAbilitesForCharacter(character);
                    if (abilities != null) info.Abilities = abilities;
                    info.VoiceClassName = CustomVoiceManager.GetVoiceClassNameFor(character);
                    _customBattleInfos.Add(character, info);
                }
            }
        }

        private void TryLoadCharacters()
        {
            //construct character info for all CharacterObject templates loaded by the game.
            //this can be safely reconstructed at each session start without the need to save/load.
            var characters = new List<CharacterObject>();
            MBObjectManager.Instance.GetAllInstancesOfObjectType<CharacterObject>(ref characters);
            foreach (var character in characters)
            {
                if (!_characterInfos.ContainsKey(character.StringId))
                {
                    var info = new CharacterExtendedInfo();
                    info.CharacterStringId = character.StringId;
                    var attributes = AttributeManager.GetAttributesFor(character.StringId);
                    if (attributes != null) info.CharacterAttributes = attributes;
                    var abilities = AbilityManager.GetAbilitesForCharacter(character.StringId);
                    if (abilities != null) info.Abilities = abilities;
                    info.VoiceClassName = CustomVoiceManager.GetVoiceClassNameFor(character.StringId);
                    _characterInfos.Add(character.StringId, info);
                }
            }
        }

        private void EventCreated(MapEvent mapEvent, PartyBase partyBase, PartyBase arg3)
        {
            if (mapEvent.IsPlayerMapEvent)
            {
                _currentPlayerEvent = mapEvent;
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
        private void OnGameLoaded()
        {
            /*
            TOWCommon.Say("save game restored with "+ _partyInfos.Count + "parties in the dictionary");
            _isloaded = true;

            _mainPartyInfo = GetPartyInfoFor(Campaign.Current.MainParty.Party.Id);
            //for later: Check if Attributes are valid, reinitalize for parties if not
            */
        }
        
        private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter campaignGameStarter)
        {
            TryLoadCharacters();
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