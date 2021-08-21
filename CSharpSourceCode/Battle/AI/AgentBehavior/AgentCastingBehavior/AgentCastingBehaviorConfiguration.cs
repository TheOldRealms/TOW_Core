using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
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
                    new Axis(0, 1, x => x, (agent, target) => 0.4f)
                };

                if (template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE)
                {
                    axes.Add(new Axis(0, 100, x => 1 - x, DistanceToTarget()));
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
                    new Axis(0, 100, x => 1 - x, DistanceToTarget()),
                    new Axis(0, 300, x => x * 2, FormationPower()),
                    new Axis(0.3f, 1, x => x, RangedUnitRatio()),
                };
            };
        }

        public static Func<AbilityTemplate, List<Axis>> CreateDirectionalMovingAoEAxis()
        {
            return template =>
            {
                return new List<Axis>
                {
                    new Axis(0, 80, ScoringFunctions.Logistic(0.5f, 4, x => 1 - x), DistanceToTarget()),
                    new Axis(0, 300, x => x, FormationPower()),
                    new Axis(0, 1, x => 1 - x, Dispersedness()),
                    new Axis(0, 1, x => 1 - x, CavalryUnitRatio()),
                };
            };
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

        private static Func<Agent, Target, float> FreeLineOfSight()
        {
            return (agent, target) => agent.Position.AsVec2.Distance(target.Formation.CurrentPosition);
        }
    }
}