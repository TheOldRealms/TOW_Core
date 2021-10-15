using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Artillery
{
    public class Artillery : UsableMachine
    {
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            return new TextObject("Use (F)");
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Artillery Battery";
        }
    }
}
