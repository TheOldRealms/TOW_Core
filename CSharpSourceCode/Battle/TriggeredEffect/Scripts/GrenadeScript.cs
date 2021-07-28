using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class GrenadeScript : ScriptComponentBehaviour
    {
        private bool hasExploded = false;
        private bool hasLaunched = false;
        private Int32 explosionTimer = 0;
        private SoundEvent _tickSound;
        private Agent shooterAgent;
        private TriggeredEffect explsion;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            explosionTimer++;
            if (_tickSound == null)
            {
                Int32 _tickSoundindex = SoundEvent.GetEventIdFromString("dwarf_hand_grenade_tick");
                _tickSound = SoundEvent.CreateEvent(_tickSoundindex, Mission.Current.Scene);
                _tickSound.SetPosition(GameEntity.GlobalPosition);
            }
            _tickSound.SetPosition(GameEntity.GlobalPosition);
            if (!hasLaunched)
            {
                hasLaunched = true;
                _tickSound.Play();
            }
            if (hasLaunched && explosionTimer % 40 == 0)
                _tickSound.Play();
            if (explosionTimer >= 135 && !hasExploded)
            {
                hasExploded = true;
                _tickSound.Release();
                explsion.Trigger(GameEntity.GlobalPosition, Vec3.Zero, shooterAgent);
            }
        }
        protected override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }
        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            _tickSound.Release();
        }
        public void SetShooterAgent(Agent shooter)
        {
            shooterAgent = shooter;
        }
        public void SetTriggeredEffect(TriggeredEffect effect)
        {
            explsion = effect;
        }
    }
}
