using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using System.Xml.Serialization;
using System.Timers;
using System.Xml.Schema;
using System.Xml;
using TOW_Core.ObjectDataExtensions;
using TaleWorlds.Library;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Battle.Damage;
using TOW_Core.Battle.TriggeredEffect;
using TaleWorlds.Engine;
using TOW_Core.Abilities.Scripts;

namespace TOW_Core.Abilities
{
    public abstract class Ability : IDisposable
    {
        private string _stringId;
        private AbilityTemplate _template;
        private int _coolDownLeft = 0;
        private Timer _timer = null;
        private bool _isCasting;
        private object _sync = new object();

        public string StringID { get => _stringId; }
        public AbilityTemplate Template { get => _template; }

        public bool IsOnCooldown() => _timer.Enabled;
        public int GetCoolDownLeft() => _coolDownLeft;

        public Ability(AbilityTemplate template)
        {
            _stringId = template.StringID;
            _template = template;
            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.Enabled = false;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _coolDownLeft -= 1;
            if (_coolDownLeft <= 0)
            {
                _coolDownLeft = 0;
                _timer.Stop();
            }
        }

        protected virtual bool CanCast(Agent casterAgent)
        {
            return casterAgent.IsActive() && casterAgent.Health > 0 && (casterAgent.GetMorale() > 1 || casterAgent.IsPlayerControlled) && casterAgent.IsAbilityUser() && !IsOnCooldown() && !_isCasting;
        }

        protected static MatrixFrame UpdateFrameRotationForAI(Agent casterAgent, MatrixFrame frame)
        {
            var wizardAiComponent = casterAgent.GetComponent<WizardAIComponent>();
            if (wizardAiComponent != null)
            {
                frame.rotation = wizardAiComponent.SpellTargetRotation;
            }

            return frame;
        }

        private void SetAnimationAction(Agent casterAgent)
        {
            if (_template.AnimationActionName != "none")
            {
                casterAgent.SetActionChannel(1, ActionIndexCache.Create(_template.AnimationActionName));
            }
        }

        public virtual void Cast(Agent casterAgent)
        {
            if (CanCast(casterAgent))
            {
                SetAnimationAction(casterAgent);
                if (_template.CastType == CastType.Instant)
                {
                    FireAbility(casterAgent);
                }
                else if(_template.CastType == CastType.WindUp)
                {
                    _isCasting = true;
                    var timer = new Timer(_template.CastTime * 1000);
                    timer.AutoReset = false;
                    timer.Elapsed += (s, e) =>
                    {
                        lock(_sync)
                        {
                            FireAbility(casterAgent);
                        }
                    };
                    timer.Start();
                }
            }
        }

        public virtual void FireAbility(Agent casterAgent)
        {
            _isCasting = false;
            _coolDownLeft = Template.CoolDown;
            _timer.Start();

            //position and spawn
            var scene = Mission.Current.Scene;
            var offset = 1f;
            var frame = casterAgent.LookFrame.Elevate(casterAgent.GetEyeGlobalHeight());
            frame = UpdateFrameRotationForAI(casterAgent, frame);
          
            GameEntity entity = null;
            if(Template.ParticleEffectPrefab != "none")
            {
                entity = GameEntity.Instantiate(scene, Template.ParticleEffectPrefab, true);
            }
            if(entity == null)
            {
                entity = GameEntity.CreateEmpty(scene);
            }
       

            //add light - optional
            if (Template.HasLight)
            {
                var light = Light.CreatePointLight(Template.LightRadius);
                light.Intensity = Template.LightIntensity;
                light.LightColor = Template.LightColorRGB;
                light.SetShadowType(Light.ShadowType.DynamicShadow);
                light.ShadowEnabled = Template.ShadowCastEnabled;
                light.SetLightFlicker(Template.LightFlickeringMagnitude, Template.LightFlickeringInterval);
                light.Frame = MatrixFrame.Identity;
                light.SetVisibility(true);
                entity.AddLight(light);
            }

            //Physics
            var mass = 1;
            entity.AddSphereAsBody(Vec3.Zero, Template.Radius, BodyFlags.Moveable);
            entity.AddPhysics(mass, entity.CenterOfMass, entity.GetBodyShape(), Vec3.Zero, Vec3.Zero, PhysicsMaterial.GetFromName("missile"), false, -1);
            entity.SetPhysicsState(true, false);

            //behaviour
            if (Template.AbilityEffectType == AbilityEffectType.MovingProjectile)
            {
                entity.CreateAndAddScriptComponent("MovingProjectileScript");
                MovingProjectileScript script = entity.GetFirstScriptOfType<MovingProjectileScript>();
                script.Initialize(this);
                script.SetAgent(casterAgent);
                entity.CallScriptCallbacks();
            }
            if (Template.AbilityEffectType == AbilityEffectType.DirectionalMovingAOE)
            {
                frame.origin.z = casterAgent.Position.z;
                entity.CreateAndAddScriptComponent("DirectionalMovingAOEScript");
                DirectionalMovingAOEScript script = entity.GetFirstScriptOfType<DirectionalMovingAOEScript>();
                script.Initialize(this);
                script.SetAgent(casterAgent);
                entity.CallScriptCallbacks();
            }
            frame = frame.Advance(offset);
            entity.SetGlobalFrame(frame);
            //and so on for the rest of the behaviour implementations. Based on AbilityEffectType enum
        }

        public void Dispose()
        {
            _timer.Dispose();
            _timer = null;
            _template = null;
        }
    }
}