using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.CampaignSupport.BattleHistory
{
    public class BattleInfoMissionLogic : MissionLogic
    {
        public List<BasicCharacterObject> EnemiesKilled { get; } = new List<BasicCharacterObject>();
        public List<BasicCharacterObject> AlliesKilled { get; } = new List<BasicCharacterObject>();

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if(affectedAgent?.Character != null && (agentState.Equals(AgentState.Killed) || agentState.Equals(AgentState.Unconscious)))
            {
                if(affectedAgent.Team.IsPlayerTeam)
                {
                    AlliesKilled.Add(affectedAgent.Character);
                }
                else
                {
                    EnemiesKilled.Add(affectedAgent.Character);
                }
            }
        }

        public override void OnMissionActivate()
        {
            EnemiesKilled.Clear();
            AlliesKilled.Clear();
        }

        protected override void OnEndMission()
        {
            BattleInfoCampaignBehavior battleInfoContainer = Campaign.Current.GetCampaignBehavior<BattleInfoCampaignBehavior>();
            BattleInfo battleInfo = new BattleInfo();
            battleInfo.EnemiesKilled = EnemiesKilled.Select(charObj => new CharacterInfo(charObj)).ToList();
            battleInfo.AlliesKilled = AlliesKilled.Select(charObj => new CharacterInfo(charObj)).ToList();
            battleInfoContainer.PlayerBattleHistory.Add(battleInfo);
        }
    }
}
