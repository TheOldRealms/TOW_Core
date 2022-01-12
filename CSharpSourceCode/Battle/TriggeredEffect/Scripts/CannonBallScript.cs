using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
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
            _explsion.Trigger(GameEntity.GlobalPosition, Vec3.Zero, _shooterAgent);
            var agentsToTearApart = Mission.Current.GetNearbyAgents(GameEntity.GlobalPosition.AsVec2, _explosionRadius).ToArray();
            for (int i = 0; i < agentsToTearApart.Length; i++)
            {
                var agent = agentsToTearApart[i];
                var distance = agent.Position.Distance(GameEntity.GlobalPosition);
                if (distance < _explosionRadius)
                {
                    var damage = (_explosionRadius - distance) / _explosionRadius * _damage;
                    agent.ApplyDamage((int)damage, _shooterAgent, doBlow: true, hasShockWave: true);
                    if (distance < 2 && agent.State == AgentState.Killed)
                    {
                        agent.Disappear();
                        for (int j = 0; j < 5; j++)
                        {
                            var limb = GameEntity.Instantiate(Mission.Current.Scene, "musical_instrument_harp", false);
                            var pos = agent.Frame.Elevate(1).origin;
                            limb.SetLocalPosition(pos);
                            var dir = GetRandomDirection();
                            limb.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.BodyOwnerEntity);
                            limb.EnableDynamicBody();
                            limb.AddPhysics(1, limb.CenterOfMass, limb.GetBodyShape(), dir * 10, dir * 2, PhysicsMaterial.GetFromName("flesh"), false, -1);
                        }
                    }
                }
            }
        }

        private Vec3 GetRandomDirection()
        {
            var x = MBRandom.RandomFloatRanged(-3, 3);
            var y = MBRandom.RandomFloatRanged(-3, 3);
            return new Vec3(x, y, 3);
        }
    }
}
