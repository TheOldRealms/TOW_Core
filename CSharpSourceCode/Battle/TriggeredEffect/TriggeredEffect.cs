using System.Xml.Serialization;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Engine;
using System.Timers;
using System;
using TOW_Core.Battle.TriggeredEffect.Scripts;

namespace TOW_Core.Battle.TriggeredEffect
{
    public class TriggeredEffect : IDisposable
    {
        private TriggeredEffectTemplate _template;
        private int _soundIndex;
        private SoundEvent _sound;

        public TriggeredEffect(TriggeredEffectTemplate template)
        {
            _template = template;
        }

        public void Trigger(Vec3 position, Vec3 normal, Agent triggererAgent, Agent[] targets = null)
        {
            Timer timer = new Timer(2000);
            if (_template.SoundEffectLength > 0)
            {
                timer.Interval = _template.SoundEffectLength * 1000;
            }
            timer.AutoReset = false;
            timer.Elapsed += (s, e) =>
            {
                Dispose();
            };
            timer.Start();

            //Cause Damage
            if (targets == null)
            {
                if (_template.DamageAmount > 0)
                {
                    TOWBattleUtilities.DamageAgentsInArea(position.AsVec2, _template.Radius, (int)(_template.DamageAmount * (1 - _template.DamageVariance)), (int)(_template.DamageAmount * (1 + _template.DamageVariance)), triggererAgent, _template.TargetType, _template.HasShockWave);
                }
                else if (_template.DamageAmount < 0)
                {
                    TOWBattleUtilities.HealAgentsInArea(position.AsVec2, _template.Radius, (int)(-_template.DamageAmount * (1 - _template.DamageVariance)), (int)(-_template.DamageAmount * (1 + _template.DamageVariance)), triggererAgent, _template.TargetType);
                }
                //Apply status effects
                if (_template.ImbuedStatusEffectID != "none")
                {
                    TOWBattleUtilities.ApplyStatusEffectToAgentsInArea(position.AsVec2, _template.Radius, _template.ImbuedStatusEffectID, triggererAgent, _template.TargetType);
                }
            }
            else
            {
                if (_template.DamageAmount > 0)
                {
                    TOWBattleUtilities.DamageAgents(targets, (int)(_template.DamageAmount * (1 - _template.DamageVariance)), (int)(_template.DamageAmount * (1 + _template.DamageVariance)), triggererAgent, _template.TargetType, _template.HasShockWave);
                }
                else if (_template.DamageAmount < 0)
                {
                    TOWBattleUtilities.HealAgents(targets, (int)(-_template.DamageAmount * (1 - _template.DamageVariance)), (int)(-_template.DamageAmount * (1 + _template.DamageVariance)), triggererAgent, _template.TargetType);
                }
                //Apply status effects
                if (_template.ImbuedStatusEffectID != "none")
                {
                    TOWBattleUtilities.ApplyStatusEffectToAgents(targets, _template.ImbuedStatusEffectID, triggererAgent, _template.TargetType);
                }
            }
            SpawnVisuals(position, normal);
            PlaySound(position);
            TriggerScript(position, triggererAgent);
        }

        private void SpawnVisuals(Vec3 position, Vec3 normal)
        {
            //play visuals
            if (_template.BurstParticleEffectPrefab != "none")
            {
                var effect = GameEntity.CreateEmpty(Mission.Current.Scene);
                MatrixFrame frame = MatrixFrame.Identity;
                ParticleSystem.CreateParticleSystemAttachedToEntity(_template.BurstParticleEffectPrefab, effect, ref frame);
                var globalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in normal), position);
                effect.SetGlobalFrame(globalFrame);
                effect.FadeOut(_template.SoundEffectLength, true);
            }
        }

        private void PlaySound(Vec3 position)
        {
            //play sound
            if (_template.SoundEffectId != "none")
            {
                _soundIndex = SoundEvent.GetEventIdFromString(_template.SoundEffectId);
                _sound = SoundEvent.CreateEvent(_soundIndex, Mission.Current.Scene);
                if (_sound != null)
                {
                    _sound.PlayInPosition(position);
                }
            }
        }

        private void TriggerScript(Vec3 position, Agent agent)
        {
            if(_template.ScriptNameToTrigger != "none")
            {
                try
                {
                    var obj = Activator.CreateInstance(Type.GetType(_template.ScriptNameToTrigger));
                    if (obj is ITriggeredScript)
                    {
                        var script = obj as ITriggeredScript;
                        script.OnTrigger(position, agent);
                    }
                }
                catch (Exception e)
                {
                    TOW_Core.Utilities.TOWCommon.Log("Tried to spawn TriggeredScript: " + _template.ScriptNameToTrigger + ", but failed.", NLog.LogLevel.Error);
                }
            }
        }

        public void Dispose()
        {
            CleanUp();
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
