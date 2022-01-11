using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Usables;

namespace TOW_Core.Battle.Artillery
{
    public class CannonBallPile : SiegeMachineStonePile
    {
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            return new TextObject("{=!}Cannonball Pile");
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return new TextObject("{=!}Cannonball Pile").ToString();
        }
    }
}
