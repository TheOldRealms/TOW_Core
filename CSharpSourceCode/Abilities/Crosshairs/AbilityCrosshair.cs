using System;
using System.Collections.Generic;
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
            _template = template;
            CrosshairType = template.CrosshairType;
            _mission = Mission.Current;
        }

        public void SetMissionScreen(MissionScreen screen)
        {
            _missionScreen = screen;
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

        public virtual void Initialize()
        {

        }

        public virtual void Dispose()
        {
            _crosshair.FadeOut(3, true);
        }

        protected virtual void AddLight(Int32 amount = 1)
        {
            if (amount > 0)
            {
                for (Int32 num = 0; num < amount; num++)
                {
                    var light = Light.CreatePointLight(_template.TargetCapturingRadius);
                    light.Intensity = 1000;
                    light.Radius = _template.TargetCapturingRadius * 2;
                    light.SetShadowType(Light.ShadowType.DynamicShadow);
                    //light.SetVolumetricProperties(true, 1f);
                    light.SetVisibility(true);
                    var entity = GameEntity.CreateEmpty(_mission.Scene);
                    MatrixFrame frame;
                    if (num == 0)
                    {
                        frame = new MatrixFrame(Mat3.Identity, new Vec3(0, 0, 2));
                    }
                    else
                    {
                        frame = new MatrixFrame(Mat3.Identity, new Vec3(0, num + 2, 2));
                    }
                    entity.SetFrame(ref frame);
                    entity.AddLight(light);
                    _crosshair.AddChild(entity);
                }
            }
        }

        protected void InitializeColors()
        {
            colors = new List<Color>();
            float r = 0.255f;
            float g = 0;
            float b = 0;
            for (g = 0; g < 0.254f; g += 0.001f)
            {
                colors.Add(new Color(r, g, b));
            }
            for (r = 0.254f; r > 0.001f; r -= 0.001f)
            {
                colors.Add(new Color(r, g, b));
            }
            for (b = 0; b < 0.254f; b += 0.001f)
            {
                colors.Add(new Color(r, g, b));
            }
            for (g = 0.254f; g > 0.001f; g -= 0.001f)
            {
                colors.Add(new Color(r, g, b));
            }
            for (r = 0; r < 0.254f; r += 0.001f)
            {
                colors.Add(new Color(r, g, b));
            }
            for (b = 0.254f; b > 0.001f; b -= 0.001f)
            {
                colors.Add(new Color(r, g, b));
            }

            //_colors = new List<uint>();
            //foreach (Color color in colors)
            //{
            //    _colors.Add(color.ToUnsignedInteger());
            //}
        }

        protected void ChangeColor()
        {
            if (_currentIndex < colors.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }
            _crosshair.SetFactorColor(colors[_currentIndex].ToUnsignedInteger());

            foreach (var child in _crosshair.GetChildren())
            {
                if (child != null && child.GetLight() != null)
                {
                    child.GetLight().LightColor = colors[_currentIndex].ToVec3();
                }
            }
        }

        protected void Rotate()
        {
            _currentFrame = _crosshair.GetFrame();
            _currentFrame.rotation.RotateAboutUp(-0.001f);
            _crosshair.SetFrame(ref _currentFrame);
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
                foreach (var child in _crosshair.GetChildren())
                {
                    child.SetVisibilityExcludeParents(value);
                }
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

        protected Int32 _currentIndex;

        protected uint? friendColor = new Color(0, 0.255f, 0, 1f).ToUnsignedInteger();

        protected uint? enemyColor = new Color(0.255f, 0, 0, 1f).ToUnsignedInteger();

        protected uint? colorLess = new Color(0, 0, 0, 0).ToUnsignedInteger();

        protected List<Color> colors;

        protected List<uint> _colors;

        protected MatrixFrame _currentFrame;

        protected AbilityTemplate _template;

        protected GameEntity _crosshair = GameEntity.CreateEmpty(Mission.Current.Scene);

        protected Mission _mission;

        protected MissionScreen _missionScreen;
    }
}
