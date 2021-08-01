using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Battle.AI.Decision.ScoringFunction;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public class DirectionalMovingAoECastingBehavior : AgentCastingBehavior
    {
        private List<Axis> axes;

        public DirectionalMovingAoECastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Positional = true;
            axes = new List<Axis>();
            axes.Add(new Axis());
        }

        public override void Execute()
        {
            var castingPosition = CalculateCastingPosition(TargetFormation);
            var worldPosition = new WorldPosition(Mission.Current.Scene, castingPosition);
            Agent.SetScriptedPosition(ref worldPosition, false);

            if (Agent.Position.AsVec2.Distance(castingPosition.AsVec2) > 4) return;

            base.Execute();
        }

        public override void Terminate()
        {
            Agent.DisableScriptedMovement();
        }

        protected override bool HaveLineOfSightToAgent(Agent targetAgent)
        {
            return true;
        }

        private static Vec3 CalculateCastingPosition(Formation targetFormation)
        {
            var targetFormationDirection = new Vec2(targetFormation.Direction.x, targetFormation.Direction.y);
            targetFormationDirection.RotateCCW(1.63f);
            targetFormationDirection = targetFormationDirection * (targetFormation.Width / 1.45f);
            targetFormationDirection = targetFormation.CurrentPosition + targetFormationDirection;

            var castingPosition = targetFormationDirection.ToVec3(targetFormation.QuerySystem.MedianPosition.GetGroundZ());
            return castingPosition;
        }

        protected override float UtilityFunction()
        {
            return ScoringAxis.CalculateGeometricMean(axes);
        }
    }
}