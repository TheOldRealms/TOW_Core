using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;
using TOW_Core.Battle.AI.AgentBehavior.Components;
using TOW_Core.Battle.TriggeredEffect.Scripts;

namespace TOW_Core.Battle.Artillery
{
    public class ArtilleryRangedSiegeWeapon : RangedSiegeWeapon
    {
        #region animations
        private ActionIndexCache _idleAnimationActionIndex;
        private ActionIndexCache _shootAnimationActionIndex;
        private ActionIndexCache _reload1AnimationActionIndex;
        private ActionIndexCache _reload2AnimationActionIndex;
        private ActionIndexCache _rotateLeftAnimationActionIndex;
        private ActionIndexCache _rotateRightAnimationActionIndex;
        private ActionIndexCache _loadAmmoBeginAnimationActionIndex;
        private ActionIndexCache _loadAmmoEndAnimationActionIndex;
        private ActionIndexCache _reload2IdleActionIndex;
        private static readonly ActionIndexCache act_pickup_boulder_begin = ActionIndexCache.Create("act_pickup_boulder_begin");
        private static readonly ActionIndexCache act_pickup_boulder_end = ActionIndexCache.Create("act_pickup_boulder_end");

        public string IdleActionName;
        public string ShootActionName;
        public string Reload1ActionName;
        public string Reload2ActionName;
        public string RotateLeftActionName;
        public string RotateRightActionName;
        public string LoadAmmoBeginActionName;
        public string LoadAmmoEndActionName;
        public string Reload2IdleActionName;
        #endregion

        private readonly string _barrelTag = "Barrel";
        private readonly string _baseTag = "Battery_Base";
        private readonly string _leftWheelTag = "Wheel_L";
        private readonly string _rightWheelTag = "Wheel_R";
        public string FireSoundID = "mortar_shot_1";
        public string FireSoundID2 = "mortar_shot_2";
        public float RecoilDuration = 0.1f;
        public float Recoil2Duration = 0.8f;
        public string DisplayName = "Artillery";
        public float BaseMuzzleVelocity = 40f;
        public bool UsesCalculatedMuzzleVelocity = false;
        private int _fireSoundIndex;
        private int _fireSoundIndex2;
        private SynchedMissionObject _body;
        private SynchedMissionObject _barrel;
        private SynchedMissionObject _wheel_R;
        private SynchedMissionObject _wheel_L;
        private float _verticalOffsetAngle;
        private MatrixFrame _barrelInitialLocalFrame;
        private BattleSideEnum _side;
        private Agent _lastLoaderAgent;
        private StandingPoint _waitStandingPoint;
        private Threat _target;
        public Threat Target => _target;
        private Timer _timer;
        private SoundEvent _fireSound;
        private MatrixFrame _currentSlideBackFrameOrig;
        private MatrixFrame _currentSlideBackFrame;
        private float _lastRecoilTimeStart;
        private float _currentRecoilTimer;
        private bool _isRotating;
        private int _rotationDirection = 0;
        private float _lastCurrentDirection;

        protected override float ShootingSpeed
        {
            get
            {
                if (UsesCalculatedMuzzleVelocity)
                {
                    var cheat = MathF.Sqrt(TargetDistance) * 3.1415f;
                    return MathF.Max(10f, cheat + TargetDistance * 0.1f);
                }
                else return BaseMuzzleVelocity;
            }
        }
        protected override Vec3 ShootingDirection => Projectile.GameEntity.GetGlobalFrame().rotation.f;
        protected override Vec3 VisualizationShootingDirection => ShootingDirection;
        public override BattleSideEnum Side => _side;
        public void SetSide(BattleSideEnum side) => _side = side;
        public Team Team { get; internal set; }
        protected override float MaximumBallisticError => 0.2f;
        private float TargetDistance
        {
            get
            {
                if (Target == null) return 0;
                else
                {
                    return (Target.Position - MissleStartingPositionForSimulation).Length;
                }
            }
        }

