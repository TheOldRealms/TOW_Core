using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.Battle.ShieldPatterns
{
    class ShieldPatternsMissionLogic : MissionLogic
    {
        private List<Agent> _unprocessedAgents = new List<Agent>();
        private bool _hasUnprocessedAgents;

        public override void OnAgentBuild (Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            if (agent.IsHuman)
            {
                _hasUnprocessedAgents = true;
                _unprocessedAgents.Add(agent);
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (_hasUnprocessedAgents)
            {
                foreach(var agent in _unprocessedAgents)
                {
                    SwitchShieldPattern(agent);
                }
                _unprocessedAgents.Clear();
                _hasUnprocessedAgents = false;
            }
        }

        private void SwitchShieldPattern(Agent agent)
        {
            var banner = ShieldPatternsManager.GetRandomBannerFor(agent.Character.Culture.StringId);
            if(banner != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (!agent.Equipment[i].IsEmpty && agent.Equipment[i].Item.Type == ItemObject.ItemTypeEnum.Shield)
                    {
                        var equipment = agent.Equipment[i];
                        if(equipment.Item.Type == ItemObject.ItemTypeEnum.Shield)
                        {
                            string stringId = equipment.Item.StringId;
                            agent.RemoveEquippedWeapon((EquipmentIndex)i);
                            var missionWeapon = new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>(stringId), equipment.ItemModifier, banner);
                            agent.EquipWeaponWithNewEntity((EquipmentIndex)i, ref missionWeapon);
                        }
                    }
                }
            }
        }
    }
}
