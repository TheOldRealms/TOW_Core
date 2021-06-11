using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using LogLevel = NLog.LogLevel;

namespace TOW_Core.Battle.FireArms
{
    public class MusketFireEffectMissionLogic : MissionLogic
    {
        private int[] _soundIndex = new int[5];
        private Random _random;

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
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            
            if(shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage == "handgun" ||shooterAgent.WieldedWeapon.CurrentUsageItem.ItemUsage == "handgunMatchlock")
            {
                
                var direction = shooterAgent.LookDirection;
                var frame = new MatrixFrame(Mat3.CreateMat3WithForward(in direction), position);
                frame = frame.Advance(1.1f);
                frame.Rotate(TOWMath.GetDegreeInRadians(90f), Vec3.Up);
                Mission.AddParticleSystemBurstByName("handgun_shoot", frame, false);
                if(this._soundIndex.Length > 0)
                {
                    int selected = this._random.Next(0, this._soundIndex.Length - 1);
                    Mission.MakeSound(this._soundIndex[selected], position, false, true, -1, -1);
                }

                //shooterAgent.GetComponent<StatusEffectComponent>().RunStatusEffect(1);

            }
            
        }
    }
}
