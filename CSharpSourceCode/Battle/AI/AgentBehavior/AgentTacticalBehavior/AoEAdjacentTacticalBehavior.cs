using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior
{
    public class AdjacentAoETacticalBehavior : AbstractAgentTacticalBehavior
    {
        public Vec3 CastingPosition;
        public AbstractAgentCastingBehavior CastingBehavior { get; set; }


        public AdjacentAoETacticalBehavior(Agent agent, HumanAIComponent aiComponent, AbstractAgentCastingBehavior castingBehavior) : base(agent, aiComponent)
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
            var medianPositionPosition = targetFormation.QuerySystem.MedianPosition;
            return medianPositionPosition.GetGroundVec3() + (targetFormation.Direction * targetFormation.GetMovementSpeedOfUnits()).ToVec3(medianPositionPosition.GetGroundZ());
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