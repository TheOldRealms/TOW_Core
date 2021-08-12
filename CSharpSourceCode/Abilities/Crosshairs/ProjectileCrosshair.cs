using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities.Crosshairs
{
    public class ProjectileCrosshair : AbilityCrosshair
    {
        public ProjectileCrosshair(AbilityTemplate template) : base(template)
        {
            OnInitialize();
            vm.IsVisible = false;
        }
        public override void Tick()
        {
            if (!isBound)
            {
                if (agent.AgentVisuals != null)
                {
                    isBound = true;
                    agent.AgentVisuals.AddChildEntity(crosshair);
                    TOWCommon.Say("YEP");
                }
            }
            UpdateFrame();
        }
        protected void UpdateFrame()
        {

        }
        public override void Show()
        {
            vm.IsVisible = true;
        }
        public override void Hide()
        {
            vm.IsVisible = true;
        }
        private bool isBound;
        private Agent agent;

        private IGauntletMovie _movie;
        private bool _isActive;
        private GauntletLayer _layer;
        private CrosshairVM vm;

        private void OnInitialize()
        {
            if (this._isActive)
            {
                return;
            }
            this.vm = new CrosshairVM();
            this._layer = new GauntletLayer(1, "GauntletLayer", false);
            this._movie = this._layer.LoadMovie("Crosshair", vm);
            if (base.mission.Mode != MissionMode.Conversation && base.mission.Mode != MissionMode.CutScene)
            {
                base.missionScreen.AddLayer(this._layer);
            }
            this._isActive = true;
        }
        private void OnFinalize()
        {
            if (!this._isActive)
            {
                return;
            }
            this._isActive = false;
            if (base.mission.Mode != MissionMode.Conversation && base.mission.Mode != MissionMode.CutScene)
            {
                base.missionScreen.RemoveLayer(this._layer);
            }
            this.vm = null;
            this._movie = null;
            this._layer = null;
        }

        private void UpdateWeaponCrosshairVisibility()
        {
            if (BannerlordConfig.DisplayTargetingReticule && base.mission.Mode != MissionMode.Conversation && base.mission.Mode != MissionMode.CutScene && !ScreenManager.GetMouseVisibility())
            {
                Agent mainAgent = base.mission.MainAgent;
                if (!mainAgent.WieldedWeapon.IsEmpty && base.mission.MainAgent.WieldedWeapon.CurrentUsageItem.IsRangedWeapon)
                {
                    vm.IsVisible = true;
                    return;
                }
            }
            vm.IsVisible = false;
        }
        private void UpdateWeaponCrosshair()
        {
            this.vm.SetReloadProperties(new float[0]);
            double[] array = new double[4];
            bool isTargetInvalid = false;

            if (base.mission.Mode != MissionMode.Conversation &&
                base.mission.Mode != MissionMode.CutScene &&
                base.mission.Mode != MissionMode.Deployment &&
                base.mission.MainAgent != null &&
                !base.missionScreen.IsViewingChar())
            {
                this.vm.CrosshairType = BannerlordConfig.CrosshairType;
                Agent mainAgent = base.mission.MainAgent;
                double num = (double)(base.missionScreen.CameraViewAngle * 0.017453292f);
                double num2 = 2.0 * Math.Tan((double)(mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) * (0.5 / Math.Tan(num * 0.5)));
                this.vm.SetProperties(num2, (double)(1f + (base.missionScreen.CombatCamera.HorizontalFov - 1.5707964f) / 1.5707964f));
                WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand);
                float numberToCheck = MBMath.WrapAngle(mainAgent.LookDirection.AsVec2.RotationInRadians - mainAgent.GetMovementDirection().AsVec2.RotationInRadians);
            }
            this.vm.SetArrowProperties(array[0], array[1], array[2], array[3]);
            this.vm.IsTargetInvalid = isTargetInvalid;
        }
    }
}
