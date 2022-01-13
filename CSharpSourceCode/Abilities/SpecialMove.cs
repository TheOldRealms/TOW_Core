using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities.Scripts;

namespace TOW_Core.Abilities
{
    public class SpecialMove : Ability
    {
        public SpecialMove(AbilityTemplate template) : base(template)
        {
        }

        public bool IsUsing
        {
            get
            {
                return (ShadowStepScript)AbilityScript != null && !((ShadowStepScript)AbilityScript).IsFadinOut;
            }
        }
    }
}
