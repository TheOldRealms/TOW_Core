using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.FireArms
{
    public class BlunderbussMissionLogic : MissionLogic
    {
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            if (shooterAgent.WieldedWeapon.Item.Name.Contains("Blunderbuss"))
            {
                var weaponData = shooterAgent.WieldedWeapon.GetWeaponComponentDataForUsage(0);
                var scattering = 1f / (weaponData.Accuracy * 1.2f);
                for (int i = 0; i < 10; i++)
                {
                    var missile = shooterAgent.WieldedWeapon.AmmoWeapon;
                    var _orientation = GetRandomOrientation(orientation, scattering);
                    Mission.AddCustomMissile(shooterAgent, missile, position, _orientation.f, _orientation, weaponData.MissileSpeed, weaponData.MissileSpeed, false, null);
                }
            }
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
