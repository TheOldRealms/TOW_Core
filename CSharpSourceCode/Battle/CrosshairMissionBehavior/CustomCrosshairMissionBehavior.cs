using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TOW_Core.Abilities;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.CrosshairMissionBehavior
{
    [OverrideView(typeof(MissionCrosshair))]
    public class CustomCrosshairMissionBehavior : MissionView
    {
        private bool isMainAgentChecked;
        private IGauntletMovie weaponMovie;
        private bool _isActive;
        private GauntletLayer weaponLayer;
        private CrosshairVM weaponCrosshair;
        private AbilityCrosshair abilityCrosshair;
        private AbilityComponent abilityComponent;

        private void OnInitializeWeaponCrosshair()
        {
            if (this._isActive)
            {
                return;
            }
            CombatLogManager.OnGenerateCombatLog += this.OnCombatLogGenerated;
            this.weaponCrosshair = new CrosshairVM();
            this.abilityCrosshair = null;
            this.weaponLayer = new GauntletLayer(1, "GauntletLayer", false);
            this.weaponMovie = this.weaponLayer.LoadMovie("Crosshair", this.weaponCrosshair);
            if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
            {
                base.MissionScreen.AddLayer(this.weaponLayer);
            }
            this._isActive = true;
        }

        private void OnFinalizeWeaponCrosshair()
        {
            if (!this._isActive)
            {
                return;
            }
            CombatLogManager.OnGenerateCombatLog -= this.OnCombatLogGenerated;
            this._isActive = false;
            if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
            {
                base.MissionScreen.RemoveLayer(this.weaponLayer);
            }
            this.weaponCrosshair = null;
            this.abilityCrosshair = null;
            this.weaponMovie = null;
            this.weaponLayer = null;
        }

        public override void OnMissionScreenTick(float dt)
        {
            if (MBEditor.EditModeEnabled && !this._isActive)
            {
                return;
            }
            if (CanUseCrosshair())
            {
                if (!isMainAgentChecked)
                {
                    isMainAgentChecked = true;
                    if ((abilityComponent = Agent.Main.GetComponent<AbilityComponent>()) != null)
                    {
                        abilityComponent.CurrentAbilityChanged += (crosshair) =>
                        {
                            abilityCrosshair = crosshair;
                        };
                    }
                }
                else if (IsUsingAbility())
                {
                    weaponCrosshair.IsVisible = false;
                    UpdateAbilityCrosshairVisibility();
                }
                else
                {
                    if (abilityCrosshair != null)
                    {
                        abilityCrosshair.Hide();
                    }
                    UpdateWeaponCrosshair();
                    UpdateWeaponCrosshairVisibility();
                }
            }
            else
            {
                if (abilityCrosshair != null)
                {
                    abilityCrosshair.Hide();
                }
                weaponCrosshair.IsVisible = false;
            }
        }

        private bool CanUseCrosshair()
        {
            return Agent.Main != null &&
                   Agent.Main.State == AgentState.Active &&
                   Mission.Mode != MissionMode.Conversation &&
                   Mission.Mode != MissionMode.Deployment &&
                   Mission.Mode != MissionMode.CutScene &&
                   MissionScreen != null &&
                   MissionScreen.CustomCamera == null &&
                   (MissionScreen.OrderFlag == null || !MissionScreen.OrderFlag.IsVisible) &&
                   !MissionScreen.IsViewingCharacter() &&
                   BannerlordConfig.DisplayTargetingReticule &&
                   !ScreenManager.GetMouseVisibility();
        }

        private bool IsUsingAbility()
        {
            return abilityComponent != null && abilityComponent.IsAbilityModeOn;
        }

        public override void OnMissionScreenInitialize()
        {
            OnInitializeWeaponCrosshair();
        }

        public override void OnMissionScreenFinalize()
        {
            OnFinalizeWeaponCrosshair();
        }

        private void UpdateAbilityCrosshairVisibility()
        {
            if (abilityCrosshair == null)
            {
                if (abilityComponent.CurrentAbility.Crosshair != null)
                {
                    abilityCrosshair = abilityComponent.CurrentAbility.Crosshair;
                }
            }
            else
            {
                if (!abilityComponent.CurrentAbility.CanCast(Agent.Main))
                {
                    abilityCrosshair.Hide();
                }
                else
                {
                    if (abilityCrosshair.CrosshairType == CrosshairType.CenteredAOE || IsRightAngleToCast())
                    {
                        abilityCrosshair.Tick();
                        abilityCrosshair.Show();
                    }
                    else
                    {
                        abilityCrosshair.Hide();
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
                weaponCrosshair.IsVisible = true;
                return;
            }
            weaponCrosshair.IsVisible = false;
        }

        private void UpdateWeaponCrosshair()
        {
            this.weaponCrosshair.SetReloadProperties(new float[0]);
            double[] array = new double[4];
            bool isTargetInvalid = false;
            this.weaponCrosshair.CrosshairType = BannerlordConfig.CrosshairType;
            Agent mainAgent = base.Mission.MainAgent;
            double num = (double)(base.MissionScreen.CameraViewAngle * 0.017453292f);
            double num2 = 2.0 * Math.Tan((double)(mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) * (0.5 / Math.Tan(num * 0.5)));
            this.weaponCrosshair.SetProperties(num2, (double)(1f + (base.MissionScreen.CombatCamera.HorizontalFov - 1.5707964f) / 1.5707964f));
            WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand);
            float numberToCheck = MBMath.WrapAngle(mainAgent.LookDirection.AsVec2.RotationInRadians - mainAgent.GetMovementDirection().RotationInRadians);
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
            this.weaponCrosshair.SetArrowProperties(array[0], array[1], array[2], array[3]);
            this.weaponCrosshair.IsTargetInvalid = isTargetInvalid;
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
            weaponLayer._gauntletUIContext.ContextAlpha = 0f;
        }

        public override void OnPhotoModeDeactivated()
        {
            weaponLayer._gauntletUIContext.ContextAlpha = 1f;
        }
    }
}
