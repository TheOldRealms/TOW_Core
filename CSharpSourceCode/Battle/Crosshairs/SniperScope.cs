using System;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;

namespace TOW_Core.Battle.Crosshairs
{
    public class SniperScope
    {
        private GameEntity _scope;
        private MissionScreen _screen = ScreenManager.TopScreen as MissionScreen;

        public SniperScope()
        {
            _scope = GameEntity.Instantiate(Mission.Current.Scene, "3d_sniper_scope_1", false);
            _scope.SetVisibilityExcludeParents(false);
        }

        public void Tick()
        {
            Agent.Main.AgentVisuals.GetEntity().SetVisibilityExcludeParents(false);
            _screen.OnMainAgentWeaponChanged();
            _scope.SetGlobalFrame(_screen.CombatCamera.Frame);
        }

        public void Show()
        {
            _scope.SetVisibilityExcludeParents(true);
        }

        public void Hide()
        {
            Agent.Main.AgentVisuals.GetEntity().SetVisibilityExcludeParents(true);
            _screen.OnMainAgentWeaponChanged();
            _scope.SetVisibilityExcludeParents(false);
        }

    }
}
