using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Battle.Artillery;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.AgentBehavior.Components
{
    public class ArtilleryAI : UsableMachineAIBase
    {
        private readonly Artillery.ArtilleryRangedSiegeWeapon _artillery;
        private Target _target;
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

                        if (_artillery.Target != null && _artillery.AimAtTarget(GetAdjustedTargetPosition(_artillery.Target)) && _artillery.PilotAgent.Formation.FiringOrder.OrderType != OrderType.HoldFire)
                        {
                            _artillery.Shoot();
                            _artillery.Target.SelectedWorldPosition = Vec3.Zero;
                        }
                    }
                    else
                    {
                        _artillery.ClearTarget();
                        _target = FindNewTarget();
                    }
                }
            }
        }

        private Vec3 GetAdjustedTargetPosition(Target target)
        {
            if (target == null || target.Formation == null) return Vec3.Zero;

            var targetAgent = target.SelectedWorldPosition == Vec3.Zero ? CommonAIFunctions.GetRandomAgent(target.Formation) : target.Agent;
            target.Agent = targetAgent;

            Vec3 velocity = target.Formation.QuerySystem.CurrentVelocity.ToVec3();
            float time = (UsableMachine as ArtilleryRangedSiegeWeapon).GetEstimatedCurrentFlightTime();

            target.SelectedWorldPosition = target.Position + velocity * time;

            return target.SelectedWorldPosition;
        }

        private Target FindNewTarget()
        {
            var findNewTarget = GetAllThreats();
            return findNewTarget.Count > 0 ? findNewTarget.MaxBy(target => target.UtilityValue) : null;
        }

        private List<Target> GetAllThreats()
        {
            List<Target> list = new List<Target>();
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
            target.UtilityValue = targetDecisionFunctions.GeometricMean(target); //ProcessTargetValue(, RangedSiegeWeaponAi.ThreatSeeker.GetTargetFlagsOfFormation());
            return target;
        }

        private IEnumerable<Formation> GetUnemployedEnemyFormations()
        {
            return from f in (from t in Mission.Current.Teams where t.Side.GetOppositeSide() == _artillery.Side select t)
                    .SelectMany((Team t) => t.FormationsIncludingSpecial)
                where f.CountOfUnits > 0
                select f;
        }

        private List<Axis> CreateTargetingFunctions()
        {
            var targetingFunctions = new List<Axis>();
            targetingFunctions.Add(new Axis(0, 300, x => 0.7f - 3 * (float) Math.Pow(x - 0.3f, 3) + (float) Math.Pow(x, 2), CommonAIDecisionFunctions.DistanceToTarget(() => _artillery.GameEntity.GlobalPosition)));
            targetingFunctions.Add(new Axis(0, CommonAIDecisionFunctions.CalculateEnemyTotalPower(_artillery.Team), x => x, CommonAIDecisionFunctions.FormationPower()));
            targetingFunctions.Add(new Axis(0, 40, x => x, CommonAIDecisionFunctions.UnitCount()));
            return targetingFunctions;
        }
    }
}