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
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Abilities
{
    public abstract class Ability : IDisposable
    {
        private int _coolDownLeft = 0;
        private Timer _timer = null;
        private bool _isCasting;
        private object _sync = new object();
        private float _cooldown_end_time;

        public bool IsCasting => _isCasting;

        public string StringID { get; }

        public AbilityTemplate Template { get; private set; }

        public AbilityScript AbilityScript { get; private set; }

        public AbilityCrosshair Crosshair { get; private set; }
        public virtual AbilityEffectType AbilityEffectType => Template.AbilityEffectType;
        public bool IsOnCooldown() => _timer.Enabled;

        public int GetCoolDownLeft() => _coolDownLeft;

        private bool IsSingleTarget() => Template.AbilityTargetType == AbilityTargetType.SingleAlly || Template.AbilityTargetType == AbilityTargetType.SingleEnemy;

        public delegate void OnCastCompleteHandler(Ability ability);
        public event OnCastCompleteHandler OnCastComplete;
        public delegate void OnCastStartHandler(Ability ability);
        public event OnCastStartHandler OnCastStart;

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

        public virtual bool CanCast(Agent casterAgent)
        {
            return !_isCasting &&
                   !IsOnCooldown() &&
                   ((casterAgent.IsPlayerControlled && IsRightAngleToCast()) || 
                   (casterAgent.IsActive() && casterAgent.Health > 0 && casterAgent.GetMorale() > 1 && casterAgent.IsAbilityUser()));
        }

        protected virtual void DoCast(Agent casterAgent)
        {
            OnCastStart?.Invoke(this);
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

            GameEntity parentEntity = GameEntity.CreateEmpty(Mission.Current.Scene, false);
            parentEntity.SetGlobalFrame(frame);

            AddLight(ref parentEntity);

            if (ShouldAddPhyics())
                AddPhysics(ref parentEntity);

            AddBehaviour(ref parentEntity, casterAgent);
            OnCastComplete?.Invoke(this);
        }

        private bool IsGroundAbility()
        {
            return Template.AbilityTargetType == AbilityTargetType.GroundAtPosition;
        }

        private bool IsMissileAbility()
        {
            return Template.AbilityEffectType == AbilityEffectType.SeekerMissile ||
                   Template.AbilityEffectType == AbilityEffectType.Missile;
        }

        private bool ShouldAddPhyics()
        {
            return Template.TriggerType == TriggerType.OnCollision ;
        }

        protected virtual MatrixFrame GetSpawnFrame(Agent casterAgent)
        {
            var frame = casterAgent.LookFrame;
            if (casterAgent.IsAIControlled)
            {
                frame = UpdateFrameRotationForAI(casterAgent, frame);
                if (IsGroundAbility())
                {
                    frame.origin.z = Mission.Current.Scene.GetGroundHeightAtPosition(frame.origin);
                    if(Template.AbilityEffectType == AbilityEffectType.Bombardment)
                    {
                        frame.origin.z += Template.Offset;
                    }
                }
                else if (IsMissileAbility())
                {
                    frame = frame.Elevate(casterAgent.GetEyeGlobalHeight()).Advance(Template.Offset);
                }
                else
                {
                    frame = frame.Elevate(1);
                }
            }
            else
            {
                switch (Template.AbilityEffectType)
                {
                    case AbilityEffectType.Missile:
                    case AbilityEffectType.SeekerMissile:
                    {
                        frame.origin = casterAgent.GetEyeGlobalPosition();
                        break;
                    }
                    case AbilityEffectType.Wind:
                        {
                            frame = Crosshair.Frame;
                            break;
                        }
                    case AbilityEffectType.Blast:
                        {
                            frame = Crosshair.Frame.Elevate(1);
                            break;
                        }
                    case AbilityEffectType.Vortex:
                    {
                        frame = Crosshair.Frame;
                        frame.rotation = casterAgent.Frame.rotation;
                        break;
                    }
                    case AbilityEffectType.AgentMoving:
                    {
                        frame = casterAgent.LookFrame;
                        break;
                    }
                    case AbilityEffectType.ArtilleryPlacement:
                    case AbilityEffectType.Hex:
                    case AbilityEffectType.Augment:
                    case AbilityEffectType.Heal:
                    case AbilityEffectType.Summoning:
                        {
                            frame = new MatrixFrame(Mat3.Identity, Crosshair.Position);
                            break;
                        }
                    case AbilityEffectType.Bombardment:
                        {
                            frame = new MatrixFrame(Mat3.Identity, Crosshair.Position);
                            frame.origin.z += Template.Offset;
                            break;
                        }
                    default:
                        break;
                }
            }

            return frame;
        }

        protected MatrixFrame UpdateFrameRotationForAI(Agent casterAgent, MatrixFrame frame)
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
                case AbilityEffectType.SeekerMissile:
                case AbilityEffectType.Missile:
                    AddExactBehaviour<MissileScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Wind:
                    AddExactBehaviour<WindScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Heal:
                    AddExactBehaviour<HealScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Augment:
                    AddExactBehaviour<AugmentScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Summoning:
                    AddExactBehaviour<SummoningScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.AgentMoving:
                    AddExactBehaviour<ShadowStepScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Hex:
                    AddExactBehaviour<HexScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Vortex:
                    AddExactBehaviour<VortexScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Blast:
                    AddExactBehaviour<BlastScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.Bombardment:
                    AddExactBehaviour<BombardmentScript>(entity, casterAgent);
                    break;
                case AbilityEffectType.ArtilleryPlacement:
                    AddExactBehaviour<ArtilleryPlacementScript>(entity, casterAgent);
                    break;
            }

            if (IsSingleTarget())
            {
                if (casterAgent.IsAIControlled)
                {
                    //TODO get logic for selecting single targets for AI
                    //AbilityScript.SetTargetSeeking(casterAgent.GetComponent<WizardAIComponent>().CurrentCastingBehavior.CurrentTarget, Template.SeekerParameters);
                }
                else if(Crosshair.CrosshairType == CrosshairType.SingleTarget)
                {
                    AbilityScript.SetExplicitSingleTarget((Crosshair as SingleTargetCrosshair).CachedTarget);
                }
            }

            if (Template.SeekerParameters != null)
            {
                if (casterAgent.IsAIControlled)
                {
                    AbilityScript.SetTargetSeeking(casterAgent.GetComponent<WizardAIComponent>().CurrentCastingBehavior.CurrentTarget, Template.SeekerParameters);
                }
                else
                {
                    Target target;
                    if (Crosshair.CrosshairType == CrosshairType.SingleTarget)
                    {
                        target = new Target { Agent = (Crosshair as SingleTargetCrosshair).CachedTarget };
                    }
                    else
                    {
                        target = new Target { Formation = casterAgent.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation };
                    }
                    AbilityScript.SetTargetSeeking(target, Template.SeekerParameters);
                }
            }
        }

        private void AddExactBehaviour<TAbilityScript>(GameEntity parentEntity, Agent casterAgent)
            where TAbilityScript : AbilityScript
        {
            parentEntity.CreateAndAddScriptComponent(typeof(TAbilityScript).Name+parentEntity.GetGuid());
            AbilityScript = parentEntity.GetFirstScriptOfType<TAbilityScript>();
            var prefabEntity = SpawnEntity();
            parentEntity.AddChild(prefabEntity);
            AbilityScript?.Initialize(this);
            AbilityScript?.SetAgent(casterAgent);
            parentEntity.CallScriptCallbacks();
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
            OnCastComplete = null;
            OnCastStart = null;
        }

        private bool IsRightAngleToCast()
        {
            if (Agent.Main.HasMount)
            {
                double xa = Agent.Main.LookDirection.X;
                double ya = Agent.Main.LookDirection.Y;
                double xb = Agent.Main.GetMovementDirection().X;
                double yb = Agent.Main.GetMovementDirection().Y;

                double angle = Math.Acos((xa * xb + ya * yb) / (Math.Sqrt(Math.Pow(xa, 2) + Math.Pow(ya, 2)) * Math.Sqrt(Math.Pow(xb, 2) + Math.Pow(yb, 2))));

                return true ? angle < 1.4 : angle >= 1.4;
            }
            else
            {
                return true;
            }
        }

    }
}