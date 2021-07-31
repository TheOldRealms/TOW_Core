using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public abstract class AgentCastingBehavior
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AgentCastingBehavior>> CastingBehaviorsByAbilityEffect =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AgentCastingBehavior>>
            {
                {AbilityEffectType.MovingProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.CenteredStaticAOE, (agent, abilityTemplate, abilityIndex) => new CenteredStaticAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DirectionalMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoECastingBehavior(agent, abilityIndex, abilityTemplate)},
            };


        protected readonly Agent Agent;
        public readonly int AbilityIndex;
        public Formation TargetFormation;

        protected AgentCastingBehavior(Agent agent, AbilityTemplate abilityTemplate, int abilityIndex)
        {
            Agent = agent;
            AbilityIndex = abilityIndex;
        }

        public void Execute()
        {
            if (Agent.GetCurrentAbility().IsOnCooldown()) return;

            if (TargetFormation == null) return;

            var medianAgent = TargetFormation.GetMedianAgent(true, false, TargetFormation.GetAveragePositionOfUnits(true, false));
            var requiredDistance = Agent.GetComponent<AbilityComponent>().CurrentAbility.Template.Name == "Fireball" ? 80 : 27;

            if (medianAgent != null && medianAgent.Position.Distance(Agent.Position) < requiredDistance)
            {
                if (HaveLineOfSightToAgent(medianAgent))
                {
                    CastSpellAtAgent(medianAgent);
                }
            }
        }

        private void CastSpellAtAgent(Agent targetAgent)
        {
            var targetPosition = targetAgent == Agent.Main ? targetAgent.Position : targetAgent.GetChestGlobalPosition();

            var velocity = targetAgent.Velocity;
            if (Agent.GetCurrentAbility().Template.Name == "Fireball")
            {
                velocity = ComputeCorrectedVelocityBySpellSpeed(targetAgent, 35);
            }

            targetPosition += velocity;
            targetPosition.z += -2f;

            CalculateSpellRotation(targetPosition);
            Agent.CastCurrentAbility();
        }

        private Vec3 ComputeCorrectedVelocityBySpellSpeed(Agent targetAgent, float spellSpeed)
        {
            var time = targetAgent.Position.Distance(Agent.Position) / spellSpeed;
            return targetAgent.Velocity * time;
        }

        private bool HaveLineOfSightToAgent(Agent targetAgent)
        {
            Agent collidedAgent = Mission.Current.RayCastForClosestAgent(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float _, Agent.Index, 0.4f);
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float distance, out GameEntity _, 0.4f);
            
            return Agent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) > 1 && (distance is Single.NaN || distance > 1) &&
                   (collidedAgent == null || collidedAgent == targetAgent || collidedAgent.IsEnemyOf(Agent) || collidedAgent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) < 4) &&
                   (float.IsNaN(distance) || Math.Abs(distance - targetAgent.Position.Distance(Agent.Position)) < 0.3);
        }


        protected Mat3 CalculateSpellRotation(Vec3 targetPosition)
        {
            return Mat3.CreateMat3WithForward(targetPosition - Agent.Position);
        }
    }
}