using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior
{
    public abstract class AbstractAgentTacticalBehavior : IAgentBehavior
    {
        protected readonly HumanAIComponent AIComponent;
        protected readonly Agent Agent;

        protected AbstractAgentTacticalBehavior(Agent agent, HumanAIComponent aiComponent)
        {
            Agent = agent;
            AIComponent = aiComponent;
        }

        protected OrderType? GetMovementOrderType()
        {
            var moveOrder = Agent?.Formation?.GetReadonlyMovementOrderReference();
            var currentOrderType = moveOrder?.OrderType;
            return currentOrderType;
        }

        public void Execute()
        {
            ApplyBehaviorParams();
            Tick();
        }

        public abstract void Tick();
        public abstract void Terminate();

        public float GetLatestScore()
        {
            throw new NotImplementedException();
        }

        public List<BehaviorOption> CalculateUtility()
        {
            throw new NotImplementedException();
        }

        public bool IsPositional()
        {
            return false;
        }

        public abstract void ApplyBehaviorParams();
    }
}