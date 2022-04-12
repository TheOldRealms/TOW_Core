using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Battle.Artillery;

namespace TOW_Core.Battle.AI.Components
{
    public class ArtilleryAI : UsableMachineAIBase
    {
        private readonly Artillery.ArtilleryRangedSiegeWeapon _artillery;
        private Threat _target;
        private List<Axis> targetDecisionFunctions;

        public ArtilleryAI(Artillery.ArtilleryRangedSiegeWeapon usableMachine) : base(usableMachine)
        {
            _artillery = usableMachine;
            targetDecisionFunctions = CreateTargetingFunctions();
        }

        public override bool HasActionCompleted => base.HasActionCompleted;

        protected override void OnTick(Func<Agent, bool> isAgentManagedByThisMachineAI, Team potentialUsersTeam, float dt)
        {
            base.OnTick(isAgentManagedByThisMachineAI, potentialUsersTeam, dt);
            if (_artillery.PilotAgent != null && _artillery.PilotAgent.IsAIControlled)
            {
                if (_artillery.State == RangedSiegeWeapon.WeaponState.Idle)
                {
                    if (_target != null && _target.Formation != null && _target.Formation.GetCountOfUnitsWithCondition(x => x.IsActive()) > 0)
                    {
                        if (_artillery.Target != _target)
                        {
                            _artillery.SetTarget(_target);
                        }

                        if (_artillery.Target != null && _artillery.AimAtTarget(GetAdjustedTargetPosition(_artillery.Target)))
                        {
                            _artillery.Shoot();
                        }
                    }
                    else
                    {
                        _target = null;
                        _artillery.ClearTarget();
                        FindNewTarget();
                    }
                }
            }
        }

        private Vec3 GetAdjustedTargetPosition(Threat target)
        {
            if (target == null || target.Formation == null) return Vec3.Zero;
            else
            {
                float speed = target.Formation.GetMovementSpeedOfUnits();
                float time = (UsableMachine as ArtilleryRangedSiegeWeapon).GetEstimatedCurrentFlightTime();
                var frame = target.Formation.GetMedianAgent(true, true, target.Formation.GetAveragePositionOfUnits(true, true)).Frame;
                frame.Advance(speed * time);
                return frame.origin;
            }
        }

        private void FindNewTarget()
        {
            _target = GetAllThreats().Count > 0 ? GetAllThreats().MaxBy(x => x.Formation.CountOfUnits) : null;
        }

        private List<Threat> GetAllThreats()
        {
            List<Threat> list = new List<Threat>();
            /*
            this._potentialTargetUsableMachines.RemoveAll((ITargetable ptum) => ptum is UsableMachine && ((ptum as UsableMachine).IsDestroyed || (ptum as UsableMachine).GameEntity == null));
            list.AddRange(from um in this._potentialTargetUsableMachines
                          select new Threat
                          {
                              WeaponEntity = um,
                              ThreatValue = this.Weapon.ProcessTargetValue(um.GetTargetValue(this.WeaponPositions), um.GetTargetFlags())
                          });
            */
            foreach (Formation formation in GetUnemployedEnemyFormations())
            {
                Target targetFormation = GetTargetValueOfFormation(formation);
                if (targetFormation.UtilityValue != -1f)
                {
                    list.Add(targetFormation);
                }
            }

            return list;
        }

        private Target GetTargetValueOfFormation(Formation formation)
        {
            var target = new Target {Formation = formation};
            target.UtilityValue = ProcessTargetValue(targetDecisionFunctions.GeometricMean(target), RangedSiegeWeaponAi.ThreatSeeker.GetTargetFlagsOfFormation());
            return target;
        }

        private IEnumerable<Formation> GetUnemployedEnemyFormations()
        {
            return from f in (from t in Mission.Current.Teams
                    where t.Side.GetOppositeSide() == _artillery.Side
                    select t).SelectMany((Team t) => t.FormationsIncludingSpecial)
                where f.CountOfUnits > 0
                select f;
        }

        private List<Axis> CreateTargetingFunctions()
        {
            var targetingFunctions = new List<Axis>();
          //  targetingFunctions.Add(new Axis(0, 120, x => 1 - x, CommonDecisionFunctions.DistanceToTarget(() => _artillery.Position)));
            targetingFunctions.Add(new Axis(0, CommonAIDecisionFunctions.CalculateEnemyTotalPower(_artillery.Team) / 4, x => x, CommonAIDecisionFunctions.FormationPower()));
            return targetingFunctions;
        }

        public float ProcessTargetValue(float baseValue, TargetFlags flags) //TODO: This is probably not necessary, we can represent it better with the axis. Normalized values are better in these scenarios.
        {
            if (flags.HasAnyFlag(TargetFlags.NotAThreat))
            {
                return -1000f;
            }

            if (flags.HasAnyFlag(TargetFlags.None))
            {
                baseValue *= 1.5f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsSiegeEngine))
            {
                baseValue *= 2f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsStructure))
            {
                baseValue *= 1.5f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsSmall))
            {
                baseValue *= 0.5f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsMoving))
            {
                baseValue *= 0.8f;
            }

            if (flags.HasAnyFlag(TargetFlags.DebugThreat))
            {
                baseValue *= 10000f;
            }

            return baseValue;
        }
    }
}