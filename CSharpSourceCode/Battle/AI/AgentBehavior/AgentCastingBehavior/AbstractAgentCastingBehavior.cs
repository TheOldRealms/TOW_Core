﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public abstract class AbstractAgentCastingBehavior : IAgentBehavior
    {
        private WizardAIComponent _component;
        public readonly Agent Agent;
        protected float Hysteresis = 0.20f;
        private readonly int _range;
        public readonly AbilityTemplate AbilityTemplate;
        protected readonly int AbilityIndex;
        private readonly List<Axis> _axisList;

        public Target CurrentTarget = new Target();
        public List<BehaviorOption> LatestScores { get; private set; }

        public AbstractAgentTacticalBehavior TacticalBehavior { get; protected set; }
        public WizardAIComponent Component => _component ?? (_component = Agent.GetComponent<WizardAIComponent>());

        protected AbstractAgentCastingBehavior(Agent agent, AbilityTemplate abilityTemplate, int abilityIndex)
        {
            Agent = agent;
            AbilityIndex = abilityIndex;
            if (abilityTemplate != null)
            {
                _range = (int) (abilityTemplate.BaseMovementSpeed * abilityTemplate.Duration) - 1;
            }

            AbilityTemplate = abilityTemplate;
            _axisList = AgentCastingBehaviorMapping.UtilityByType[GetType()](this);
            TacticalBehavior = new KeepSafeAgentTacticalBehavior(Agent, Agent.GetComponent<WizardAIComponent>());
        }


        public abstract Boolean IsPositional();

        public virtual void Execute()
        {
            if (Agent.GetAbility(AbilityIndex).IsOnCooldown()) return;

            var medianAgent = CurrentTarget.Formation?.GetMedianAgent(true, false, CurrentTarget.Formation.GetAveragePositionOfUnits(true, false));

            if (medianAgent != null && (IsPositional() || medianAgent.Position.Distance(Agent.Position) < _range))
            {
                if (HaveLineOfSightToAgent(medianAgent))
                {
                    Agent.SelectAbility(AbilityIndex);
                    CastSpellAtTargetPosition(medianAgent.GetChestGlobalPosition());
                }
            }
        }

        public virtual void Terminate()
        {
        }

        protected virtual bool HaveLineOfSightToAgent(Agent targetAgent)
        {
            return true;
        }

        protected virtual void CastSpellAtTargetPosition(Vec3 targetPosition)
        {
            var wizardAIComponent = Agent.GetComponent<WizardAIComponent>();
            wizardAIComponent.SpellTargetRotation = CalculateSpellRotation(targetPosition);
            Agent.CastCurrentAbility();
        }

        protected Vec3 ComputeSpellAngleVelocityCorrection(Vec3 targetPosition, Vec3 targetVelocity)
        {
            var time = targetPosition.Distance(Agent.Position) / AbilityTemplate.BaseMovementSpeed;
            return targetVelocity * time;
        }


        protected Mat3 CalculateSpellRotation(Vec3 targetPosition)
        {
            return Mat3.CreateMat3WithForward(targetPosition - Agent.Position);
        }

        public List<BehaviorOption> CalculateUtility()
        {
            LatestScores = FindTargets(Agent, AbilityTemplate.AbilityTargetType)
                .Select(CalculateUtility)
                .Select(target => new BehaviorOption {Target = target, Behavior = this})
                .ToList();

            return LatestScores;
        }

        protected virtual Target CalculateUtility(Target target)
        {
            if (Agent.GetAbility(AbilityIndex).IsOnCooldown() || IsPositional() && !CommonAIStateFunctions.CanAgentMoveFreely(Agent))
            {
                target.UtilityValue = 0.0f;
                return target;
            }

            var hysteresis = Component.CurrentCastingBehavior == this && target.Formation == CurrentTarget.Formation ? Hysteresis : 0.0f;
            _axisList.GeometricMean(target);
            target.UtilityValue += hysteresis;
            return target;
        }

        protected static List<Target> FindTargets(Agent agent, AbilityTargetType targetType)
        {
            switch (targetType)
            {
                case AbilityTargetType.AlliesInAOE:
                    return agent.Team.QuerySystem.AllyTeams
                        .SelectMany(team => team.Team.Formations)
                        .Select(form => new Target {Formation = form})
                        .ToList();
                case AbilityTargetType.Self:
                    return new List<Target>()
                    {
                        new Target {Agent = agent}
                    };
                default:
                    return agent.Team.QuerySystem.EnemyTeams
                        .SelectMany(team => team.Team.Formations)
                        .Select(form => new Target {Formation = form})
                        .ToList();
            }
        }
    }
}