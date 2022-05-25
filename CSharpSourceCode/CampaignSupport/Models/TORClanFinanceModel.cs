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
    public class TORClanFinanceModel : DefaultClanFinanceModel
    {
        private static readonly string _cheatGoldAdjustmentName = "AI Gold Adjustment";

        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
            var num = base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals);
            if(clan.Kingdom != null && clan.Kingdom.RulingClan != clan && clan != Clan.PlayerClan && !clan.IsMinorFaction && num.ResultNumber < 0 && clan.Gold < 100000)
            {
                AdjustIncomeForAI(ref num);
            }
            return num;
        }

        public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
            var income = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals);
            var num = CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals);
            var cheat = num.GetLines().Where(x => x.name == _cheatGoldAdjustmentName);
            if(cheat != null && cheat.Count() > 0)
            {
                income.Add(cheat.FirstOrDefault().number, new TextObject(_cheatGoldAdjustmentName));
            }
            return income;
        }

        private void AdjustIncomeForAI(ref ExplainedNumber num)
        {
            num.Add(Math.Abs(num.ResultNumber) + 200, new TextObject(_cheatGoldAdjustmentName));
        }
    }
}
