using SandBox.Source.Missions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Quests
{
    public class HideoutAlertMissionLogic : MissionLogic
    {

        private bool _enemiesAreAlarmed = true;
        
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation,
            bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);


            if (_enemiesAreAlarmed) return;
            
            var itemUsage = shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage;
            if (!itemUsage.Contains("handgun") && !itemUsage.Contains("pistol")) return;
                
            var hideoutMissionController = Mission.Current.GetMissionBehavior<HideoutMissionController>();
            if (hideoutMissionController == null) return;
            foreach (var agent in base.Mission.PlayerEnemyTeam.TeamAgents)
            {
                hideoutMissionController.OnAgentAlarmedStateChanged(agent, Agent.AIStateFlag.Alarmed);
                agent.SetWatchState(Agent.WatchState.Alarmed);
            }
            _enemiesAreAlarmed=true;
        }
    }
}