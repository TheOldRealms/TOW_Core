using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.AttributeDataSystem
{
    /// <summary>
    /// Reads out PartyAttributeManager during Combat
    /// </summary>
    public class StaticAttributeMissionLogic : MissionLogic
    {
        private Mission.TeamCollection teams;
        private ExtendedInfoManager _infoManager;

        private List<MobilePartyExtendedInfo> _missionPartyInfos;
        

        private List<Agent> _agents;


        private List<MobilePartyExtendedInfo> _attackerInfos;
        private List<MobilePartyExtendedInfo> _defenderInfos;

        private MobilePartyExtendedInfo _playerPartyInfo;

        private bool _isCustomBattle;

        public event OnPlayerPartyAttributeAssigned NotifyPlayerPartyAttributeAssignedObservers;

        public List<MobilePartyExtendedInfo> GetAttackerAttributes()
        {
            return _attackerInfos;
        }
        
        public List<MobilePartyExtendedInfo> GetDefenderAttributes()
        {
            return _defenderInfos;
        }

        public MobilePartyExtendedInfo GetPlayerAttribute()
        {
            return _playerPartyInfo;
        }
        
        public override void AfterStart()
        {
            base.AfterStart();
            teams= Mission.Current.Teams;
            _missionPartyInfos = new List<MobilePartyExtendedInfo>();
            _agents = new List<Agent>();
            _attackerInfos = new List<MobilePartyExtendedInfo>();
            _defenderInfos = new List<MobilePartyExtendedInfo>();
            if (Campaign.Current != null)
            {
                _infoManager = Campaign.Current.GetCampaignBehavior<ExtendedInfoManager>();
                _missionPartyInfos = _infoManager.GetInfoForActiveInvolvedParties();
            }
            else
            {
                SetForCustomBattle();
                _isCustomBattle = true;
            }
            
        }
        
         public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if (Mission.Current.IsFriendlyMission)
            {
                return;
            }
            
            base.OnAgentBuild(agent, banner);

            if (agent.IsMount)
                return;

            if (_isCustomBattle)
            {
                if (agent.Character.IsSoldier) return;
                
                if (agent.Team.IsAttacker)
                {
                    if (agent== Mission.Current.MainAgent)
                    {
                        _playerPartyInfo = _attackerInfos[0];
                    }
                    AddStaticAttributeComponent(agent, _attackerInfos[0].RegularTroopAttributes[0], _attackerInfos[0]);
                }
                else
                {
                    if (agent== Mission.Current.MainAgent)
                    {
                        _playerPartyInfo = _defenderInfos[0];
                        
                    }
                    AddStaticAttributeComponent(agent, _defenderInfos[0].RegularTroopAttributes[0], _defenderInfos[0]);
                }
                
            }
            else
            {
                foreach (var partyAttribute in _missionPartyInfos)
                {
                    if (agent.Origin.BattleCombatant== partyAttribute.PartyBase)
                    {
                        var partyType = partyAttribute.PartyType;
                        switch (partyType)
                        {
                            case PartyType.BanditParty:
                                AddStaticAttributeComponent(agent,partyAttribute.RegularTroopAttributes[0],partyAttribute);
                                break;
                            
                            case PartyType.Regular:
                                var regularTroopAttribute = FindAttribute(agent.Origin.Troop.ToString(), partyAttribute.RegularTroopAttributes);
                                if (regularTroopAttribute != null)
                                    AddStaticAttributeComponent(agent, regularTroopAttribute, partyAttribute);
                                break;
                            
                            case PartyType.LordParty:
                                if (!agent.IsHero)
                                {
                                    if (agent.Character.IsSoldier && !partyAttribute.RegularTroopAttributes.IsEmpty())
                                    {
                                        var LordPartyRegularTroopAttribute = FindAttribute(agent.Origin.Troop.ToString(), partyAttribute.RegularTroopAttributes);
                                        if (LordPartyRegularTroopAttribute != null)
                                            AddStaticAttributeComponent(agent, LordPartyRegularTroopAttribute, partyAttribute);
                                    }
                                }
                                else
                                {
                                    if (agent.Character.Name == partyAttribute.Leader.Name)
                                    {
                                        var leaderAttribute = partyAttribute.LeaderAttribute;
                                        AddStaticAttributeComponent(agent, leaderAttribute, partyAttribute);
                                        break;
                                    }
                                    if (!partyAttribute.CompanionAttributes.IsEmpty())
                                    {
                                        var CompanionAttribute = FindAttribute(agent.Character.Name.ToString(), partyAttribute.RegularTroopAttributes);
                                        if (CompanionAttribute != null)
                                            AddStaticAttributeComponent(agent, CompanionAttribute, partyAttribute);
                                    }
                                }
                                break;
                        }
                    }
                }

            }
            _agents.Add(agent);

            
        }


         private void SetForCustomBattle()
         {
             _missionPartyInfos = new List<MobilePartyExtendedInfo>();
             MobilePartyExtendedInfo attackerAttribute = new MobilePartyExtendedInfo();
             attackerAttribute.PartyBaseId = "attacker";
             _attackerInfos.Add(attackerAttribute);
             MobilePartyExtendedInfo defenderAttribute = new MobilePartyExtendedInfo();
             defenderAttribute.PartyBaseId = "defender";
             _defenderInfos.Add(defenderAttribute);
             _missionPartyInfos.Add(attackerAttribute);
             _missionPartyInfos.Add(defenderAttribute);
             foreach (var partyAttribute in _missionPartyInfos)
             {
                 CharacterExtendedInfo standardAttribute = new CharacterExtendedInfo(partyAttribute);
                 partyAttribute.RegularTroopAttributes.Add(standardAttribute);
                 partyAttribute.WindsOfMagic = 30f;
                 partyAttribute.IsMagicUserParty = true;
             }
         }

         private CharacterExtendedInfo FindAttribute(string id, List<CharacterExtendedInfo> attributes)
         {
             foreach (var attribute in attributes)
             {
                 if (id == attribute.id)
                 {
                     return attribute;
                 }
             }

             return null;
         }
        
         
        private void AddStaticAttributeComponent(Agent agent, CharacterExtendedInfo attribute, MobilePartyExtendedInfo partyAttribute)
        {
            AgentExtendedInfoComponent agentComponent = new AgentExtendedInfoComponent(agent);
            agentComponent.SetParty(partyAttribute);
            agentComponent.SetAttribute(attribute);
            
            
           //agent.InitializePartyAttribute(partyAttribute);
            
            agent.AddComponent(agentComponent);
            
            if (agent == Mission.Current.MainAgent)
            {
                _playerPartyInfo = partyAttribute;
                NotifyPlayerPartyAttributeAssignedObservers?.Invoke(); 
            }
            
        }
        
        
        public delegate void OnPlayerPartyAttributeAssigned();
    }
}