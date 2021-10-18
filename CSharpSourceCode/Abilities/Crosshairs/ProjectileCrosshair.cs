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
            get => vm.IsVisible;
            protected set => vm.IsVisible = value;
        }

        public override void Initialize()
        {
            this.vm = new ProjectileCrosshair_VM();
            this._layer = new GauntletLayer(100, "GauntletLayer", false);
            this._movie = this._layer.LoadMovie("ProjectileCrosshair", vm);
            if (base._mission.Mode != MissionMode.Conversation && base._mission.Mode != MissionMode.CutScene)
            {
                base._missionScreen.AddLayer(this._layer);
            }
        }

        private void OnFinalize()
        {
            if (base._mission.Mode != MissionMode.Conversation && base._mission.Mode != MissionMode.CutScene)
            {
                base._missionScreen.RemoveLayer(this._layer);
            }
            this.vm = null;
            this._movie = null;
            this._layer = null;
        }

        private IGauntletMovie _movie;

        private GauntletLayer _layer;

        private ProjectileCrosshair_VM vm;
    }
}
