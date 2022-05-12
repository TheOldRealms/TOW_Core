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
                switch (settlement.OwnerClan.Culture.StringId)
                {
                    case "khuzait":
                        result.Add(2f, new TextObject("Bonus"));
                        break;
                    case "empire":
                        result.Add(1f, new TextObject("Bonus"));
                        break;
                    default:
                        result.Add(1f, new TextObject("Bonus"));
                        break;
                }
            }
            else if (settlement.IsTown)
            {
                switch (settlement.OwnerClan.Culture.StringId)
                {
                    case "khuzait":
                        result.Add(4f, new TextObject("Bonus"));
                        break;
                    case "empire":
                        result.Add(3f, new TextObject("Bonus"));
                        break;
                    default:
                        result.Add(2f, new TextObject("Bonus"));
                        break;
                }
            }
            return result;
        }
    }
}
