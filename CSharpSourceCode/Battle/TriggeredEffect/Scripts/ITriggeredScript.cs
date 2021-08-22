using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public interface ITriggeredScript
    {
        void OnTrigger(Vec3 position, Agent triggeredByAgent);
    }
}
