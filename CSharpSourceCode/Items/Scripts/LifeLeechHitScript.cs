using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Items.Scripts
{
    public class LifeLeechHitScript : IMagicWeaponHitEffect
    {
        public void OnHit(Agent affectedAgent, Agent affectorAgent, MissionWeapon affectorWeapon)
        {
            TOW_Core.Utilities.TOWCommon.Say("Life leech hit script called.");
        }
    }
}
