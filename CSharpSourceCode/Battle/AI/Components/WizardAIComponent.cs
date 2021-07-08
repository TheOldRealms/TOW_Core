using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Components
{
    public class WizardAIComponent : HumanAIComponent
    {
        public Mat3 SpellTargetRotation = Mat3.Identity;

        public WizardAIComponent(Agent agent) : base(agent)
        {
            Agent.SelectAbility(0);
        }

        public override void OnTickAsAI(float dt)
        {
            UpdateBehavior();
            CastSpell();

            base.OnTickAsAI(dt);
        }

        private void UpdateBehavior()
        {
            SetBehaviorParams(AISimpleBehaviorKind.Melee, 0, 5, 0, 20, 0);
            SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
            SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 5f, 2.5f, 3f, 10f, 0.0f);

            var moveOrder = Agent?.Formation?.GetReadonlyMovementOrderReference();
            var currentOrderType = moveOrder?.OrderType;

            if (currentOrderType == null || currentOrderType == OrderType.None) return;

            if (currentOrderType == OrderType.Charge || currentOrderType == OrderType.ChargeWithTarget)
            {
                SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 10f, 10f, 10f, 20f, 10f);
            }
        }

        private void CastSpell()
        {
            var targetFormation = Agent?.Formation?.QuerySystem?.ClosestEnemyFormation?.Formation;

            if (targetFormation != null)
            {
                var medianAgent = targetFormation.GetMedianAgent(
                    true,
                    false,
                    targetFormation.GetAveragePositionOfUnits(true, false)
                );

                if (medianAgent != null && medianAgent.Position.Distance(Agent.Position) < 60)
                {
                    var targetPosition = medianAgent.Position;
                    targetPosition.z += -2;

                    CalculateSpellRotation(targetPosition);
                    Agent.CastCurrentAbility();
                }
            }
        }

        private void CalculateSpellRotation(Vec3 targetPosition)
        {
            SpellTargetRotation = Mat3.CreateMat3WithForward(targetPosition - Agent.Position);
        }
    }
}