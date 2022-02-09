using System;
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
        private bool _isActive;
        private bool _isUsingSniperScope;
        private Crosshair _weaponCrosshair;
        private SniperScope _sniperScope;
        private AbilityCrosshair _abilityCrosshair;
        private AbilityComponent _abilityComponent;
        private AbilityManagerMissionLogic _missionLogic;

        public bool IsUsingSniperScope
        {
            get => _isUsingSniperScope;
        }

        public override void OnMissionScreenTick(float dt)
        {
            if (!_isActive)
            {
                return;
            }
            if (CanUseCrosshair())
            {
                if (_abilityCrosshair != null && _missionLogic != null && _missionLogic.CurrentState != AbilityModeState.Off)
                {
                    _weaponCrosshair.Hide();
                    _sniperScope.Hide();
                    if (!_abilityComponent.CurrentAbility.CanCast(Agent.Main))
                    {
                        _abilityCrosshair.Hide();
                    }
                    else
                    {
                        if (_abilityCrosshair.CrosshairType == CrosshairType.CenteredAOE || IsRightAngleToCast())
                        {
                            _abilityCrosshair.Tick();
                            _abilityCrosshair.Show();
                        }
                        else
                        {
                            _abilityCrosshair.Hide();
                        }
                    }
                }
                else
                {
                    _abilityCrosshair?.Hide();
                    if (!Agent.Main.WieldedWeapon.IsEmpty && Agent.Main.WieldedWeapon.CurrentUsageItem.IsRangedWeapon)
                    {
                        if (CanUseSniperScope())
                        {
                            _weaponCrosshair.Hide();
                            _sniperScope.Tick();
                            _sniperScope.Show();
                        }
                        else
                        {
                            _sniperScope.Hide();
                            _weaponCrosshair.Tick();
                            _weaponCrosshair.Show();
                        }
                    }
                    else
                    {
                        _sniperScope.Hide();
                        _weaponCrosshair.Hide();
                    }
                }
            }
            else
            {
                _sniperScope?.Hide();
                _weaponCrosshair?.Hide();
                _abilityCrosshair?.Hide();
            }
        }

        private bool CanUseCrosshair()
        {
            return _isActive &&
                   Agent.Main != null &&
                   Agent.Main.State == AgentState.Active &&
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

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            if (!_isActive)
            {
                return;
            }
            _weaponCrosshair.FinalizeCrosshair();
            _abilityComponent = null;
            _abilityCrosshair = null;
            _sniperScope = null;
            _isActive = false;
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

        private bool CanUseSniperScope()
        {
            _isUsingSniperScope = Mission.CameraIsFirstPerson &&
                                  Input.IsKeyDown(InputKey.LeftShift) &&
                                  Input.IsKeyDown(InputKey.LeftMouseButton) &&
                                  Agent.Main.GetCurrentActionType(1) == Agent.ActionCodeType.ReadyRanged &&
                                  Agent.Main.WieldedWeapon.Item.StringId.Contains("longrifle") &&
                                  IsRightAngleToShoot();
            return _isUsingSniperScope;
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

        private bool IsPlayerAimingWithSniperRifle()
        {
            return Mission.CameraIsFirstPerson &&
                   Input.IsKeyDown(InputKey.LeftShift) &&
                   Input.IsKeyDown(InputKey.LeftMouseButton) &&
                   Agent.Main.WieldedWeapon.Item.StringId.Contains("longrifle");
        }

        protected override void OnAgentControllerChanged(Agent agent, Agent.ControllerType oldController)
        {
            if (agent.Controller != Agent.ControllerType.Player || Agent.Main == null)
            {
                return;
            }

            _weaponCrosshair = new Crosshair(Mission, MissionScreen);
            _weaponCrosshair.InitializeCrosshair();
            _sniperScope = new SniperScope();
            _isActive = true;

            if (Agent.Main.IsAbilityUser() && (_abilityComponent = Agent.Main.GetComponent<AbilityComponent>()) != null)
            {
                _missionLogic = Mission.Current.GetMissionBehavior<AbilityManagerMissionLogic>();
                _abilityComponent.CurrentAbilityChanged += (crosshair) =>
                {
                    _abilityCrosshair?.Hide();
                    _abilityCrosshair = crosshair;
                };
                _abilityCrosshair = _abilityComponent.CurrentAbility?.Crosshair;
            }
        }

        private bool IsRightAngleToShoot()
        {
            float numberToCheck = MBMath.WrapAngle(Agent.Main.LookDirection.AsVec2.RotationInRadians - Agent.Main.GetMovementDirection().RotationInRadians);
            Vec2 bodyRotationConstraint = Agent.Main.GetBodyRotationConstraint(1);
            return !(Mission.Current.MainAgent.MountAgent != null && !MBMath.IsBetween(numberToCheck, bodyRotationConstraint.x, bodyRotationConstraint.y) && (bodyRotationConstraint.x < -0.1f || bodyRotationConstraint.y > 0.1f));
        }
    }
}