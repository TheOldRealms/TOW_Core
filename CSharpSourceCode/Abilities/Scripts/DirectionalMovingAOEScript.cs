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
            float height = 0.2f;
            var terrainHeight = Mission.Current.Scene.GetTerrainHeight(frame.origin.AsVec2);
            Mission.Current.Scene.GetHeightAtPoint(frame.origin.AsVec2, BodyFlags.CommonCollisionExcludeFlagsForCombat, ref height);
            if (height - terrainHeight > 0.5f)
            {
                frame.origin.z = height;
            }
            else frame.origin.z = terrainHeight;
            return frame;
        }
    }
}
