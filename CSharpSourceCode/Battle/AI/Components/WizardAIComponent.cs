using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Components
{
    public class WizardAIComponent : HumanAIComponent
    {
        public Mat3 SpellTargetRotation = Mat3.Identity;

        public WizardAIComponent(Agent agent) : base(agent)
        {
        }

        public override void OnTickAsAI(float dt)
        {
            UpdateBehavior();
            Agent.SelectAbility(0);
            CastSpell();
            Agent.SelectAbility(1);
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
                SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 3f, 8f, 5f, 20f, 6f);

                var querySystem = Agent?.Formation?.QuerySystem;
                var allyPower = querySystem?.LocalAllyPower;
                if (allyPower < 20 || allyPower < querySystem?.LocalEnemyPower / 2)
                {
                    SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 5f, 7f, 3f, 20f, 5.5f);
                }
                else
                {
                    SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 0.0f, 15f, 0.0f, 30f, 0.0f);
                }
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

                var requiredDistance = Agent.GetComponent<AbilityComponent>().CurrentAbility is FireBallAbility ? 60 : 27;

                if (medianAgent != null && medianAgent.Position.Distance(Agent.Position) < requiredDistance)
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