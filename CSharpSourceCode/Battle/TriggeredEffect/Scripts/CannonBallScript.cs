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
        private SoundEvent _sound;
        private int _explosionDamage = 150;
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

        public override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        public void SetExplosionParameteres(int explosionDamage, float explosionRadius)
        {
            _explosionDamage = explosionDamage;
            _explosionRadius = explosionRadius;
        }

        protected override void OnRemoved(int removeReason)
        {
            RunVisualEffects();
            RunSoundEffects();

            var nearbyAgents = Mission.Current.GetNearbyAgents(GameEntity.GlobalPosition.AsVec2, _explosionRadius).ToArray();
            for (int i = 0; i < nearbyAgents.Length; i++)
            {
                var agent = nearbyAgents[i];
                var distance = agent.Position.Distance(GameEntity.GlobalPosition);
                if (distance < _explosionRadius)
                {
                    var damage = (_explosionRadius - distance) / _explosionRadius * _explosionDamage;
                    agent.ApplyDamage((int)damage, _shooterAgent, doBlow: true, hasShockWave: true, impactPosition: GameEntity.GlobalPosition);
                    if (distance < 3 && agent.State == AgentState.Killed)
                    {
                        agent.Disappear();
                        var frame = agent.Frame.Elevate(1);
                        if (distance <= 1.5f)
                        {
                            ExplodeNearVictim(frame);
                        }
                        else
                        {
                            ExplodeFarVictim(frame);
                        }
                    }
                }
            }
        }

        private void RunVisualEffects()
        {
            var effect = GameEntity.CreateEmpty(Mission.Current.Scene);
            MatrixFrame frame = MatrixFrame.Identity;
            ParticleSystem.CreateParticleSystemAttachedToEntity("cannonball_explosion_7", effect, ref frame);
            var globalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), GameEntity.GlobalPosition);
            effect.SetGlobalFrame(globalFrame);
        }

        private void RunSoundEffects()
        {
            var distanceFromPlayer = GameEntity.GlobalPosition.Distance(Mission.Current.GetCameraFrame().origin);
            int soundIndex = distanceFromPlayer < 30 ? SoundEvent.GetEventIdFromString("cannonball_explosion_close") : SoundEvent.GetEventIdFromString("cannonball_explosion_far");
            _sound = SoundEvent.CreateEvent(soundIndex, Mission.Current.Scene);
            if (_sound != null)
            {
                _sound.PlayInPosition(GameEntity.GlobalPosition);
            }
        }

        private void ExplodeNearVictim(MatrixFrame frame)
        {
            LaunchLimb(frame, "exploded_head_001"); 
            LaunchLimb(frame, "exploded_arms_001");
            LaunchLimb(frame, "exploded_arms_002");
            LaunchLimb(frame, "exploded_legs_002");
            LaunchLimb(frame, "exploded_legs_003");

            LaunchLimb(frame, "exploded_flesh_pieces_001");
            LaunchLimb(frame, "exploded_flesh_pieces_002");
            LaunchLimb(frame, "exploded_flesh_pieces_003");

            LaunchLimb(frame, "exploded_limb_pieces_001");
            LaunchLimb(frame, "exploded_limb_pieces_002");
            LaunchLimb(frame, "exploded_limb_pieces_003");
        }

        private void ExplodeFarVictim(MatrixFrame frame)
        {
            LaunchLimb(frame, "exploded_torso_001");
            LaunchLimb(frame, "exploded_legs_001");

            LaunchLimb(frame, "exploded_flesh_pieces_001");
            LaunchLimb(frame, "exploded_flesh_pieces_002");
            LaunchLimb(frame, "exploded_limb_pieces_001");
            LaunchLimb(frame, "exploded_limb_pieces_002");
        }

        private void LaunchLimb(MatrixFrame frame, string name)
        {
            var limb = GameEntity.Instantiate(Mission.Current.Scene, name, false);
            limb.SetGlobalFrame(frame);
            limb.CreateAndAddScriptComponent("SmokingLimbScript");
            limb.CallScriptCallbacks();
            var dir = GetRandomDirection();
            limb.EnableDynamicBody();
            var multiplier = 30 / limb.Mass;
            limb.AddPhysics(limb.Mass, limb.CenterOfMass, limb.GetBodyShape(), dir * multiplier, dir * 2, PhysicsMaterial.GetFromName("flesh"), false, -1);
        }

        private Vec3 GetRandomDirection()
        {
            var x = MBRandom.RandomFloatRanged(-3, 3);
            var y = MBRandom.RandomFloatRanged(-3, 3);
            return new Vec3(x, y, 1);
        }
    }
}
