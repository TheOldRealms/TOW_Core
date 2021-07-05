using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Utilities;

namespace TOW_Core.AttributeDataSystem
{
    /// <summary>
    /// Manages with PartyAttributes and StaticAttributes all relevant TOW Information  for Parties and Characters in game during Campaign gameplay.
    ///
    /// Makes use of TaleWorlds methods in order to Save and load data.
    /// </summary>
    public class PartyAttributeManager: CampaignBehaviorBase
    {

        public PartyAttributeManager() {}
        

        private PartyAttribute _playerPartyAttribute;

        private CharacterObject _currentTroop;
        private Hero _currentAddedHero;
        private MapEvent _currentPlayerEvent;
        private bool playerIsInBattle;
        private bool isFilling;
        private bool _isloaded;
        private Dictionary<string, int> _cultureCounts = new Dictionary<string, int>();
        private Dictionary<string, PartyAttribute> _partyAttributes = new Dictionary<string, PartyAttribute>();
        
        private List<PartyAttribute> _activePartyAttributes;
        private Action<float> deltaTime;

        public List<PartyAttribute> GetActiveInvolvedParties()
        {
            return _activePartyAttributes;
        }

        public PartyAttribute GetPlayerPartyAttribute()
        {
            return _playerPartyAttribute;
        }
        
        public EventHandler<BattleAttributesArgs> NotifyBattlePartyObservers;
        public override void  RegisterEvents()
        {
            //Game Saving Events
            CampaignEvents.OnBeforeSaveEvent.AddNonSerializedListener(this, OnGameSaving());
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this,OnNewGameCreatedPartialFollowUpEnd);
            
            //Tick events
            CampaignEvents.TickEvent.AddNonSerializedListener(this, deltaTime => FillWindsOfMagic(deltaTime));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this,OnDailyTick());
            
