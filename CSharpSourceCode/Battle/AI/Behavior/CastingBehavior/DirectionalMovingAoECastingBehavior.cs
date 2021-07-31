using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.Behavior.CastingBehavior
{
    public class DirectionalMovingAoECastingBehavior : AgentCastingBehavior
    {
        public DirectionalMovingAoECastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }

        private void CastSpellFromPosition()
        {
            var targetFormation = Agent?.Formation?.QuerySystem.ClosestSignificantlyLargeEnemyFormation?.Formation;

            var targetFormationDirection = new Vec2(targetFormation.Direction.x, targetFormation.Direction.y);
            targetFormationDirection.RotateCCW(1.57f);
            targetFormationDirection = targetFormationDirection * (targetFormation.Width / 1.45f);
            targetFormationDirection = targetFormation.CurrentPosition + targetFormationDirection;

            var castingPosition = targetFormationDirection.ToVec3(targetFormation.QuerySystem.MedianPosition.GetGroundZ());
            var worldPosition = new WorldPosition(Mission.Current.Scene, castingPosition);
            Agent.SetScriptedPosition(ref worldPosition, false);

            if (targetFormation != null)
            {
                var medianAgent = targetFormation.GetMedianAgent(
                    true,
                    true,
                    targetFormation.GetAveragePositionOfUnits(true, true)
                );

                //   var requiredDistance = Agent.GetComponent<AbilityComponent>().CurrentAbility is FireBallAbility ? 60 : 25;

                if (medianAgent != null && Agent.Position.AsVec2.Distance(castingPosition.AsVec2) < 3)
                {
                    var targetPosition = medianAgent.Position;
                    targetPosition.z += -2;

                    CalculateSpellRotation(targetPosition);
                    Agent.CastCurrentAbility();
                }
            }
        }
    }
}