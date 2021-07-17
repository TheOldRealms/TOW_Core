using System.Xml.Serialization;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Engine;
using System.Timers;

namespace TOW_Core.Battle.TriggeredEffect
{
    public class TriggeredEffect
    {
        private TriggeredEffectTemplate _template;
        private int _soundIndex;
        private SoundEvent _sound;

        public TriggeredEffect(TriggeredEffectTemplate template)
        {
            _template = template;
        }

        public void Trigger(Vec3 position, Vec3 normal, Agent triggererAgent)
        {
            Timer timer = new Timer(_template.SoundEffectLength * 1000);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) =>
            {
                CleanUp();
            };
            timer.Start();
            //Cause Damage
            if (_template.DamageAmount > 0)
            {
                TOWBattleUtilities.DamageAgentsInArea(position.AsVec2, _template.Radius, (int)(_template.DamageAmount * (1 - _template.DamageVariance)), (int)(_template.DamageAmount * (1 + _template.DamageVariance)),triggererAgent, !_template.FriendlyFire);
            }
            //Apply status effects
            if(_template.ImbuedStatusEffectID != "none")
            {
                TOWBattleUtilities.ApplyStatusEffectToAgentsInArea(position.AsVec2, _template.Radius, _template.ImbuedStatusEffectID, triggererAgent, !_template.FriendlyFire);
            }
            //play visuals
            if(_template.BurstParticleEffectPrefab != "none")
            {
                var effect = GameEntity.CreateEmpty(Mission.Current.Scene);
                MatrixFrame frame = MatrixFrame.Identity;
                ParticleSystem.CreateParticleSystemAttachedToEntity(_template.BurstParticleEffectPrefab, effect, ref frame);
                var globalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in normal), position);
                effect.SetGlobalFrame(globalFrame);
                effect.FadeOut(_template.SoundEffectLength, true);
            }
            //play sound
            if(_template.SoundEffectId != "none")
            {
                _soundIndex = SoundEvent.GetEventIdFromString(_template.SoundEffectId);
                _sound = SoundEvent.CreateEvent(_soundIndex, Mission.Current.Scene);
                if(_sound != null)
                {
                    _sound.PlayInPosition(position);
                }
            }
            //trigger script
        }

        private void CleanUp()
        {
            if (_sound != null) _sound.Release();
            _sound = null;
            _soundIndex = -1;
            _template = null;
        }
    }
}
