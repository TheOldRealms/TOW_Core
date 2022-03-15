using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORCombatXpModel : DefaultCombatXpModel
    {
        public override SkillObject GetSkillForWeapon(WeaponComponentData weapon, bool isSiegeWeaponHit)
        {
            SkillObject result = DefaultSkills.Athletics;
            if (weapon != null)
            {
                if (weapon.WeaponClass == WeaponClass.Cartridge)
                {
                    result = DefaultSkills.Crossbow;
                }
                else
                {
                    result = base.GetSkillForWeapon(weapon, isSiegeWeaponHit);
                }
            }
            return result;
        }
    }
}
