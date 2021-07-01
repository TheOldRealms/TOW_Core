using SandBox;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.Battle.AttributeSystem.CustomBattleMoralModel
{
    public class CustomBattleMoralModel
    {
        public class TOWCampaignBattleMoralModel : DefaultBattleMoraleModel
        {
            public override bool CanPanicDueToMorale(Agent agent)
            {
                if (agent.IsUndead()) return false;
                else return base.CanPanicDueToMorale(agent);
            }
        }

    }
}