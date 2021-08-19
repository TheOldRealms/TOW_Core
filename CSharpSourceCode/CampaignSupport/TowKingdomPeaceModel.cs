using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace TOW_Core.CampaignSupport
{
    public class TowKingdomPeaceModel : DefaultKingdomDecisionPermissionModel
    {
        public override bool IsAnnexationDecisionAllowed(Settlement annexedSettlement) => false;

        public override bool IsExpulsionDecisionAllowed(Clan expelledClan) => false;

        public override bool IsKingSelectionDecisionAllowed(Kingdom kingdom) => false;

        public override bool IsPeaceDecisionAllowedBetweenKingdoms(Kingdom kingdom1, IFaction kingdom2) => false;

        public override bool IsPolicyDecisionAllowed(PolicyObject policy) => false;

        public override bool IsWarDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2) => false;
    }
}
