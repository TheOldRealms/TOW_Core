using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities;
using TOW_Core.Battle.ObjectDataExtensions;
using TOW_Core.Battle.Voices;
using TOW_Core.Utilities;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Manages with PartyAttributes and StaticAttributes all relevant TOW Information  for Parties and Characters in game during Campaign gameplay.
    ///
    /// Makes use of TaleWorlds methods in order to Save and load data.
    /// </summary>
    public class ExtendedInfoManager: CampaignBehaviorBase
    {

        public ExtendedInfoManager() {}
        
        private MobilePartyExtendedInfo _mainPartyInfo;
        private MapEvent _currentPlayerEvent;
        private bool playerIsInBattle;
        private bool _isloaded;
        private Dictionary<string, MobilePartyExtendedInfo> _partyInfos = new Dictionary<string, MobilePartyExtendedInfo>();
        private Dictionary<string, CharacterExtendedInfo> _characterInfos = new Dictionary<string, CharacterExtendedInfo>();
        private Dictionary<string, HeroExtendedInfo> _heroInfos = new Dictionary<string, HeroExtendedInfo>();

        private List<MobilePartyExtendedInfo> _eventPartyInfos;

        public List<MobilePartyExtendedInfo> GetInfoForActiveInvolvedParties()
        {
            return _eventPartyInfos;
        }

        public MobilePartyExtendedInfo GetPlayerPartyInfo()
        {
            return _mainPartyInfo;
        }
        
        internal CharacterExtendedInfo GetCharacterInfoFor(string id)
        {
            return _characterInfos.ContainsKey(id) ? _characterInfos[id] : null;
        }

        public EventHandler<BattleAttributesArgs> NotifyBattlePartyObservers;
        public override void  RegisterEvents()
        {
            //Game Saving Events
            CampaignEvents.OnBeforeSaveEvent.AddNonSerializedListener(this, OnGameSaving());
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionStart);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this,OnNewGameCreatedPartialFollowUpEnd);
            
            //Tick events
            CampaignEvents.TickEvent.AddNonSerializedListener(this, deltaTime => FillWindsOfMagic(deltaTime));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this,OnDailyTick());
            
            //Events and Battles
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this,EventCreated);
            CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, OnMissionStarted);
            
            //Parties created and destroyed
            CampaignEvents.MobilePartyCreated.AddNonSerializedListener(this,RegisterParty);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this,DeregisterParty);
            
            //Units added
            CampaignEvents.NewCompanionAdded.AddNonSerializedListener(this,OnCompanionAdded);
        }

        private void OnSessionStart(CampaignGameStarter obj)
        {
            //construct character info for all CharacterObject templates loaded by the game.
            //this can be safely reconstructed at each session start without the need to save/load.
            var characters = new List<CharacterObject>();
            MBObjectManager.Instance.GetAllInstancesOfObjectType<CharacterObject>(ref characters);
            foreach(var character in characters)
            {
                var info = new CharacterExtendedInfo();
                info.CharacterStringId = character.StringId;
                info.CharacterAttributes = AttributeManager.GetAttributesFor(character.StringId);
                info.Abilities = AbilityManager.GetAbilitesForCharacter(character.StringId);
                info.VoiceClassName = CustomVoiceManager.GetVoiceClassNameFor(character.StringId);
                _characterInfos.Add(character.StringId, info);
            }
        }

        private void OnMissionEnded(MapEvent mapEvent)
        {
            if (playerIsInBattle)
            {
                playerIsInBattle = false;
            }
        }

        private void OnCompanionAdded(Hero hero)
        {
            _currentAddedHero = hero;
        }

        private void EventCreated(MapEvent mapEvent, PartyBase partyBase, PartyBase arg3)
        {
            if (mapEvent.IsPlayerMapEvent)
            {
                _currentPlayerEvent = mapEvent;
            }
        }

        private void OnMissionStarted()
        {
            // tries to add active parties for the next battlefield for a better overview text output optional 
            if (_currentPlayerEvent != null)
            {
                var eventPartyInfos = new List<MobilePartyExtendedInfo>();
                string text = "PLAYERFIGHT";
                foreach (var party in _currentPlayerEvent.AttackerSide.Parties)
                {
                    MobilePartyExtendedInfo attackerInfo= GetPartyInfoFor(party.Party.Id.ToString());
                    eventPartyInfos.Add(attackerInfo);
                    text += attackerInfo.PartyBaseId;
                }

                text += " are supporting the attackers (" + _currentPlayerEvent.AttackerSide.Parties.Count+")";
            
                foreach (var party in _currentPlayerEvent.DefenderSide.Parties)
                {
                    MobilePartyExtendedInfo  defenderInfo= GetPartyInfoFor(party.Party.Id.ToString());
                    eventPartyInfos.Add(defenderInfo);
                    text += defenderInfo.PartyBaseId;
                }
            
                text += " are supporting the defenders(" + _currentPlayerEvent.DefenderSide.Parties.Count+")";
                
                _eventPartyInfos = eventPartyInfos;
                BattleAttributesArgs e = new BattleAttributesArgs()
                {
                    activeParties = eventPartyInfos
                };
                
                NotifyBattlePartyObservers?.Invoke(this,e);
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

            if (party.IsMainParty)
            {
                _mainPartyInfo = partyInfo;
            }

            if (party.IsBandit)
            {
                partyInfo.PartyType = PartyType.BanditParty;
            }
            
            if (party.LeaderHero != null || party.IsMainParty)       //initialize LordParties
            {   
                Hero Leader = party.LeaderHero;
                partyInfo.Leader = Leader;
                partyInfo.LeaderInfo = leaderAttribute;
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
                TOWCommon.Say("Already added"); 
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
            TOWCommon.Say("save game restored with "+ _partyInfos.Count + "parties in the dictionary");
            _isloaded = true;

            _mainPartyInfo = GetPartyInfoFor(Campaign.Current.MainParty.Party.Id);
            //for later: Check if Attributes are valid, reinitalize for parties if not
        }
        
        private Action OnGameSaving()
        {
            return new Action(OnGameSaveAction);
        }
        
        private void OnGameSaveAction()
        {
            TOWCommon.Say("save game stored with "+ _partyInfos.Count + "parties in the dictionary");
        }
        
        private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter campaignGameStarter)
        {
            InitializeParties();
            _isloaded = true;
        }

        private void FillWindsOfMagic(float TickValue)
        {
            foreach (var entry in _partyInfos)
            {
                if (entry.Value.IsMagicUserParty)
                    entry.Value.WindsOfMagic += TickValue;
                if (entry.Value.WindsOfMagic > 30)
                {
                    entry.Value.WindsOfMagic = 30;
                }
            }
        }

        private Action OnDailyTick()
        {
            return new Action(DailyMessage);
        }

        private void DailyMessage()
        {
            string text = "";
            text +=" Main player has WOM: "+ GetPartyInfoFor(Campaign.Current.MainParty.Party.Id).WindsOfMagic;
            TOWCommon.Say(text);
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
            dataStore.SyncData("_partyInfos", ref _partyInfos);
        }
        
    }
    
        public class BattleAttributesArgs : EventArgs
        {
            public List<MobilePartyExtendedInfo> activeParties;
        }
}