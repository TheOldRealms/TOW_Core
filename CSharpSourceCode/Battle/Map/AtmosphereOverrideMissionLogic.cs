using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Map
{
    public class AtmosphereOverrideMissionLogic : MissionLogic
    {
        private readonly string _forceAtmosphereKey = "forceatmo";

        public override void OnRenderingStarted()
        {
            base.OnRenderingStarted();
            if (Mission.Scene != null && Mission.SceneName.Contains(_forceAtmosphereKey))
            {
                Mission.Scene.SetAtmosphereWithName(Mission.SceneName);
            }
        }
    }
}
