using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;
using TaleWorlds.ScreenSystem;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Crosshairs
{
    public class SniperScope : ICrosshair
    {
        private bool _isVisible;
        private Agent _mainAgent = Agent.Main;
        private GameEntity _scope;
        private MissionScreen _screen = ScreenManager.TopScreen as MissionScreen;

        public bool IsVisible
        {
            get => _isVisible;
        }

        public SniperScope()
        {
            _scope = GameEntity.Instantiate(Mission.Current.Scene, "3d_sniper_scope_1", false);
            _scope.SetVisibilityExcludeParents(false);
        }

        public void Tick()
        {
            _scope.SetGlobalFrame(_screen.CombatCamera.Frame);
        }

        public void Show()
        {
            _isVisible = true;
            _mainAgent.Disappear();
            _screen.OnMainAgentWeaponChanged();
            _scope.SetVisibilityExcludeParents(true);
        }

        public void Hide()
        {
            _isVisible = false;
            _mainAgent.Appear();
            _screen.OnMainAgentWeaponChanged();
            _scope.SetVisibilityExcludeParents(false);
        }

        public void FinalizeCrosshair()
        {
            _mainAgent = null;
            _screen = null;
            _scope.FadeOut(0.5f, true);
        }
    }
}
