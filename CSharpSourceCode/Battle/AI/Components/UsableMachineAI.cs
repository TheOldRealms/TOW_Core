using System;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Components
{
    public class UsableMachineAI : UsableMachineAIBase
    {
        private readonly Artillery.Artillery _artillery;

        public UsableMachineAI(Artillery.Artillery usableMachine) : base(usableMachine)
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
                    _artillery.Shoot();
                }
            }
        }
    }
}