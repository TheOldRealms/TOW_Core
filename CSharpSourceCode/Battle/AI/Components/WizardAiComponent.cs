using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Components
{
    public class WizardAiComponent : HumanAIComponent
    {
        public WizardAiComponent(Agent agent) : base(agent)
        {
            SetBehaviorParams(AISimpleBehaviorKind.Melee, 0, 5, 0, 20, 0);
            SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
        }

        public override void OnTickAsAI(float dt)
        {
            base.OnTickAsAI(dt);
            SetBehaviorParams(AISimpleBehaviorKind.Melee, 0, 5, 0, 20, 0);
            SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
        }
    }
}