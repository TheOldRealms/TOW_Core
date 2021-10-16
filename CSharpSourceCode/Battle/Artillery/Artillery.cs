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

namespace TOW_Core.Battle.Artillery
{


    public class Artillery : UsableMachine
    {
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

        public GameEntity CameraHolder { get; private set; }
        public GameEntity Barrel { get; private set; }
        public GameEntity Base { get; private set; }
        private GameEntity _projectileReleasePoint;
        private ItemObject _ammoItem;
        private bool _isRotating;
        private ArtilleryState _currentState;
        private MatrixFrame _cameraHolderInitialFrame;
        private MissionTimer _timer;
        private StandingPointWithWeaponRequirement _ammoLoadPoint;

        public string CameraHolderTag { get; private set; } = "CameraHolder";
        public string BarrelTag { get; private set; } = "Barrel";
        public string BaseTag { get; private set; } = "Battery_Base";
        public string ReloadTag { get; private set; } = "reload";
        public string ProjectileReleaseTag { get; private set; } = "projectile_release";
        public string Name = "Artillery piece";
        public string ProjectilePrefabName  = "pot";
        public float ShootingAnimationLength = 0.35f;

        public int MoveSoundIndex { get; private set; }
        public SoundEvent MoveSound { get; private set; }

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

        protected override void OnInit()
        {
            base.OnInit();
            List<GameEntity> list = base.GameEntity.CollectChildrenEntitiesWithTag(this.CameraHolderTag);
            if (list.Count > 0)
            {
                CameraHolder = list[0];
                _cameraHolderInitialFrame = CameraHolder.GetFrame();
            }
            List<GameEntity> list2 = base.GameEntity.CollectChildrenEntitiesWithTag(this.BarrelTag);
            if (list2.Count > 0)
            {
                Barrel = list2[0];
            }
            List<GameEntity> list3 = base.GameEntity.CollectChildrenEntitiesWithTag(this.ProjectileReleaseTag);
            if (list3.Count > 0)
            {
                _projectileReleasePoint = list3[0];
            }
            List<GameEntity> list4 = base.GameEntity.CollectChildrenEntitiesWithTag(this.BaseTag);
            if (list4.Count > 0)
            {
                Base = list4[0];
            }
            this._ammoItem = MBObjectManager.Instance.GetObject<ItemObject>(ProjectilePrefabName);
            LoadSounds();
            RegisterAnimationParameters();
            foreach (StandingPoint point in base.StandingPoints)
            {
                point.AddComponent(new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none));
                if(point is StandingPointWithWeaponRequirement && point.GameEntity.HasTag(ReloadTag))
                {
                    _ammoLoadPoint = point as StandingPointWithWeaponRequirement;
                    _ammoLoadPoint.InitRequiredWeapon(_ammoItem);
                }
                else if(point is StandingPointWithWeaponRequirement && point.GameEntity.HasTag(AmmoPickUpTag))
                {
                    var ammoPickUpPoint = point as StandingPointWithWeaponRequirement;
                    ammoPickUpPoint.InitGivenWeapon(_ammoItem);
                }
            }
            _currentState = ArtilleryState.Loaded;
        }

        private void LoadSounds()
        {
            this.MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/move");
        }

        internal void GiveInput(float deltaYaw, float deltaPitch, float deltaTime, bool hasinput)
        {
            if (CanRotate())
            {
                if (hasinput && !_isRotating)
                {
                    OnRotationStarted();
                }
                if (_isRotating == true && !hasinput)
                {
                    OnRotationStopped();
                }
                var frame = Base.GetFrame();
                frame.rotation.RotateAboutUp(deltaYaw * deltaTime * 0.2f);
                Base.SetFrame(ref frame);
                if (Barrel != null)
                {
                    var frame2 = Barrel.GetFrame();
                    frame2.rotation.RotateAboutSide(deltaPitch * deltaTime * 0.2f);
                    Barrel.SetFrame(ref frame2);
                }
            }
        }