            //Events and Battles
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this,EventCreated);
            CampaignEvents.BattleStarted.AddNonSerializedListener(this,OnBattleStarted);
            CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, OnMissionStarted);
            
            //Parties created and destroyed
            CampaignEvents.MobilePartyCreated.AddNonSerializedListener(this,RegisterParty);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this,DeregisterParty);
            
            //Units added
            CampaignEvents.OnPartySizeChangedEvent.AddNonSerializedListener(this,OnPartySizeChanged);
            CampaignEvents.NewCompanionAdded.AddNonSerializedListener(this,OnCompanionAdded);
            CampaignEvents.OnTroopRecruitedEvent.AddNonSerializedListener(this,OnTroopRecruited);
            CampaignEvents.OnUnitRecruitedEvent.AddNonSerializedListener(this, OnPlayerRecruitedTroop);
                //missing : OnTroopRemoved
        }

        private void OnMissionEnded(MapEvent mapEvent)
        {
            if (playerIsInBattle)
            {
                playerIsInBattle = false;
            }
        }
        
        private void OnPlayerRecruitedTroop(CharacterObject troop, int amount)
        {
            _currentTroop = troop;
            
            PartyAttribute PlayerParty = GetAttribute(Campaign.Current.MainParty.Party.Id); 
            AddRegularTroopToPartyAttributes(PlayerParty,troop);
        }
        private void OnTroopRecruited(Hero hero, Settlement settlement, Hero RecruitmentSource, CharacterObject Troop, int amount)
        {
            _currentTroop = Troop;
            if(hero==null) return;
            PartyAttribute partyAttribute = GetAttribute(hero.PartyBelongedTo.Party.Id);
            AddRegularTroopToPartyAttributes(partyAttribute, Troop);
        }

        private void OnCompanionAdded(Hero hero)
        {
            _currentAddedHero = hero;
        }
        
        private void AddRegularTroopToPartyAttributes(PartyAttribute party, CharacterObject troop)
        {
            foreach (var attribute in party.RegularTroopAttributes)
            {
                if (attribute.id == troop.ToString());
                    return;
            }
                
            StaticAttribute staticAttribute = new StaticAttribute(party);
            if (troop.Occupation == Occupation.Bandit)
            {
                staticAttribute.id = troop.ToString();

                string undeadMoral = "Undead";
                staticAttribute.CharacterAttributes.Add(undeadMoral);
                party.RegularTroopAttributes.Add(staticAttribute);
            }
            
            staticAttribute.id = troop.ToString();
            party.RegularTroopAttributes.Add(staticAttribute);

            _currentTroop = null;
        }
        private void OnPartySizeChanged(PartyBase partyBase)
        {
            if (!_isloaded)
                return;
            
            if (_currentAddedHero != null && partyBase.MobileParty.IsMainParty)
            {
                //workaround since we are not in knowledge which party is receiving the hero.
                AddHeroToParty(_currentAddedHero, partyBase);
                _currentAddedHero = null;
            }
   
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
                var ActivePartyAttributes = new List<PartyAttribute>();
                string text = "PLAYERFIGHT";
                foreach (var party in _currentPlayerEvent.AttackerSide.Parties)
                {
                    PartyAttribute attackPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    ActivePartyAttributes.Add(attackPartyPartyAttribute);
                    text += attackPartyPartyAttribute.PartyBaseId;
                }

                text += " are supporting the attackers (" + _currentPlayerEvent.AttackerSide.Parties.Count+")";
            
                foreach (var party in _currentPlayerEvent.DefenderSide.Parties)
                {
                    PartyAttribute  defenderPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    ActivePartyAttributes.Add(defenderPartyPartyAttribute);
                    text += defenderPartyPartyAttribute.PartyBaseId;
                }
            
                text += " are supporting the defenders(" + _currentPlayerEvent.DefenderSide.Parties.Count+")";
                
                _activePartyAttributes = ActivePartyAttributes;
                BattleAttributesArgs e = new BattleAttributesArgs()
                {
                    activeParties = ActivePartyAttributes
                };
                
                NotifyBattlePartyObservers?.Invoke(this,e);
            }
        }
        
        private void OnBattleStarted(
            PartyBase attackerParty,
            PartyBase defenderParty,
            object subject,
            bool showNotification)
        {
            PartyAttribute attackPartyPartyAttribute= GetAttribute(attackerParty.Id.ToString());
            PartyAttribute defenderPartyPartyAttribute= GetAttribute(defenderParty.Id.ToString());
            if (attackPartyPartyAttribute != null || defenderPartyPartyAttribute != null)
            {
             //   TOWCommon.Say(attackPartyWorldMapAttribute.id+ "("+attackPartyWorldMapAttribute.culture+")"+ " is fighting now " + defenderPartyWorldMapAttribute.id+ "("+defenderPartyWorldMapAttribute.culture+")");
            }
                
        }

        

        public  PartyAttribute GetAttribute(string id)
        {
            if (_partyAttributes.ContainsKey(id))
            {
                return _partyAttributes[id];
            }
            else
            {
                return null;
            }
            
        }
        
        private void EnterPartyIntoDictionary(MobileParty party)
        {

            if (_partyAttributes.ContainsKey(party.Party.Id))
            {
                return;
            }
            
            PartyAttribute partyAttribute = new PartyAttribute();
            partyAttribute.PartyBaseId = party.Party.Id;
            partyAttribute.PartyBase = party.Party;

            if (party.IsMainParty)
            {
                _playerPartyAttribute = partyAttribute;
            }
            
            foreach (var troop in party.Party.MemberRoster.GetTroopRoster())
            {
                if(troop.Character.IsHero)
                    continue;
                
                StaticAttribute staticAttribute = new StaticAttribute(partyAttribute);
                staticAttribute.id = troop.ToString();
                partyAttribute.RegularTroopAttributes.Add(staticAttribute);
            }

            if (party.IsBandit)
            {
               
                StaticAttribute staticAttribute = new StaticAttribute(partyAttribute);
                string attribute = "";
                //attribute = "Undead";
                attribute = "Human";
                staticAttribute.CharacterAttributes.Add(attribute);
                partyAttribute.RegularTroopAttributes.Add(staticAttribute);//note, bandits only have this, and only one. You cant figure out rosters from bandits
            }
            
            if (party.LeaderHero != null|| party.IsMainParty)       //initialize LordParties
            {   
                Hero Leader = party.LeaderHero;
                partyAttribute.Leader = Leader;
                StaticAttribute leaderAttribute = new StaticAttribute(partyAttribute);
                StaticAttribute companionAttribute = new StaticAttribute(partyAttribute);
                partyAttribute.LeaderAttribute = leaderAttribute;
                partyAttribute.PartyType = PartyType.LordParty;
                
                foreach (var companion in Leader.CompanionsInParty)
                {
                    partyAttribute.CompanionAttributes.Add(companionAttribute);
                }
            }
            
            if (party.IsCaravan || party.IsVillager || party.IsMilitia)         //regular parties
            {
                partyAttribute.PartyType= PartyType.Regular;
            }
            
            _partyAttributes.Add(partyAttribute.PartyBaseId, partyAttribute);
        }

        private void AddHeroToParty(Hero hero, PartyBase party)
        {
            //reading out from an external file, just dummy data for now
            PartyAttribute PartyAttribute = GetAttribute(party.Id);
            StaticAttribute attribute = new StaticAttribute(PartyAttribute);

            attribute.id = hero.CharacterObject.StringId;
            
            AbilityManager.LoadAbilities();

            attribute.Abilities = AbilityManager.GetAbilitesForCharacter(hero.CharacterObject.StringId);
            
            

            PartyAttribute.CompanionAttributes.Add(attribute);
        }
        
        private void RemoveHeroFromParty(Hero hero, PartyBase party)
        {
            PartyAttribute partyAttribute = GetAttribute(party.Id);

            foreach (var companionAttribute in partyAttribute.CompanionAttributes)
            {
                if (companionAttribute.id == hero.Name.ToString())
                {
                    partyAttribute.CompanionAttributes.Remove(companionAttribute);
                    break;
                }
            }
        }
        
        public void RegisterParty(MobileParty party)
        {
            if (!_partyAttributes.ContainsKey(party.Party.Id.ToString()))
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
            if(_partyAttributes.ContainsKey(party.Party.Id))
            {
                _partyAttributes.Remove(party.Party.ToString());
            }
        }
        private void OnGameLoaded()
        {
            TOWCommon.Say("save game restored with "+ _partyAttributes.Count + "parties in the dictionary");
            _isloaded = true;

            _playerPartyAttribute = GetAttribute(Campaign.Current.MainParty.Party.Id);
            //for later: Check if Attributes are valid, reinitalize for parties if not
        }
        
        private Action OnGameSaving()
        {
            
            return new Action(OnGameSaveAction);
        }
        
        private void OnGameSaveAction()
        {
            TOWCommon.Say("save game stored with "+ _partyAttributes.Count + "parties in the dictionary");
        }
        
        private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter campaignGameStarter)
        {
            InitalizeAttributes();
            _isloaded = true;
        }

        private void FillWindsOfMagic(float TickValue)
        {
            foreach (var entry in _partyAttributes)
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
            text +=" Main player has WOM: "+ GetAttribute(Campaign.Current.MainParty.Party.Id).WindsOfMagic;
            TOWCommon.Say(text);
        }
        
        private void InitalizeAttributes()
        {
            var parties = Campaign.Current.MobileParties;

            foreach (MobileParty party in parties)
            {
                if (_partyAttributes.ContainsKey(party.Party.Id))
                {
                    continue;
                }
                EnterPartyIntoDictionary(party);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_partyAttributes", ref _partyAttributes);
        }
        
    }
    
        public class BattleAttributesArgs : EventArgs
        {
            public List<PartyAttribute> activeParties;
        }
}