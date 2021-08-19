using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public abstract class AbstractAgentCastingBehavior : IAgentBehavior
    {
        protected readonly Agent Agent;
        public int Range;
        public readonly AbilityTemplate AbilityTemplate;
        public readonly int AbilityIndex;
        public Target Target = new Target();
        public Dictionary<(IAgentBehavior, Target), float> LatestScores { get; private set; }

        protected AbstractAgentCastingBehavior(Agent agent, AbilityTemplate abilityTemplate, int abilityIndex)
        {
            Agent = agent;
            AbilityIndex = abilityIndex;
            if (abilityTemplate != null)
            {
                Range = (int) (abilityTemplate.BaseMovementSpeed * abilityTemplate.Duration) - 1;
            }

            AbilityTemplate = abilityTemplate;
        }

        public abstract Boolean IsPositional();
        protected abstract float UtilityFunction(Target target);

        public virtual void Execute()
        {
            if (Agent.GetCurrentAbility().IsOnCooldown()) return;

            var medianAgent = Target.Formation?.GetMedianAgent(true, false, Target.Formation.GetAveragePositionOfUnits(true, false));

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

        public abstract void Terminate();

        public Dictionary<(IAgentBehavior, Target), float> CalculateUtility()
        {
            LatestScores = new Dictionary<(IAgentBehavior, Target), float>();

            var target = new Target();
            target.Formation = ChooseTargetFormation(Agent, Target.Formation);

            LatestScores.Add((this, target), UtilityFunction(target)); //TODO: Should consider more targets
            return LatestScores;
        }

        protected static Formation ChooseTargetFormation(Agent agent, Formation currentTargetFormation)
        {
            var formation = agent?.Formation?.QuerySystem?.ClosestEnemyFormation?.Formation;
            if (!(formation != null && (currentTargetFormation == null || !formation.HasPlayer || formation.Distance < currentTargetFormation.Distance && formation.Distance < 15 || currentTargetFormation.GetFormationPower() < 15)))
            {
                formation = currentTargetFormation;
            }

            return formation;
        }
    }
}