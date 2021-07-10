using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
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
        private Formation _targetFormation;

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
            ChooseTargetFormation();

            if (_targetFormation != null)
            {
                var medianAgent = _targetFormation.GetMedianAgent(
                    true,
                    false,
                    _targetFormation.GetAveragePositionOfUnits(true, false)
                );


                var requiredDistance = Agent.GetComponent<AbilityComponent>().CurrentAbility is FireBallAbility ? 80 : 27;

                if (medianAgent != null && medianAgent.Position.Distance(Agent.Position) < requiredDistance)
                {
                    var targetPosition = medianAgent == Agent.Main ? medianAgent.Position : medianAgent.GetChestGlobalPosition();
                    targetPosition.z += -2;

                    Agent collidedAgent = Mission.Current.RayCastForClosestAgent(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), medianAgent.GetChestGlobalPosition(), out float _, Agent.Index, 0.4f);
                    Mission.Current.Scene.RayCastForClosestEntityOrTerrain(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), medianAgent.GetChestGlobalPosition(), out float distance, out GameEntity _, 0.4f);
                
                    if ((collidedAgent == null || collidedAgent == medianAgent || collidedAgent.IsEnemyOf(Agent) || collidedAgent.GetChestGlobalPosition().Distance(medianAgent.GetChestGlobalPosition()) < 3) &&
                        (float.IsNaN(distance) || Math.Abs(distance - targetPosition.Distance(Agent.Position)) < 0.3))
                    {
                        CalculateSpellRotation(targetPosition);
                        Agent.CastCurrentAbility();
                    }
                }
            }
        }

        private void ChooseTargetFormation()
        {
            var formation = Agent?.Formation?.QuerySystem?.ClosestEnemyFormation?.Formation;
            if (formation != null && (_targetFormation == null || !formation.HasPlayer || formation.Distance < _targetFormation.Distance && formation.Distance < 15 || _targetFormation.GetFormationPower() < 15))
            {
                _targetFormation = formation;
            }
        }

        private void CalculateSpellRotation(Vec3 targetPosition)
        {
            SpellTargetRotation = Mat3.CreateMat3WithForward(targetPosition - Agent.Position);
        }
    }
}