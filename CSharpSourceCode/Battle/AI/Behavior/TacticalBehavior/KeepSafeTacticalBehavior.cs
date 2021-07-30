using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Behavior.TacticalBehavior
{
    public class KeepSafeTacticalBehavior : AgentCombatBehavior
    {
        public KeepSafeTacticalBehavior(Agent agent, HumanAIComponent aiComponent) : base(agent, aiComponent)
        {
        }

        public override void ApplyBehavior()
        {
            AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.Melee, 0f, 3f, 0f, 20f, 0f);
            AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
            AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.RangedHorseback, 5f, 2.5f, 3f, 10f, 0.0f);

            var currentOrderType = GetMovementOrderType();
            TOWCommon.Say(currentOrderType.ToString());
            if (currentOrderType != null && (currentOrderType == OrderType.Charge || currentOrderType == OrderType.ChargeWithTarget))
            {
                if (Agent.HasMount)
                {
                    AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.Melee, 4f, 3f, 1f, 20f, 1f);
                }

                AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.GoToPos, 3f, 8f, 5f, 20f, 6f);
                if (ShouldAgentSkirmish())
                {
                    AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.RangedHorseback, 5f, 7f, 3f, 20f, 5.5f);
                }
                else
                {
                    AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.RangedHorseback, 0.0f, 15f, 0.0f, 30f, 0.0f);
                }
            }
        }


        protected bool ShouldAgentSkirmish()
        {
            var querySystem = Agent?.Formation?.QuerySystem;
            var allyPower = querySystem?.LocalAllyPower;
            return allyPower < 20 || allyPower < querySystem?.LocalEnemyPower / 2;
        }
    }
}