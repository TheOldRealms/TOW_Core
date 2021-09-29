using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Scripts
{
    public class DirectionalMovingAOEScript : AbilityScript
    {

        protected override MatrixFrame GetNextFrame(MatrixFrame oldFrame, float dt)
        {
            var frame = base.GetNextFrame(oldFrame, dt);
            var heightAtPosition = Mission.Current.Scene.GetGroundHeightAtPosition(frame.origin);
            frame.origin.z = heightAtPosition + base._ability.Template.Radius / 2;
            return frame;
        }
    }
}
