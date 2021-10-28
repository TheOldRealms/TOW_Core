using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.FireArms
{
    public class BlunderbussMissionLogic : MissionLogic
    {
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            if (shooterAgent.WieldedWeapon.Item.Name.Contains("Blunderbuss"))
            {
                for (int i = 0; i < 10; i++)
                {
                    var missile = shooterAgent.WieldedWeapon.AmmoWeapon;
                    var orient = GetRandomOrientation(orientation);
                    Mission.AddCustomMissile(shooterAgent, missile, position, orient.f, orient, 150, 150, false, null);
                }
            }
        }

        private Mat3 GetRandomOrientation(Mat3 orientation)
        {
            float rand1 = MBRandom.RandomFloatRanged(-_deviation, _deviation);
            orientation.f.RotateAboutX(rand1);
            float rand2 = MBRandom.RandomFloatRanged(-_deviation, _deviation);
            orientation.f.RotateAboutY(rand2);
            float rand3 = MBRandom.RandomFloatRanged(-_deviation, _deviation);
            orientation.f.RotateAboutZ(rand3);
            return orientation;
        }

        private float _deviation = 0.15f;
    }
}
