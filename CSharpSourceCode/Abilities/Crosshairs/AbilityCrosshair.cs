using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Battle.CrosshairMissionBehavior;

namespace TOW_Core.Abilities.Crosshairs
{
    /// <summary>
    /// Base class for all crosshairs
    /// </summary>
    public abstract class AbilityCrosshair : IDisposable
    {
        public AbilityCrosshair(AbilityTemplate template)
        {
            this.template = template;
            this.CrosshairType = template.CrosshairType;
            this._mission = Mission.Current;
            this._missionScreen = _mission.GetMissionBehaviour<CustomCrosshairMissionBehavior>().MissionScreen;
        }
        /// <summary>
        /// Method that executes every time the mission screen ticks
        /// </summary>
        public virtual void Tick()
        {

        }
        /// <summary>
        /// Method that makes the crosshair visible
        /// </summary>
        public virtual void Show()
        {
            IsVisible = true;
        }
        /// <summary>
        /// Method that makes the crosshair invisible
        /// </summary>
        public virtual void Hide()
        {
            IsVisible = false;
        }
        
        public virtual void Dispose()
        {
            _crosshair.FadeOut(3, true);
        }

        protected void AddLight()
        {
            var light = Light.CreatePointLight(template.TargetCapturingRadius);
            light.Intensity = 100;
            light.LightColor = new Vec3(255f, 170f, 0f);
            light.SetShadowType(Light.ShadowType.DynamicShadow);
            light.Frame = MatrixFrame.Identity;
            light.SetVisibility(true);
            _crosshair.AddLight(light);
        }

        public virtual bool IsVisible
        {
            get
            {
                return this._crosshair.IsVisibleIncludeParents();
            }
            protected set
            {
                this._crosshair.SetVisibilityExcludeParents(value);
            }
        }
        public Vec3 Position
        {
            get
            {
                return this._crosshair.GlobalPosition;
            }
            protected set
            {
                MatrixFrame frame = this._crosshair.GetFrame();
                frame.origin = value;
                this._crosshair.SetFrame(ref frame);
            }
        }
        public Mat3 Rotation
        {
            get => _crosshair.GetFrame().rotation;
            protected set
            {
                MatrixFrame frame = _crosshair.GetFrame();
                frame.rotation = value;
                _crosshair.SetFrame(ref frame);
            }
        }
        public MatrixFrame Frame
        {
            get => _crosshair.GetFrame();
            protected set
            {
                _crosshair.SetFrame(ref value);
            }
        }
        public CrosshairType CrosshairType { get; }

        protected AbilityTemplate template;
        protected GameEntity _crosshair = GameEntity.CreateEmpty(Mission.Current.Scene);
        protected Mission _mission;
        protected MissionScreen _missionScreen;
        protected uint? friendColor = new Color(0, 0.255f, 0, 1f).ToUnsignedInteger();
        protected uint? enemyColor = new Color(0.255f, 0, 0, 1f).ToUnsignedInteger();
        protected uint? colorLess = new Color(0, 0, 0, 0).ToUnsignedInteger();
    }
}
