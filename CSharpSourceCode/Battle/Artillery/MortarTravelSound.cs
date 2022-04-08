using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Artillery
{
    public class MortarTravelSound : ScriptComponentBehavior
    {
        private SoundEvent _projectileMoveSound;
        private bool _soundStarted;
        public string MortarProjectileTraveling = "mortar_traveling";
        
        protected void SetProjectileMovementSound(Vec3 position)
        {
            if(_projectileMoveSound != null)
            {
                _projectileMoveSound.SetPosition(position);
                if (IsSoundPlaying()) return;
                else
                {
                    if (!_soundStarted)
                    {
                        _projectileMoveSound.Play();
                        _soundStarted = true;
                    }
                    
                }
            }
        }

        
        public override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }
        
        protected override void OnInit()
        {
            base.OnInit();
            Init();
            SetScriptComponentToTick(GetTickRequirement());
        }
        
        public void Init()
        {
            var index  = SoundEvent.GetEventIdFromString(MortarProjectileTraveling);
            _projectileMoveSound = SoundEvent.CreateEvent(index, Scene);
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            
            ProjectileDestroyed();
        }

        
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            var pos= this.GameEntity.GetFrame().origin;
            SetProjectileMovementSound(pos);
        }
        
       

        private bool IsSoundPlaying()
        {
            return _projectileMoveSound != null && _projectileMoveSound.IsValid && _projectileMoveSound.IsPlaying();
        }

        private void ProjectileDestroyed()
        {
            _projectileMoveSound.Release();
            _projectileMoveSound = null;
        }
    }
}