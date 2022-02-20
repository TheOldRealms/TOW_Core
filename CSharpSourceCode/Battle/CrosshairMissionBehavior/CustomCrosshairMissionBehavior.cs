using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Abilities;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Battle.Crosshairs;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.CrosshairMissionBehavior
{
    [OverrideView(typeof(MissionCrosshair))]
    public class CustomCrosshairMissionBehavior : MissionView
    {
        private bool _areCrosshairsInitialized;
        private ICrosshair _currentCrosshair;
        private Crosshair _weaponCrosshair;
        private SniperScope _sniperScope;
        private AbilityCrosshair _abilityCrosshair;
        private AbilityComponent _abilityComponent;
        private AbilityManagerMissionLogic _missionLogic;

        public ICrosshair CurrentCrosshair { get => _currentCrosshair; }

        public override void OnMissionScreenTick(float dt)
        {
            if (!_areCrosshairsInitialized)
            {
                if (Agent.Main != null && MissionScreen != null)
                    InitializeCrosshairs();
                else
                    return;
            }
            if (CanUseCrosshair())
            {
                if (CanUseAbilityCrosshair())
                {
                    if (_currentCrosshair != _abilityCrosshair)
                        ChangeCrosshair(_abilityCrosshair);
                }
                else if (!Agent.Main.WieldedWeapon.IsEmpty && Agent.Main.WieldedWeapon.CurrentUsageItem.IsRangedWeapon)
                {
                    if (CanUseSniperScope())
                    {
                        if (_currentCrosshair != _sniperScope)
                            ChangeCrosshair(_sniperScope);
                    }
                    else
                    {
                        if (_currentCrosshair != _weaponCrosshair)
                            ChangeCrosshair(_weaponCrosshair);
                    }
                }
                else ChangeCrosshair(null);
                if (_currentCrosshair != null) _currentCrosshair.Tick();
            }
            else if(_currentCrosshair != null) ChangeCrosshair(null);
        }

        private void ChangeCrosshair(ICrosshair crosshair)
        {
            _currentCrosshair?.Hide();
            _currentCrosshair = crosshair;
            if(_currentCrosshair != null) _currentCrosshair.Show();
        }

        private bool CanUseCrosshair()
        {
            return Agent.Main != null &&
                   Agent.Main.State == AgentState.Active &&
                   (_abilityComponent == null ||
                   _abilityComponent.SpecialMove == null ||
                   !_abilityComponent.SpecialMove.IsUsing) &&
                   Mission.Mode != MissionMode.Conversation &&
                   Mission.Mode != MissionMode.Deployment &&
                   Mission.Mode != MissionMode.CutScene &&
                   MissionScreen != null &&
                   MissionScreen.CustomCamera == null &&
                   (MissionScreen.OrderFlag == null || !MissionScreen.OrderFlag.IsVisible) &&
                   !MissionScreen.IsViewingCharacter() &&
                   !MBEditor.EditModeEnabled &&
                   BannerlordConfig.DisplayTargetingReticule &&
                   !ScreenManager.GetMouseVisibility();
        }

        private bool CanUseAbilityCrosshair()
        {
            return !Mission.IsFriendlyMission &&
                   _missionLogic != null &&
                   _missionLogic.CurrentState != AbilityModeState.Off &&
                   _abilityComponent.CurrentAbility.CanCast(Agent.Main);
        }

        private void InitializeCrosshairs()
        {
            _weaponCrosshair = new Crosshair();
            _weaponCrosshair.InitializeCrosshair();
            _sniperScope = new SniperScope();

            if (Agent.Main.IsAbilityUser() && (_abilityComponent = Agent.Main.GetComponent<AbilityComponent>()) != null)
            {
                _missionLogic = Mission.Current.GetMissionBehavior<AbilityManagerMissionLogic>();
                _abilityComponent.CurrentAbilityChanged += ChangeAbilityCrosshair;
                _abilityComponent.InitializeCrosshairs();
                _abilityCrosshair = _abilityComponent.CurrentAbility?.Crosshair;
            }
            _areCrosshairsInitialized = true;
        }

        public override void OnMissionScreenFinalize()
        {
            if (!_areCrosshairsInitialized)
            {
                return;
            }
            _weaponCrosshair.FinalizeCrosshair();
            _abilityComponent = null;
            _abilityCrosshair = null;
            _sniperScope.FinalizeCrosshair();
            _sniperScope = null;
            _areCrosshairsInitialized = false;
            if (_abilityComponent != null)
            {
                _abilityComponent.CurrentAbilityChanged -= ChangeAbilityCrosshair;
            }
        }

        private bool CanUseSniperScope()
        {
            return Mission.CameraIsFirstPerson &&
                   Input.IsKeyDown(InputKey.LeftShift) &&
                   Input.IsKeyDown(InputKey.LeftMouseButton) &&
                   Agent.Main.GetCurrentActionType(1) == Agent.ActionCodeType.ReadyRanged &&
                   Agent.Main.WieldedWeapon.Item.StringId.Contains("longrifle") &&
                   IsRightAngleToShoot();
        }

        public override void OnPhotoModeActivated()
        {
            base.OnPhotoModeActivated();
            _weaponCrosshair.OnPhotoModeActivated();
        }

        public override void OnPhotoModeDeactivated()
        {
            base.OnPhotoModeDeactivated();
            _weaponCrosshair.OnPhotoModeDeactivated();
        }

        private void ChangeAbilityCrosshair(AbilityCrosshair crosshair)
        {
            _abilityCrosshair?.Hide();
            _abilityCrosshair = crosshair;
        }

        private bool IsRightAngleToShoot()
        {
            float numberToCheck = MBMath.WrapAngle(Agent.Main.LookDirection.AsVec2.RotationInRadians - Agent.Main.GetMovementDirection().RotationInRadians);
            Vec2 bodyRotationConstraint = Agent.Main.GetBodyRotationConstraint(1);
            return !(Mission.Current.MainAgent.MountAgent != null && !MBMath.IsBetween(numberToCheck, bodyRotationConstraint.x, bodyRotationConstraint.y) && (bodyRotationConstraint.x < -0.1f || bodyRotationConstraint.y > 0.1f));
        }
    }
}