using TaleWorlds.Engine;
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

        private Vec3 CalculateCastingPosition(Formation targetFormation)
        {
            var formationDirection = targetFormation.QuerySystem.EstimatedDirection;
            var medianAgent = targetFormation.GetMedianAgent(true, false, targetFormation.GetAveragePositionOfUnits(true, false));

            var flankDistance = targetFormation.Width / 1.45f;
            var left = medianAgent.Position + formationDirection.LeftVec().ToVec3() * flankDistance;
            var right = medianAgent.Position + formationDirection.RightVec().ToVec3() * flankDistance;

            return Agent.Position.Distance(left) < Agent.Position.Distance(right) ? left : right;
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