using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TOW_Core.Abilities;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Battle.Crosshairs;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.CrosshairMissionBehavior
{
    [OverrideView(typeof(MissionCrosshair))]
    public class CustomCrosshairMissionBehavior : MissionView
    {
        private bool isMainAgentChecked;
        private bool _isActive;
        private Crosshair _weaponCrosshair;
        private AbilityCrosshair _abilityCrosshair;
        private AbilityComponent _abilityComponent;

        public override void OnMissionScreenTick(float dt)
        {
            New();
        }

        private void New()
        {
            if (CanUseCrosshair())
            {
                if (!isMainAgentChecked)
                {
                    isMainAgentChecked = true;
                    if ((_abilityComponent = Agent.Main.GetComponent<AbilityComponent>()) != null)
                    {
                        _abilityComponent.CurrentAbilityChanged += (crosshair) =>
                        {
                            _abilityCrosshair?.Hide();
                            _abilityCrosshair = crosshair;
                        };
                        _abilityCrosshair = _abilityComponent.CurrentAbility?.Crosshair;
                    }
                }
                else if (_abilityCrosshair != null && _abilityComponent.IsSpellModeOn)
                {
                    _weaponCrosshair.Hide();
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
                        _weaponCrosshair.Tick();
                        _weaponCrosshair.Show();
                    }
                    else
                    {
                        _weaponCrosshair.Hide();
                    }
                }
            }
        }

        private void Old()
        {
            if (CanUseCrosshair())
            {
                if (!isMainAgentChecked)
                {
                    isMainAgentChecked = true;
                    if ((_abilityComponent = Agent.Main.GetComponent<AbilityComponent>()) != null)
                    {
                        _abilityComponent.CurrentAbilityChanged += (crosshair) =>
                        {
                            if (_abilityCrosshair != null)
                            {
                                _abilityCrosshair.Hide();
                            }
                            _abilityCrosshair = crosshair;
                        };
                        _abilityCrosshair = _abilityComponent.CurrentAbility?.Crosshair;
                    }
                }
                else
                {
                    if (_abilityCrosshair != null)
                    {
                        _abilityCrosshair.Hide();
                    }
                    _weaponCrosshair.Tick();
                    UpdateWeaponCrosshairVisibility();
                }
            }
            else
            {
                if (_abilityCrosshair != null)
                {
                    _abilityCrosshair.Hide();
                }
                _weaponCrosshair.Hide();
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

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            if (_isActive)
            {
                return;
            }
            _weaponCrosshair = new Crosshair(Mission, MissionScreen);
            _weaponCrosshair.InitializeCrosshair();
            _isActive = true;
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
            _isActive = false;
        }

        private void UpdateAbilityCrosshairVisibility()
        {
            if (_abilityCrosshair == null)
            {
                if (_abilityComponent.CurrentAbility.Crosshair != null)
                {
                    _abilityCrosshair = _abilityComponent.CurrentAbility.Crosshair;
                }
            }
            else
            {
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

        private void UpdateWeaponCrosshairVisibility()
        {
            if (!Agent.Main.WieldedWeapon.IsEmpty && Agent.Main.WieldedWeapon.CurrentUsageItem.IsRangedWeapon)
            {
                //&& Agent.Main.WieldedWeapon.Item.Name.Contains("scope")
                if (Mission.GetMainAgentMaxCameraZoom() > 1)
                {
                    _weaponCrosshair.Hide();
                }
                else if (BannerlordConfig.DisplayTargetingReticule)
                {
                    _weaponCrosshair.Show();
                }
                return;
            }
            _weaponCrosshair.Hide();
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
    }
}