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
    public class DirectionalMovingAoEAgentCastingBehavior : AbstractAgentCastingBehavior
    {
        public DirectionalMovingAoEAgentCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.35f;
        }

        public override void Execute()
        {
            var castingPosition = CurrentTarget.Formation != null ? CalculateCastingPosition(CurrentTarget.Formation) : Agent.Position;
            var worldPosition = new WorldPosition(Mission.Current.Scene, castingPosition);
            Agent.SetScriptedPosition(ref worldPosition, false);

            if (Agent.Position.AsVec2.Distance(castingPosition.AsVec2) > 6) return;

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


        public override bool IsPositional()
        {
            return true;
        }
    }
}