        public override UsableMachineAIBase CreateAIBehaviorObject()
        {
            return new ArtilleryAI(this);
        }
        protected override void OnInit()
        {
            CollectEntities();
            base.OnInit();
            Projectile.SetVisibleSynched(false);
            if(MissileStartingPositionEntityForSimulation == null)
            {
                List<GameEntity> entities = new List<GameEntity>();
                GameEntity.GetChildrenRecursive(ref entities);
                MissileStartingPositionEntityForSimulation = Enumerable.FirstOrDefault(entities, x => x.Name == "projectile_leaving_position");
            }
            foreach(var sp in StandingPoints)
            {
                if (sp.GameEntity.HasTag(WaitStandingPointTag))
                {
                    _waitStandingPoint = sp;
                    break;
                }
            }
            timeGapBetweenShootActionAndProjectileLeaving = 0f;
            timeGapBetweenShootingEndAndReloadingStart = 0f;
            EnemyRangeToStopUsing = 5f;
            PilotStandingPoint.AddComponent(new ClearHandInverseKinematicsOnStopUsageComponent());
            _barrelInitialLocalFrame = _barrel.GameEntity.GetFrame();
            Vec3 v = new Vec3(0f, ShootingDirection.AsVec2.Length, ShootingDirection.Z);
            _verticalOffsetAngle = Vec3.AngleBetweenTwoVectors(v, Vec3.Forward);
            _lastCurrentDirection = currentDirection;
            ApplyAimChange();
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            HandleAnimations();
            HandleAmmoPickup();
            HandleAmmoLoad();
            ForceAmmoPointUsage();
            HandleWaitingTimer();
            UpdateRecoilEffect(dt);
            UpdateWheelRotation(dt);
        }

        private void HandleWaitingTimer()
        {
            if(State == WeaponState.WaitingBeforeIdle)
            {
                if(_timer != null && _timer.Check(Mission.Current.CurrentTime))
                {
                    _timer = null;
                    State = WeaponState.Idle;
                }
            }
        }

        private void HandleAnimations()
        {
            return;
        }

        private void HandleAmmoLoad()
        {
            if (LoadAmmoStandingPoint != null && LoadAmmoStandingPoint.HasUser)
            {
                var user = LoadAmmoStandingPoint.UserAgent;
                _lastLoaderAgent = user;
                if (user.GetCurrentAction(1) == _loadAmmoEndAnimationActionIndex)
                {
                    EquipmentIndex wieldedItemIndex = user.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                    if (wieldedItemIndex != EquipmentIndex.None && user.Equipment[wieldedItemIndex].CurrentUsageItem.WeaponClass == OriginalMissileItem.PrimaryWeapon.WeaponClass)
                    {
                        user.RemoveEquippedWeapon(wieldedItemIndex);
                        user.StopUsingGameObject(true, false);
                        State = WeaponState.WaitingBeforeIdle;
                    }
                    user.StopUsingGameObject(true, true);
                }
                else
                {
                    if (user.GetCurrentAction(1) != _loadAmmoBeginAnimationActionIndex && !LoadAmmoStandingPoint.UserAgent.SetActionChannel(1, _loadAmmoBeginAnimationActionIndex))
                    {
                        for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                        {
                            if (!user.Equipment[equipmentIndex].IsEmpty && user.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == OriginalMissileItem.PrimaryWeapon.WeaponClass)
                            {
                                user.RemoveEquippedWeapon(equipmentIndex);
                            }
                        }
                        user.StopUsingGameObject(true, true);
                    }
                }
            }
        }

        private void HandleAmmoPickup()
        {
            foreach (var sp in AmmoPickUpPoints)
            {
                if (sp is StandingPointWithWeaponRequirement)
                {
                    var point = sp as StandingPointWithWeaponRequirement;
                    if (point.HasUser)
                    {
                        var user = point.UserAgent;
                        var action = user.GetCurrentAction(1);
                        if (!(action == act_pickup_boulder_begin))
                        {
                            if (action == act_pickup_boulder_end)
                            {
                                MissionWeapon missionWeapon = new MissionWeapon(LoadedMissileItem, null, null, 1);
                                user.EquipWeaponToExtraSlotAndWield(ref missionWeapon);
                                user.StopUsingGameObject(true, false);
                                if (user.IsAIControlled)
                                {
                                    if (!LoadAmmoStandingPoint.HasUser && !LoadAmmoStandingPoint.IsDeactivated)
                                    {
                                        user.AIMoveToGameObjectEnable(LoadAmmoStandingPoint, this, Agent.AIScriptedFrameFlags.NoAttack);
                                    }
                                    else if (ReloaderAgentOriginalPoint != null && !ReloaderAgentOriginalPoint.HasUser && !ReloaderAgentOriginalPoint.HasAIMovingTo)
                                    {
                                        user.AIMoveToGameObjectEnable(ReloaderAgentOriginalPoint, this, Agent.AIScriptedFrameFlags.NoAttack);
                                    }
                                    else
                                    {
                                        Agent reloaderAgent = ReloaderAgent;
                                        if (reloaderAgent != null)
                                        {
                                            Formation formation = reloaderAgent.Formation;
                                            if (formation != null)
                                            {
                                                formation.AttachUnit(ReloaderAgent);
                                            }
                                        }
                                        ReloaderAgent = null;
                                    }
                                }
                            }
                            else if (!user.SetActionChannel(1, act_pickup_boulder_begin))
                            {
                                user.StopUsingGameObject(true, true);
                            }
                        }
                    }
                }
            }
        }

