using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
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
            /*
            if (Math.Abs(heightAtPosition - frame.origin.z) < 0.5f)
            {
                frame.origin.z = heightAtPosition;
            }
            */
            frame.origin.z = heightAtPosition + base._ability.Template.Radius / 2;
            return frame;
        }
    }
}
