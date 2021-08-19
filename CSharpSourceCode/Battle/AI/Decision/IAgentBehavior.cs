using System.Collections.Generic;

namespace TOW_Core.Battle.AI.Decision
{
    public interface IAgentBehavior
    {
        void Execute();
        void Terminate();
        Dictionary<(IAgentBehavior, Target), float> CalculateUtility();
    }
}