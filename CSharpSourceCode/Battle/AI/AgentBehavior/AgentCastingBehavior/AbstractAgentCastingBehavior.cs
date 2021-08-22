using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
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
        private WizardAIComponent _component;
        protected readonly Agent Agent;
        protected float Hysteresis = 0.20f;
        protected int Range;
        protected readonly AbilityTemplate AbilityTemplate;
        protected readonly int AbilityIndex;
        protected List<Axis> AxisList;

        public Target CurrentTarget = new Target();
        public Dictionary<(IAgentBehavior, Target), float> LatestScores { get; private set; }

        public WizardAIComponent Component => _component ?? (_component = Agent.GetComponent<WizardAIComponent>());


        protected AbstractAgentCastingBehavior(Agent agent, AbilityTemplate abilityTemplate, int abilityIndex)
        {
            Agent = agent;
            AbilityIndex = abilityIndex;
            if (abilityTemplate != null)
            {
                Range = (int) (abilityTemplate.BaseMovementSpeed * abilityTemplate.Duration) - 1;
            }

            AbilityTemplate = abilityTemplate;
            AxisList = AgentCastingBehaviorMapping.UtilityByType[GetType()](abilityTemplate);
        }


        public abstract Boolean IsPositional();

        public virtual void Execute()
        {
            if (Agent.GetCurrentAbility().IsOnCooldown()) return;

            var medianAgent = CurrentTarget.Formation?.GetMedianAgent(true, false, CurrentTarget.Formation.GetAveragePositionOfUnits(true, false));

            if (medianAgent != null && (IsPositional() || medianAgent.Position.Distance(Agent.Position) < Range))
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

            FindTargets(Agent, AbilityTemplate.AbilityTargetType)
                .Select(target => (target, UtilityFunction(target)))
                .Do(pair => LatestScores.Add((this, pair.target), pair.Item2));

            return LatestScores;
        }

        protected virtual float UtilityFunction(Target target)
        {
            if (Agent.GetAbility(AbilityIndex).IsOnCooldown())
            {
                return 0.0f;
            }

            var hysteresis = Component.CurrentCastingBehavior == this && target.Formation == CurrentTarget.Formation ? Hysteresis : 0.0f;
            return hysteresis + AxisList.GeometricMean(Agent, target);
        }

        protected static List<Target> FindTargets(Agent agent, AbilityTargetType targetType)
        {
            switch (targetType)
            {
                case AbilityTargetType.Allies:
                    return agent.Team.QuerySystem.AllyTeams
                        .SelectMany(team => team.Team.Formations)
                        .Select(form => new Target {Formation = form, AbilityTargetType = targetType})
                        .ToList();
                case AbilityTargetType.Self:
                    return new List<Target>()
                    {
                        new Target {Agent = agent, AbilityTargetType = targetType}
                    };
                default:
                    return agent.Team.QuerySystem.EnemyTeams
                        .SelectMany(team => team.Team.Formations)
                        .Select(form => new Target {Formation = form, AbilityTargetType = targetType})
                        .ToList();
            }
        }
    }
}