using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class MissileCastingBehavior : AbstractAgentCastingBehavior
    {
        public MissileCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
        }

        public override void Terminate()
        {
        }

        public override bool IsPositional()
        {
            return false;
        }
        
        protected override bool HaveLineOfSightToAgent(Agent targetAgent)
        {
            Agent collidedAgent = Mission.Current.RayCastForClosestAgent(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float _, Agent.Index, 0.4f);
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(Agent.Position + new Vec3(z: Agent.GetEyeGlobalHeight()), targetAgent.GetChestGlobalPosition(), out float distance, out GameEntity _, 0.4f);

            return Agent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) > 1 && (distance is Single.NaN || distance > 1) &&
                   (collidedAgent == null || collidedAgent == targetAgent || collidedAgent.IsEnemyOf(Agent) || collidedAgent.GetChestGlobalPosition().Distance(targetAgent.GetChestGlobalPosition()) < 4) &&
                   (float.IsNaN(distance) || Math.Abs(distance - targetAgent.Position.Distance(Agent.Position)) < 0.3);
        }

    }
}