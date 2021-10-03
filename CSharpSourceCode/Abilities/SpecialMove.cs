using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities.Scripts;

namespace TOW_Core.Abilities
{
    public class SpecialMove : Ability
    {
        public SpecialMove(AbilityTemplate template) : base(template)
        {
        }

        public override void TryCast(Agent casterAgent)
        {
            base.DoCast(casterAgent);
        }

        public void Stop()
        {
            ((SpecialMoveScript)AbilityScript).Stop();
        }


        public bool IsUsing
        {
            get
            {
                return AbilityScript != null && !((SpecialMoveScript)AbilityScript).IsFadinOut;
            }
        }
    }
}
