using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Decision
{
    public static class CommonAIDecisionFunctions
    {
        public static Func<Target, float> FormationUnderFire()
        {
            return target => { return target.Formation.QuerySystem.UnderRangedAttackRatio; };
        }

        public static Func<Target, float> FormationCasualties()
        {
            return target => target.Formation.QuerySystem.CasualtyRatio;
        }

        public static Func<Target, float> FormationDistanceToHostiles()
        {
            return target =>
            {
                var querySystemClosestEnemyFormation = target.Formation.QuerySystem.ClosestEnemyFormation;
                if (querySystemClosestEnemyFormation == null)
                {
                    return float.MaxValue;
                }

                return target.Position.AsVec2.Distance(querySystemClosestEnemyFormation.AveragePosition);
            };
        }

        public static Func<Target, float> DistanceToTarget(Func<Vec3> provider)
        {
            return target => provider.Invoke().Distance(target.Position);
        }

        public static Func<Target, float> FormationPower()
        {
            return target => target.Formation.QuerySystem.FormationPower;
        }

        public static Func<Target, float> RangedUnitRatio()
        {
            return target => target.Formation.QuerySystem.RangedUnitRatio;
        }

        public static Func<Target, float> InfantryUnitRatio()
        {
            return target => target.Formation.QuerySystem.InfantryUnitRatio;
        }

        public static Func<Target, float> CavalryUnitRatio()
        {
            return target => target.Formation.QuerySystem.CavalryUnitRatio;
        }

        public static Func<Target, float> Dispersedness()
        {
            return target => target.Formation.UnitSpacing;
        }

        public static Func<Target, float> TargetSpeed()
        {
            return target => target.Formation.QuerySystem.CurrentVelocity.Length;
        }

        public static Func<Target, float> BalanceOfPower(Agent agent)
        {
            return target => agent.Team.QuerySystem.TeamPower / (CalculateEnemyTotalPower(agent.Team) + agent.Team.QuerySystem.TeamPower);
        }

        public static Func<Target, float> LocalBalanceOfPower(Agent agent)
        {
            return target => Math.Max(1, agent.Formation.QuerySystem.LocalPowerRatio);
        }

        public static float CalculateEnemyTotalPower(Team chosenTeam)
        {
            float power = 0;
            foreach (var team in chosenTeam.QuerySystem.EnemyTeams)
            {
                power += team.TeamPower;
            }

            return power;
        }
    }

    public static class CommonAIStateFunctions
    {
        public static bool CanAgentMoveFreely(Agent agent)
        {
            var movementOrder = agent?.Formation?.GetReadonlyMovementOrderReference();
            //TODO: Skirmish detection is incomplete. Multiple Formation skirmish behaviors exist.
            return movementOrder.HasValue && (movementOrder.Value.OrderType == OrderType.Charge || movementOrder.Value.OrderType == OrderType.ChargeWithTarget || agent?.Formation?.AI?.ActiveBehavior is BehaviorSkirmish);
        }
    }
}