using System;
using TaleWorlds.Engine;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using SandBox.Source.Missions;

namespace TOW_Core.Battle.FireArms
{
    public class MusketFireEffectMissionLogic : MissionLogic
    {
        private int[] _soundIndex = new int[5];
        private Random _random;
        private bool areEnemiesAlarmed = false;

        public MusketFireEffectMissionLogic()
        {
            for (int i = 0; i < 5; i++)
            {
                this._soundIndex[i] = SoundEvent.GetEventIdFromString("musket_fire_sound_" + (i + 1));
            }
            this._random = new Random();
        }

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            if (shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage == "handgun" || shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage == "handgunMatchlock" || shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage == "pistol")
            {
                var frame = new MatrixFrame(orientation, position);
                frame = frame.Advance(1.1f);
                Mission.AddParticleSystemBurstByName("handgun_shoot", frame, false);
                if (this._soundIndex.Length > 0)
                {
                    int selected = this._random.Next(0, this._soundIndex.Length - 1);
                    Mission.MakeSound(this._soundIndex[selected], position, false, true, -1, -1);
                }
                if (!areEnemiesAlarmed)
                {
                    areEnemiesAlarmed = true;
                    var spawnLogic = Mission.Current.GetMissionBehavior<HideoutMissionController>();
                    if (spawnLogic != null)
                    {
                        foreach (var agent in base.Mission.PlayerEnemyTeam.TeamAgents)
                        {
                            spawnLogic.OnAgentAlarmedStateChanged(agent, Agent.AIStateFlag.Alarmed);
                            agent.SetWatchState(Agent.WatchState.Alarmed);
                        }
                    }
                }
            }
        }
    }
}
