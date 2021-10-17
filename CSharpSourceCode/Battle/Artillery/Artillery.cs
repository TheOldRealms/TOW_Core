using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Artillery
{


    public class Artillery : UsableMachine
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
        private GameEntity _projectileReleasePoint;
        private GameEntity _wheel_L;
        private GameEntity _wheel_R;
        private ItemObject _ammoItem;
        private bool _isRotating;
        private float _rotationDirection = 0;
        private ArtilleryState _currentState;
        private MissionTimer _shootingTimer;
        private float _lastRecoilTimeStart;
        private float _currentRecoilTimer;
        private StandingPointWithWeaponRequirement _ammoLoadPoint;
        private int _moveSoundIndex;
        private SoundEvent _moveSound;
        private int _fireSoundIndex;
        private SoundEvent _fireSound;
        private MatrixFrame _currentSlideBackFrame;
        private MatrixFrame _currentSlideBackFrameOrig;

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
        #endregion

        protected override void OnInit()
        {
            base.OnInit();
            _ammoItem = MBObjectManager.Instance.GetObject<ItemObject>(ProjectilePrefabName);
            CollectEntities();
            LoadSounds();
            RegisterAnimationParameters();
            InitStandingPoints();
            _currentState = ArtilleryState.Loaded;
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
            foreach (StandingPoint point in base.StandingPoints)
            {
                point.AddComponent(new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none));
                if (point is StandingPointWithWeaponRequirement && point.GameEntity.HasTag(ReloadTag))
                {
                    _ammoLoadPoint = point as StandingPointWithWeaponRequirement;
                    _ammoLoadPoint.InitRequiredWeapon(_ammoItem);
                }
                else if (point is StandingPointWithWeaponRequirement && point.GameEntity.HasTag(AmmoPickUpTag))
                {
                    var ammoPickUpPoint = point as StandingPointWithWeaponRequirement;
                    ammoPickUpPoint.InitGivenWeapon(_ammoItem);
                }
            }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (PilotAgent != null)
            {
                if (_currentState == ArtilleryState.Loaded)
                {
                    if (PilotAgent.MovementFlags.HasAnyFlag(Agent.MovementControlFlag.AttackMask))
                    {
                        DoShooting();
                    }
                }
                else if (_currentState == ArtilleryState.Shooting && _shootingTimer != null)
                {
                    if (_shootingTimer.Check())
                    {
                        FireProjectile();
                        _shootingTimer = null;
                    }
                }
            }
            HandleAmmoLoad();
            HandleAmmoPickUp();
            HandleAnimations(dt);
            UpdateWheelRotation(dt);
            UpdateRecoilEffect(dt);
        }
        private void DoShooting()
        {
            _currentState = ArtilleryState.Shooting;
            _shootingTimer = new MissionTimer(ShootingAgentAnimationLength);
        }

        private void FireProjectile()
        {
            var frame = _projectileReleasePoint.GetGlobalFrame();
            MissionWeapon projectile = new MissionWeapon(_ammoItem, null, null);
            if (PilotAgent != null)
            {
                Mission.Current.AddCustomMissile(this.PilotAgent, projectile, frame.origin, frame.rotation.f.NormalizedCopy(), frame.rotation, 0, 20, false, null);
            }
            if (this._fireSound == null || !this._fireSound.IsValid)
            {
                this._fireSound = SoundEvent.CreateEvent(this._fireSoundIndex, base.Scene);
                this._fireSound.PlayInPosition(GameEntity.GlobalPosition);
            }
            DoSlideBack();
            PilotAgent?.StopUsingGameObject(true, true);
        }

        private void DoSlideBack()
        {
            _currentState = ArtilleryState.SlideBack;
            var frame = _artilleryBase.GetFrame();
            _currentSlideBackFrameOrig = frame;
            _currentSlideBackFrame = frame.Advance(-0.6f);
            _lastRecoilTimeStart = Mission.Current.CurrentTime;
            _currentRecoilTimer = 0;
        }

        private void UpdateRecoilEffect(float dt)
        {
            if (_currentState != ArtilleryState.SlideBack) return;
            _currentRecoilTimer += dt;
            if(_currentRecoilTimer  > RecoilDuration + Recoil2Duration)
            {
                _currentState = ArtilleryState.WaitingForReload;
                if(_fireSound != null)
                {
                    _fireSound.Stop();
                    _fireSound.Release();
                    _fireSound = null;
                }
                return;
            }
            if(_currentRecoilTimer < RecoilDuration)
            {
                var frame = _artilleryBase.GetFrame();
                var amount = _currentRecoilTimer / RecoilDuration;
                frame = MatrixFrame.Lerp(_currentSlideBackFrameOrig, _currentSlideBackFrame, amount);
                if(amount < 0.5f)
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
            if (_ammoLoadPoint != null && _ammoLoadPoint.HasUser)
            {
                var user = _ammoLoadPoint.UserAgent;
                if (user.GetCurrentAction(1) == this._loadAmmoEndAnimationActionIndex)
                {
                    EquipmentIndex wieldedItemIndex = user.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                    if (wieldedItemIndex != EquipmentIndex.None && user.Equipment[wieldedItemIndex].CurrentUsageItem.WeaponClass == this._ammoItem.PrimaryWeapon.WeaponClass)
                    {
                        user.RemoveEquippedWeapon(wieldedItemIndex);
                        this._currentState = ArtilleryState.Loaded;
                    }
                    this._ammoLoadPoint.UserAgent.StopUsingGameObject(true, true);
                }
                else
                {
                    if (user.GetCurrentAction(1) != this._loadAmmoBeginAnimationActionIndex && !this._ammoLoadPoint.UserAgent.SetActionChannel(1, this._loadAmmoBeginAnimationActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                    {
                        for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                        {
                            if (!user.Equipment[equipmentIndex].IsEmpty && user.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == this._ammoItem.PrimaryWeapon.WeaponClass)
                            {
                                user.RemoveEquippedWeapon(equipmentIndex);
                            }
                        }
                        this._ammoLoadPoint.UserAgent.StopUsingGameObject(true, true);
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
                        if (!(action == Artillery.act_pickup_boulder_begin))
                        {
                            if (action == Artillery.act_pickup_boulder_end)
                            {
                                MissionWeapon missionWeapon = new MissionWeapon(this._ammoItem, null, null, 1);
                                user.EquipWeaponToExtraSlotAndWield(ref missionWeapon);
                                user.StopUsingGameObject(true, true);
                            }
                            else if (!user.SetActionChannel(1, Artillery.act_pickup_boulder_begin, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                            {
                                user.StopUsingGameObject(true, true);
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
                if (_currentState != ArtilleryState.Shooting && _isRotating)
                {
                    if (!base.PilotAgent.SetActionChannel(1, this._rotateLeftAnimationActionIndex, true, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                    {
                        base.PilotAgent.StopUsingGameObject(true, true);
                    }
                }
                else if (_currentState != ArtilleryState.Shooting && action != _shootAnimationActionIndex)
                {
                    if (!base.PilotAgent.SetActionChannel(1, this._idleAnimationActionIndex, true, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                    {
                        base.PilotAgent.StopUsingGameObject(true, true);
                    }
                }
                else if (_currentState == ArtilleryState.Shooting)
                {
                    if (!base.PilotAgent.SetActionChannel(1, this._shootAnimationActionIndex, true, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                    {
                        base.PilotAgent.StopUsingGameObject(true, true);
                    }
                }
            }
        }

        private void UpdateWheelRotation(float dt)
        {
            if (_isRotating)
            {
                DoWheelRotation(dt, _rotationDirection, -_rotationDirection);
            }
        }

        internal void GiveInput(float deltaYaw, float deltaPitch, float deltaTime, bool hasinput)
        {
            if (CanRotate())
            {
                if (hasinput && !_isRotating)
                {
                    OnRotationStarted(deltaYaw);
                }
                if (_isRotating == true && !hasinput)
                {
                    OnRotationStopped();
                }
                var frame = _artilleryBase.GetFrame();
                frame.rotation.RotateAboutUp(deltaYaw * deltaTime * 0.2f);
                _artilleryBase.SetFrame(ref frame);
                if (_barrel != null)
                {
                    var frame2 = _barrel.GetFrame();
                    frame2.rotation.RotateAboutSide(deltaPitch * deltaTime * 0.2f);
                    var angles = frame2.rotation.GetEulerAngles();
                    var currentelevation = TOWMath.GetDegreeFromRadians(angles.x);
                    if (currentelevation <= MaxPitch && currentelevation >= MinPitch)
                    {
                        _barrel.SetFrame(ref frame2);
                    }
                }
            }
        }

        private bool CanRotate()
        {
            return _currentState == ArtilleryState.Loaded || _currentState == ArtilleryState.WaitingForReload;
        }

        protected void OnRotationStarted(float direction)
        {
            _isRotating = true;
            _rotationDirection = direction;
            if (this._moveSound == null || !this._moveSound.IsValid)
            {
                this._moveSound = SoundEvent.CreateEvent(this._moveSoundIndex, base.Scene);
                this._moveSound.PlayInPosition(GameEntity.GlobalPosition);
            }
        }

        protected void OnRotationStopped()
        {
            _isRotating = false;
            _rotationDirection = 0;
            this._moveSound.Stop();
            this._moveSound = null;
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

        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject;
            if (usableGameObject.GameEntity.HasTag(this.ReloadTag))
            {
                textObject = new TextObject("{=Na81xuXn}{KEY} Reload", null);
            }
            else if (usableGameObject.GameEntity.HasTag(this.AmmoPickUpTag))
            {
                textObject = new TextObject("{=bNYm3K6b}{KEY} Pick Up", null);
            }
            else
            {
                textObject = new TextObject("{=fEQAPJ2e}{KEY} Use", null);
            }
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            if (gameEntity.HasTag(this.AmmoPickUpTag))
            {
                return new TextObject("{=pzfbPbWW}Boulder", null).ToString();
            }
            return new TextObject(this.Name, null).ToString();
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            _moveSound?.Release();
            _moveSound = null;
        }

        protected override bool IsStandingPointNotUsedOnAccountOfBeingAmmoLoad(StandingPoint standingPoint)
        {
            return standingPoint.GameEntity.HasTag(ReloadTag);
        }

        private enum ArtilleryState
        {
            Loaded,
            Shooting,
            SlideBack,
            WaitingForReload,
            LoadingAmmo
        }
    }
}
