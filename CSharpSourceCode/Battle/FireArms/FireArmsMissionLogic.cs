﻿using System;
using TaleWorlds.Engine;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using SandBox.Source.Missions;

namespace TOW_Core.Battle.FireArms
{
    public class FireArmsMissionLogic : MissionLogic
    {
        private int[] _soundIndex = new int[5];
        private Random _random;
        private bool areEnemiesAlarmed = false;

        public FireArmsMissionLogic()
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
            if (shooterAgent.WieldedWeapon.Item.Name.Contains("Blunderbuss"))
            {
                var weaponData = shooterAgent.WieldedWeapon.GetWeaponComponentDataForUsage(0);
                var scattering = 1f / (weaponData.Accuracy * 1.2f);
                for (int i = 0; i < 10; i++)
                {
                    var missile = shooterAgent.WieldedWeapon.AmmoWeapon;
                    var _orientation = GetRandomOrientationForBlunderbass(orientation, scattering);
                    Mission.AddCustomMissile(shooterAgent, missile, position, _orientation.f, _orientation, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
                }
            }
        }

        private Mat3 GetRandomOrientationForBlunderbass(Mat3 orientation, float scattering)
        {
            float rand1 = MBRandom.RandomFloatRanged(-scattering, scattering);
            orientation.f.RotateAboutX(rand1);
            float rand2 = MBRandom.RandomFloatRanged(-scattering, scattering);
            orientation.f.RotateAboutY(rand2);
            float rand3 = MBRandom.RandomFloatRanged(-scattering, scattering);
            orientation.f.RotateAboutZ(rand3);
            return orientation;
        }
    }
}