using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior
{
    public static class AgentCastingBehaviorMapping
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>> BehaviorByType =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>>
            {
                // {AbilityEffectType.Augment, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},
                // {AbilityEffectType.AgentMoving, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},
                // {AbilityEffectType.Blast, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},
                // {AbilityEffectType.Bombardment, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},
                // {AbilityEffectType.Heal, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},
                // {AbilityEffectType.Hex, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.Missile, (agent, abilityTemplate, abilityIndex) => new MissileCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.SeekerMissile, (agent, abilityTemplate, abilityIndex) => new MissileCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.Summoning, (agent, abilityTemplate, abilityIndex) => new SummoningCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.Vortex, (agent, abilityTemplate, abilityIndex) => new AoEDirectionalCastingBehavior(agent, abilityIndex, abilityTemplate)},
                {AbilityEffectType.Wind, (agent, abilityTemplate, abilityIndex) => new AoEDirectionalCastingBehavior(agent, abilityIndex, abilityTemplate)},
            };


        public static readonly Dictionary<Type, Func<AbstractAgentCastingBehavior, List<Axis>>> UtilityByType =
            new Dictionary<Type, Func<AbstractAgentCastingBehavior, List<Axis>>>
            {
                {typeof(PreserveWindsAgentCastingBehavior), CreatePreserveWindsAxis()},

                {typeof(AoEAdjacentCastingBehavior), CreateStaticAoEAxis()},
                {typeof(AoETargetedCastingBehavior), CreateStaticAoEAxis()},
                {typeof(SummoningCastingBehavior), CreateSummoningAxis()},

                {typeof(AoEDirectionalCastingBehavior), CreateDirectionalMovingAoEAxis()},
                {typeof(MissileCastingBehavior), CreateMovingProjectileAxis()},
            };

        public static List<AbstractAgentCastingBehavior> PrepareCastingBehaviors(Agent agent)
        {
            var castingBehaviors = new List<AbstractAgentCastingBehavior>();
            var index = 0;
            foreach (var knownAbilityTemplate in agent.GetComponent<AbilityComponent>().GetKnownAbilityTemplates())
            {
                castingBehaviors
                    .Add(BehaviorByType.GetValueOrDefault(knownAbilityTemplate.AbilityEffectType, BehaviorByType[AbilityEffectType.Missile])
                        .Invoke(agent, index, knownAbilityTemplate));
                index++;
            }

            castingBehaviors.Add(new PreserveWindsAgentCastingBehavior(agent, new AbilityTemplate {AbilityTargetType = AbilityTargetType.Self}, index));
            return castingBehaviors;
        }

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreateSummoningAxis()
        {
            return behavior =>
            {
                var axes = new List<Axis>();

                axes.Add(new Axis(0, 1f, x => 1 - x, CommonAIDecisionFunctions.BalanceOfPower(behavior.Agent)));

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
                    axes.Add(new Axis(0, 100, x => 1 - x, CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)));
                    axes.Add(new Axis(0, 7, x => 1 - x, CommonAIDecisionFunctions.FormationDistanceToHostiles()));
                    axes.Add(new Axis(0, 3, x => 1 - x + 0.1f, CommonAIDecisionFunctions.TargetSpeed()));
                    axes.Add(new Axis(0, 0.5f, x => x + 0.01f, CommonAIDecisionFunctions.FormationUnderFire()));
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
                    new Axis(0, 120, x => 1 - x, CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)),
                    new Axis(0, CommonAIDecisionFunctions.CalculateEnemyTotalPower(behavior.Agent.Team) / 4, x => x, CommonAIDecisionFunctions.FormationPower()),
                    new Axis(0.0f, 1, x => x + 0.3f, CommonAIDecisionFunctions.RangedUnitRatio()),
                };
            };
        }


        public static Func<AbstractAgentCastingBehavior, List<Axis>> CreateDirectionalMovingAoEAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 50, x => ScoringFunctions.Logistic(0.4f, 1, 20).Invoke(1 - x), CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)),
                    new Axis(0, 15, x => 1 - x, CommonAIDecisionFunctions.FormationDistanceToHostiles()),
                    new Axis(0, CommonAIDecisionFunctions.CalculateEnemyTotalPower(behavior.Agent.Team), x => x, CommonAIDecisionFunctions.FormationPower()),
                    new Axis(1, 2.5f, x => 1 - x, CommonAIDecisionFunctions.Dispersedness()),
                    new Axis(0, 1, x => 1 - x, CommonAIDecisionFunctions.CavalryUnitRatio()),
                };
            };
        }
    }
}