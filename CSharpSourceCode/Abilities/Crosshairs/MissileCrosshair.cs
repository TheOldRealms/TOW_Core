using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;

namespace TOW_Core.Abilities.Crosshairs
{
    public class MissileCrosshair : AbilityCrosshair
    {
        public MissileCrosshair(AbilityTemplate template) : base(template)
        {
            _vm = new ProjectileCrosshair_VM();
            _layer = new GauntletLayer(100, "GauntletLayer", false);
            _movie = _layer.LoadMovie("ProjectileCrosshair", _vm);
            _missionScreen.AddLayer(_layer);
        }

        public override void Tick()
        {
            if (_mission.CameraIsFirstPerson)
            {
                _movie.RootWidget.MarginBottom = 0;
            }
            else
            {
                // check if zoom is pressed
                if (_missionScreen.InputManager.IsGameKeyDown(24))
                {
                    _movie.RootWidget.MarginBottom = 330;
                }
                else
                {
                    _movie.RootWidget.MarginBottom = 175;
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
