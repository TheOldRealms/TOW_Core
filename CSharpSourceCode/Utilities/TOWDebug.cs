using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.Utilities
{
    public static class TOWDebug
    {
        public static Agent SpawnAgent(Agent agent, MatrixFrame frame = default(MatrixFrame))
        {
            AgentBuildData agentBuildData = new AgentBuildData(agent.Character);
            agentBuildData.NoHorses(true);
            agentBuildData.NoWeapons(true);
            agentBuildData.NoArmor(false);
            agentBuildData.Team(Mission.Current.PlayerEnemyTeam);
            agentBuildData.TroopOrigin(agent.Origin);

            if (frame == default(MatrixFrame))
            {
                frame.origin = agent.Position;
                frame.rotation = agent.Frame.rotation;
            }
            agentBuildData.InitialPosition(frame.origin);

            return Mission.Current.SpawnAgent(agentBuildData, false, 0);
        }

        public static void EquipWeapon(Agent agent, String weaponName)
        {
            ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(weaponName);
            MissionWeapon weapon = new MissionWeapon(item, null, Banner.CreateRandomBanner());
            agent.EquipWeaponToExtraSlotAndWield(ref weapon);
        }
    }
}
