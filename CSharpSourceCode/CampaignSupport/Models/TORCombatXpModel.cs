using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORCombatXpModel : DefaultCombatXpModel
    {
        public override SkillObject GetSkillForWeapon(WeaponComponentData weapon)
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
                    result = weapon.RelevantSkill;
                }
            }
            return result;
        }
    }
}
