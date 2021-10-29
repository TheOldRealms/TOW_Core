using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Decision
{
    public interface IAgentBehavior
    {
        void Execute();
        void Terminate();
        Dictionary<IAgentBehavior, Target> CalculateUtility();

        bool IsPositional();
    }
}