using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORSettlementMilitiaModel : DefaultSettlementMilitiaModel
    {
        public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
        {
            var result = base.CalculateMilitiaChange(settlement, includeDescriptions);
            if (settlement.IsCastle)
            {
                result.Add(2f, new TextObject("Bonus"));
            }
            else if (settlement.IsTown)
            {
                result.Add(4f, new TextObject("Bonus"));
            }
            return result;
        }
    }
}
