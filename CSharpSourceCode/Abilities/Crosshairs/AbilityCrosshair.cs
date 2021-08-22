﻿using System;
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
            _missionScreen = _mission.GetMissionBehaviour<CustomCrosshairMissionBehavior>().MissionScreen;
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
            var light = Light.CreatePointLight(_template.TargetCapturingRadius);
            light.Intensity = _template.TargetCapturingRadius / 10;
            light.LightColor = new Vec3(0.255f, 170f, 0);
            light.SetShadowType(Light.ShadowType.DynamicShadow);
            light.Frame = MatrixFrame.Identity;
            light.SetVisibility(true);
            _crosshair.AddLight(light);
        }

        protected void InitializeColors()
        {
            List<Color> colors = new List<Color>();
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

            _colors = new List<uint>();
            foreach (Color color in colors)
            {
                _colors.Add(color.ToUnsignedInteger());
            }
        }

        protected void ChangeColor()
        {
            if (_currentIndex < _colors.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }
            _crosshair.SetFactorColor(_colors[_currentIndex]);
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
        protected List<uint> _colors;
        protected MatrixFrame _currentFrame;
        protected AbilityTemplate _template;
        protected GameEntity _crosshair = GameEntity.CreateEmpty(Mission.Current.Scene);
        protected Mission _mission;
        protected MissionScreen _missionScreen;
    }
}