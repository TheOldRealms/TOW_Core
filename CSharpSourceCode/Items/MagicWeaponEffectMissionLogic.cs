using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Items
{
    public class MagicWeaponEffectMissionLogic : MissionLogic
    {
        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if (HasMagicWeapon(agent))
            {
                var comp = new MagicWeaponAgentComponent(agent);
                agent.AddComponent(comp);
                agent.OnAgentWieldedItemChange += comp.OnWieldedItemChanged;
            }
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
        {
            if(affectorWeapon.Item != null && affectorWeapon.Item.IsMagicWeapon())
            {
                var magiceffect = affectorWeapon.Item.GetMagicalProperties();
                if(magiceffect.ImbuedStatusEffectId != "none")
                {
                    affectedAgent.ApplyStatusEffect(magiceffect.ImbuedStatusEffectId, affectorAgent);
                }
                //TODO: disabling this for first release, we dont actually have an item script. This just clogs system resources and spams the screen with debug messages.
                /*
                if(magiceffect.OnHitScriptName != "none")
                {
                    try
                    {
                        var obj = Activator.CreateInstance(Type.GetType(magiceffect.OnHitScriptName));
                        if(obj is IMagicWeaponHitEffect)
                        {
                            var script = obj as IMagicWeaponHitEffect;
                            script.OnHit(affectedAgent, affectorAgent, affectorWeapon);
                        }
                    }
                    catch(Exception)
                    {
                        TOW_Core.Utilities.TOWCommon.Log("Tried to create magicweapon onhitscript: " + magiceffect.OnHitScriptName + ", but failed.", NLog.LogLevel.Error);
                    }
                }
                */
            }
        }

        private bool HasMagicWeapon(Agent agent)
        {
            if (agent.IsHuman)
            {
                for (int i = 0; i < 4; i++)
                {
                    var weapon = agent.Equipment[i];
                    if (weapon.Item != null)
                    {
                        var magiceffect = weapon.Item.GetMagicalProperties();
                        if (magiceffect != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
