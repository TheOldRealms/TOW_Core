using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

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

            if (targetFormation.CountOfUnits > 10)
            {
                var adjustedPosition = medianAgent.Position;
                
                var direction = targetFormation.QuerySystem.EstimatedDirection;
                var rightVec = direction.RightVec();
                
                adjustedPosition += direction.ToVec3() * (float) (_random.NextDouble() * targetFormation.Depth - targetFormation.Depth / 2);
                adjustedPosition += rightVec.ToVec3() * (float) (_random.NextDouble() * targetFormation.Width - 2 - (targetFormation.Width - 1) / 2);
                
                medianAgent = targetFormation.GetMedianAgent(true, false, adjustedPosition.AsVec2);
                target.Agent = medianAgent;
                
                adjustedPosition = medianAgent.Position;
                adjustedPosition += ComputeSpellAngleVelocityCorrection(medianAgent.Position, medianAgent.Velocity);
                target.SelectedWorldPosition = adjustedPosition;
            }

            target.Agent = medianAgent;
            return target;
        }

        protected override bool HaveLineOfSightToTarget(Target target)
        {
            var targetPoint = target.GetPosition();
            targetPoint.z += 0.75f;
            Agent collidedAgent = Mission.Current.RayCastForClosestAgent(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetPoint, out float _, Agent.Index, 0.4f);
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetPoint, out float distance, out GameEntity _, 0.4f);

            return Agent.GetChestGlobalPosition().Distance(targetPoint) > 1 && (distance is Single.NaN || distance > 1) &&
                   (collidedAgent == null || collidedAgent.IsEnemyOf(Agent) || collidedAgent.GetChestGlobalPosition().Distance(targetPoint) < 4) &&
                   (float.IsNaN(distance) || Math.Abs(distance - targetPoint.Distance(Agent.Position)) < 0.3);
        }
    }
}