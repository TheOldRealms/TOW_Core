﻿using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior
{
    public class DirectionalAoETacticalBehavior : AbstractAgentTacticalBehavior
    {
        public Vec3 CastingPosition;
        public AbstractAgentCastingBehavior CastingBehavior { get; set; }


        public DirectionalAoETacticalBehavior(Agent agent, HumanAIComponent aiComponent, AbstractAgentCastingBehavior castingBehavior) : base(agent, aiComponent)
        {
            CastingBehavior = castingBehavior;
        }

        private void CalculateCastingTarget(Target target)
        {
            CastingPosition = target.Formation != null ? CalculateCastingPosition(target.Formation) : Agent.Position;
            var worldPosition = new WorldPosition(Mission.Current.Scene, CastingPosition);
            Agent.SetScriptedPosition(ref worldPosition, false);
        }

        private static Vec3 CalculateCastingPosition(Formation targetFormation)
        {
            var targetFormationDirection = targetFormation.QuerySystem.EstimatedDirection.LeftVec();
            targetFormationDirection *= targetFormation.Width / 1.45f;
            targetFormationDirection = targetFormation.CurrentPosition + targetFormationDirection;
            var castingPosition = targetFormationDirection.ToVec3(targetFormation.QuerySystem.MedianPosition.GetGroundZ());
            return castingPosition;
        }

        public override void ApplyBehaviorParams()
        {
        }

        public override void Tick()
        {
            CalculateCastingTarget(CastingBehavior.CurrentTarget);
        }

        public override void Terminate()
        {
            Agent.DisableScriptedMovement();
        }
    }
}