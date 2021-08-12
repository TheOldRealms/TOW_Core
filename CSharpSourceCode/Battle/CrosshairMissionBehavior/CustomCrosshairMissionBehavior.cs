using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TOW_Core.Abilities;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.CrosshairMissionBehavior
{
    [OverrideView(typeof(MissionCrosshair))]
    public class CustomCrosshairMissionBehavior : MissionView
    {
        private bool isMainAgentChecked;
        private IGauntletMovie _movie;
        private bool _isActive;
        private GauntletLayer _layer;
        private CrosshairVM weaponCrosshair;
        private AbilityCrosshair currentAbilityCrosshair;
        private AbilityComponent abilityComponent;

        private void OnInitialize()
        {
            if (this._isActive)
            {
                return;
            }
            CombatLogManager.OnGenerateCombatLog += this.OnCombatLogGenerated;
            this.weaponCrosshair = new CrosshairVM();
            this.currentAbilityCrosshair = null;
            this._layer = new GauntletLayer(1, "GauntletLayer", false);
            this._movie = this._layer.LoadMovie("Crosshair", this.weaponCrosshair);
            if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
            {
                base.MissionScreen.AddLayer(this._layer);
            }
            this._isActive = true;
        }
        private void OnFinalize()
        {
            if (!this._isActive)
            {
                return;
            }
            CombatLogManager.OnGenerateCombatLog -= this.OnCombatLogGenerated;
            this._isActive = false;
            if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
            {
                base.MissionScreen.RemoveLayer(this._layer);
            }
            this.weaponCrosshair = null;
            this.currentAbilityCrosshair = null;
            this._movie = null;
            this._layer = null;
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (MBEditor.EditModeEnabled && !this._isActive)
            {
                return;
            }
            if (abilityComponent != null && abilityComponent.IsAbilityModeOn)
            {
                if (weaponCrosshair.IsVisible)
                    weaponCrosshair.IsVisible = false;
                if (currentAbilityCrosshair != null)
                {
                    if (abilityComponent.CurrentAbility.IsOnCooldown())
                    {
                        if (currentAbilityCrosshair.IsVisible)
                            currentAbilityCrosshair.Hide();
                    }
                    else
                    {
                        currentAbilityCrosshair.Tick();
                        if (!currentAbilityCrosshair.IsVisible)
                            currentAbilityCrosshair.Show();
                    }
                }
            }
            else if (!isMainAgentChecked && Agent.Main != null)
            {
                isMainAgentChecked = true;
                if((abilityComponent = Agent.Main.GetComponent<AbilityComponent>()) != null)
                {
                    abilityComponent.CurrentAbilityChanged += (crosshair) =>
                    {
                        currentAbilityCrosshair = crosshair;
                    };
                }
            }
            else
            {
                UpdateWeaponCrosshair();
                UpdateWeaponCrosshairVisibility();
            }
        }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this.OnInitialize();
        }
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            this.OnFinalize();
        }

        private void UpdateWeaponCrosshairVisibility()
        {
            if (BannerlordConfig.DisplayTargetingReticule && base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene && !ScreenManager.GetMouseVisibility())
            {
                Agent mainAgent = base.Mission.MainAgent;
                if (mainAgent != null && (currentAbilityCrosshair == null || !currentAbilityCrosshair.IsVisible))
                {
                    if (!mainAgent.WieldedWeapon.IsEmpty && base.Mission.MainAgent.WieldedWeapon.CurrentUsageItem.IsRangedWeapon)
                    {
                        if (!base.MissionScreen.IsViewingChar() && !this.IsMissionScreenUsingCustomCamera())
                        {
                            weaponCrosshair.IsVisible = true;
                            return;
                        }
                    }
                }
            }
            weaponCrosshair.IsVisible = false;
        }
        private void UpdateWeaponCrosshair()
        {
            this.weaponCrosshair.SetReloadProperties(new float[0]);
            double[] array = new double[4];
            bool isTargetInvalid = false;

            if (base.Mission.Mode != MissionMode.Conversation &&
                base.Mission.Mode != MissionMode.CutScene &&
                base.Mission.Mode != MissionMode.Deployment &&
                base.Mission.MainAgent != null &&
                !base.MissionScreen.IsViewingChar() &&
                !this.IsMissionScreenUsingCustomCamera())
            {
                this.weaponCrosshair.CrosshairType = BannerlordConfig.CrosshairType;
                Agent mainAgent = base.Mission.MainAgent;
                double num = (double)(base.MissionScreen.CameraViewAngle * 0.017453292f);
                double num2 = 2.0 * Math.Tan((double)(mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) * (0.5 / Math.Tan(num * 0.5)));
                this.weaponCrosshair.SetProperties(num2, (double)(1f + (base.MissionScreen.CombatCamera.HorizontalFov - 1.5707964f) / 1.5707964f));
                WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand);
                float numberToCheck = MBMath.WrapAngle(mainAgent.LookDirection.AsVec2.RotationInRadians - mainAgent.GetMovementDirection().AsVec2.RotationInRadians);
                if (wieldedWeaponInfo != null && wieldedWeaponInfo.IsRangedWeapon && BannerlordConfig.DisplayTargetingReticule)
                {
                    Agent.ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);
                    if (mainAgent.WieldedWeapon.Item.Type == ItemObject.ItemTypeEnum.Crossbow && currentActionType == Agent.ActionCodeType.Reload)
                    {
                        bool flag = mainAgent.WieldedWeapon.Item.Type == ItemObject.ItemTypeEnum.Crossbow;
                        float[] array2 = flag ? new float[2] : new float[1];
                        Agent.ActionStage currentActionStage = mainAgent.GetCurrentActionStage(1);
                        int num3 = (!flag) ? 0 : ((currentActionStage == Agent.ActionStage.ReloadPhase2) ? 1 : 0);
                        if (flag && currentActionStage == Agent.ActionStage.ReloadPhase2)
                        {
                            array2[0] = 1f;
                        }
                        array2[num3] = mainAgent.GetCurrentActionProgress(1);
                        if (mainAgent.GetCurrentAction(1).Index != -1)
                        {
                            float num4 = 1f - MBActionSet.GetActionBlendOutStartProgress(mainAgent.ActionSet, mainAgent.GetCurrentAction(1));
                            array2[num3] = mainAgent.GetCurrentActionProgress(1) + num4;
                        }
                        this.weaponCrosshair.SetReloadProperties(array2);
                    }
                    if (currentActionType == Agent.ActionCodeType.ReadyRanged)
                    {
                        Vec2 bodyRotationConstraint = mainAgent.GetBodyRotationConstraint(1);
                        isTargetInvalid = (base.Mission.MainAgent.MountAgent != null && !MBMath.IsBetween(numberToCheck, bodyRotationConstraint.x, bodyRotationConstraint.y) && (bodyRotationConstraint.x < -0.1f || bodyRotationConstraint.y > 0.1f));
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
                                    array[0] = 0.7;
                                    break;
                                case Agent.UsageDirection.AttackDown:
                                    array[2] = 0.7;
                                    break;
                                case Agent.UsageDirection.AttackLeft:
                                    array[3] = 0.7;
                                    break;
                                case Agent.UsageDirection.AttackRight:
                                    array[1] = 0.7;
                                    break;
                            }
                        }
                        else
                        {
                            isTargetInvalid = true;
                            switch (currentActionDirection)
                            {
                                case Agent.UsageDirection.AttackEnd:
                                    array[0] = 0.7;
                                    break;
                                case Agent.UsageDirection.DefendDown:
                                    array[2] = 0.7;
                                    break;
                                case Agent.UsageDirection.DefendLeft:
                                    array[3] = 0.7;
                                    break;
                                case Agent.UsageDirection.DefendRight:
                                    array[1] = 0.7;
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
                                array[0] = 0.7;
                            }
                            else if (usageDirection == Agent.UsageDirection.AttackRight)
                            {
                                array[1] = 0.7;
                            }
                            else if (usageDirection == Agent.UsageDirection.AttackDown)
                            {
                                array[2] = 0.7;
                            }
                            else if (usageDirection == Agent.UsageDirection.AttackLeft)
                            {
                                array[3] = 0.7;
                            }
                        }
                    }
                }
            }
            this.weaponCrosshair.SetArrowProperties(array[0], array[1], array[2], array[3]);
            this.weaponCrosshair.IsTargetInvalid = isTargetInvalid;
        }

        private bool IsMissionScreenUsingCustomCamera()
        {
            return base.MissionScreen.CustomCamera != null;
        }
        private void OnCombatLogGenerated(CombatLogData logData)
        {
            bool isAttackerAgentMine = logData.IsAttackerAgentMine;
            bool flag = !logData.IsVictimAgentSameAsAttackerAgent && !logData.IsFriendlyFire;
            bool flag2 = logData.IsAttackerAgentHuman && logData.BodyPartHit == BoneBodyPartType.Head;
            if (isAttackerAgentMine && flag && logData.TotalDamage > 0)
            {
                this.weaponCrosshair.ShowHitMarker(logData.IsFatalDamage, flag2);
            }
        }
        public override void OnPhotoModeActivated()
        {
            base.OnPhotoModeActivated();
            this._layer._gauntletUIContext.ContextAlpha = 0f;
        }
        public override void OnPhotoModeDeactivated()
        {
            base.OnPhotoModeDeactivated();
            this._layer._gauntletUIContext.ContextAlpha = 1f;
        }
    }
}
