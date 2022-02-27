using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class ProjectileCrosshair : AbilityCrosshair
    {
        public ProjectileCrosshair(AbilityTemplate template) : base(template)
        {
            _vm = new ProjectileCrosshair_VM();
            _layer = new GauntletLayer(100, "GauntletLayer", false);
            _movie = _layer.LoadMovie("ProjectileCrosshair", _vm);
            _missionScreen.AddLayer(_layer);
        }

        public override void Tick()
        {
            if (Mission.Current.CameraIsFirstPerson)
            {
                _movie.RootWidget.MarginBottom = 0;
            }
            else
            {
                bool isZoomKeyDown = HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey(24).KeyboardKey.InputKey.IsDown();
                if (isZoomKeyDown)
                {
                    _movie.RootWidget.MarginBottom = 300;
                }
                else
                {
                    _movie.RootWidget.MarginBottom = 160;
                }
            }
        }

        public override void Show()
        {
            IsVisible = true;
        }

        public override void Hide()
        {
            IsVisible = false;
        }

        public override void Dispose()
        {
            base.Dispose();
            OnFinalize();
        }

        public override bool IsVisible
        {
            get => _vm.IsVisible;
            protected set => _vm.IsVisible = value;
        }

        private void OnFinalize()
        {
            if (_mission.Mode != MissionMode.Conversation && _mission.Mode != MissionMode.CutScene)
            {
                _missionScreen.RemoveLayer(_layer);
            }
            _vm = null;
            _movie.Release();
            _movie = null;
            _layer = null;
        }

        private IGauntletMovie _movie;

        private GauntletLayer _layer;

        private ProjectileCrosshair_VM _vm;
    }
}