        private void ForceAmmoPointUsage()
        {
            if(State == WeaponState.LoadingAmmo && !LoadAmmoStandingPoint.HasUser && !LoadAmmoStandingPoint.HasAIMovingTo)
            {
                foreach (var sp in AmmoPickUpStandingPoints)
                {
                    if (sp.IsDeactivated) sp.SetIsDeactivatedSynched(false);
                }
            }
            else
            {
                foreach (var sp in AmmoPickUpStandingPoints)
                {
                    if (!sp.IsDeactivated) sp.SetIsDeactivatedSynched(true);
                }
            }
        }

        protected override void OnRangedSiegeWeaponStateChange()
        {
            base.OnRangedSiegeWeaponStateChange();
            switch (State)
            {
                case WeaponState.Shooting:
                    {
                        PlayFireProjectileEffects();
                        AddCannonballScript();
                        State = WeaponState.WaitingAfterShooting;
                        return;
                    }
                case WeaponState.WaitingAfterShooting:
                    {
                        DoSlideBack();
                        return;
                    }
                case WeaponState.WaitingBeforeIdle:
                    {
                        SendLoaderAgentToWaitingPoint();
                        SetWaitingTimer();
                        return;
                    }
                case WeaponState.LoadingAmmo:
                    {
                        SetActivationWaitingPoint(false);
                        return;
                    }
            }
        }

        private void SendLoaderAgentToWaitingPoint()
        {
            if(_lastLoaderAgent != null && _waitStandingPoint != null)
            {
                SetActivationWaitingPoint(true);
                _lastLoaderAgent.AIMoveToGameObjectEnable(_waitStandingPoint, this, Agent.AIScriptedFrameFlags.NoAttack);
            }
        }

        protected override void ApplyAimChange()
        {
            base.ApplyAimChange();
            MatrixFrame barrelFrame = _barrelInitialLocalFrame;
            barrelFrame.rotation.RotateAboutSide(-currentReleaseAngle + _verticalOffsetAngle);
            _barrel.GameEntity.SetFrame(ref barrelFrame);
        }

        protected override void ApplyCurrentDirectionToEntity()
        {
            if(_lastCurrentDirection != currentDirection)
            {
                _isRotating = true;
                if (currentDirection - _lastCurrentDirection > 0) _rotationDirection = 1;
                else if(currentDirection - _lastCurrentDirection < 0) _rotationDirection = -1;
                else _rotationDirection = 0;
            }
            else
            {
                _isRotating = false;
                _rotationDirection = 0;
            }
            base.ApplyCurrentDirectionToEntity();
            _lastCurrentDirection = currentDirection;
        }

        public override bool AimAtTarget(Vec3 target)
        {
            return base.AimAtTarget(target);
        }

        private void CollectEntities()
        {
            _body = GameEntity.CollectObjectsWithTag<SynchedMissionObject>(_baseTag)[0];
            _barrel = GameEntity.CollectObjectsWithTag<SynchedMissionObject>(_barrelTag)[0];
            _wheel_L = GameEntity.CollectObjectsWithTag<SynchedMissionObject>(_leftWheelTag)[0];
            _wheel_R = GameEntity.CollectObjectsWithTag<SynchedMissionObject>(_rightWheelTag)[0];
            RotationObject = _body;
        }

        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject;
            if (usableGameObject.GameEntity.HasTag(AmmoLoadTag))
            {
                textObject = new TextObject("{=Na81xuXn}{KEY} Reload");
            }
            else if (usableGameObject.GameEntity.HasTag(AmmoPickUpTag))
            {
                textObject = new TextObject("{=bNYm3K6b}{KEY} Pick Up");
            }
            else
            {
                textObject = new TextObject("{=fEQAPJ2e}{KEY} Use");
            }
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return new TextObject(DisplayName, null).ToString();
        }

