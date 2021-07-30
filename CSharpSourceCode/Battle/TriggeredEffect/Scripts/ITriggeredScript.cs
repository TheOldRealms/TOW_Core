using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public interface ITriggeredScript
    {
        void OnTrigger(Vec3 position, Agent triggeredByAgent);
    }
}
