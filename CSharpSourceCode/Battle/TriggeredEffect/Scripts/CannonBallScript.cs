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

        public override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        protected override void OnRemoved(int removeReason)
        {
            var distanceFromPlayer = GameEntity.GlobalPosition.Distance(Mission.Current.GetCameraFrame().origin);
            RunVisualAndSoundEffects(distanceFromPlayer);

            var agentsToTearApart = Mission.Current.GetNearbyAgents(GameEntity.GlobalPosition.AsVec2, _explosionRadius).ToArray();
            for (int i = 0; i < agentsToTearApart.Length; i++)
            {
                var agent = agentsToTearApart[i];
                if (distanceFromPlayer < _explosionRadius)
                {
                    var damage = (_explosionRadius - distanceFromPlayer) / _explosionRadius * _damage;
                    agent.ApplyDamage((int)damage, _shooterAgent, doBlow: true, hasShockWave: true, impactPosition: GameEntity.GlobalPosition);
                    if (distanceFromPlayer < 2 && agent.State == AgentState.Killed)
                    {
                        agent.Disappear();
                        var frame = agent.Frame.Elevate(1);
                        LaunchLimb(frame, "exploded_torso_001");
                        LaunchLimb(frame, "exploded_torso_001");
                        LaunchLimb(frame, "exploded_torso_001");
                        LaunchLimb(frame, "exploded_torso_001");
                        LaunchLimb(frame, "exploded_torso_001");
                    }
                }
            }
        }

        private void RunVisualAndSoundEffects(float distanceFromPlayer)
        {
            //Visual effect
            var effect = GameEntity.CreateEmpty(Mission.Current.Scene);
            MatrixFrame frame = MatrixFrame.Identity;
            ParticleSystem.CreateParticleSystemAttachedToEntity("cannonball_explosion_6", effect, ref frame);
            var globalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), GameEntity.GlobalPosition);
            effect.SetGlobalFrame(globalFrame);
            effect.FadeOut(10, true);

            //Sound effect
            int soundIndex = distanceFromPlayer < 30 ? SoundEvent.GetEventIdFromString("cannonball_explosion_close") : SoundEvent.GetEventIdFromString("cannonball_explosion_far");
            _sound = SoundEvent.CreateEvent(soundIndex, Mission.Current.Scene);
            if (_sound != null)
            {
                _sound.PlayInPosition(GameEntity.GlobalPosition);
            }
        }

        private void LaunchLimb(MatrixFrame frame, string name)
        {
            var limb = GameEntity.Instantiate(Mission.Current.Scene, name, false);
            limb.SetGlobalFrame(frame);
            limb.CreateAndAddScriptComponent("SmokingLimbScript");
            limb.CallScriptCallbacks();
            var dir = GetRandomDirection();
            limb.EnableDynamicBody();
            limb.AddPhysics(limb.Mass, limb.CenterOfMass, limb.GetBodyShape(), dir * 5, dir * 2, PhysicsMaterial.GetFromName("flesh"), false, -1);
        }

        private Vec3 GetRandomDirection()
        {
            var x = MBRandom.RandomFloatRanged(-3, 3);
            var y = MBRandom.RandomFloatRanged(-3, 3);
            return new Vec3(x, y, 1);
        }
    }
}
