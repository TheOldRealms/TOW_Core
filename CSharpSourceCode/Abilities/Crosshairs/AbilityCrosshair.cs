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
        public AbilityCrosshair()
        {
            this.mission = Mission.Current;
            this.missionScreen = mission.GetMissionBehaviour<CustomCrosshairMissionBehavior>().MissionScreen;
        }
        public virtual void Tick()
        {
        }
        protected void AddLight()
        {
            //crosshair.SetFactorColor(new Color(0.255f, 0, 0, 1f).ToUnsignedInteger());
            var light = Light.CreatePointLight(2);
            light.Intensity = 100;
            light.LightColor = new Vec3(255f, 170f, 0f);
            light.SetShadowType(Light.ShadowType.DynamicShadow);
            light.ShadowEnabled = true;
            //light.SetLightFlicker(Template.LightFlickeringMagnitude, Template.LightFlickeringInterval);
            light.Frame = MatrixFrame.Identity;
            light.SetVisibility(true);
            crosshair.AddLight(light);
        }

        public bool IsVisible
        {
            get
            {
                return this.crosshair.IsVisibleIncludeParents();
            }
            set
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
        public void Dispose()
        {
            crosshair.FadeOut(3, true);
        }

        protected GameEntity crosshair = GameEntity.CreateEmpty(Mission.Current.Scene);
        protected Mission mission;
        protected MissionScreen missionScreen;
        protected uint? friendColor = new Color(0, 0.255f, 0, 1f).ToUnsignedInteger();
        protected uint? enemyColor = new Color(0.255f, 0, 0, 1f).ToUnsignedInteger();
        protected uint? colorLess = new Color(0, 0, 0, 0).ToUnsignedInteger();
    }
}
