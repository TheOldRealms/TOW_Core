using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Battle.CrosshairMissionBehavior;

namespace TOW_Core.Abilities.Crosshairs
{
    public abstract class AbilityCrosshair : IDisposable
    {
        public AbilityCrosshair(AbilityTemplate template)
        {
            this.template = template;
            this.CrosshairType = template.CrosshairType;
            this.mission = Mission.Current;
            this.missionScreen = mission.GetMissionBehaviour<CustomCrosshairMissionBehavior>().MissionScreen;
        }

        public virtual void Tick()
        {

        }
        public virtual void Show()
        {
            IsVisible = true;
        }
        public virtual void Hide()
        {
            IsVisible = false;
        }
        public virtual void Dispose()
        {
            crosshair.FadeOut(3, true);
        }
        protected void AddLight()
        {
            var light = Light.CreatePointLight(template.TargetCapturingRadius);
            light.Intensity = 100;
            light.LightColor = new Vec3(255f, 170f, 0f);
            light.SetShadowType(Light.ShadowType.DynamicShadow);
            light.Frame = MatrixFrame.Identity;
            light.SetVisibility(true);
            crosshair.AddLight(light);
        }

        public virtual bool IsVisible
        {
            get
            {
                return this.crosshair.IsVisibleIncludeParents();
            }
            protected set
            {
                this.crosshair.SetVisibilityExcludeParents(value);
            }
        }
        public Vec3 Position
        {
            get
            {
                return this.crosshair.GlobalPosition;
            }
            protected set
            {
                MatrixFrame frame = this.crosshair.GetFrame();
                frame.origin = value;
                this.crosshair.SetFrame(ref frame);
            }
        }
        public CrosshairType CrosshairType { get; }

        protected AbilityTemplate template;
        protected GameEntity crosshair = GameEntity.CreateEmpty(Mission.Current.Scene);
        protected Mission mission;
        protected MissionScreen missionScreen;
        protected uint? friendColor = new Color(0, 0.255f, 0, 1f).ToUnsignedInteger();
        protected uint? enemyColor = new Color(0.255f, 0, 0, 1f).ToUnsignedInteger();
        protected uint? colorLess = new Color(0, 0, 0, 0).ToUnsignedInteger();
    }
}
