using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Reads out ExtendedInfoManager during Combat
    /// </summary>
    public class ExtendedInfoMissionLogic : MissionLogic
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
            if (Mission.Current.IsFriendlyMission || agent.IsMount)
            {
                return;
            }
            
            base.OnAgentBuild(agent, banner);

            if (_isCustomBattle)
            {
                if (agent.Character.IsSoldier) return;
                
                if (agent.Team.IsAttacker)
                {
                    if (agent == Mission.Current.MainAgent)
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
                foreach (var info in _missionPartyInfos)
                {
                    if (agent.Origin.BattleCombatant == info.PartyBase)
                    {
                        var component = new AgentExtendedInfoComponent(agent);
                        component.SetParty(info);
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
             foreach (var info in _missionPartyInfos)
             {
                 CharacterExtendedInfo standardAttribute = new CharacterExtendedInfo(info);
                 info.RegularTroopAttributes.Add(standardAttribute);
                 info.WindsOfMagic = 30f;
                 info.IsMagicUserParty = true;
             }
         }
               
        
        public delegate void OnPlayerPartyAttributeAssigned();
    }
}