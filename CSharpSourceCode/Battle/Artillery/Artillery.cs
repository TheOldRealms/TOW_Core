using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Artillery
{
    public class Artillery : SiegeWeapon
    {
        #region animation fields

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

        public GameEntity CameraHolder;
        private GameEntity _barrel;
        private GameEntity _artilleryBase;

        public Vec3 Position
        {
            get => _artilleryBase.GlobalPosition;
        }

        private GameEntity _projectileReleasePoint;
        private GameEntity _wheel_L;
        private GameEntity _wheel_R;
        private ItemObject _ammoItem;
        private BattleSideEnum _side;
        private bool _isRotating;
        private float _rotationDirection;
        private float _elevationDirection;
        private bool _isPitchDirty;
        private RangedSiegeWeapon.WeaponState _currentState;
        private MissionTimer _shootingTimer;
        private float _lastRecoilTimeStart;
        private float _currentRecoilTimer;
        private StandingPointWithWeaponRequirement _loadAmmoStandingPoint;
        private List<StandingPointWithWeaponRequirement> _ammoPickUpStandingPoints = new List<StandingPointWithWeaponRequirement>();
        private List<StandingPoint> _reloadStandingPoints = new List<StandingPoint>();
        private int _moveSoundIndex;
        private SoundEvent _moveSound;
        private int _fireSoundIndex;
        private SoundEvent _fireSound;
        private MatrixFrame _currentSlideBackFrame;
        private MatrixFrame _currentSlideBackFrameOrig;
        private Threat _target;
        private Vec3 _originalDirection;
        private float _currentPitch;
        private float _currentYaw;
        private float _tolerance = 0.1f;


        private float miniumMuzzleVelocity = 10f;
        private float maximumMuzzleVelocity;
        private float _calculatedMuzzle;

        #region Prefab editable fields

        public string Name = "Artillery piece";
        public string ProjectilePrefabName = "pot";
        public float ShootingAgentAnimationLength = 0.35f;
        public float RecoilDuration = 0.8f;
        public float Recoil2Duration = 2.0f;
        public float MinPitch = 10.0f;
        public float MaxPitch = 50.0f;
        public string CameraHolderTag = "CameraHolder";
        public string BarrelTag = "Barrel";
        public string BaseTag = "Battery_Base";
        public string ReloadTag = "reload";
        public string LeftWheelTag = "Wheel_L";
        public string RightWheelTag = "Wheel_R";
        public string FireSoundID = "mortar_shot";
        public string ProjectileReleaseTag = "projectile_release";
        public float MuzzleVelocity = 40;
        public float MinRange = 20;
        public Team Team;

        #endregion

        public RangedSiegeWeapon.WeaponState State => _currentState;
        public override BattleSideEnum Side => _side;
        public void SetSide(BattleSideEnum side) => _side = side;
        internal bool HasTarget => _target != null && _target.Formation != null && _target.Formation.GetCountOfUnitsWithCondition(x => x.IsActive()) > 0;
        private Vec3 CurrentDirection => _artilleryBase.GetGlobalFrame().rotation.f.NormalizedCopy();
        private int SideCorrection => Team.IsDefender ? -1 : 1;

        protected override void OnInit()
        {
            base.OnInit();
            _ammoItem = MBObjectManager.Instance.GetObject<ItemObject>(ProjectilePrefabName);
            CollectEntities();
            LoadSounds();
            RegisterAnimationParameters();
            InitStandingPoints();
            _currentState = RangedSiegeWeapon.WeaponState.Idle;
            ForcedUse = true;
            _originalDirection = _artilleryBase.GetGlobalFrame().rotation.f.NormalizedCopy();
            _currentPitch = TOWMath.GetDegreeFromRadians(_barrel.GetGlobalFrame().rotation.f.RotationX);
            _currentYaw = TOWMath.GetDegreeFromRadians(_artilleryBase.GetGlobalFrame().rotation.f.RotationZ);
            EnemyRangeToStopUsing = MinRange;
            maximumMuzzleVelocity = MuzzleVelocity + 20f;
            _calculatedMuzzle = maximumMuzzleVelocity;
        }

        private void CollectEntities()
        {
            _artilleryBase = GameEntity.CollectChildrenEntitiesWithTag(BaseTag).FirstOrDefault();
            _projectileReleasePoint = GameEntity.CollectChildrenEntitiesWithTag(ProjectileReleaseTag).FirstOrDefault();
            CameraHolder = GameEntity.CollectChildrenEntitiesWithTag(CameraHolderTag).FirstOrDefault();
            _barrel = GameEntity.CollectChildrenEntitiesWithTag(BarrelTag).FirstOrDefault();
            _wheel_L = GameEntity.CollectChildrenEntitiesWithTag(LeftWheelTag).FirstOrDefault();
            _wheel_R = GameEntity.CollectChildrenEntitiesWithTag(RightWheelTag).FirstOrDefault();
        }

        private void LoadSounds()
        {
            _moveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/move");
            _fireSoundIndex = SoundEvent.GetEventIdFromString(FireSoundID);
        }

        private void RegisterAnimationParameters()
        {
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

        private void InitStandingPoints()
        {
            foreach (StandingPoint point in StandingPoints)
            {
                point.AddComponent(new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none));
                if (point.GameEntity.HasTag(ReloadTag))
                    _reloadStandingPoints.Add(point);

                if (point is StandingPointWithWeaponRequirement && point.GameEntity.HasTag(ReloadTag))
                {
                    _loadAmmoStandingPoint = point as StandingPointWithWeaponRequirement;
                    _loadAmmoStandingPoint.InitRequiredWeapon(_ammoItem);
                    _loadAmmoStandingPoint.SetIsDeactivatedSynched(true);
                }
                else if (point is StandingPointWithWeaponRequirement && point.GameEntity.HasTag(AmmoPickUpTag))
                {
                    var ammoPickUpPoint = point as StandingPointWithWeaponRequirement;
                    _ammoPickUpStandingPoints.Add(ammoPickUpPoint);
                    ammoPickUpPoint.InitGivenWeapon(_ammoItem);
                }
            }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (PilotAgent != null)
            {
                if (_currentState == RangedSiegeWeapon.WeaponState.Idle)
                {
                    if (PilotAgent.MovementFlags.HasAnyFlag(Agent.MovementControlFlag.AttackMask))
                    {
                        Shoot();
                    }
                }

                if (_currentState == RangedSiegeWeapon.WeaponState.Shooting && _shootingTimer != null)
                {
                    if (_shootingTimer.Check())
                    {
                        FireProjectile();
                        _shootingTimer = null;
                    }
                }
            }
            else if (_isRotating)
            {
                OnRotationStopped();
            }

            HandleAmmoLoad();
            HandleAmmoPickUp();
            HandleAnimations(dt);
            UpdateRotation(dt);
            AdjustElevation(dt);
            UpdateWheelRotation(dt);
            UpdateRecoilEffect(dt);
            HandleAimingForAI(dt);
            ForceAmmoPointUsage();
        }

        private void ForceAmmoPointUsage()
        {
            if(PilotAgent == null || State != RangedSiegeWeapon.WeaponState.WaitingBeforeReloading)
            {
                foreach(var sp in _ammoPickUpStandingPoints)
                {
                    if (!sp.IsDeactivated) sp.SetIsDeactivatedSynched(true);
                }
            }
            else
            {
                foreach (var sp in _ammoPickUpStandingPoints)
                {
                    if (sp.IsDeactivated) sp.SetIsDeactivatedSynched(false);
                }
            }
        }

        public void Shoot()
        {
            if (_currentState == RangedSiegeWeapon.WeaponState.Idle)
            {
                _currentState = RangedSiegeWeapon.WeaponState.Shooting;
                _shootingTimer = new MissionTimer(ShootingAgentAnimationLength);
            }
        }

        private void FireProjectile()
        {
            var frame = _projectileReleasePoint.GetGlobalFrame();
            MissionWeapon projectile = new MissionWeapon(_ammoItem, null, null);
            
            if (PilotAgent != null)
            {
                var mf = _calculatedMuzzle;
                TOWCommon.Say(""+ItemObject.GetAirFrictionConstant(WeaponClass.Boulder,WeaponFlags.RangedWeapon));
                Mission.Current.AddCustomMissile(PilotAgent, projectile, frame.origin, frame.rotation.f.NormalizedCopy(), frame.rotation, 0, mf, false, null);
            }

            if (_fireSound == null || !_fireSound.IsValid)
            {
                _fireSound = SoundEvent.CreateEvent(_fireSoundIndex, Scene);
                _fireSound.PlayInPosition(GameEntity.GlobalPosition);
            }

            DoSlideBack();
        }

        private void DoSlideBack()
        {
            _currentState = RangedSiegeWeapon.WeaponState.WaitingAfterShooting;
            var frame = _artilleryBase.GetFrame();
            _currentSlideBackFrameOrig = frame;
            _currentSlideBackFrame = frame.Advance(-0.6f);
            _lastRecoilTimeStart = Mission.Current.CurrentTime;
            _currentRecoilTimer = 0;
        }

        private void UpdateRecoilEffect(float dt)
        {
            if (_currentState != RangedSiegeWeapon.WeaponState.WaitingAfterShooting) return;
            _currentRecoilTimer += dt;
            if (_currentRecoilTimer > RecoilDuration + Recoil2Duration)
            {
                _currentState = RangedSiegeWeapon.WeaponState.WaitingBeforeReloading;
                _loadAmmoStandingPoint.SetIsDeactivatedSynched(false);
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
                var frame = _artilleryBase.GetFrame();
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

                _artilleryBase.SetFrame(ref frame);
                DoWheelRotation(dt, 1, 1, 5);
            }
            else if (_currentRecoilTimer < Recoil2Duration)
            {
                var frame = _artilleryBase.GetFrame();
                var amount = (_currentRecoilTimer - RecoilDuration) / Recoil2Duration;
                frame = MatrixFrame.Lerp(_currentSlideBackFrame, _currentSlideBackFrameOrig, amount);
                _artilleryBase.SetFrame(ref frame);
                DoWheelRotation(dt, -1, -1);
            }
        }

        private void HandleAmmoLoad()
        {
            if (_loadAmmoStandingPoint != null && _loadAmmoStandingPoint.HasUser)
            {
                var user = _loadAmmoStandingPoint.UserAgent;
                if (user.GetCurrentAction(1) == _loadAmmoEndAnimationActionIndex)
                {
                    EquipmentIndex wieldedItemIndex = user.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                    if (wieldedItemIndex != EquipmentIndex.None && user.Equipment[wieldedItemIndex].CurrentUsageItem.WeaponClass == _ammoItem.PrimaryWeapon.WeaponClass)
                    {
                        user.RemoveEquippedWeapon(wieldedItemIndex);
                        _currentState = RangedSiegeWeapon.WeaponState.Idle;
                        _loadAmmoStandingPoint.SetIsDeactivatedSynched(true);
                    }
                    //user.StopUsingGameObject();
                }
                else
                {
                    if (user.GetCurrentAction(1) != _loadAmmoBeginAnimationActionIndex && !_loadAmmoStandingPoint.UserAgent.SetActionChannel(1, _loadAmmoBeginAnimationActionIndex))
                    {
                        for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                        {
                            if (!user.Equipment[equipmentIndex].IsEmpty && user.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == _ammoItem.PrimaryWeapon.WeaponClass)
                            {
                                user.RemoveEquippedWeapon(equipmentIndex);
                            }
                        }

                        user.StopUsingGameObject();
                    }
                }
            }
        }

        private void HandleAmmoPickUp()
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
                                MissionWeapon missionWeapon = new MissionWeapon(_ammoItem, null, null, 1);
                                user.EquipWeaponToExtraSlotAndWield(ref missionWeapon);
                                user.StopUsingGameObject();
                                _currentState = RangedSiegeWeapon.WeaponState.Reloading;
                            }
                            else if (!user.SetActionChannel(1, act_pickup_boulder_begin))
                            {
                                user.StopUsingGameObject();
                            }
                        }
                    }
                }
            }
        }

        private void HandleAnimations(float dt)
        {
            if (PilotAgent != null)
            {
                var action = PilotAgent.GetCurrentAction(1);
                if (_currentState != RangedSiegeWeapon.WeaponState.Shooting && _isRotating)
                {
                    if (!PilotAgent.SetActionChannel(1, _rotateLeftAnimationActionIndex, true))
                    {
                        PilotAgent.StopUsingGameObject();
                    }
                }
                else if (_currentState != RangedSiegeWeapon.WeaponState.Shooting && action != _shootAnimationActionIndex)
                {
                    if (!PilotAgent.SetActionChannel(1, _idleAnimationActionIndex, true))
                    {
                        PilotAgent.StopUsingGameObject();
                    }
                }
                else if (_currentState == RangedSiegeWeapon.WeaponState.Shooting)
                {
                    if (!PilotAgent.SetActionChannel(1, _shootAnimationActionIndex, true))
                    {
                        PilotAgent.StopUsingGameObject();
                    }
                }
            }
        }

        private void UpdateRotation(float dt)
        {
            if (!_isRotating) return;
            var frame = _artilleryBase.GetGlobalFrame();
            var amount = _rotationDirection * dt * 0.2f;
            frame.rotation.RotateAboutUp(amount);
            _artilleryBase.SetGlobalFrame(frame);
            _currentYaw = TOWMath.GetDegreeFromRadians(frame.rotation.f.RotationZ) * SideCorrection;
        }

        private void UpdateWheelRotation(float dt)
        {
            if (_isRotating)
            {
                DoWheelRotation(dt, _rotationDirection, -_rotationDirection);
            }
        }

        internal void GiveInput(float deltaYaw, float deltaPitch)
        {
            _isPitchDirty = false;
            if (CanRotate())
            {
                if (deltaYaw != 0 && !_isRotating)
                {
                    OnRotationStarted(deltaYaw);
                }
                else if (deltaYaw != 0 && _isRotating)
                {
                    _rotationDirection = deltaYaw;
                }

                if (_isRotating && deltaYaw == 0)
                {
                    OnRotationStopped();
                }

                if (deltaPitch != 0)
                {
                    _isPitchDirty = true;
                    _elevationDirection = deltaPitch;
                }
            }
        }

        private void AdjustElevation(float dt)
        {
            if (_barrel != null && _isPitchDirty)
            {
                var frame = _barrel.GetGlobalFrame();
                frame.rotation.RotateAboutSide(_elevationDirection * dt * 0.2f);
                var elevation = TOWMath.GetDegreeFromRadians(frame.rotation.f.RotationX);
                if (elevation >= MinPitch && elevation <= MaxPitch)
                {
                    _barrel.SetGlobalFrame(frame);
                    _currentPitch = elevation;
                }
            }
        }

        private bool CanRotate()
        {
            return _currentState == RangedSiegeWeapon.WeaponState.Idle; // || _currentState == RangedSiegeWeapon.WeaponState.WaitingBeforeReloading;
        }

        protected void OnRotationStarted(float direction)
        {
            _isRotating = true;
            _rotationDirection = direction;
            if (_moveSound == null || !_moveSound.IsValid)
            {
                _moveSound = SoundEvent.CreateEvent(_moveSoundIndex, Scene);
                _moveSound.PlayInPosition(GameEntity.GlobalPosition);
            }
        }

        protected void OnRotationStopped()
        {
            _isRotating = false;
            _rotationDirection = 0;
            _moveSound.Stop();
            _moveSound = null;
        }

        private void DoWheelRotation(float dt, float leftwheeldirection, float rightwheeldirection, float speed = 1)
        {
            var frame = _wheel_L.GetFrame();
            frame.rotation.RotateAboutSide(leftwheeldirection * dt * speed);
            _wheel_L.SetFrame(ref frame);
            var frame2 = _wheel_R.GetFrame();
            frame2.rotation.RotateAboutSide(rightwheeldirection * dt * speed);
            _wheel_R.SetFrame(ref frame2);
        }

        private void HandleAimingForAI(float dt)
        {
            if (!HasTarget || PilotAgent == null || !PilotAgent.IsAIControlled) return;
            float requiredElevation = GetRequiredPitch();
            float requiredYaw = GetRequiredYaw();
            //TOWCommon.Say("P" +requiredElevation+ "Y" + requiredYaw);
            float x = 0;
            float y = 0;
            if (!IsWithinToleranceRange(requiredElevation, _currentPitch))
            {
                if (GetShortestAngleIsClockwise(requiredElevation, _currentPitch))
                {
                    y = -1;
                }
                else
                {
                    y = 1;
                }
            }

            if (!IsWithinToleranceRange(requiredYaw, _currentYaw))
            {
                if (GetShortestAngleIsClockwise(requiredYaw, _currentYaw))
                {
                    x = 1;
                }
                else
                {
                    x = -1;
                }
            }

            GiveInput(x, y);
        }

        private bool IsWithinToleranceRange(float num1, float num2)
        {
            return num1 > num2 - _tolerance && num1 < num2 + _tolerance;
        }

        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject;
            if (usableGameObject.GameEntity.HasTag(ReloadTag))
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
            if (gameEntity.HasTag(AmmoPickUpTag))
            {
                return new TextObject("{=pzfbPbWW}Cannonball").ToString();
            }

            return new TextObject(Name).ToString();
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            _moveSound?.Release();
            _moveSound = null;
        }

        protected override float GetWeightOfStandingPoint(StandingPoint sp)
        {
            return base.GetWeightOfStandingPoint(sp);
        }

        //Copied from Mangonel.cs
        public override TargetFlags GetTargetFlags()
        {
            TargetFlags targetFlags = (TargetFlags) (0 | 2 | 8 | 16);
            if (IsDestroyed || IsDeactivated)
                targetFlags |= TargetFlags.NotAThreat;
            if (Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToMangonels)
                targetFlags |= TargetFlags.DebugThreat;
            if (Side == BattleSideEnum.Defender && DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToMangonels)
                targetFlags |= TargetFlags.DebugThreat;
            return targetFlags;
        }

        //Copied from Mangonel.cs
        public override float GetTargetValue(List<Vec3> weaponPos) => 40f * GetUserMultiplierOfWeapon() * GetDistanceMultiplierOfWeapon(weaponPos[0]) * GetHitPointMultiplierOfWeapon();

        //Copied from Mangonel.cs
        public override SiegeEngineType GetSiegeEngineType() => Side != BattleSideEnum.Attacker ? DefaultSiegeEngineTypes.Catapult : DefaultSiegeEngineTypes.Onager;

        protected override float GetDetachmentWeightAux(BattleSideEnum side)
        {
            if (IsDisabledForBattleSideAI(side)) return float.MinValue;

            var num = float.MinValue;
            _usableStandingPoints.Clear();
            for (int i = 0; i < StandingPoints.Count; i++)
            {
                var sp = StandingPoints[i];
                if (!sp.IsDisabled)
                {
                    if (!sp.HasUser && !sp.HasAIMovingTo)
                    {
                        _usableStandingPoints.Add(new ValueTuple<int, StandingPoint>(i, sp));
                    }
                }
            }

            if (_usableStandingPoints.Count > 0) _areUsableStandingPointsVacant = true;
            foreach (var sp in StandingPoints)
            {
                if (!sp.HasUser && !sp.HasAIMovingTo && !sp.IsDisabled)
                {
                    if (sp == PilotStandingPoint && State == RangedSiegeWeapon.WeaponState.Idle)
                    {
                        return 1;
                    }
                    else if (State == RangedSiegeWeapon.WeaponState.WaitingBeforeReloading && !AmmoPickUpPoints.Any(x => x.HasAIMovingTo || x.HasUser))
                    {
                        return 1;
                    }

                    if (sp == _loadAmmoStandingPoint && State == RangedSiegeWeapon.WeaponState.Reloading)
                    {
                        return 1;
                    }
                }
            }

            return num;
        }

        internal bool CanShootAtTarget()
        {
            if (!HasTarget) return false;
            else
            {
                return CanShootAtPoint(GetTargetPosition());
            }
        }

        private Vec3 GetTargetPosition()
        {
            return _target.Formation.GetMedianAgent(true, true, _target.Formation.GetAveragePositionOfUnits(true, true)).Frame.origin;
        }

        public bool CanShootAtPoint(Vec3 target)
        {
            if ((target - _artilleryBase.GlobalPosition).Length <= MinRange)
            {
                return false;
            }
            else if (!IsInRange(target))
            {
                return false;
            }

            float requiredElevation = GetRequiredPitch();
            float requiredYaw = GetRequiredYaw();
            if (requiredElevation < MinPitch || requiredElevation > MaxPitch)
            {
                return false;
            }

            //TOWCommon.Say("here" + "pitch: " +_currentPitch+ " req :"+ requiredElevation+ IsWithinToleranceRange(requiredElevation, _currentPitch) +" "+ Math.Abs(requiredElevation - _currentPitch));
            return IsWithinToleranceRange(requiredElevation, _currentPitch) && IsWithinToleranceRange(requiredYaw, _currentYaw);// && Scene.CheckPointCanSeePoint(_projectileReleasePoint.GetGlobalFrame().Advance(1).origin, target, null);
        }

        private bool IsInRange(Vec3 target)
        {
            var maxrange = BallisticSolver.GetMaxRange(maximumMuzzleVelocity, MBGlobals.Gravity, (target - _projectileReleasePoint.GlobalPosition).z);
            var distancetotarget = (target - _projectileReleasePoint.GlobalPosition).AsVec2.Length;
            return maxrange >= distancetotarget;
        }

        public virtual float GetTargetReleaseAngle(Vec3 target)
        {
           // Vec3 lowAngle, highAngle;
            
            Vec3 lowAngle= Vec3.Zero;
            Vec3 highAngle = Vec3.Zero;
            float calculatedMuzzle = MuzzleVelocity;
            
            for (float i = miniumMuzzleVelocity; i <maximumMuzzleVelocity; i += 0.1f)
            {
                var result = BallisticSolver.SolveBallisticArc(_projectileReleasePoint.GlobalPosition, i, target, MBGlobals.Gravity, out lowAngle, out highAngle);

                if (result != 0)
                {
                    calculatedMuzzle = i;
                    break;
                }
            }

            _calculatedMuzzle = calculatedMuzzle;
            
            float low, high;
            low = TOWMath.GetDegreeFromRadians(Vec3.AngleBetweenTwoVectors(_artilleryBase.GetGlobalFrame().rotation.f, lowAngle));
            high = TOWMath.GetDegreeFromRadians(Vec3.AngleBetweenTwoVectors(_artilleryBase.GetGlobalFrame().rotation.f, highAngle));
            if (low > MinPitch && low < MaxPitch) return low;
            else if (high < MaxPitch && high > MinPitch) return high;
            else return 0;
        }

        private Vec3 GetTargetDirection(Vec3 target)
        {
            return (target - _artilleryBase.GetGlobalFrame().origin).NormalizedCopy();
        }

        private float GetRequiredYaw()
        {
            var dir = GetTargetDirection(GetTargetPosition());
            var angle = Vec3.AngleBetweenTwoVectors(dir, _originalDirection);
            return TOWMath.GetDegreeFromRadians(angle);
        }

        private float GetRequiredPitch()
        {
            return GetTargetReleaseAngle(GetTargetPosition());
        }

        internal void SetTarget(Threat target) => _target = target;
        internal void ClearTarget() => _target = null;
        internal Threat GetTarget() => _target;

        public override bool IsDisabledForBattleSideAI(BattleSideEnum sideEnum)
        {
            return sideEnum != Side;
        }
        
        private bool GetShortestAngleIsClockwise(float targetDegree, float startDegree)
        {
            bool result=Math.Abs(startDegree - targetDegree) < 180;
            
            if (startDegree - targetDegree<0)
            {
                result = !result;
            }
            return result;
        }

        private float SimulateRotationMovement(float current, float target, float deltaTime)
        {
            float val = ((Math.Abs(target - Math.Abs(current))) / 3) * 5f;
            return  val*deltaTime;
        }

        public override UsableMachineAIBase CreateAIBehaviorObject()
        {
            return new ArtilleryAI(this);
        }

    }
}