        private bool CanRotate()
        {
            return _currentState != ArtilleryState.Shooting;
        }

        protected void OnRotationStarted()
        {
            _isRotating = true;
            if (this.MoveSound == null || !this.MoveSound.IsValid)
            {
                this.MoveSound = SoundEvent.CreateEvent(this.MoveSoundIndex, base.Scene);
                this.MoveSound.PlayInPosition(GameEntity.GlobalPosition);
            }
        }

        protected void OnRotationStopped()
        {
            _isRotating = false;
            this.MoveSound.Stop();
            this.MoveSound = null;
        }

        private void DoShooting()
        {
            _currentState = ArtilleryState.Shooting;
            _timer = new MissionTimer(ShootingAnimationLength);
        }

        private void FireProjectile()
        {
            _currentState = ArtilleryState.WaitingForReload;
            var frame = _projectileReleasePoint.GetGlobalFrame();
            MissionWeapon projectile = new MissionWeapon(_ammoItem, null, null);
            if (PilotAgent != null)
            {
                Mission.Current.AddCustomMissile(this.PilotAgent, projectile, frame.origin, frame.rotation.f.NormalizedCopy(), frame.rotation, 0, 20, false, null);
            }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if(PilotAgent != null)
            {
                if(_currentState == ArtilleryState.Loaded && !_isRotating)
                {
                    if (PilotAgent.MovementFlags.HasAnyFlag(Agent.MovementControlFlag.AttackMask))
                    {
                        DoShooting();
                    }
                }
                else if(_currentState == ArtilleryState.Shooting && _timer != null)
                {
                    if (_timer.Check())
                    {
                        FireProjectile();
                        _timer = null;
                    }
                }
            }
            if(_ammoLoadPoint != null && _ammoLoadPoint.HasUser)
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
            HandleAmmoPickUp();
            HandleAnimations(dt);
        }

        private void HandleAmmoPickUp()
        {
            foreach(var sp in AmmoPickUpPoints)
            {
                if(sp is StandingPointWithWeaponRequirement)
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

        private void RegisterAnimationParameters()
        {
            this._idleAnimationActionIndex = ActionIndexCache.Create(this.IdleActionName);
            this._shootAnimationActionIndex = ActionIndexCache.Create(this.ShootActionName);
            this._reload1AnimationActionIndex = ActionIndexCache.Create(this.Reload1ActionName);
            this._reload2AnimationActionIndex = ActionIndexCache.Create(this.Reload2ActionName);
            this._rotateLeftAnimationActionIndex = ActionIndexCache.Create(this.RotateLeftActionName);
            this._rotateRightAnimationActionIndex = ActionIndexCache.Create(this.RotateRightActionName);
            this._loadAmmoBeginAnimationActionIndex = ActionIndexCache.Create(this.LoadAmmoBeginActionName);
            this._loadAmmoEndAnimationActionIndex = ActionIndexCache.Create(this.LoadAmmoEndActionName);
            this._reload2IdleActionIndex = ActionIndexCache.Create(this.Reload2IdleActionName);
        }

        private void HandleAnimations(float dt)
        {
            if(PilotAgent != null)
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
                else if(_currentState == ArtilleryState.Shooting)
                {
                    if (!base.PilotAgent.SetActionChannel(1, this._shootAnimationActionIndex, true, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                    {
                        base.PilotAgent.StopUsingGameObject(true, true);
                    }
                }
            }
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            MoveSound?.Release();
            MoveSound = null;
        }

        protected override bool IsStandingPointNotUsedOnAccountOfBeingAmmoLoad(StandingPoint standingPoint)
        {
            return standingPoint.GameEntity.HasTag(ReloadTag);
        }

        private enum ArtilleryState
        {
            Loaded,
            Shooting,
            WaitingForReload,
            LoadingAmmo
        }


    }
}
