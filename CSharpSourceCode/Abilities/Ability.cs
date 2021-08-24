﻿using System;
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
        private string _stringId;
        private AbilityTemplate _template;
        private int _coolDownLeft = 0;
        private Timer _timer = null;
        private bool _isCasting;
        private object _sync = new object();
        private AbilityCrosshair _crosshair;

        public string StringID { get => _stringId; }
        public AbilityTemplate Template { get => _template; }
        public AbilityCrosshair Crosshair { get => _crosshair; }

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

            var entity = SpawnEntity();
            entity.SetGlobalFrame(frame);

            AddLight(ref entity);

            if (IsDynamicAbility())
                AddPhysics(ref entity);

            AddBehaviour(ref entity, casterAgent);
        }

        private bool IsGroundAbility()
        {
            return Template.AbilityEffectType == AbilityEffectType.DirectionalMovingAOE || Template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE || Template.AbilityEffectType == AbilityEffectType.TargetedStaticAOE || Template.AbilityEffectType == AbilityEffectType.RandomMovingAOE;
        }

        private bool IsDynamicAbility()
        {
            return Template.AbilityEffectType == AbilityEffectType.DynamicProjectile ||
                   Template.AbilityEffectType == AbilityEffectType.MovingProjectile ||
                   Template.AbilityEffectType == AbilityEffectType.DirectionalMovingAOE ||
                   Template.AbilityEffectType == AbilityEffectType.RandomMovingAOE;
        }

        protected virtual MatrixFrame GetSpawnFrame(Agent casterAgent)
        {
            var frame = casterAgent.LookFrame;
            if (casterAgent.IsAIControlled)
            {
                if (_template.AbilityEffectType == AbilityEffectType.CenteredStaticAOE)
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
                    else if (_template.AbilityEffectType == AbilityEffectType.MovingProjectile || _template.AbilityEffectType == AbilityEffectType.DynamicProjectile)
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
                switch (_template.AbilityEffectType)
                {
                    case AbilityEffectType.MovingProjectile:
                    case AbilityEffectType.DynamicProjectile:
                        {
                            frame = frame.Elevate(casterAgent.GetEyeGlobalHeight());
                            break;
                        }
                    case AbilityEffectType.DirectionalMovingAOE:
                        {
                            frame = Crosshair.Frame;
                            break;
                        }
                    case AbilityEffectType.CenteredStaticAOE:
                        {
                            frame = casterAgent.AgentVisuals.GetGlobalFrame();
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
            entity.AddSphereAsBody(Vec3.Zero, Template.Radius, BodyFlags.Moveable);
            entity.AddPhysics(mass, entity.CenterOfMass, entity.GetBodyShape(), Vec3.Zero, Vec3.Zero, PhysicsMaterial.GetFromName("missile"), false, -1);
            entity.SetPhysicsState(true, false);
        }

        private void AddBehaviour(ref GameEntity entity, Agent casterAgent)
        {
            AbilityScript script = null;
            switch (Template.AbilityEffectType)
            {
                case AbilityEffectType.MovingProjectile:
                    {
                        entity.CreateAndAddScriptComponent("MovingProjectileScript");
                        script = entity.GetFirstScriptOfType<MovingProjectileScript>();
                        break;
                    }
                case AbilityEffectType.DirectionalMovingAOE:
                    {
                        entity.CreateAndAddScriptComponent("DirectionalMovingAOEScript");
                        script = entity.GetFirstScriptOfType<DirectionalMovingAOEScript>();
                        break;
                    }
                case AbilityEffectType.CenteredStaticAOE:
                    {
                        entity.CreateAndAddScriptComponent("CenteredStaticAOEScript");
                        script = entity.GetFirstScriptOfType<CenteredStaticAOEScript>();
                        break;
                    }
                case AbilityEffectType.TargetedStaticAOE:
                    {
                        entity.CreateAndAddScriptComponent("TargetedStaticAOEScript");
                        script = entity.GetFirstScriptOfType<TargetedStaticAOEScript>();
                        break;
                    }
                case AbilityEffectType.Summoning:
                    {
                        entity.CreateAndAddScriptComponent("SummoningScript");
                        script = entity.GetFirstScriptOfType<SummoningScript>();
                        break;
                    }
            }
            if (script != null)
            {
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

        public void SetCrosshair(AbilityCrosshair crosshair)
        {
            this._crosshair = crosshair;
        }

        public void Dispose()
        {
            _timer.Dispose();
            _timer = null;
            _template = null;
        }
    }
}