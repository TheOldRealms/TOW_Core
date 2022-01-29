using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class CannonBallScript : ScriptComponentBehavior
    {
        private Agent _shooterAgent;
        private TriggeredEffect _explsion;
        private int _damage = 150;
        private float _explosionRadius = 10;

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
                    agent.ApplyDamage((int)damage, _shooterAgent, doBlow: true, hasShockWave: true, impactPosition: GameEntity.GlobalPosition);
                    if (distance < 2 && agent.State == AgentState.Killed)
                    {
                        agent.Disappear();
                        var position = agent.Frame.Elevate(1).origin;
                        LaunchLimb(position, "musical_instrument_harp");
                        LaunchLimb(position, "musical_instrument_harp");
                        LaunchLimb(position, "musical_instrument_harp");
                        LaunchLimb(position, "musical_instrument_harp");
                        LaunchLimb(position, "musical_instrument_harp");
                    }
                }
            }
        }

        private void LaunchLimb(Vec3 position, string name)
        {
            var limb = GameEntity.Instantiate(Mission.Current.Scene, name, false);
            limb.SetLocalPosition(position);
            limb.CreateAndAddScriptComponent("SmokingLimbScript");
            limb.CallScriptCallbacks();
            var dir = GetRandomDirection();
            limb.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.BodyOwnerEntity);
            limb.EnableDynamicBody();
            limb.AddPhysics(1, limb.CenterOfMass, limb.GetBodyShape(), dir * 5, dir * 2, PhysicsMaterial.GetFromName("flesh"), false, -1);
        }

        private Vec3 GetRandomDirection()
        {
            var x = MBRandom.RandomFloatRanged(-3, 3);
            var y = MBRandom.RandomFloatRanged(-3, 3);
            return new Vec3(x, y, 1);
        }
    }
}
