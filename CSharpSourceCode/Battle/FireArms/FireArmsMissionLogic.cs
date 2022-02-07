using System;
using TaleWorlds.Engine;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using SandBox.Source.Missions;
using System.Linq;
using TOW_Core.Battle.TriggeredEffect.Scripts;
using TOW_Core.Battle.TriggeredEffect;
using TaleWorlds.InputSystem;
using TOW_Core.Utilities;

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

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(InputKey.P))
            {
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_gun_grenade_launcher_001", EquipmentIndex.ExtraWeaponSlot);
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_gun_grenade_launcher_001", EquipmentIndex.Weapon3);
                TOWDebug.EquipWeapon(Agent.Main, "tor_empire_weapon_grenade_grenade", EquipmentIndex.Weapon4);
            }
        }

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            if (shooterAgent.WieldedWeapon.AmmoWeapon.Item != null && shooterAgent.WieldedWeapon.AmmoWeapon.Item.StringId.Contains("musket_ball"))
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

                if (shooterAgent.WieldedWeapon.Item.StringId.Contains("blunderbuss"))
                {
                    DoBlunderbussShot(shooterAgent, position, orientation);
                }
                else if (shooterAgent.WieldedWeapon.Item.StringId.Contains("two_barrels"))
                {
                    DoTwoBarrelsShot(shooterAgent, position, orientation);
                }
                else if (shooterAgent.WieldedWeapon.Item.StringId.Contains("four_barrels"))
                {
                    DoFourBarrelsShot(shooterAgent, position, orientation);
                }
                else if (shooterAgent.WieldedWeapon.Item.StringId.Contains("grenade_launcher"))
                {
                    AddGrenadeScript(shooterAgent);
                }
            }
        }

        private void DoBlunderbussShot(Agent shooterAgent, Vec3 position, Mat3 orientation)
        {
            var weaponData = shooterAgent.WieldedWeapon.CurrentUsageItem;
            var scattering = 1f / (weaponData.Accuracy * 1.2f);
            for (int i = 0; i < 10; i++)
            {
                var missile = shooterAgent.WieldedWeapon.AmmoWeapon;
                var _orientation = GetRandomOrientationForBlunderbass(orientation, scattering);
                Mission.AddCustomMissile(shooterAgent, missile, position, _orientation.f, _orientation, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
            }
        }

        private void DoTwoBarrelsShot(Agent shooterAgent, Vec3 position, Mat3 orientation)
        {
            var weaponData = shooterAgent.WieldedWeapon.CurrentUsageItem;
            var missile = shooterAgent.WieldedWeapon.AmmoWeapon;
            var pos = position;
            pos.y -= 0.1f;
            Mission.AddCustomMissile(shooterAgent, missile, pos, orientation.f, orientation, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
        }

        private void DoFourBarrelsShot(Agent shooterAgent, Vec3 position, Mat3 orientation)
        {
            var weaponData = shooterAgent.WieldedWeapon.GetWeaponComponentDataForUsage(0);
            var missile = shooterAgent.WieldedWeapon.AmmoWeapon;

            Mat3 orient1 = orientation;
            orient1.RotateAboutUp(10f);
            Mission.AddCustomMissile(shooterAgent, missile, position, orientation.f, orient1, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);

            Mat3 orient2 = orientation;
            orient2.RotateAboutUp(20f);
            Mission.AddCustomMissile(shooterAgent, missile, position, orientation.f, orient2, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);

            Mat3 orient3 = orientation;
            orient3.RotateAboutUp(-15f);
            Mission.AddCustomMissile(shooterAgent, missile, position, orientation.f, orient3, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
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

        private void AddGrenadeScript(Agent shooterAgent)
        {
                Mission.Missile grenade = Mission.Missiles.FirstOrDefault(m => m.ShooterAgent == shooterAgent &&
                                                                               m.Weapon.Item.StringId.Contains("grenade") &&
                                                                               !m.Entity.HasScriptOfType<GrenadeScript>());
                if (grenade != null) Activate(grenade);
        }

        private void Activate(Mission.Missile grenade)
        {
            GameEntity grenadeEntity = grenade.Entity;
            grenadeEntity.CreateAndAddScriptComponent("GrenadeScript");
            GrenadeScript grenadeScript = grenadeEntity.GetFirstScriptOfType<GrenadeScript>();
            grenadeScript.SetShooterAgent(grenade.ShooterAgent);
            grenadeScript.SetTriggeredEffect(TriggeredEffectManager.CreateNew("grenade_explosion"));
            //grenadeEntity.CallScriptCallbacks();
        }
    }
}
