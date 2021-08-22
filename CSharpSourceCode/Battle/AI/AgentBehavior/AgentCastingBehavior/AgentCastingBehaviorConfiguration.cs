using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public static class AgentCastingBehaviorMapping
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>> BehaviorByType =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>>
            {
                {AbilityEffectType.MovingProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DynamicProjectile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},

                {AbilityEffectType.CenteredStaticAOE, (agent, abilityTemplate, abilityIndex) => new CenteredStaticAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},

                {AbilityEffectType.TargetedStaticAOE, (agent, abilityTemplate, abilityIndex) => new TargetedStaticAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},

                {AbilityEffectType.RandomMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.DirectionalMovingAOE, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
            };


        public static readonly Dictionary<Type, Func<AbilityTemplate, List<Axis>>> UtilityByType =
            new Dictionary<Type, Func<AbilityTemplate, List<Axis>>>
            {
                {typeof(PreserveWindsAgentCastingBehavior), CreatePreserveWindsAxis()},

                {typeof(CenteredStaticAoEAgentCastingBehavior), CreateStaticAoEAxis()},
                {typeof(TargetedStaticAoEAgentCastingBehavior), CreateStaticAoEAxis()},

                {typeof(DirectionalMovingAoEAgentCastingBehavior), CreateDirectionalMovingAoEAxis()},
                {typeof(MovingProjectileAgentCastingBehavior), CreateMovingProjectileAxis()},
            };

        private static Func<AbilityTemplate, List<Axis>> CreateStaticAoEAxis()
        {
            return template =>
            {
                var axes = new List<Axis>
                {
                    new Axis(0, 1, x => x, (agent, target) => 0.45f)
                };

                if (template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE)
                {
                    axes.Add(new Axis(0, 100, x => 1 - x, DistanceToTarget()));
                    axes.Add(new Axis(0, 3, x => 1 - x, TargetSpeed()));
                    axes.Add(new Axis(0, 1f, x => x*2+0.01f, FormationUnderFire()));
                    axes.Add(new Axis(0, 1f, x => x*3+0.01f, FormationCasualties()));
                }

                return axes;
            };
        }

        private static Func<AbilityTemplate, List<Axis>> CreatePreserveWindsAxis()
        {
            return template =>
            {
                return new List<Axis>
                {
                    new Axis(0, 1, x => x, (agent, target) => 0.4f)
                };
            };
        }

        public static Func<AbilityTemplate, List<Axis>> CreateMovingProjectileAxis()
        {
            return template =>
            {
                return new List<Axis>
                {
                    new Axis(0, 120, x => 1 - x, DistanceToTarget()),
                    new Axis(0, 80, x => x, FormationPower()),
                    new Axis(0.0f, 1, x => x + 0.2f, RangedUnitRatio()),
                    new Axis(0.0f, 1, x => x + 0.1f, InfatryUnitRatio()),
                };
            };
        }

        public static Func<AbilityTemplate, List<Axis>> CreateDirectionalMovingAoEAxis()
        {
            return template =>
            {
                return new List<Axis>
                {
                    new Axis(0, 50, x => ScoringFunctions.Logistic(0.4f, 1, 20).Invoke(1 - x), DistanceToTarget()),
                    new Axis(0, 15, x => 1 - x, DistanceToHostiles()),
                    new Axis(0, 300, x => x, FormationPower()),
                    new Axis(1, 4, x => 1 - x, Dispersedness()),
                    new Axis(0, 1, x => 1 - x * 2, CavalryUnitRatio()),
                };
            };
        }

        private static Func<Agent, Target, float> FormationUnderFire()
        {
            return (agent, target) =>
            {
                var querySystemUnderRangedAttackRatio = target.Formation.QuerySystem.UnderRangedAttackRatio;
                TOWCommon.Say(querySystemUnderRangedAttackRatio.ToString());
                return querySystemUnderRangedAttackRatio;
            };
        }

        private static Func<Agent, Target, float> FormationCasualties()
        {
            return (agent, target) => target.Formation.QuerySystem.CasualtyRatio;
        }

        private static Func<Agent, Target, float> DistanceToHostiles()
        {
            return (agent, target) =>
                target.Agent == null
                    ? target.Formation.CurrentPosition.Distance(target.Formation.QuerySystem.ClosestEnemyFormation.AveragePosition)
                    : agent.Position.AsVec2.Distance(target.Agent.Formation.QuerySystem.ClosestEnemyFormation.AveragePosition);
        }

        private static Func<Agent, Target, float> DistanceToTarget()
        {
            return (agent, target) => target.Agent == null ? agent.Position.AsVec2.Distance(target.Formation.CurrentPosition) : agent.Position.Distance(target.Agent.Position);
        }

        private static Func<Agent, Target, float> FormationPower()
        {
            return (agent, target) => target.Formation.QuerySystem.FormationPower;
        }

        private static Func<Agent, Target, float> RangedUnitRatio()
        {
            return (agent, target) => target.Formation.QuerySystem.RangedUnitRatio;
        }
        
        private static Func<Agent, Target, float> InfatryUnitRatio()
        {
            return (agent, target) => target.Formation.QuerySystem.InfantryUnitRatio;
        }

        private static Func<Agent, Target, float> CavalryUnitRatio()
        {
            return (agent, target) => target.Formation.QuerySystem.CavalryUnitRatio;
        }

        private static Func<Agent, Target, float> Dispersedness()
        {
            return (agent, target) => target.Formation.QuerySystem.FormationDispersedness;
        }

        private static Func<Agent, Target, float> TargetSpeed()
        {
            return (agent, target) => target.Formation.QuerySystem.MovementSpeed;
        }

        private static Func<Agent, Target, float> FreeLineOfSight()
        {
            return (agent, target) => agent.Position.AsVec2.Distance(target.Formation.CurrentPosition);
        }
    }
}