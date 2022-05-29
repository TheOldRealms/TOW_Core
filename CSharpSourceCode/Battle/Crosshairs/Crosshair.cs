using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;

namespace TOW_Core.Battle.Crosshairs
{
    public class Crosshair : ICrosshair
    {
        public void InitializeCrosshair()
        {
            _mission = Mission.Current;
            _missionScreen = ScreenManager.TopScreen as MissionScreen;
            CombatLogManager.OnGenerateCombatLog += OnCombatLogGenerated;
            _crosshairVM = new CrosshairVM();
            _weaponLayer = new GauntletLayer(1, "GauntletLayer", false);
            _weaponMovie = _weaponLayer.LoadMovie("Crosshair", _crosshairVM);
            if (_mission.Mode != MissionMode.Conversation && _mission.Mode != MissionMode.CutScene)
            {
                _missionScreen.AddLayer(_weaponLayer);
            }
        }

        public void FinalizeCrosshair()
        {
            CombatLogManager.OnGenerateCombatLog -= this.OnCombatLogGenerated;
            if (_mission.Mode != MissionMode.Conversation && _mission.Mode != MissionMode.CutScene)
            {
                _missionScreen.RemoveLayer(this._weaponLayer);
            }
            this._mission = null;
            this._missionScreen = null;
            this._crosshairVM = null;
            _weaponMovie.Release();
            this._weaponMovie = null;
            this._weaponLayer = null;
        }

