using System;
using TaleWorlds.Engine;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using SandBox.Source.Missions;
using System.Linq;
using TOW_Core.Battle.TriggeredEffect.Scripts;
using TOW_Core.Battle.TriggeredEffect;

namespace TOW_Core.Battle.FireArms
{
    public class BlackPowderWeaponMissionLogic : MissionLogic
    {
        private int[] _soundIndex = new int[5];
        private Random _random;
        private bool areEnemiesAlarmed = false;

        public BlackPowderWeaponMissionLogic()
        {
            for (int i = 0; i < 5; i++)
            {
                this._soundIndex[i] = SoundEvent.GetEventIdFromString("musket_fire_sound_" + (i + 1));
            }
            this._random = new Random();
        }

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            var itemUsage = shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage;
            if (shooterAgent.WieldedWeapon.Item.Type == ItemObject.ItemTypeEnum.Thrown)
            {
                Mission.Missile grenade = Mission.Missiles.FirstOrDefault(m => m.ShooterAgent == shooterAgent &&
                                                                               m.Weapon.Item.StringId.Contains("hand_grenade") &&
                                                                               !m.Entity.HasScriptOfType<HandGrenadeScript>());
                if (grenade != null)
                {
                    AddMissileScript<HandGrenadeScript>(grenade, "grenade_explosion");
                }
            }
            if (itemUsage.Contains("handgun") || itemUsage.Contains("pistol"))
            {
                var frame = new MatrixFrame(orientation, position);
                frame = frame.Advance(1.1f);
                // run particles of smoke
                Mission.AddParticleSystemBurstByName("handgun_shoot", frame, false);
                // play sound of shot
                if (this._soundIndex.Length > 0)
                {
                    int selected = this._random.Next(0, this._soundIndex.Length - 1);
                    Mission.MakeSound(this._soundIndex[selected], position, false, true, -1, -1);
                }
                // alarm enemies if it's hideout mission
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
                // run firearms script
                if (shooterAgent.WieldedWeapon.Item.StringId.Contains("blunderbuss"))
                {
                    if (shooterAgent.WieldedWeapon.AmmoWeapon.Item.StringId.Contains("grenade"))
                    {
                        Mission.Missile grenade = Mission.Missiles.FirstOrDefault(m => m.ShooterAgent == shooterAgent &&
                                                               m.Weapon.Item.StringId.Contains("ammo_grenade") &&
                                                               !m.Entity.HasScriptOfType<GrenadeScript>());
                        if (grenade != null)
                        {
                            AddMissileScript<GrenadeScript>(grenade, "grenade_explosion");
                        }
                    }
                    else
                    {
                        DoShotgunShot(shooterAgent, position, orientation, 4);
                    }
                }
                else if (shooterAgent.WieldedWeapon.Item.StringId.Contains("two_barrels"))
                {
                    DoTwoBarrelsShot(shooterAgent, position, orientation);
                }
                else if (shooterAgent.WieldedWeapon.Item.StringId.Contains("four_barrels"))
                {
                    DoFourBarrelsShot(shooterAgent, position, orientation);
                }
            }
        }

        /// <summary>
        /// The script will check each ammo weapon (that fits current usage of shooter's weapon) for bullets to do blunderbuss shot. 
        /// It can use multiple ammo weapons to collect required amount of bullets.
        /// </summary>
        /// <param name="requiredAmmoAmount">Max amount of bullets to do blunderbuss shot.</param>
        private void DoShotgunShot(Agent shooterAgent, Vec3 shotPosition, Mat3 shotOrientation, short requiredAmmoAmount)
        {
            MissionWeapon weapon = MissionWeapon.Invalid;
            short foundAmmoAmount = 0;
            for (EquipmentIndex index = EquipmentIndex.WeaponItemBeginSlot; index < EquipmentIndex.NumAllWeaponSlots; index++)
            {
                weapon = shooterAgent.Equipment[index];
                // Weapon hit points mean amount of ammo
                if (weapon.CurrentUsageItem.WeaponClass == shooterAgent.WieldedWeapon.CurrentUsageItem.AmmoClass && weapon.HitPoints > 0)
                {
                    foundAmmoAmount += Math.Min(requiredAmmoAmount, weapon.HitPoints);
                    short newAmount = (short)(weapon.HitPoints - Math.Min(foundAmmoAmount, weapon.HitPoints));
                    shooterAgent.SetWeaponAmountInSlot(index, newAmount, false);
                    if (foundAmmoAmount == requiredAmmoAmount)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            WeaponComponentData weaponData = shooterAgent.WieldedWeapon.CurrentUsageItem;
            float scattering = 1f / (weaponData.Accuracy * 1.2f);
            while (foundAmmoAmount > 0)
            {
                foundAmmoAmount--;
                var _orientation = GetRandomOrientation(shotOrientation, scattering);
                Mission.AddCustomMissile(shooterAgent, weapon, shotPosition, _orientation.f, _orientation, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
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

        private Mat3 GetRandomOrientation(Mat3 orientation, float scattering)
        {
            float rand1 = MBRandom.RandomFloatRanged(-scattering, scattering);
            orientation.f.RotateAboutX(rand1);
            float rand2 = MBRandom.RandomFloatRanged(-scattering, scattering);
            orientation.f.RotateAboutY(rand2);
            float rand3 = MBRandom.RandomFloatRanged(-scattering, scattering);
            orientation.f.RotateAboutZ(rand3);
            return orientation;
        }

        private void AddMissileScript<TMissileScript>(Mission.Missile grenade, string triggeredEffectName) where TMissileScript : BlackPowderWeaponScript
        {
            GameEntity grenadeEntity = grenade.Entity;
            grenadeEntity.CreateAndAddScriptComponent(typeof(TMissileScript).Name);
            TMissileScript grenadeScript = grenadeEntity.GetFirstScriptOfType<TMissileScript>();
            grenadeScript.SetShooterAgent(grenade.ShooterAgent);
            grenadeScript.SetTriggeredEffect(TriggeredEffectManager.CreateNew(triggeredEffectName));
            grenadeEntity.CallScriptCallbacks();
        }
    }
}
