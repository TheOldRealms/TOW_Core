using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior
{
    public class KeepSafeAgentTacticalBehavior : AbstractAgentTacticalBehavior
    {
        public KeepSafeAgentTacticalBehavior(Agent agent, HumanAIComponent aiComponent) : base(agent, aiComponent)
        {
        }

        public override void Execute()
        {
            ApplyBehaviorParams();
        }

        public override void Terminate()
        {
        }

        public override void ApplyBehaviorParams()
        {
            AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.Melee, 4f, 3f, 1f, 20f, 1f);
            AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
            AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.RangedHorseback, 0f, 2.5f, 0f, 10f, 0.0f);

            var currentOrderType = GetMovementOrderType();

            if (currentOrderType != null && (currentOrderType == OrderType.Charge || currentOrderType == OrderType.ChargeWithTarget))
            {
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