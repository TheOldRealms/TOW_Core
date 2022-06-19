using System;
using TaleWorlds.Engine;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
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
        

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            var itemUsage = shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage;
            
            if (itemUsage.Contains("handgun") || itemUsage.Contains("pistol"))
            {
                var frame = new MatrixFrame(orientation, position);
                frame.Advance((float)(shooterAgent.WieldedWeapon.CurrentUsageItem.WeaponLength/100));
                CreateSmokeParticles(frame,"handgun_shoot_2");
                CreateMuzzleFireSound(position);
              
                // run firearms script
                if (shooterAgent.WieldedWeapon.Item.StringId.Contains("blunderbuss"))
                {
                    DoShotgunShot(shooterAgent,weaponIndex, position, orientation, 6, 4);
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



        private bool ConsumeAmmoOfAgent(int amount, Agent agent, WeaponClass ammoType)
        {
            MissionEquipment equipment = agent.Equipment;

            var d = agent.WieldedWeapon.CurrentUsageIndex;
            
            

            EquipmentIndex equipmentIndex =(EquipmentIndex) equipment.GetAmmoSlotIndexOfWeapon(ammoType);

            var currentAmmo = equipment.GetAmmoAmount(ammoType);

            if (currentAmmo >= amount)
            {
                short newAmount = (short)(currentAmmo - amount);
                //equipment.SetHitPointsOfSlot(equipmentIndex,newAmount);
                //equipment.SetHitPointsOfSlot((EquipmentIndex)d,newAmount);
                //equipment.SetAmountOfSlot((EquipmentIndex)d, newAmount,false);
                //equipment.SetAmountOfSlot(equipmentIndex,newAmount,false);
               //equipment.SetConsumedAmmoOfSlot(equipmentIndex, (short) amount);
                //agent.WieldedWeapon.ConsumeAmmo((short)amount);
                
                agent.SetWeaponAmountInSlot(equipmentIndex,newAmount,false);
               // agent.WieldedWeapon.AmmoWeapon.ConsumeAmmo();
                return true;
            }

            return false;
        }

        private void RestoreAmmo(Agent agent, WeaponClass ammoType)
        {
            MissionEquipment equipment = agent.Equipment;
            EquipmentIndex equipmentIndex =(EquipmentIndex) equipment.GetAmmoSlotIndexOfWeapon(ammoType);
            short restoredAmmo = (short) (equipment.GetAmmoAmount(ammoType)+1);
            agent.SetWeaponAmountInSlot(equipmentIndex,restoredAmmo,false);
        }

        /// <summary>
        /// The script will check each ammo weapon (that fits current usage of shooter's weapon) for bullets to do blunderbuss shot. 
        /// It can use multiple ammo weapons to collect required amount of bullets.
        /// </summary>
        /// <param name="requiredAmmoAmount">Max amount of bullets to do blunderbuss shot.</param>
        ///
        ///
        private void DoShotgunShot(Agent shooterAgent,EquipmentIndex weaponIndex,Vec3 shotPosition ,Mat3 shotOrientation, short scatterShots, short requiredAmmoAmount)
        {
            MissionWeapon weaponAtIndex = shooterAgent.Equipment[weaponIndex];

            var weaponData = weaponAtIndex.CurrentUsageItem;
            
            if (weaponData!=null&&weaponAtIndex.CurrentUsageItem.IsRangedWeapon)
            {
            
                RemoveLastProjectile(shooterAgent);
                RestoreAmmo(shooterAgent,weaponData.AmmoClass);
                if (!weaponAtIndex.AmmoWeapon.IsEmpty)
                {
                    var accuracy = 1f / (weaponData.Accuracy * 1.2f);
                    /*MissionEquipment equipment = shooterAgent.Equipment;
                    var t = equipment.GetAmmoAmount(weaponData.AmmoClass);
                    EquipmentIndex index = (EquipmentIndex) equipment.GetAmmoSlotIndexOfWeapon(weaponData.AmmoClass);
                    equipment.SetHitPointsOfSlot(index,(short) (equipment[index].HitPoints- requiredAmmoAmount));*/

                    if (ConsumeAmmoOfAgent(requiredAmmoAmount, shooterAgent, weaponData.AmmoClass))
                    {
                        ScatterShot(shooterAgent,accuracy,weaponAtIndex.AmmoWeapon,shotPosition,shotOrientation,scatterShots, 1.2f,weaponData.MissileSpeed);
                    }
                }
               
                
                /*float scattering = 1f / (weaponData.Accuracy * 1.2f);
                while (foundAmmoAmount > 0)
                {
                    foundAmmoAmount--;
                    var _orientation = GetRandomOrientation(shotOrientation, scattering);
                    Mission.AddCustomMissile(shooterAgent, weapon, shotPosition, _orientation.f, _orientation, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
                }
              
                
                if (foundAmmoAmount == requiredAmmoAmount)
                {
                            
                    
                }
                TOWCommon.Say(ammo.ToString());*/
            }
        }
        
        


        private void ScatterShot(Agent shooterAgent, float accuracy, MissionWeapon ammo, Vec3 shotPosition, Mat3 shotOrientation, short scatterShots, float scatteringDeviation, float missleSpeed)
        {
            for (int i = 0; i < scatterShots; i++)
            {
                var deviation  = GetRandomOrientation(shotOrientation, accuracy);
                Mission.AddCustomMissile(shooterAgent, ammo, shotPosition, deviation.f, deviation, missleSpeed, missleSpeed, false, null);
            }
        }

        /*private void SetAmmunition(Agent shooterAgent, MissionWeapon rangedWeapon, short ammo, bool IsLoaded)
        {
            shooterAgent.Character.Equipment.GetEquipmentFromSlot()
            var ammoIndex = rangedWeapon.AmmoWeapon.CurrentUsageIndex;
            var lastStage = rangedWeapon.ReloadPhaseCount;
            shooterAgent.SetReloadAmmoInSlot(index,ammoIndex,ammo);
            shooterAgent.SetWeaponReloadPhaseAsClient(index, lastStage);
            rangedWeapon.;
        }*/

        private void RemoveLastProjectile(Agent shooterAgent)
        {
            var falseMissle= Mission.Missiles.FirstOrDefault(missle => missle.ShooterAgent == shooterAgent);
                        
                if(falseMissle!=null)
                    Mission.RemoveMissileAsClient(falseMissle.Index);
        }
        private void DoShotgunShot(Agent shooterAgent, Vec3 shotPosition, Mat3 shotOrientation, short requiredAmmoAmount, WeaponClass ammoType =WeaponClass.Cartridge)
        {
            short foundAmmoAmount = 0;
            
           
            for (EquipmentIndex index = EquipmentIndex.WeaponItemBeginSlot; index < EquipmentIndex.NumAllWeaponSlots; index++)
            {
                var weapon = shooterAgent.Equipment[index];

               
                
                if (weapon.CurrentUsageItem!=null && weapon.CurrentUsageItem.WeaponClass == ammoType)
                {
                    WeaponComponentData weaponData = shooterAgent.WieldedWeapon.CurrentUsageItem;
                    // Weapon hit points mean amount of ammo

                    if (weapon.CurrentUsageItem.WeaponClass == WeaponClass.Cartridge && weapon.HitPoints > 0)
                    {
                        foundAmmoAmount += Math.Min(requiredAmmoAmount, weapon.HitPoints);
                        short newAmount = (short)(weapon.HitPoints - Math.Min(foundAmmoAmount, weapon.HitPoints));
                        
                       // shooterAgent.SetReloadAmmoInSlot(index,);
                        shooterAgent.SetWeaponAmountInSlot(index, newAmount, false);
                        if (foundAmmoAmount == requiredAmmoAmount)
                        {
                            
                            float scattering = 1f / (weaponData.Accuracy * 1.2f);
                            while (foundAmmoAmount > 0)
                            {
                                foundAmmoAmount--;
                                var _orientation = GetRandomOrientation(shotOrientation, scattering);
                                Mission.AddCustomMissile(shooterAgent, weapon, shotPosition, _orientation.f, _orientation, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    if(weapon.CurrentUsageItem!=null)
                    {
                        if (weapon.CurrentUsageItem.IsRangedWeapon)
                        {
                            if (weapon.CurrentUsageItem.WeaponClass != ammoType)
                            {
                                if(weapon.HitPoints>0)
                                    shooterAgent.SetWeaponAmountInSlot(index,(short) (weapon.HitPoints+1),false);
                        
                                var falseMissle= Mission.Missiles.FirstOrDefault(missle => missle.ShooterAgent == shooterAgent);
                        
                                if(falseMissle!=null)
                                    Mission.RemoveMissileAsClient(falseMissle.Index);
                            }
                        }
                    }
                    
                    else
                    {
                        //TODO maybe check for null current usage 
                    }
                }

                
                
               
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
