using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ShieldPatterns
{
    class ShieldPatternsMissionLogic : MissionLogic
    {
        private Queue<Agent> _unprocessedAgents = new Queue<Agent>();
        private bool _hasUnprocessedAgents;
        private int counter = 0;
        private readonly int NthAgentToAddBannerTo = 15;

        public override void OnAgentBuild (Agent agent, Banner banner)
        {
            if (!Mission.Current.HasMissionBehavior<BattleSpawnLogic>() && Game.Current.GameType is Campaign) return;
            if (agent.IsHuman)
            {
                _hasUnprocessedAgents = true;
                _unprocessedAgents.Enqueue(agent);
            }
        }

        public override void OnMissionTick(float dt)
        {
            if (!Mission.Current.HasMissionBehavior<BattleSpawnLogic>() && Game.Current.GameType is Campaign) return;
            if (_hasUnprocessedAgents)
            {
                while(_unprocessedAgents.Count > 0)
                {
                    var agent = _unprocessedAgents.Dequeue();
                    try
                    {
                        SwitchShieldPattern(agent);
                    }
                    catch
                    {
                        Utilities.TOWCommon.Log("Tried to assign shield pattern to agent but failed.", NLog.LogLevel.Error);
                    }
                }
                _hasUnprocessedAgents = false;
            }
        }

        private void SwitchShieldPattern(Agent agent)
        {
            string factionId = "";
            if(Game.Current.GameType is Campaign)
            {
                if(agent.Origin is PartyAgentOrigin)
                {
                    var origin = agent.Origin as PartyAgentOrigin;
                    factionId = origin.Party.MapFaction.StringId;
                }
            }

            var banner = ShieldPatternsManager.GetRandomBannerFor(agent.Character.Culture.StringId, factionId);
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
                counter++;
                if(counter > NthAgentToAddBannerTo)
                {
                    var equipment = agent.Equipment[EquipmentIndex.Weapon3];
                    if(equipment.IsEmpty)
                    {
                        counter = 0;
                        var itemId = GetBannerNameForAgent(agent);
                        bool withBanner = itemId == "tor_empire_faction_banner_001" ? true : false;
                        var bannerWeapon = new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>(itemId), null, withBanner ? banner : null);
                        agent.EquipWeaponWithNewEntity(EquipmentIndex.Weapon3, ref bannerWeapon);
                    }
                }
            }
        }

        private string GetBannerNameForAgent(Agent agent)
        {
            List<string> list = new List<string>();
            list.Add("tor_empire_faction_banner_001");
            if (agent.IsUndead())
            {
                list.Add("tor_vc_weapon_banner_002");
                list.Add("tor_vc_weapon_banner_003");
                list.Add("tor_vc_weapon_banner_undead_001");
            }
            else if(agent.Origin is PartyAgentOrigin)
            {
                var origin = agent.Origin as PartyAgentOrigin;
                if(origin.Party.Owner.Clan.StringId == "chaos_clan_1")
                {
                    list.Add("tor_chaos_weapon_banner_001");
                    list.Add("tor_chaos_weapon_banner_002");
                }
                else if(origin.Party.MapFaction.StringId == "averland" && (origin.Troop.IsHero || origin.Troop.Level >= 26))
                {
                    list.Add("tor_empire_weapon_banner_002");
                    list.Add("tor_empire_weapon_banner_003");
                }
                else if (origin.Party.MapFaction.StringId == "stirland" && (origin.Troop.IsHero || origin.Troop.Level >= 26))
                {
                    list.Add("tor_empire_weapon_banner_001");
                }
            }
            return list.TakeRandom(1).FirstOrDefault();
        }
    }
}
