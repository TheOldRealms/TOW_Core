using SandBox;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomBattleMoralModel
{
    public class CustomBattleMoralModel
    {
        public class TOWCampaignBattleMoraleModel : DefaultBattleMoraleModel
        {
            public override bool CanPanicDueToMorale(Agent agent)
            {
                if (agent.IsUndead() || agent.IsUnbreakable()) return false;
                else return base.CanPanicDueToMorale(agent);
            }

            public override float GetImportance(Agent agent)
            {
                if (agent.IsExpendable()) return 0f;
                else return base.GetImportance(agent);
            }
        }
    }
}