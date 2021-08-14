using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;

namespace TOW_Core.Abilities.Crosshairs
{
    public class ProjectileCrosshair : AbilityCrosshair
    {
        public ProjectileCrosshair(AbilityTemplate template) : base(template)
        {
            OnInitialize();
            vm.IsVisible = false;
        }
       
        public override void Show()
        {
            vm.IsVisible = true;
        }
        public override void Hide()
        {
            vm.IsVisible = false;
        }
        public override void Dispose()
        {
            base.Dispose();
            OnFinalize();
        }

        private void OnInitialize()
        {
            this.vm = new ProjectileCrosshair_VM();
            this._layer = new GauntletLayer(100, "GauntletLayer", false);
            this._movie = this._layer.LoadMovie("ProjectileCrosshair", vm);
            if (base.mission.Mode != MissionMode.Conversation && base.mission.Mode != MissionMode.CutScene)
            {
                base.missionScreen.AddLayer(this._layer);
            }
        }
        private void OnFinalize()
        {
            if (base.mission.Mode != MissionMode.Conversation && base.mission.Mode != MissionMode.CutScene)
            {
                base.missionScreen.RemoveLayer(this._layer);
            }
            this.vm = null;
            this._movie = null;
            this._layer = null;
        }

        public override bool IsVisible
        {
            get => vm.IsVisible;
            protected set => vm.IsVisible = value;
        }

        private IGauntletMovie _movie;
        private GauntletLayer _layer;
        private ProjectileCrosshair_VM vm;
    }
}
