using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screen;

namespace TOW_Core.Abilities.SpellBook
{
    [GameStateScreen(typeof(SpellBookState))]
    public class SpellBookScreen : ScreenBase, IGameStateListener
    {
        private GauntletLayer _gauntletLayer;
        private SpellBookVM _vm;

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            LoadingWindow.DisableGlobalLoadingWindow();
            if (this._gauntletLayer.Input.IsHotKeyDownAndReleased("Exit") || this._gauntletLayer.Input.IsGameKeyDownAndReleased(41))
            {
                this.CloseScreen();
            }
        }

        void IGameStateListener.OnActivate()
        {
            base.OnActivate();
            _vm = new SpellBookVM(CloseScreen);
            _gauntletLayer = new GauntletLayer(1, "GauntletLayer", true);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _gauntletLayer.LoadMovie("SpellBook", _vm);
            _gauntletLayer.IsFocusLayer = true;
            base.AddLayer(_gauntletLayer);
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        void IGameStateListener.OnDeactivate()
        {
            base.OnDeactivate();
        }

        void IGameStateListener.OnFinalize()
        {
            base.OnFinalize();
        }

        void IGameStateListener.OnInitialize()
        {
            base.OnInitialize();
        }

        private void CloseScreen()
        {
            Game.Current.GameStateManager.PopState(0);
        }
    }
}
