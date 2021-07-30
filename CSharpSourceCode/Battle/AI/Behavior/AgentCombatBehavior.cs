using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Behavior
{
    public abstract class AgentCombatBehavior
    {
        protected readonly HumanAIComponent AIComponent;
        protected readonly Agent Agent;

        protected AgentCombatBehavior(Agent agent, HumanAIComponent aiComponent)
        {
            Agent = agent;
            AIComponent = aiComponent;
        }

        public abstract void ApplyBehaviorParams();

        protected OrderType? GetMovementOrderType()
        {
            var moveOrder = Agent?.Formation?.GetReadonlyMovementOrderReference();
            var currentOrderType = moveOrder?.OrderType;
            return currentOrderType;
        }
    }
}