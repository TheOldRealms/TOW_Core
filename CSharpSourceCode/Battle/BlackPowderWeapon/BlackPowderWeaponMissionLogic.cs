using System;
using TaleWorlds.Engine;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using System.Linq;

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

        private void CreateSmokeParticles(MatrixFrame frame, string particleEffectId)
        {
            Mission.AddParticleSystemBurstByName(particleEffectId, frame, false);
        }

        
        private void CreateMuzzleFireSound(Vec3 position)
        {
            if (this._soundIndex.Length > 0)
            {
                int selected = this._random.Next(0, this._soundIndex.Length - 1);
                Mission.MakeSound(this._soundIndex[selected], position, false, true, -1, -1);
            }
        }

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position,
            Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            var itemUsage = shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage;
            var succesfulShot = false;
            if (itemUsage.Contains("handgun") || itemUsage.Contains("pistol"))
            {
                // run firearms script
                if (shooterAgent.WieldedWeapon.Item.StringId.Contains("blunderbuss"))
                {
                    succesfulShot = TryShotgunShot(shooterAgent, weaponIndex, position, orientation, 6, 4);
                }
                else
                {
                    succesfulShot = true; //no requirement regular guns logic
                }

                if (succesfulShot)
                {
                    var frame = new MatrixFrame(orientation, position);
                    frame.Advance((float)((shooterAgent.WieldedWeapon.CurrentUsageItem.WeaponLength + 20) / 100));
                    CreateSmokeParticles(frame, "handgun_shoot_2");
                    CreateMuzzleFireSound(position);
                }
                else
                {
                    SkipReloadPhase(shooterAgent, weaponIndex);
                }
            }
            
        }

        private bool CanConsumeAmmoOfAgent(int amount, Agent agent, WeaponClass ammoType)
        {
            MissionEquipment equipment = agent.Equipment;
            var d = agent.WieldedWeapon.CurrentUsageIndex;
            EquipmentIndex equipmentIndex = (EquipmentIndex)equipment.GetAmmoSlotIndexOfWeapon(ammoType);
            var currentAmmo = equipment.GetAmmoAmount(ammoType);
            if (currentAmmo >= amount)
            {
                short newAmount = (short)(currentAmmo - amount);
                agent.SetWeaponAmountInSlot(equipmentIndex, newAmount, false);
                return true;
            }

            return false;
        }

        private void SkipReloadPhase(Agent agent, EquipmentIndex index)
        {
            //this script seem not work as intended the reload phase is not automatically finalized.
            MissionEquipment equipment = agent.Equipment;
            equipment.SetReloadPhaseOfSlot(index, agent.WieldedWeapon.ReloadPhaseCount);
        }
        
        private void RemoveLastProjectile(Agent shooterAgent)
        {
            var falseMissle = Mission.Missiles.FirstOrDefault(missle => missle.ShooterAgent == shooterAgent);
            if (falseMissle != null) Mission.RemoveMissileAsClient(falseMissle.Index);
        }

        private void RestoreAmmo(Agent agent, WeaponClass ammoType)
        {
            MissionEquipment equipment = agent.Equipment;
            EquipmentIndex equipmentIndex = (EquipmentIndex)equipment.GetAmmoSlotIndexOfWeapon(ammoType);
            short restoredAmmo = (short)(equipment.GetAmmoAmount(ammoType) + 1);
            agent.SetWeaponAmountInSlot(equipmentIndex, restoredAmmo, false);
        }

        private bool TryShotgunShot(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 shotPosition,
            Mat3 shotOrientation, short scatterShots, short requiredAmmoAmount)
        {
            MissionWeapon weaponAtIndex = shooterAgent.Equipment[weaponIndex];
            var weaponData = weaponAtIndex.CurrentUsageItem;
            if (weaponData != null && weaponAtIndex.CurrentUsageItem.IsRangedWeapon)
            {
                RemoveLastProjectile(shooterAgent);
                RestoreAmmo(shooterAgent, weaponData.AmmoClass);
                if (!weaponAtIndex.AmmoWeapon.IsEmpty)
                {
                    var accuracy = 1f / (weaponData.Accuracy * 1.2f); //this should be definable via XML or other data format.
                    if (CanConsumeAmmoOfAgent(requiredAmmoAmount, shooterAgent, weaponData.AmmoClass))
                    {
                        ScatterShot(shooterAgent, accuracy, weaponAtIndex.AmmoWeapon, shotPosition, shotOrientation, weaponData.MissileSpeed, scatterShots);
                        return true;
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// Allows to Create a scattered shot with several projectiles at a given position. Does not require a gun to be executed.
        /// </summary>
        public void ScatterShot(Agent shooterAgent, float accuracy, MissionWeapon projectileType, Vec3 shotPosition,
            Mat3 shotOrientation, float missleSpeed,short scatterShotAmount)
        {
            for (int i = 0; i < scatterShotAmount; i++)
            {
                var deviation = GetRandomOrientation(shotOrientation, accuracy);
                Mission.AddCustomMissile(shooterAgent, projectileType, shotPosition, deviation.f, deviation,
                    missleSpeed, missleSpeed, false, null);
            }
        }


        public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent,
            sbyte attachedBoneIndex)
        {
            base.OnMissileCollisionReaction(collisionReaction, attackerAgent, attachedAgent, attachedBoneIndex);

            if (collisionReaction != Mission.MissileCollisionReaction.BecomeInvisible) return;
            var missileObj = Mission.Missiles.FirstOrDefault(missile => missile.ShooterAgent == attackerAgent);
                
            if (missileObj != null&&missileObj.Weapon.Item.StringId.Contains("grenade"))
            {
                var frame = missileObj.Entity.GetFrame();
                CreateSmokeParticles(frame, "psys_grenade_explosion_1");
            }
        }





        private void DoTwoBarrelsShot(Agent shooterAgent, Vec3 position, Mat3 orientation)
        {
            var weaponData = shooterAgent.WieldedWeapon.CurrentUsageItem;
            var missile = shooterAgent.WieldedWeapon.AmmoWeapon;
            var pos = position;
            pos.y -= 0.1f;
            Mission.AddCustomMissile(shooterAgent, missile, pos, orientation.f, orientation, weaponData.MissileSpeed,
                weaponData.MissileSpeed, false, null);
        }

        private void DoFourBarrelsShot(Agent shooterAgent, Vec3 position, Mat3 orientation)
        {
            var weaponData = shooterAgent.WieldedWeapon.GetWeaponComponentDataForUsage(0);
            var missile = shooterAgent.WieldedWeapon.AmmoWeapon;
            Mat3 orient1 = orientation;
            orient1.RotateAboutUp(10f);
            Mission.AddCustomMissile(shooterAgent, missile, position, orientation.f, orient1, weaponData.MissileSpeed,
                weaponData.MissileSpeed, false, null);
            Mat3 orient2 = orientation;
            orient2.RotateAboutUp(20f);
            Mission.AddCustomMissile(shooterAgent, missile, position, orientation.f, orient2, weaponData.MissileSpeed,
                weaponData.MissileSpeed, false, null);
            Mat3 orient3 = orientation;
            orient3.RotateAboutUp(-15f);
            Mission.AddCustomMissile(shooterAgent, missile, position, orientation.f, orient3, weaponData.MissileSpeed,
                weaponData.MissileSpeed, false, null);
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
    }
}