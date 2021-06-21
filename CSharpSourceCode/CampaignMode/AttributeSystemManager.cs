using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Definition;
using TaleWorlds.SaveSystem.Load;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignMode
{
    public class AttributeSystemManager: CampaignBehaviorBase
    {
        private static  AttributeSystemManager  _instance = new AttributeSystemManager();

        private AttributeSystemManager()
        {
            
        }
        public static AttributeSystemManager Instance
        {
            get
            {
                return _instance;
            }
        }

        private CharacterObject _currentTroop;
        private Hero _currentAddedHero;
        private MapEvent currentPlayerEvent;
        private bool playerIsInBattle;
        private bool isFilling;
        private bool _isloaded;
        private Dictionary<string, int> _cultureCounts = new Dictionary<string, int>();
        private Dictionary<string, PartyAttribute> _partyAttributes = new Dictionary<string, PartyAttribute>();
        
        public EventHandler<BattleAttributesArgs> NotifyBattlePartyObservers;

        private List<PartyAttribute> _activePartyAttributes;
        private List<PartyAttribute> _activeDefenderAttributes;

        private Action<float> deltaTime;

        public List<PartyAttribute> GetActiveInvolvedParties()
        {
            return _activePartyAttributes;
        }
        
        public List<PartyAttribute> GetActiveDefenderAttributes()
        {
            return _activeDefenderAttributes;
        }
       
        public override void  RegisterEvents()
        {
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoaded);
            //CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.MobilePartyCreated.AddNonSerializedListener(this,RegisterParty);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this,DeregisterParty);
            CampaignEvents.OnBeforeSaveEvent.AddNonSerializedListener(this, OnGameSaving());
            // CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this,OnNewGameCreatedPartialFollowUpEnd);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, deltaTime => FillWindsOfMagic(deltaTime));
            
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this,OnDailyTick());
            CampaignEvents.BattleStarted.AddNonSerializedListener(this,OnBattleStarted);
            //CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this,OnMissionStarted());
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this,EventCreated);
            //CampaignEvents.OnPartyJoinedArmyEvent.AddNonSerializedListener(this,OnArmyJoinedEvent);
           // CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMissionStarted);
            CampaignEvents.PartyAttachedAnotherParty.AddNonSerializedListener(this, OnArmyJoinedEvent);
            
            CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, OnMissionStarted);
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

        
        private void OnArmyJoinedEvent(MobileParty mobileParty)
        {
            PartyAttribute addedAttribute = GetAttribute(mobileParty.Party.Id);
            TOWCommon.Say(addedAttribute.id + "joined the battle");
            

        }

        private void OnPlayerRecruitedTroop(CharacterObject troop, int amount)
        {
            TOWCommon.Say("Player Troop Recruited");
            _currentTroop = troop;
            _currentTroop.Occupation.Ba
            PartyAttribute PlayerParty = GetAttribute(Campaign.Current.MainParty.Party.Id); 
            AddRegularTroopToPartyAttributes(PlayerParty,troop);
        }
        private void OnTroopRecruited(Hero hero, Settlement settlement, Hero RecruitmentSource, CharacterObject Troop, int amount)
        {
            _currentTroop = Troop;
            TOWCommon.Say("Troop Recruited");
            if(hero==null) return;
            PartyAttribute partyAttribute = GetAttribute(hero.PartyBelongedTo.Party.Id);
            
            AddRegularTroopToPartyAttributes(partyAttribute, Troop);
            
        }

        private void OnCompanionAdded(Hero hero)
        { TOWCommon.Say("Hero created");
            _currentAddedHero = hero;
            
        }


        private void AddRegularTroopToPartyAttributes(PartyAttribute party, CharacterObject troop)
        {
            foreach (var attribute in party.RegularTroopAttributes)
            {
                if (attribute.id == troop.ToString());
                return;
            }
                
            StaticAttribute staticAttribute = new StaticAttribute();
            if (troop.Occupation == Occupation.Bandit)
            {
                
                //random data   work around for rogue members
                staticAttribute.race = troop.Culture.StringId;
                staticAttribute.status = "not so ready";
                staticAttribute.MagicEffects = new List<string>();
                staticAttribute.id = troop.ToString();
                staticAttribute.number = party.PartyBase.MemberRoster.GetTroopCount(troop);
                party.RegularTroopAttributes.Add(staticAttribute);
            }
            
            //random data
            staticAttribute.race = troop.Culture.StringId;
            staticAttribute.status = "battle ready";
            staticAttribute.MagicEffects = new List<string>();
            staticAttribute.id = troop.ToString();
            staticAttribute.number = party.PartyBase.MemberRoster.GetTroopCount(troop);
            party.RegularTroopAttributes.Add(staticAttribute);
            
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

        private void OnMissionStarted()
        {
            if (currentPlayerEvent != null)
            {
                var ActivePartyAttributes = new List<PartyAttribute>();
                var ActiveDefenderAttributeList = new List<PartyAttribute>();
                string text = "PLAYERFIGHT";
                foreach (var party in currentPlayerEvent.AttackerSide.Parties)
                {
                    
                    PartyAttribute attackPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    ActivePartyAttributes.Add(attackPartyPartyAttribute);
                    text += attackPartyPartyAttribute.id;
                }

                text += " are supporting the attackers (" + currentPlayerEvent.AttackerSide.Parties.Count+")";
            
                foreach (var party in currentPlayerEvent.DefenderSide.Parties)
                {
                    
                    PartyAttribute  defenderPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    ActivePartyAttributes.Add(defenderPartyPartyAttribute);
                    text += defenderPartyPartyAttribute.id;
                
                }
            
                text += " are supporting the defenders(" + currentPlayerEvent.DefenderSide.Parties.Count+")";;

                _activePartyAttributes = ActivePartyAttributes;
                // TOWCommon.Say(text);
                BattleAttributesArgs e = new BattleAttributesArgs()
                {
                    activeParties = ActivePartyAttributes,
                };
                
                NotifyBattlePartyObservers?.Invoke(this,e);


                
            }
        }


       

        private void EventCreated(MapEvent mapEvent, PartyBase partyBase, PartyBase arg3)
        {
            currentPlayerEvent = mapEvent;
            

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
                TOWCommon.Say("couldnt find party");
                return null;
            }
            
        }
        
        private void EnterPartyIntoDictionary(MobileParty party)
        {
            if (_partyAttributes.ContainsKey(party.Party.Id.ToString()))
            {
                TOWCommon.Say(party.Id.ToString()+  " was already there");
                return;
            }
            
            PartyAttribute partyAttribute = new PartyAttribute();
            partyAttribute.id = party.Party.Id.ToString();

            partyAttribute.PartyBase = party.Party;
            
            foreach (var troop in party.Party.MemberRoster.GetTroopRoster())
            {
                if(troop.Character.IsHero)
                    continue;
                
                StaticAttribute staticAttribute = new StaticAttribute();
                staticAttribute.race = troop.Character.Culture.StringId;
                staticAttribute.status = "battle ready";
                staticAttribute.MagicEffects = new List<string>();
                staticAttribute.id = troop.ToString();
                staticAttribute.number = party.Party.MemberRoster.Count;
                partyAttribute.RegularTroopAttributes.Add(staticAttribute);
            }

            if (party.IsBandit)
            {
                StaticAttribute staticAttribute = new StaticAttribute();
                
                staticAttribute.race = "bandit";
                staticAttribute.status = "battle ready";
                staticAttribute.MagicEffects = new List<string>();

                staticAttribute.id = party.Party.ToString();
                partyAttribute.PartyType = PartyType.RogueParty;
                partyAttribute.RegularTroopAttributes.Add(staticAttribute);
            }

            if (party.IsMainParty)
            {
               // TOWCommon.Say("hey");       //Dirty, Somehow the Heroes are 
               // partyAttribute.RegularTroopAttributes.Clear();
            }

           
            
            if (party.LeaderHero != null|| party.IsMainParty)       //initialize LordParties
            {   
                Hero Leader = party.LeaderHero;
                partyAttribute.Leader = Leader;
                StaticAttribute leaderAttribute = new StaticAttribute();
                StaticAttribute companionAttribute = new StaticAttribute();
                leaderAttribute.race = Leader.Culture.ToString();
                leaderAttribute.MagicUser = true;   //neeeds a proper check
                leaderAttribute.faith = 10;
                partyAttribute.LeaderAttribute = leaderAttribute;
                partyAttribute.PartyType = PartyType.LordParty;

                if (leaderAttribute.MagicUser)
                    partyAttribute.MagicUserParty = true;

                foreach (var companion in Leader.CompanionsInParty)
                {
                    
                    companionAttribute.race = companion.Culture.ToString();
                    companionAttribute.MagicUser = true;    //here aswell proper check of magic abilities
                    partyAttribute.CompanionAttributes.Add(companionAttribute);
                    
                    if (companionAttribute.MagicUser)
                        partyAttribute.MagicUserParty = true;
                }
            }
            
            
            if (party.IsCaravan || party.IsVillager || party.IsMilitia)         //regular parties
            {
                partyAttribute.PartyType= PartyType.Regular;
            }

            partyAttribute.numberOfRegularTroops = party.Party.NumberOfRegularMembers;
            
            
            _partyAttributes.Add(partyAttribute.id, partyAttribute);
        }

        private void AddHeroToParty(Hero hero, PartyBase party)
        {
            //reading out from an external file, just dummy code for now
            PartyAttribute PartyAttribute = GetAttribute(party.Id);
            StaticAttribute attribute = new StaticAttribute();
            attribute.id = hero.Name.ToString();
            attribute.faith = 5;
            attribute.MagicUser = true; //hero.IsMagicUser
            attribute.MagicEffects = new List<string>();
            
            attribute.MagicEffects.Add("Fireball");
            attribute.MagicEffects.Add("BurningSkull");
            attribute.SkillBuffs.Add("EternalFire");

            attribute.race = hero.Culture.ToString();

            attribute.status = "blessed";
            
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
            if(_partyAttributes.ContainsKey(party.Id.ToString()))
            {
                _partyAttributes.Remove(party.ToString());
                TOWCommon.Say("Removed + " + party.Id.ToString()); 
            }
        }
        private void OnGameLoaded()
        {
            TOWCommon.Say("save game restored with "+ _partyAttributes.Count + "parties in the dictionary");
            _isloaded = true;
            
            TOWCommon.Say(GetAttribute(Campaign.Current.MainParty.Party.Id).LeaderAttribute.race);
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


        private void FillTimer(float TickValue)
        {
            
        }
        
        private void FillWindsOfMagic(float TickValue)
        {
            foreach (var entry in _partyAttributes)
            {
                if (entry.Value.MagicUserParty)
                    entry.Value.WindsOfMagic += TickValue;
            }
        }

        private Action OnDailyTick()
        {
            return new Action(DailyMessage);
        }

        private void DailyMessage()
        {
            string text = "";
            foreach (var culture in _cultureCounts)
            {
                text+=(culture.Key + " " + _cultureCounts[culture.Key]+ ", ");
            }

          //  text +=" Main player has WOM: "+ GetAttribute(Campaign.Current.MainParty.Party.Id).WindsOfMagic;
            TOWCommon.Say(text);
        }
        
        private void InitalizeAttributes()
        {
            var parties = Campaign.Current.MobileParties;

            foreach (MobileParty party in parties)
            {
                if (_partyAttributes.ContainsKey(party.Id.ToString()))
                {
                    TOWCommon.Say(party.Id.ToString()+  " was already there");
                    continue;
                }
                EnterPartyIntoDictionary(party);
            }
            TOWCommon.Say(_partyAttributes.Count + "of "+  parties.Count+ " were initalized");
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