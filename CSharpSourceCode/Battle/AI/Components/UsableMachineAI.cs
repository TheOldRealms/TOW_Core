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

        protected override void OnTick(Func<Agent, bool> isAgentManagedByThisMachineAI, Team potentialUsersTeam, float dt)
        {
            base.OnTick(isAgentManagedByThisMachineAI, potentialUsersTeam, dt);

            _artillery.Shoot(); //Targeting logic goes around this. Should probably trigger it by .MovementFlags.HasAnyFlag(Agent.MovementControlFlag.AttackMask)) instead?
        }
    }
}