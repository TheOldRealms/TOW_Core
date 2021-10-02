using TaleWorlds.MountAndBlade;

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
    }
}