        public override SiegeEngineType GetSiegeEngineType() => Side != BattleSideEnum.Attacker ? DefaultSiegeEngineTypes.Catapult : DefaultSiegeEngineTypes.Onager;

        public override TargetFlags GetTargetFlags()
        {
            TargetFlags targetFlags = (TargetFlags)(0 | 2 | 8 | 16);
            if (IsDestroyed || IsDeactivated)
                targetFlags |= TargetFlags.NotAThreat;
            if (Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToMangonels)
                targetFlags |= TargetFlags.DebugThreat;
            if (Side == BattleSideEnum.Defender && DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToMangonels)
                targetFlags |= TargetFlags.DebugThreat;
            return targetFlags;
        }

        public override float GetTargetValue(List<Vec3> weaponPos) => 40f * GetUserMultiplierOfWeapon() * GetDistanceMultiplierOfWeapon(weaponPos[0]) * GetHitPointMultiplierOfWeapon();

        public override float ProcessTargetValue(float baseValue, TargetFlags flags)
        {
            if (flags.HasAnyFlag(TargetFlags.NotAThreat))
            {
                return -1000f;
            }
            if (flags.HasAnyFlag(TargetFlags.IsSiegeEngine))
            {
                baseValue *= 0.2f;
            }
            if (flags.HasAnyFlag(TargetFlags.IsStructure))
            {
                baseValue *= 0.05f;
            }
            if (flags.HasAnyFlag(TargetFlags.DebugThreat))
            {
                baseValue *= 10000f;
            }
            return baseValue;
        }

        protected override void GetSoundEventIndices()
        {
            MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/move");
            _fireSoundIndex = SoundEvent.GetEventIdFromString(FireSoundID);
            _fireSoundIndex2 = SoundEvent.GetEventIdFromString(FireSoundID2);
        }

        protected override void SetActivationLoadAmmoPoint(bool activate)
        {
            LoadAmmoStandingPoint.SetIsDeactivatedSynched(!activate);
        }

        private void SetActivationWaitingPoint(bool activate)
        {
            _waitStandingPoint.SetIsDeactivatedSynched(!activate);
        }

        private void SetWaitingTimer()
        {
            _timer = new Timer(Mission.Current.CurrentTime, 2f, false);
        }

        protected override void RegisterAnimationParameters()
        {
            SkeletonOwnerObjects = new SynchedMissionObject[0];
            _idleAnimationActionIndex = ActionIndexCache.Create(IdleActionName);
            _shootAnimationActionIndex = ActionIndexCache.Create(ShootActionName);
            _reload1AnimationActionIndex = ActionIndexCache.Create(Reload1ActionName);
            _reload2AnimationActionIndex = ActionIndexCache.Create(Reload2ActionName);
            _rotateLeftAnimationActionIndex = ActionIndexCache.Create(RotateLeftActionName);
            _rotateRightAnimationActionIndex = ActionIndexCache.Create(RotateRightActionName);
            _loadAmmoBeginAnimationActionIndex = ActionIndexCache.Create(LoadAmmoBeginActionName);
            _loadAmmoEndAnimationActionIndex = ActionIndexCache.Create(LoadAmmoEndActionName);
            _reload2IdleActionIndex = ActionIndexCache.Create(Reload2IdleActionName);
        }
        internal void SetTarget(Threat target) => _target = target;
        internal void ClearTarget() => _target = null;

        private void AddCannonballScript()
        {
            var cannonball = Mission.Current.Missiles.FirstOrDefault(missile => missile.ShooterAgent == PilotAgent);
            if (cannonball != null)
            {
                GameEntity entity = cannonball.Entity;
                entity.CreateAndAddScriptComponent("CannonBallScript");
                CannonBallScript cannonBallScript = entity.GetFirstScriptOfType<CannonBallScript>();
                cannonBallScript.SetShooterAgent(PilotAgent);
                entity.CallScriptCallbacks();
            }
        }

        private void PlayFireProjectileEffects()
        {
            var frame = MissileStartingPositionEntityForSimulation.GetGlobalFrame();
            Mission.Current.AddParticleSystemBurstByName("psys_cannon_shot_1", frame, false);
            if (_fireSound == null || !_fireSound.IsValid)
            {
                if (MBRandom.RandomFloat > 0.5f)
                {
                    _fireSound = SoundEvent.CreateEvent(_fireSoundIndex, Scene);
                }
                else
                {
                    _fireSound = SoundEvent.CreateEvent(_fireSoundIndex2, Scene);
                }

                _fireSound.PlayInPosition(GameEntity.GlobalPosition);
            }
        }

