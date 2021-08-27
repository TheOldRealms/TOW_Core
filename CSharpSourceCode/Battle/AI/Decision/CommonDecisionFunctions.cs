using System;
using System.Linq;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Decision
{
    public static class CommonDecisionFunctions
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

                return target.Formation.CurrentPosition.Distance(querySystemClosestEnemyFormation.AveragePosition);
            };
        }

        public static Func<Target, float> DistanceToTarget(Agent agent)
        {
            return target => target.Agent != null
                ? agent.Position.Distance(target.Agent.Position)
                : agent.Position.AsVec2.Distance(target.Formation.CurrentPosition);
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
            return target =>
            {
                var calculateEnemyTotalPower = agent.Team.QuerySystem.TeamPower / (CalculateEnemyTotalPower(agent.Team)+agent.Team.QuerySystem.TeamPower);
                TOWCommon.Say("Power ratio: " + calculateEnemyTotalPower);
                return calculateEnemyTotalPower;
            };
        }

        public static Func<Target, float> LocalBalanceOfPower(Agent agent)
        {
            return target =>
            {
                TOWCommon.Say("Local Power Ratio: " + agent.Formation.QuerySystem.LocalPowerRatio);
                return Math.Max(1, agent.Formation.QuerySystem.LocalPowerRatio);
            };
        }

        public static float CalculateEnemyTotalPower(Team chosenTeam)
        {
            var enemyPower = chosenTeam.QuerySystem.EnemyTeams
                .Select(team => team.TeamPower)
                .Aggregate((a, x) => a + x);
            return enemyPower;
        }
    }
}