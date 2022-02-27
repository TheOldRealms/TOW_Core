using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public static class AgentCastingBehaviorMapping
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>> BehaviorByType =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>>
            {
                {AbilityEffectType.Missile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.SeekerMissile, (agent, abilityTemplate, abilityIndex) => new MovingProjectileAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},

                //{AbilityEffectType.CenteredStaticAOE, (agent, abilityTemplate, abilityIndex) => new CenteredStaticAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},

                //{AbilityEffectType.TargetedStaticAOE, (agent, abilityTemplate, abilityIndex) => new TargetedStaticAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                //{AbilityEffectType.SingleTarget, (agent, abilityTemplate, abilityIndex) => new TargetedStaticAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.Summoning, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},

                {AbilityEffectType.Vortex, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.Wind, (agent, abilityTemplate, abilityIndex) => new DirectionalMovingAoEAgentCastingBehavior(agent, abilityIndex, abilityTemplate)},
            };


        public static readonly Dictionary<Type, Func<AbstractAgentCastingBehavior, List<Axis>>> UtilityByType =
            new Dictionary<Type, Func<AbstractAgentCastingBehavior, List<Axis>>>
            {
                {typeof(PreserveWindsAgentCastingBehavior), CreatePreserveWindsAxis()},

                {typeof(CenteredStaticAoEAgentCastingBehavior), CreateStaticAoEAxis()},
                {typeof(TargetedStaticAoEAgentCastingBehavior), CreateStaticAoEAxis()},
                {typeof(SummoningCastingBehavior), CreateSummoningAxis()},

                {typeof(DirectionalMovingAoEAgentCastingBehavior), CreateDirectionalMovingAoEAxis()},
                {typeof(MovingProjectileAgentCastingBehavior), CreateMovingProjectileAxis()},
            };

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreateSummoningAxis()
        {
            return behavior =>
            {
                var axes = new List<Axis>();

                axes.Add(new Axis(0, 1f, x => 1 - x, CommonDecisionFunctions.BalanceOfPower(behavior.Agent)));

                return axes;
            };
        }

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreateStaticAoEAxis()
        {
            return behavior =>
            {
                var axes = new List<Axis>
                {
                    new Axis(0, 1, x => x, (target) => 0.45f)
                };

                if (behavior.AbilityTemplate.AbilityTargetType != AbilityTargetType.Self)
                {
                    axes.Add(new Axis(0, 100, x => 1 - x, CommonDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)));
                    axes.Add(new Axis(0, 7, x => 1 - x, CommonDecisionFunctions.FormationDistanceToHostiles()));
                    axes.Add(new Axis(0, 3, x => 1 - x + 0.1f, CommonDecisionFunctions.TargetSpeed()));
                    axes.Add(new Axis(0, 0.5f, x => x + 0.01f, CommonDecisionFunctions.FormationUnderFire()));
                }

                return axes;
            };
        }

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreatePreserveWindsAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 1, x => x, target => 0.4f)
                };
            };
        }

        public static Func<AbstractAgentCastingBehavior, List<Axis>> CreateMovingProjectileAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 120, x => 1 - x, CommonDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)),
                    new Axis(0, CommonDecisionFunctions.CalculateEnemyTotalPower(behavior.Agent.Team) / 4, x => x, CommonDecisionFunctions.FormationPower()),
                    new Axis(0.0f, 1, x => x + 0.3f, CommonDecisionFunctions.RangedUnitRatio()),
                 //   new Axis(0.0f, 1, x => x * 2 / 3 + 0.1f, CommonDecisionParameterFunctions.InfantryUnitRatio()),
                };
            };
        }


        public static Func<AbstractAgentCastingBehavior, List<Axis>> CreateDirectionalMovingAoEAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 50, x => ScoringFunctions.Logistic(0.4f, 1, 20).Invoke(1 - x), CommonDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)),
                    new Axis(0, 15, x => 1 - x, CommonDecisionFunctions.FormationDistanceToHostiles()),
                    new Axis(0, CommonDecisionFunctions.CalculateEnemyTotalPower(behavior.Agent.Team), x => x, CommonDecisionFunctions.FormationPower()),
                    new Axis(1, 2.5f, x => 1 - x, CommonDecisionFunctions.Dispersedness()),
                    new Axis(0, 1, x => 1 - x, CommonDecisionFunctions.CavalryUnitRatio()),
                };
            };
        }


     
    }
}