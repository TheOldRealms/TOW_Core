using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle
{
    public class TORCustomBattleAgentStatCalculateModel : CustomBattleAgentStatCalculateModel
    {
        public override float GetMaxCameraZoom(Agent agent)
        {
            bool isAimingWithSniperRifle = Mission.Current.CameraIsFirstPerson &&
                                           Input.IsKeyDown(InputKey.LeftMouseButton) &&
                                           Input.IsKeyDown(InputKey.LeftShift) &&
                                           !Agent.Main.WieldedWeapon.IsEmpty &&
                                           Agent.Main.WieldedWeapon.Item.StringId.Contains("longrifle");
            if (isAimingWithSniperRifle)
            {
                return 3;
            }
            return base.GetMaxCameraZoom(agent);
        }
    }
}
