using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Components
{
    public class WizardAiComponent : HumanAIComponent
    {
        public WizardAiComponent(Agent agent) : base(agent)
        {
            SetBehaviorParams(AISimpleBehaviorKind.Melee, 0, 5, 0, 20, 0);
            SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
            SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 2f, 15f, 6.5f, 30f, 5.5f);
        }

        public override void OnTickAsAI(float dt)
        {
            SetBehaviorParams(AISimpleBehaviorKind.Melee, 0, 5, 0, 20, 0);
            SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
            SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 6.5f, 7, 4.9f, 10f, 0.0f);
            SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 0f, 15f, 0f, 40f, 6f);

            var currentOrderType = Agent?.Formation?.GetReadonlyMovementOrderReference().OrderType;

            if (currentOrderType == null || currentOrderType == OrderType.None) return;

            TOWCommon.Say(currentOrderType.Description());

            if (currentOrderType == OrderType.Charge || currentOrderType == OrderType.ChargeWithTarget)
            {
                SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 0f, 15f, 0f, 40f, 6f);
                SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 2f, 15f, 6.5f, 25f, 5.5f);
            }

            base.OnTickAsAI(dt);
        }
    }
}