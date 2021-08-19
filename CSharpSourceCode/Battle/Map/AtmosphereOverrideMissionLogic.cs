using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Map
{
    public class AtmosphereOverrideMissionLogic : MissionLogic
    {
        public static string currentSceneName = "";
        private readonly string _forceAtmosphereKey = "forceatmo";
        public override void EarlyStart()
        {
            if (Mission.Scene != null && currentSceneName != null && currentSceneName.Contains(_forceAtmosphereKey))
            {
                Mission.Scene.SetAtmosphereWithName(currentSceneName);
            }
        }
    }
}
