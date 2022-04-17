using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.AgentBehavior.Components;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public abstract class AbstractAgentCastingBehavior : IAgentBehavior
    {
        private WizardAIComponent _component;
        public readonly Agent Agent;
        protected float Hysteresis = 0.20f;
        private readonly int _abilityRange;
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
                _abilityRange = (int) (abilityTemplate.BaseMovementSpeed * abilityTemplate.Duration) - 1;
            }

            AbilityTemplate = abilityTemplate;
            _axisList = AgentCastingBehaviorConfiguration.UtilityByType[GetType()](this);
            TacticalBehavior = new KeepSafeAgentTacticalBehavior(Agent, Agent.GetComponent<WizardAIComponent>());
        }

        public virtual void Execute()
        {
            if (Agent.GetAbility(AbilityIndex).IsOnCooldown()) return;

            CurrentTarget = UpdateTarget(CurrentTarget);

            if (HaveLineOfSightToTarget(CurrentTarget))
            {
                Agent.SelectAbility(AbilityIndex);
                CastSpellAtCurrentTarget();
            }
        }

        public virtual void Terminate()
        {
        }

        protected virtual Target UpdateTarget(Target target)
        {
            return target;
        }

        protected virtual bool HaveLineOfSightToTarget(Target target)
        {
            return true;
        }

        protected virtual void CastSpellAtCurrentTarget()
        {
            Agent.CastCurrentAbility();
        }

        protected Vec3 ComputeSpellAngleVelocityCorrection(Vec3 targetPosition, Vec3 targetVelocity)
        {
            float time;
            switch (AbilityTemplate.AbilityEffectType)
            {
                case AbilityEffectType.Bombardment:
                case AbilityEffectType.Vortex:
                case AbilityEffectType.Heal:
                case AbilityEffectType.Hex:
                case AbilityEffectType.Augment:
                {
                    time = AbilityTemplate.CastTime;
                    break;
                }
                default:
                {
                    time = AbilityTemplate.BaseMovementSpeed != 0 ? targetPosition.Distance(Agent.Position) / AbilityTemplate.BaseMovementSpeed : AbilityTemplate.CastTime;
                    break;
                }
            }

            return targetVelocity * time;
        }


        public Mat3 CalculateSpellRotation(Vec3 targetPosition)
        {
            return Mat3.CreateMat3WithForward(targetPosition - Agent.Position);
        }

        public List<BehaviorOption> CalculateUtility()
        {
            LatestScores = AgentCastingBehaviorConfiguration.FindTargets(Agent, AbilityTemplate)
                .Select(target =>
                {
                    target.UtilityValue = CalculateUtility(target);
                    return target;
                })
                .Select(target => new BehaviorOption {Target = target, Behavior = this})
                .ToList();

            return LatestScores;
        }

        protected virtual float CalculateUtility(Target target)
        {
            var ability = Agent.GetAbility(AbilityIndex);
            if (ability.IsOnCooldown() || !ability.CanCast(Agent))
            {
                return 0.0f;
            }

            var hysteresis = Component.CurrentCastingBehavior == this && target.Formation == CurrentTarget.Formation ? Hysteresis : 0.0f;
            return _axisList.GeometricMean(target) + hysteresis;
        }
    }
}