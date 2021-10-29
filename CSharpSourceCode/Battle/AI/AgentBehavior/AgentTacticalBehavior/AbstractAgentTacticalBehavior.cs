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

        public abstract void ApplyBehaviorParams();

        protected OrderType? GetMovementOrderType()
        {
            var moveOrder = Agent?.Formation?.GetReadonlyMovementOrderReference();
            var currentOrderType = moveOrder?.OrderType;
            return currentOrderType;
        }

        public abstract void Execute();
        public abstract void Terminate();

        public float GetLatestScore()
        {
            throw new NotImplementedException();
        }

        public Dictionary<IAgentBehavior, Target> CalculateUtility()
        {
            throw new NotImplementedException();
        }

        public bool IsPositional()
        {
            return false;
        }
    }
}