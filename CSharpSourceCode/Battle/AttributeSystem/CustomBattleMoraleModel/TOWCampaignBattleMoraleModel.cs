using SandBox;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomBattleMoralModel
{
    public class CustomBattleMoralModel
    {
        public class TOWCampaignBattleMoraleModel : SandboxBattleMoraleModel
        {
            public override bool CanPanicDueToMorale(Agent agent)
            {
                if (agent.IsUndead() || agent.IsUnbreakable() || agent.Origin is SummonedAgentOrigin) return false;
                else return base.CanPanicDueToMorale(agent);
            }

            public override float GetEffectiveInitialMorale(Agent agent, float baseMorale)
            {
                if (agent.Origin is SummonedAgentOrigin) return baseMorale;
                else return base.GetEffectiveInitialMorale(agent, baseMorale);
            }
        }
    }
}