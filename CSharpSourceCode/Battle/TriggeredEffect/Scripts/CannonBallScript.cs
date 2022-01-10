using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class CannonBallScript : ScriptComponentBehavior
    {
        private Agent _shooterAgent;
        private TriggeredEffect _explsion;
        private int _damage = 150;
        private float _explosionRadius = 20;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }

        public void SetShooterAgent(Agent shooterAgent)
        {
            _shooterAgent = shooterAgent;
        }

        public void SetTriggeredEffect(TriggeredEffect effect)
        {
            _explsion = effect;
        }

        public override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        protected override void OnRemoved(int removeReason)
        {
            _explsion.Trigger(GameEntity.GlobalPosition, Vec3.Zero, _shooterAgent); //just visuals and sound
            var agentsToTearApart = Mission.Current.GetNearbyAgents(GameEntity.GlobalPosition.AsVec2, _explosionRadius).ToArray();
            for (int i = 0; i < agentsToTearApart.Length; i++)
            {
                var agent = agentsToTearApart[i];
                var distance = agent.Position.Distance(GameEntity.GlobalPosition);
                if (distance < _explosionRadius)
                {
                    if (distance < 1)
                    {
                        agent.ApplyDamage(_damage, _shooterAgent, doBlow: true, hasShockWave: true);
                        if (agent.State == AgentState.Killed)
                        {
                            //Tear apart victim
                        }
                    }
                    else
                    {
                        var recalculatedDamage = (_explosionRadius - distance) / _explosionRadius * 200;
                        agent.ApplyDamage((int)recalculatedDamage, _shooterAgent, doBlow: true, hasShockWave: true);
                    }
                }
            }
        }
    }
}
