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

        public MissileCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
        }

        protected override void CastSpellAtTargetPosition(Vec3 target)
        {
            var targetFormation = CurrentTarget.Formation;
            var medianAgent = targetFormation?.GetMedianAgent(true, false, targetFormation.GetAveragePositionOfUnits(true, false));

            var adjustedPosition = target;
            adjustedPosition += ComputeSpellAngleVelocityCorrection(medianAgent.Position, medianAgent.Velocity);
            adjustedPosition.z += -2f;

            if (targetFormation?.CountOfUnits > 10)
            {
                var direction = targetFormation.QuerySystem.EstimatedDirection;
                var rightVec = direction.RightVec();
                adjustedPosition += direction.ToVec3() * (float) (_random.NextDouble() * targetFormation.Depth - targetFormation.Depth / 2);
                adjustedPosition += rightVec.ToVec3() * (float) (_random.NextDouble() * targetFormation.Width - 2 - (targetFormation.Width - 1) / 2);
            }

            base.CastSpellAtTargetPosition(adjustedPosition);
        }


        protected override bool HaveLineOfSightToTarget(Target target)
        {
            Agent targetAgent = CurrentTarget.Formation?.GetMedianAgent(true, false, CurrentTarget.Formation.GetAveragePositionOfUnits(true, false));
            Agent collidedAgent = Mission.Current.RayCastForClosestAgent(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float _, Agent.Index, 0.4f);
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float distance, out GameEntity _, 0.4f);

            return Agent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) > 1 && (distance is Single.NaN || distance > 1) &&
                   (collidedAgent == null || collidedAgent == targetAgent || collidedAgent.IsEnemyOf(Agent) || collidedAgent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) < 4) &&
                   (float.IsNaN(distance) || Math.Abs(distance - targetAgent.Position.Distance(Agent.Position)) < 0.3);
        }
    }
}