using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class MissileCastingBehavior : AbstractAgentCastingBehavior
    {
        private Random _random = new Random();

        public MissileCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template,
            abilityIndex)
        {
            Hysteresis = 0.1f;
        }

        protected override Target UpdateTarget(Target target)
        {
            var targetFormation = CurrentTarget.Formation;
            var medianAgent = targetFormation?.GetMedianAgent(true, false, targetFormation.GetAveragePositionOfUnits(true, false));
            if (medianAgent == null) return target;

            var adjustedPosition = medianAgent.Position; //Intentional, want to bypass the stored WorldPosition.
            adjustedPosition += ComputeSpellAngleVelocityCorrection(medianAgent.Position, medianAgent.Velocity);

            if (targetFormation.CountOfUnits > 10)
            {
                var direction = targetFormation.QuerySystem.EstimatedDirection;
                var rightVec = direction.RightVec();
                adjustedPosition += direction.ToVec3() * (float) (_random.NextDouble() * targetFormation.Depth - targetFormation.Depth / 2);
                adjustedPosition += rightVec.ToVec3() * (float) (_random.NextDouble() * targetFormation.Width - 2 - (targetFormation.Width - 1) / 2);
            }

            adjustedPosition.z += Mission.Current.Scene.GetGroundHeightAtPosition(adjustedPosition);
            target.WorldPosition = adjustedPosition;

            return target;
        }

        protected override bool HaveLineOfSightToTarget(Target target)
        {
            var targetPoint = target.GetPosition();
            Agent targetAgent = CurrentTarget.Formation?.GetMedianAgent(true, false, targetPoint.AsVec2);
            Agent collidedAgent = Mission.Current.RayCastForClosestAgent(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetPoint, out float _, Agent.Index, 0.4f);
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetPoint, out float distance, out GameEntity _, 0.4f);

            return Agent.GetChestGlobalPosition().Distance(targetPoint) > 1 && (distance is Single.NaN || distance > 1) &&
                   (collidedAgent == null || collidedAgent == targetAgent || collidedAgent.IsEnemyOf(Agent) || collidedAgent.GetChestGlobalPosition().Distance(targetPoint) < 4) &&
                   (float.IsNaN(distance) || Math.Abs(distance - targetPoint.Distance(Agent.Position)) < 0.3);
        }
    }
}