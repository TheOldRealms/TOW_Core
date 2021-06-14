﻿using System;
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

        private MapEvent currentPlayerEvent;
        private bool playerIsInBattle;
        private bool isFilling;
        private Dictionary<string, int> _cultureCounts = new Dictionary<string, int>();
        private Dictionary<string, PartyAttribute> _partyAttributes = new Dictionary<string, PartyAttribute>();

        private Action<float> deltaTime;

        
       
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

        private void OnMissionStarted()
        {
            if (currentPlayerEvent != null)
            {
                string text = "PLAYERFIGHT";
                foreach (var party in currentPlayerEvent.AttackerSide.Parties)
                {
                    
                    PartyAttribute attackPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    text += attackPartyPartyAttribute.id;
                }

                text += " are supporting the attackers (" + currentPlayerEvent.AttackerSide.Parties.Count+")";
            
                foreach (var party in currentPlayerEvent.DefenderSide.Parties)
                {
                    
                   
                    PartyAttribute  defenderPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    text += defenderPartyPartyAttribute.id;
                
                }
            
                text += " are supporting the defenders(" + currentPlayerEvent.DefenderSide.Parties.Count+")";;


                TOWCommon.Say(text);
            }
        }


        private void FillAttributesInParty(PartyBase party)
        {
            PartyAttribute partyAttribute = GetAttribute(party.Id);
            
            //Army attributes
            foreach (var troop in party.MobileParty.MemberRoster.GetTroopRoster())
            {
              // troop.Character.
            }
            //Hero attributes 
            var Hero = party.LeaderHero;
            //Hero.
            foreach (var companions  in Hero.CompanionsInParty)
            {
                
            }
        }

        private void EventCreated(MapEvent mapEvent, PartyBase partyBase, PartyBase arg3)
        {
            
            if (mapEvent.IsPlayerMapEvent)
            {
                currentPlayerEvent = mapEvent;

                string text = "PLAYERFIGHT";
                foreach (var party in mapEvent.AttackerSide.Parties)
                {
                
                    PartyAttribute attackPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    text += attackPartyPartyAttribute.id;
                }

                text += " are supporting the attackers (" + mapEvent.AttackerSide.Parties.Count+")";
            
                foreach (var party in mapEvent.DefenderSide.Parties)
                {
                
                    PartyAttribute  defenderPartyPartyAttribute= GetAttribute(party.Party.Id.ToString());
                    text += defenderPartyPartyAttribute.id;
                
                }
            
                text += " are supporting the defenders(" + mapEvent.DefenderSide.Parties.Count+")";;


               // TOWCommon.Say(text);
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

            
            PartyAttribute attribute = new PartyAttribute();
            attribute.id = party.Party.Id.ToString();


            foreach (var troop in party.Party.MemberRoster.GetTroopRoster())
            {
                StaticAttribute staticAttribute = new StaticAttribute();
                staticAttribute.race = troop.Character.Culture.StringId;
                staticAttribute.status = "battle ready";
                staticAttribute.MagicEffects = new List<string>();
                attribute.RegularTroopAttributes.Add(staticAttribute);
            }
            
            if (party.LeaderHero != null)
            {
                Hero Leader = party.LeaderHero;
                StaticAttribute leaderAttribute = new StaticAttribute();
                leaderAttribute.race = Leader.Culture.ToString();
                leaderAttribute.MagicUser = true;
                leaderAttribute.faith = 10;
                attribute.LeaderAttribute = leaderAttribute;

                foreach (var companion in Leader.CompanionsInParty)
                {
                    StaticAttribute companionAttribute = new StaticAttribute();
                    companionAttribute.race = companion.Culture.ToString();
                    companionAttribute.MagicUser = true;
                    attribute.CompanionAttributes.Add(companionAttribute);
                }
            }
        

            _partyAttributes.Add(attribute.id, attribute);
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
        }


        private void FillTimer(float TickValue)
        {
            
        }
        
        private void FillWindsOfMagic(float TickValue)
        {
            foreach (var party in _partyAttributes)
            {
                /*if (party.Value.MagicUsers)
                {
                    party.Value.WindsOfMagic += TickValue * 1f;
                }*/
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

            text +=" Main player has WOM: "+ GetAttribute(Campaign.Current.MainParty.Party.Id).WindsOfMagic;
            TOWCommon.Say(text);
        }
        
        private void InitalizeAttributes()
        {
            var parties = Campaign.Current.MobileParties;
           // TOWCommon.Say("Initialize attributes" + Campaign.Current.MobileParties.Count);
            
            foreach (MobileParty party in parties)
            {
                //TOWCommon.Say(party.Party.Id);
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
}