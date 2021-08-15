using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Battle.AI.Decision.ScoringFunction;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public abstract class AgentCastingBehavior : IUtilityObject
    {
        protected readonly Agent Agent;
        protected bool Positional = false;
        public int Range;
        public readonly AbilityTemplate AbilityTemplate;
        public readonly int AbilityIndex;
        public Formation TargetFormation;

        public float LatestScore;

        protected AgentCastingBehavior(Agent agent, AbilityTemplate abilityTemplate, int abilityIndex)
        {
            Agent = agent;
            AbilityIndex = abilityIndex;
            if (abilityTemplate != null)
            {
                Range = (int) (abilityTemplate.BaseMovementSpeed * abilityTemplate.Duration) - 1;
            }

            AbilityTemplate = abilityTemplate;
        }

        public virtual void Execute()
        {
            if (Agent.GetCurrentAbility().IsOnCooldown()) return;

            if (TargetFormation == null) return;

            var medianAgent = TargetFormation.GetMedianAgent(true, false, TargetFormation.GetAveragePositionOfUnits(true, false));

            if (medianAgent != null && medianAgent.Position.Distance(Agent.Position) < Range)
            {
                if (HaveLineOfSightToAgent(medianAgent))
                {
                    Agent.SelectAbility(AbilityIndex);
                    CastSpellAtAgent(medianAgent);
                }
            }
        }

        protected void CastSpellAtAgent(Agent targetAgent)
        {
            var targetPosition = targetAgent == Agent.Main ? targetAgent.Position : targetAgent.GetChestGlobalPosition();

            var velocity = targetAgent.Velocity;
            if (Agent.GetCurrentAbility().GetAbilityEffectType() == AbilityEffectType.MovingProjectile)
            {
                velocity = ComputeCorrectedVelocityBySpellSpeed(targetAgent);
            }

            targetPosition += velocity;
            targetPosition.z += -2f;

            var wizardAIComponent = Agent.GetComponent<WizardAIComponent>();
            wizardAIComponent.SpellTargetRotation = CalculateSpellRotation(targetPosition);
            Agent.CastCurrentAbility();
        }

        protected virtual bool HaveLineOfSightToAgent(Agent targetAgent)
        {
            Agent collidedAgent = Mission.Current.RayCastForClosestAgent(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float _, Agent.Index, 0.4f);
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float distance, out GameEntity _, 0.4f);

            return Agent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) > 1 && (distance is Single.NaN || distance > 1) &&
                   (collidedAgent == null || collidedAgent == targetAgent || collidedAgent.IsEnemyOf(Agent) || collidedAgent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) < 4) &&
                   (float.IsNaN(distance) || Math.Abs(distance - targetAgent.Position.Distance(Agent.Position)) < 0.3);
        }

        private Vec3 ComputeCorrectedVelocityBySpellSpeed(Agent targetAgent)
        {
            var time = targetAgent.Position.Distance(Agent.Position) / AbilityTemplate.BaseMovementSpeed;
            return targetAgent.Velocity * time;
        }


        protected Mat3 CalculateSpellRotation(Vec3 targetPosition)
        {
            return Mat3.CreateMat3WithForward(targetPosition - Agent.Position);
        }

        public virtual void Terminate()
        {
        }

        public float GetLatestScore()
        {
            return LatestScore;
        }

        public float CalculateUtility()
        {
            LatestScore = UtilityFunction();
            return LatestScore;
        }

        protected abstract float UtilityFunction();

        private static Formation ChooseTargetFormation(Agent agent, Formation targetFormation)
        {
            var formation = agent?.Formation?.QuerySystem?.ClosestEnemyFormation?.Formation;
            if (!(formation != null && (targetFormation == null || !formation.HasPlayer || formation.Distance < targetFormation.Distance && formation.Distance < 15 || targetFormation.GetFormationPower() < 15)))
            {
                formation = targetFormation;
            }

            return formation;
        }
    }
}