        public void Tick()
        {
            bool flag = true;
            bool isTargetInvalid = false;
            for (int i = 0; i < this._targetGadgetOpacities.Length; i++)
            {
                this._targetGadgetOpacities[i] = 0.0;
            }
            _crosshairVM.CrosshairType = BannerlordConfig.CrosshairType;
            Agent mainAgent = Mission.Current.MainAgent;
            double num = (double)(_missionScreen.CameraViewAngle * _viewAngleConst);
            double accuracy = 2.0 * Math.Tan((double)(mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) * (0.5 / Math.Tan(num * 0.5)));
            _crosshairVM.SetProperties(accuracy, (double)(1f + (_missionScreen.CombatCamera.HorizontalFov - _crosshairAngleConstant) / _crosshairAngleConstant));
            WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand);
            float numberToCheck = MBMath.WrapAngle(mainAgent.LookDirection.AsVec2.RotationInRadians - mainAgent.GetMovementDirection().RotationInRadians);
            if (wieldedWeaponInfo != null && wieldedWeaponInfo.IsRangedWeapon && BannerlordConfig.DisplayTargetingReticule)
            {
                Agent.ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);
                MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;
                if (wieldedWeapon.ReloadPhaseCount > 1 && wieldedWeapon.IsReloading && currentActionType == Agent.ActionCodeType.Reload)
                {
                    ValueTuple<float, float>[] array = new ValueTuple<float, float>[(int)wieldedWeapon.ReloadPhaseCount];
                    ActionIndexCache itemUsageReloadActionCode = MBItem.GetItemUsageReloadActionCode(wieldedWeapon.CurrentUsageItem.ItemUsage, 9, mainAgent.HasMount, -1, mainAgent.GetIsLeftStance());
                    this.FillReloadDurationsFromActions(array, mainAgent, itemUsageReloadActionCode);
                    float num2 = mainAgent.GetCurrentActionProgress(1);
                    if (mainAgent.GetCurrentAction(1).Index != -1)
                    {
                        float num3 = 1f - MBActionSet.GetActionBlendOutStartProgress(mainAgent.ActionSet, mainAgent.GetCurrentAction(1));
                        num2 += num3;
                    }
                    float animationParameter = MBAnimation.GetAnimationParameter2(mainAgent.AgentVisuals.GetSkeleton().GetAnimationAtChannel(1));
                    bool flag2 = num2 > animationParameter;
                    float item = flag2 ? 1f : (num2 / animationParameter);
                    short reloadPhase = wieldedWeapon.ReloadPhase;
                    for (int j = 0; j < (int)reloadPhase; j++)
                    {
                        array[j].Item1 = 1f;
                    }
                    if (!flag2)
                    {
                        array[(int)reloadPhase].Item1 = item;
                        _crosshairVM.SetReloadProperties(array);
                    }
                    flag = false;
                }
                if (currentActionType == Agent.ActionCodeType.ReadyRanged)
                {
                    Vec2 bodyRotationConstraint = mainAgent.GetBodyRotationConstraint(1);
                    isTargetInvalid = (Mission.Current.MainAgent.MountAgent != null && !MBMath.IsBetween(numberToCheck, bodyRotationConstraint.x, bodyRotationConstraint.y) && (bodyRotationConstraint.x < -0.1f || bodyRotationConstraint.y > 0.1f));
                }
            }
            else if ((wieldedWeaponInfo != null && wieldedWeaponInfo.IsMeleeWeapon) || wieldedWeaponInfo == null)
            {
                Agent.ActionCodeType currentActionType2 = mainAgent.GetCurrentActionType(1);
                Agent.UsageDirection currentActionDirection = mainAgent.GetCurrentActionDirection(1);
                if (BannerlordConfig.DisplayAttackDirection && (currentActionType2 == Agent.ActionCodeType.ReadyMelee || currentActionDirection != Agent.UsageDirection.None))
                {
                    if (currentActionType2 == Agent.ActionCodeType.ReadyMelee)
                    {
                        switch (mainAgent.AttackDirection)
                        {
                            case Agent.UsageDirection.AttackUp:
                                this._targetGadgetOpacities[0] = 0.7;
                                break;
                            case Agent.UsageDirection.AttackDown:
                                this._targetGadgetOpacities[2] = 0.7;
                                break;
                            case Agent.UsageDirection.AttackLeft:
                                this._targetGadgetOpacities[3] = 0.7;
                                break;
                            case Agent.UsageDirection.AttackRight:
                                this._targetGadgetOpacities[1] = 0.7;
                                break;
                        }
                    }
                    else
                    {
                        isTargetInvalid = true;
                        switch (currentActionDirection)
                        {
                            case Agent.UsageDirection.AttackEnd:
                                this._targetGadgetOpacities[0] = 0.7;
                                break;
                            case Agent.UsageDirection.DefendDown:
                                this._targetGadgetOpacities[2] = 0.7;
                                break;
                            case Agent.UsageDirection.DefendLeft:
                                this._targetGadgetOpacities[3] = 0.7;
                                break;
                            case Agent.UsageDirection.DefendRight:
                                this._targetGadgetOpacities[1] = 0.7;
                                break;
                        }
                    }
                }
                else if (BannerlordConfig.DisplayAttackDirection)
                {
                    Agent.UsageDirection usageDirection = mainAgent.PlayerAttackDirection();
                    if (usageDirection >= Agent.UsageDirection.AttackUp && usageDirection < Agent.UsageDirection.AttackEnd)
                    {
                        if (usageDirection == Agent.UsageDirection.AttackUp)
                        {
                            this._targetGadgetOpacities[0] = 0.7;
                        }
                        else if (usageDirection == Agent.UsageDirection.AttackRight)
                        {
                            this._targetGadgetOpacities[1] = 0.7;
                        }
                        else if (usageDirection == Agent.UsageDirection.AttackDown)
                        {
                            this._targetGadgetOpacities[2] = 0.7;
                        }
                        else if (usageDirection == Agent.UsageDirection.AttackLeft)
                        {
                            this._targetGadgetOpacities[3] = 0.7;
                        }
                    }
                }
            }
            if (flag)
            {
                _crosshairVM.SetReloadProperties(new ValueTuple<float, float>[0]);
            }
            _crosshairVM.SetArrowProperties(this._targetGadgetOpacities[0], this._targetGadgetOpacities[1], this._targetGadgetOpacities[2], this._targetGadgetOpacities[3]);
            _crosshairVM.IsTargetInvalid = isTargetInvalid;

        }

        public void Show()
        {
            _crosshairVM.IsVisible = true;
        }

        public void Hide()
        {
            _crosshairVM.IsVisible = false;
        }

        private void FillReloadDurationsFromActions(ValueTuple<float, float>[] reloadPhases, Agent mainAgent, ActionIndexCache reloadAction)
        {
            float num = 0f;
            for (int i = 0; i < reloadPhases.Length; i++)
            {
                if (reloadAction != ActionIndexCache.act_none)
                {
                    float num2 = MBAnimation.GetAnimationParameter2(MBActionSet.GetAnimationIndexOfAction(mainAgent.ActionSet, reloadAction)) * MBActionSet.GetActionAnimationDuration(mainAgent.ActionSet, reloadAction);
                    reloadPhases[i].Item2 = num2;
                    if (num2 > num)
                    {
                        num = num2;
                    }
                    reloadAction = MBActionSet.GetActionAnimationContinueToAction(mainAgent.ActionSet, reloadAction);
                }
            }
            if (num > 1E-05f)
            {
                for (int j = 0; j < reloadPhases.Length; j++)
                {
                    int num3 = j;
                    reloadPhases[num3].Item2 = reloadPhases[num3].Item2 / num;
                }
            }
        }

        public void OnPhotoModeActivated()
        {
            this._weaponLayer._gauntletUIContext.ContextAlpha = 0f;
        }

        public void OnPhotoModeDeactivated()
        {
            _weaponLayer._gauntletUIContext.ContextAlpha = 1f;
        }

        private void OnCombatLogGenerated(CombatLogData logData)
        {
            bool isAttackerAgentMine = logData.IsAttackerAgentMine;
            bool flag = !logData.IsVictimAgentSameAsAttackerAgent && !logData.IsFriendlyFire;
            bool flag2 = logData.IsAttackerAgentHuman && logData.BodyPartHit == BoneBodyPartType.Head;
            if (isAttackerAgentMine && flag && logData.TotalDamage > 0)
            {
                _crosshairVM.ShowHitMarker(logData.IsFatalDamage, flag2);
            }
        }

        private const float _viewAngleConst = 0.017453292f;
        private const float _crosshairAngleConstant = 1.5707964f;
        private double[] _targetGadgetOpacities = new double[4];
        private IGauntletMovie _weaponMovie;
        private GauntletLayer _weaponLayer;
        private CrosshairVM _crosshairVM;
        private Mission _mission;
        private MissionScreen _missionScreen;

        public bool IsVisible => _crosshairVM.IsVisible;
    }
}
