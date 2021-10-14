using System;
using TaleWorlds.MountAndBlade;
using System.Timers;
using TaleWorlds.Library;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities.Extensions;
using TaleWorlds.Engine;
using TOW_Core.Abilities.Scripts;
using Timer = System.Timers.Timer;
using TOW_Core.Abilities.Crosshairs;

namespace TOW_Core.Abilities
{
    public abstract class Ability : IDisposable
    {
        private int _coolDownLeft = 0;
        private Timer _timer = null;
        private bool _isCasting;
        private object _sync = new object();
        private float _cooldown_end_time;

        public string StringID { get; }

        public AbilityTemplate Template { get; private set; }

        public AbilityScript AbilityScript { get; private set; }

        public AbilityCrosshair Crosshair { get; private set; }

        public bool IsOnCooldown() => _timer.Enabled;

        public int GetCoolDownLeft() => _coolDownLeft;

        public Ability(AbilityTemplate template)
        {
            StringID = template.StringID;
            Template = template;
            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.Enabled = false;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Mission.Current == null)
            {
                FinalizeTimer();
                return;
            }

            _coolDownLeft = (int) (_cooldown_end_time - Mission.Current.CurrentTime);
            if (_coolDownLeft <= 0)
            {
                FinalizeTimer();
            }
        }

        private void FinalizeTimer()
        {
            _coolDownLeft = 0;
            _timer.Stop();
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

        public virtual bool CanCast(Agent casterAgent)
        {
            return casterAgent.IsActive() && casterAgent.Health > 0 && (casterAgent.GetMorale() > 1 || casterAgent.IsPlayerControlled) && casterAgent.IsAbilityUser() && !IsOnCooldown() && !_isCasting;
        }

        protected virtual void DoCast(Agent casterAgent)
        {
            SetAnimationAction(casterAgent);
            if (Template.CastType == CastType.Instant)
            {
                ActivateAbility(casterAgent);
            }
            else if (Template.CastType == CastType.WindUp)
            {
                _isCasting = true;
                var timer = new Timer(Template.CastTime * 1000);
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
            _cooldown_end_time = Mission.Current.CurrentTime + _coolDownLeft + 0.8f; //Adjustment was needed for natural tick on UI
            _timer.Start();

            var frame = GetSpawnFrame(casterAgent);

            var entity = SpawnEntity();
            entity.SetGlobalFrame(frame);

            AddLight(ref entity);

            if (IsDynamicAbility())
                AddPhysics(ref entity);

            AddBehaviour(ref entity, casterAgent);
        }

        private bool IsGroundAbility()
        {
            return Template.AbilityEffectType == AbilityEffectType.DirectionalMovingAOE
                   || Template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE
                   || Template.AbilityEffectType == AbilityEffectType.TargetedStaticAOE
                   || Template.AbilityEffectType == AbilityEffectType.RandomMovingAOE
                   || Template.AbilityEffectType == AbilityEffectType.Summoning
                   || Template.AbilityEffectType == AbilityEffectType.AgentMoving;
        }

        private bool IsDynamicAbility()
        {
            return Template.AbilityEffectType == AbilityEffectType.DynamicProjectile ||
                   Template.AbilityEffectType == AbilityEffectType.MovingProjectile;
        }

        protected virtual MatrixFrame GetSpawnFrame(Agent casterAgent)
        {
            var frame = casterAgent.LookFrame;
            if (casterAgent.IsAIControlled)
            {
                if (Template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE)
                {
                    frame = casterAgent.AgentVisuals.GetGlobalFrame();
                }
                else
                {
                    frame = UpdateFrameRotationForAI(casterAgent, frame);
                    frame = frame.Advance(Template.Offset);
                    if (IsGroundAbility())
                    {
                        frame.origin.z = Mission.Current.Scene.GetGroundHeightAtPosition(frame.origin);
                    }
                    else if (IsDynamicAbility())
                    {
                        frame = frame.Elevate(casterAgent.GetEyeGlobalHeight());
                    }
                    else
                    {
                        frame = frame.Elevate(1);
                    }
                }
            }
            else
            {
                switch (Template.AbilityEffectType)
                {
                    case AbilityEffectType.MovingProjectile:
                    case AbilityEffectType.DynamicProjectile:
                        {
                            frame.origin = casterAgent.GetEyeGlobalPosition();
                            break;
                        }
                    case AbilityEffectType.DirectionalMovingAOE:
                        {
                            frame = Crosshair.Frame;
                            break;
                        }
                    case AbilityEffectType.CenteredStaticAOE:
                    case AbilityEffectType.AgentMoving:
                        {
                            frame = casterAgent.LookFrame;
                            break;
                        }
                    case AbilityEffectType.TargetedStaticAOE:
                    case AbilityEffectType.TargetedStatic:
                        {
                            //frame = crosshair.Target.GetFrame();
                            break;
                        }
                    case AbilityEffectType.Summoning:
                        {
                            frame = new MatrixFrame(Mat3.Identity, Crosshair.Position);
                            break;
                        }
                }
            }

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

        private GameEntity SpawnEntity()
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
            entity.AddSphereAsBody(Vec3.Zero, Template.Radius, BodyFlags.Dynamic);
            entity.AddPhysics(mass, entity.CenterOfMass, entity.GetBodyShape(), Vec3.Zero, Vec3.Zero, PhysicsMaterial.GetFromName("missile"), false, 1);
            entity.SetPhysicsState(true, false);
        }

        private void AddBehaviour(ref GameEntity entity, Agent casterAgent)
        {
            switch (Template.AbilityEffectType)
            {
                case AbilityEffectType.MovingProjectile:
                    AddExactBehaviour<MovingProjectileScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.DirectionalMovingAOE:
                    AddExactBehaviour<DirectionalMovingAOEScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.CenteredStaticAOE:
                    AddExactBehaviour<CenteredStaticAOEScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.TargetedStaticAOE:
                    AddExactBehaviour<TargetedStaticAOEScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Summoning:
                    AddExactBehaviour<SummoningScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.AgentMoving:
                    AddExactBehaviour<SpecialMoveScript>(entity, casterAgent);
                    break;
            }
        }

        private void AddExactBehaviour<TAbilityScript>(GameEntity entity, Agent casterAgent)
            where TAbilityScript : AbilityScript
        {
            entity.CreateAndAddScriptComponent(typeof(TAbilityScript).Name);
            AbilityScript = entity.GetFirstScriptOfType<TAbilityScript>();
            AbilityScript?.Initialize(this);
            AbilityScript?.SetAgent(casterAgent);
            entity.CallScriptCallbacks();
        }

        private void SetAnimationAction(Agent casterAgent)
        {
            if (Template.AnimationActionName != "none")
            {
                casterAgent.SetActionChannel(1, ActionIndexCache.Create(Template.AnimationActionName));
            }
        }

        public void SetCrosshair(AbilityCrosshair crosshair)
        {
            Crosshair = crosshair;
        }

        public void Dispose()
        {
            _timer.Dispose();
            _timer = null;
            Template = null;
        }
    }
}