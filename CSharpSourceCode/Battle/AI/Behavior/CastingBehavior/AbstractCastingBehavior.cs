using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public abstract class AbstractCastingBehavior : AgentCombatBehavior
    {
        protected AbstractCastingBehavior(Agent agent, HumanAIComponent aiComponent) : base(agent, aiComponent)
        {
        }
    }
}