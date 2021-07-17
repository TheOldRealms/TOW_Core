using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Grenades
{
    public class GrenadeScript : ScriptComponentBehaviour
    {
        private bool hasExploded = false;
        private Int32 explosionTimer = 0;
        private const Int32 maxDamage = 110;
        private const float radius = 5f;
        private SoundEvent _explosionSound;
        private Agent shooterAgent;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            explosionTimer++;
            if (explosionTimer >= 100 && !hasExploded)
            {
                hasExploded = true;
                ExplodeGrenade(GameEntity);
            }
        }
        protected override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }
        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            if (_explosionSound != null) _explosionSound.Release();
        }
        private void ExplodeGrenade(GameEntity entity)
        {
            entity.FadeOut(2.9f, true);
            Vec3 position = entity.GlobalPosition;
            TOWBattleUtilities.DamageAgentsWithGrenade(position.AsVec2, radius, maxDamage, shooterAgent, false);
            Int32 _explosionSoundindex = SoundEvent.GetEventIdFromString("fireball_explosion");
            var explosion = GameEntity.CreateEmpty(Mission.Current.Scene);
            MatrixFrame frame = MatrixFrame.Identity;
            ParticleSystem.CreateParticleSystemAttachedToEntity("psys_burning_projectile_default_coll", explosion, ref frame);
            var globalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), position);
            explosion.SetGlobalFrame(globalFrame);
            explosion.FadeOut(3, true);
            _explosionSound = SoundEvent.CreateEvent(_explosionSoundindex, Mission.Current.Scene);
            _explosionSound.SetPosition(globalFrame.origin);
            _explosionSound.Play();
        }
        public void SetShooterAgent(Agent shooter)
        {
            shooterAgent = shooter;
        }
    }
}
