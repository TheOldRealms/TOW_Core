using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.SpellBook
{
    [GameStateScreen(typeof(SpellBookState))]
    public class SpellBookScreen : ScreenBase, IGameStateListener
    {
        private GauntletLayer _gauntletLayer;
        private SpellBookVM _vm;
        private SpellBookState _state;

        public SpellBookScreen(SpellBookState state)
        {
            _state = state;
            _state.Listener = this;
        }
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
            var heroes = MobileParty.MainParty.GetSpellCasterMemberHeroes();
            if(heroes.Count == 0) heroes.Add(Hero.MainHero);
            _vm = new SpellBookVM(CloseScreen, heroes, _state.IsTrainerMode, _state.TrainerCulture);
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
            base.RemoveLayer(_gauntletLayer);
            _gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
        }

        void IGameStateListener.OnFinalize()
        {
            _gauntletLayer = null;
            _vm = null;
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
