using System;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.Decision
{
    public static class CommonDecisionParameterFunctions
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
                TOWCommon.Say("Overall Power ratio: " + agent.Team.QuerySystem.OverallPowerRatio);
                TOWCommon.Say("Power ratio with casualties: " + agent.Team.QuerySystem.PowerRatioIncludingCasualties);
                return agent.Team.QuerySystem.OverallPowerRatio;
            };
        }

        public static Func<Target, float> LocalBalanceOfPower(Agent agent)
        {
            return target =>
            {
                TOWCommon.Say("Local Power Ratio: " + agent.Formation.QuerySystem.LocalPowerRatio);
                return agent.Formation.QuerySystem.LocalPowerRatio;
            };
        }
    }
}