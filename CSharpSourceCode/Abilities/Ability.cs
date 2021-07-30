using System;
using TaleWorlds.MountAndBlade;
using System.Timers;
using TaleWorlds.Library;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities.Extensions;
using TaleWorlds.Engine;
using TOW_Core.Abilities.Scripts;
using Timer = System.Timers.Timer;

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

        public virtual void TryCast(Agent casterAgent)
        {
            if (CanCast(casterAgent))
            {
                DoCast(casterAgent);
            }
        }

        public virtual AbilityEffectType GetAbilityEffectType()
        {
            return this.Template.AbilityEffectType;
        }

        protected virtual bool CanCast(Agent casterAgent)
        {
            return casterAgent.IsActive() && casterAgent.Health > 0 && (casterAgent.GetMorale() > 1 || casterAgent.IsPlayerControlled) && casterAgent.IsAbilityUser() && !IsOnCooldown() && !_isCasting;
        }

        protected virtual void DoCast(Agent casterAgent)
        {
            SetAnimationAction(casterAgent);
            if (_template.CastType == CastType.Instant)
            {
                ActivateAbility(casterAgent);
            }
            else if (_template.CastType == CastType.WindUp)
            {
                _isCasting = true;
                var timer = new Timer(_template.CastTime * 1000);
                timer.AutoReset = false;
                timer.Elapsed += (s, e) =>
                {
                    lock (_sync)
                    {
                        ActivateAbility(casterAgent);
                    }
                };
                timer.Start();
            }
        }

        public virtual void ActivateAbility(Agent casterAgent)
        {
            _isCasting = false;
            _coolDownLeft = Template.CoolDown;
            _timer.Start();

            var frame = GetSpawnFrame(casterAgent);

            var entity = SpawnEntity(frame);

            AddLight(ref entity);

            AddPhysics(ref entity);

            AddBehaviour(ref entity, casterAgent);
            
            frame = frame.Advance(Template.Offset);
            
            if (IsGroundAbility())
            {
                frame.origin.z = Mission.Current.Scene.GetGroundHeightAtPosition(frame.origin);
            }

            entity.SetGlobalFrame(frame);
        }

        private bool IsGroundAbility()
        {
            return Template.AbilityEffectType == AbilityEffectType.DirectionalMovingAOE || Template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE;
        }

        private MatrixFrame GetSpawnFrame(Agent casterAgent)
        {
            var frame = casterAgent.LookFrame.Elevate(casterAgent.GetEyeGlobalHeight());
            frame = UpdateFrameRotationForAI(casterAgent, frame);
            return frame;
        }

        protected static MatrixFrame UpdateFrameRotationForAI(Agent casterAgent, MatrixFrame frame)
        {
            var wizardAiComponent = casterAgent.GetComponent<WizardAIComponent>();
            if (wizardAiComponent != null && casterAgent.IsAIControlled)
            {
                frame.rotation = wizardAiComponent.SpellTargetRotation;
            }

            return frame;
        }

        private GameEntity SpawnEntity(MatrixFrame frame)
        {
            GameEntity entity = null;
            if (Template.ParticleEffectPrefab != "none")
            {
                entity = GameEntity.Instantiate(Mission.Current.Scene, Template.ParticleEffectPrefab, true);
            }

            if (entity == null)
            {
                entity = GameEntity.CreateEmpty(Mission.Current.Scene);
            }

            return entity;
        }

        private void AddLight(ref GameEntity entity)
        {
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
        }

        private void AddPhysics(ref GameEntity entity)
        {
            var mass = 1;
            entity.AddSphereAsBody(Vec3.Zero, Template.Radius, BodyFlags.Moveable);
            entity.AddPhysics(mass, entity.CenterOfMass, entity.GetBodyShape(), Vec3.Zero, Vec3.Zero, PhysicsMaterial.GetFromName("missile"), false, -1);
            entity.SetPhysicsState(true, false);
        }

        private void AddBehaviour(ref GameEntity entity, Agent casterAgent)
        {
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
                entity.CreateAndAddScriptComponent("DirectionalMovingAOEScript");
                DirectionalMovingAOEScript script = entity.GetFirstScriptOfType<DirectionalMovingAOEScript>();
                script.Initialize(this);
                script.SetAgent(casterAgent);
                entity.CallScriptCallbacks();
            }

            if (Template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE)
            {
                entity.CreateAndAddScriptComponent("CenteredStaticAOEScript");
                CenteredStaticAOEScript script = entity.GetFirstScriptOfType<CenteredStaticAOEScript>();
                script.Initialize(this);
                script.SetAgent(casterAgent);
                entity.CallScriptCallbacks();
            }
            //and so on for the rest of the behaviour implementations. Based on AbilityEffectType enum
        }

        private void SetAnimationAction(Agent casterAgent)
        {
            if (_template.AnimationActionName != "none")
            {
                casterAgent.SetActionChannel(1, ActionIndexCache.Create(_template.AnimationActionName));
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
            _timer = null;
            _template = null;
        }
    }
}