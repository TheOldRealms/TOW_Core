using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Items
{
    public interface IWeaponHitEffect
    {
        void OnHit(Agent affectedAgent, Agent affectorAgent, MissionWeapon affectorWeapon);
    }
}
