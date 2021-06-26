using StoryMode.GameModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class QuestBattleLocationMenuModel : DefaultEncounterGameMenuModel
    {
        public override string GetEncounterMenu(PartyBase attackerParty, PartyBase defenderParty, out bool startBattle, out bool joinBattle)
        {
            var settlement = this.GetEncounteredPartyBase(attackerParty, defenderParty).Settlement;
            if (settlement != null && settlement.GetComponent<QuestBattleComponent>() != null)
            {
                startBattle = false;
                joinBattle = false;
                return "questlocation_menu";
            }
            else return base.GetEncounterMenu(attackerParty, defenderParty, out startBattle, out joinBattle);
        }
    }
}
