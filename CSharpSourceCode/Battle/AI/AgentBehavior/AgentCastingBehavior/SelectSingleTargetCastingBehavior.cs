using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class SelectSingleTargetCastingBehavior : AoETargetedCastingBehavior
    {
        public SelectSingleTargetCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
        }

        protected override Target UpdateTarget(Target target)
        {
            switch (AbilityTemplate.AbilityTargetType)
            {
                case AbilityTargetType.Self:
                {
                    target.Agent = Agent;
                    break;
                }
                default:
                {
                    target = base.UpdateTarget(target);
                    break;
                }
            }

            //  target.Agent = PowerfulSingleAgentTrackerMissionLogic.ProvideAgentForTeam(Agent.Team);
            return target;
        }
    }
}