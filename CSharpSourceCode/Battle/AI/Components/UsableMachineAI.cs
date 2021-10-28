using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Components
{
    public class ArtilleryAI : UsableMachineAIBase
    {
        private readonly Artillery.Artillery _artillery;
        private Threat _target;

        public ArtilleryAI(Artillery.Artillery usableMachine) : base(usableMachine)
        {
            this._artillery = usableMachine;
        }

        public override bool HasActionCompleted => base.HasActionCompleted;

        protected override void OnTick(Func<Agent, bool> isAgentManagedByThisMachineAI, Team potentialUsersTeam, float dt)
        {
            base.OnTick(isAgentManagedByThisMachineAI, potentialUsersTeam, dt);
            if(_artillery.PilotAgent != null && _artillery.PilotAgent.IsAIControlled)
            {
                if(_artillery.State == RangedSiegeWeapon.WeaponState.Idle)
                {
                    if(_target != null && _target.Formation != null && _target.Formation.GetCountOfUnitsWithCondition(x => x.IsActive()) > 0)
                    {
                        if (_artillery.GetTarget() != _target)
                        {
                            _artillery.SetTarget(_target);
                        }
                        if (_artillery.CanShootAtTarget())
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

        private void FindNewTarget()
        {
            List<Threat> allThreats = GetAllThreats();
            _target = allThreats.MaxBy(x => x.ThreatValue);
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
            foreach (Formation formation in this.GetUnemployedEnemyFormations())
            {
                float targetValueOfFormation = GetTargetValueOfFormation(formation);
                if (targetValueOfFormation != -1f)
                {
                    list.Add(new Threat
                    {
                        Formation = formation,
                        ThreatValue = _artillery.ProcessTargetValue(targetValueOfFormation, RangedSiegeWeaponAi.ThreatSeeker.GetTargetFlagsOfFormation())
                    });
                }
            }
            return list;
        }

        private float GetTargetValueOfFormation(Formation formation)
        {
            if (formation.QuerySystem.LocalEnemyPower / formation.QuerySystem.LocalAllyPower > 0.5f)
            {
                return -1f;
            }
            float num = (float)formation.CountOfUnits * 3f;
            float num2 = MBMath.ClampFloat(formation.QuerySystem.LocalAllyPower / (formation.QuerySystem.LocalEnemyPower + 0.01f), 0f, 5f) / 5f;
            return num * num2;
        }

        private IEnumerable<Formation> GetUnemployedEnemyFormations()
        {
            return from f in (from t in Mission.Current.Teams
                              where t.Side.GetOppositeSide() == _artillery.Side
                              select t).SelectMany((Team t) => t.FormationsIncludingSpecial)
                   where f.CountOfUnits > 0
                   select f;
        }
    }
}