        private void DoSlideBack()
        {
            var frame = _body.GameEntity.GetFrame();
            _currentSlideBackFrameOrig = frame;
            _currentSlideBackFrame = frame.Advance(0.6f);
            _lastRecoilTimeStart = Mission.Current.CurrentTime;
            _currentRecoilTimer = 0;
        }

        private void UpdateRecoilEffect(float dt)
        {
            if (State != WeaponState.WaitingAfterShooting) return;
            _currentRecoilTimer += dt;
            if (_currentRecoilTimer > RecoilDuration + Recoil2Duration)
            {
                State = WeaponState.LoadingAmmo;
                if (_fireSound != null)
                {
                    _fireSound.Stop();
                    _fireSound.Release();
                    _fireSound = null;
                }
                return;
            }

            if (_currentRecoilTimer < RecoilDuration)
            {
                var frame = _body.GameEntity.GetFrame();
                var amount = _currentRecoilTimer / RecoilDuration;
                frame = MatrixFrame.Lerp(_currentSlideBackFrameOrig, _currentSlideBackFrame, amount);
                if (amount < 0.5f)
                {
                    frame.origin.z = MBMath.Lerp(frame.origin.z, frame.origin.z + 0.2f, amount * 2);
                }
                else
                {
                    frame.origin.z = MBMath.Lerp(frame.origin.z, frame.origin.z + 0.2f, 1 - amount);
                }

                _body.GameEntity.SetFrame(ref frame);
                DoWheelRotation(dt, 1, -1, 5);
            }
            else if (_currentRecoilTimer < Recoil2Duration)
            {
                var frame = _body.GameEntity.GetFrame();
                var amount = (_currentRecoilTimer - RecoilDuration) / Recoil2Duration;
                frame = MatrixFrame.Lerp(_currentSlideBackFrame, _currentSlideBackFrameOrig, amount);
                _body.GameEntity.SetFrame(ref frame);
                DoWheelRotation(dt, -1, 1);
            }
        }

        private void DoWheelRotation(float dt, float leftwheeldirection, float rightwheeldirection, float speed = 1)
        {
            var frame = _wheel_L.GameEntity.GetFrame();
            frame.rotation.RotateAboutSide(leftwheeldirection * dt * speed);
            _wheel_L.GameEntity.SetFrame(ref frame);
            var frame2 = _wheel_R.GameEntity.GetFrame();
            frame2.rotation.RotateAboutSide(rightwheeldirection * dt * speed);
            _wheel_R.GameEntity.SetFrame(ref frame2);
        }

        private void UpdateWheelRotation(float dt)
        {
            if(!CanRotate()) _isRotating = false;
            if (_isRotating)
            {
                DoWheelRotation(dt, _rotationDirection, _rotationDirection);
            }
        }

        public override bool IsDisabledForBattleSideAI(BattleSideEnum sideEnum)
        {
            return sideEnum != Side;
        }

        public float GetEstimatedCurrentFlightTime()
        {
            if (Target == null) return 0;
            return GetTimeOfProjectileFlight(ShootingSpeed, targetReleaseAngle, MissleStartingPositionForSimulation.Z, Target.Position.Z);
        }

        public static float GetTimeOfProjectileFlight(float velocity, float angle, float heightBegin, float heightEnd)
        {
            //calculate maximum height 
            var traveledHeight = (velocity * velocity * (Mathf.Sin(angle) * Mathf.Sin(angle))) / (2 * MBGlobals.Gravity);
            var maximumHeight = traveledHeight + heightBegin;

            var timeTraveledToMaximumHeight = Mathf.Abs((2 * velocity * Mathf.Sin(angle)) / MBGlobals.Gravity) / 2;

            //calculate from the maximum height down to the end height
            var maximumRelativeToEnd = traveledHeight - heightEnd;

            var term = (velocity * Mathf.Sin(0) + (float)Math.Pow((velocity * Mathf.Sin(0)), 2) + 2 * MBGlobals.Gravity * maximumRelativeToEnd) / MBGlobals.Gravity; ;
            var timeTravelFromMiddleToEnd = velocity * Mathf.Sin(0);

            return timeTraveledToMaximumHeight + timeTravelFromMiddleToEnd;
        }
    }
}
