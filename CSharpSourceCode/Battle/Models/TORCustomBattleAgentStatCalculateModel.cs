using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.CrosshairMissionBehavior;

namespace TOW_Core.Battle
{
    public class TORCustomBattleAgentStatCalculateModel : CustomBattleAgentStatCalculateModel
    {
        private CustomCrosshairMissionBehavior _crosshairBehavior;

        public override float GetMaxCameraZoom(Agent agent)
        {
            if (_crosshairBehavior == null)
            {
                _crosshairBehavior = Mission.Current.GetMissionBehavior<CustomCrosshairMissionBehavior>();
            }

            if (_crosshairBehavior != null && _crosshairBehavior.IsUsingSniperScope)
            {
                return 3;
            }
            return base.GetMaxCameraZoom(agent);
        }
    }
}
