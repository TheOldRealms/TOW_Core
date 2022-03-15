using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    class TORCompanionHiringPriceCalculationModel : DefaultCompanionHiringPriceCalculationModel
	{
        public override int GetCompanionHiringPrice(Hero companion)
        {
            if (companion.Template.IsTOWTemplate() && companion.IsWanderer)
            {
                return 2000;
            }
            else return base.GetCompanionHiringPrice(companion);
		}
    }
}
