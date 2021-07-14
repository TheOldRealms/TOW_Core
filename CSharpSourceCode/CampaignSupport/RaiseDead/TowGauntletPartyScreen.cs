using SandBox.GauntletUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;

namespace TOW_Core.CampaignSupport.RaiseDead
{
    public class TowGauntletPartyScreen : GauntletPartyScreen, IGameStateListener
    {
        private GauntletLayer _gauntletLayer;
        private TowPartyVm _dataSource;
        private PartyState _partyState;
        private SpriteCategory _partyscreenCategory;

        public TowGauntletPartyScreen(PartyState partyState) : base(partyState)
        {
            _partyState = partyState;
            _partyState.Listener = this;
        }

        protected override void OnFrameTick(float dt)
        {
            LoadingWindow.DisableGlobalLoadingWindow();
            this._dataSource.IsFiveStackModifierActive = this._gauntletLayer.Input.IsHotKeyDown("FiveStackModifier");
            this._dataSource.IsEntireStackModifierActive = this._gauntletLayer.Input.IsHotKeyDown("EntireStackModifier");
            if (!this._partyState.IsActive || this._gauntletLayer.Input.IsHotKeyReleased("Exit") || (!this._gauntletLayer.Input.IsControlDown() && this._gauntletLayer.Input.IsGameKeyPressed(42)))
            {
                this.HandleCancelInput();
                return;
            }
            if (this._gauntletLayer.Input.IsHotKeyReleased("Confirm"))
            {
                this.HandleDoneInput();
                return;
            }
            if (!this._dataSource.IsAnyPopUpOpen)
            {
                if (this._gauntletLayer.Input.IsHotKeyPressed("TakeAllTroops"))
                {
                    this._dataSource.ExecuteTransferAllOtherTroops();
                    return;
                }
                if (this._gauntletLayer.Input.IsHotKeyPressed("GiveAllTroops"))
                {
                    this._dataSource.ExecuteTransferAllMainTroops();
                    return;
                }
                if (this._gauntletLayer.Input.IsHotKeyPressed("TakeAllPrisoners"))
                {
                    this._dataSource.ExecuteTransferAllOtherPrisoners();
                    return;
                }
                if (this._gauntletLayer.Input.IsHotKeyPressed("GiveAllPrisoners"))
                {
                    this._dataSource.ExecuteTransferAllMainPrisoners();
                }
            }
        }

        void IGameStateListener.OnActivate()
        {
            base.OnActivate();
            Layers.ToList().ForEach(layer => RemoveLayer(layer));
            SpriteData spriteData = UIResourceManager.SpriteData;
            TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
            ResourceDepot uiresourceDepot = UIResourceManager.UIResourceDepot;
            this._partyscreenCategory = spriteData.SpriteCategories["ui_partyscreen"];
            this._partyscreenCategory.Load(resourceContext, uiresourceDepot);

            SetUpDataSource();
            _partyState.Handler = _dataSource;
            _gauntletLayer = new GauntletLayer(1, "GauntletLayer", true);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("PartyHotKeyCategory"));
            AddLayer(_gauntletLayer);
            _gauntletLayer.LoadMovie("TowPartyScreen", _dataSource);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        private void SetUpDataSource()
        {
            _dataSource = new TowPartyVm(Game.Current, _partyState.PartyScreenLogic, GetFiveStackShortcutkeyText(), GetEntireStackShortcutkeyText());
            _dataSource.SetCancelInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").RegisteredHotKeys.FirstOrDefault((HotKey g) => ((g != null) ? g.Id : null) == "Exit"));
            _dataSource.SetDoneInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").RegisteredHotKeys.FirstOrDefault((HotKey g) => ((g != null) ? g.Id : null) == "Confirm"));
            _dataSource.SetTakeAllTroopsInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").RegisteredHotKeys.FirstOrDefault((HotKey g) => ((g != null) ? g.Id : null) == "TakeAllTroops"));
            _dataSource.SetDismissAllTroopsInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").RegisteredHotKeys.FirstOrDefault((HotKey g) => ((g != null) ? g.Id : null) == "GiveAllTroops"));
            _dataSource.SetTakeAllPrisonersInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").RegisteredHotKeys.FirstOrDefault((HotKey g) => ((g != null) ? g.Id : null) == "TakeAllPrisoners"));
            _dataSource.SetDismissAllPrisonersInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").RegisteredHotKeys.FirstOrDefault((HotKey g) => ((g != null) ? g.Id : null) == "GiveAllPrisoners"));
            _dataSource.SetTakeAllRaiseDeadInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").RegisteredHotKeys.FirstOrDefault((HotKey g) => ((g != null) ? g.Id : null) == "TakeAllRaiseDead"));
        }

        private string GetFiveStackShortcutkeyText()
        {
            if (!Input.IsControllerConnected || Input.IsMouseActive)
            {
                return Module.CurrentModule.GlobalTextManager.FindText("str_game_key_text", "anyshift").ToString();
            }
            return string.Empty;
        }

        private string GetEntireStackShortcutkeyText()
        {
            if (!Input.IsControllerConnected || Input.IsMouseActive)
            {
                return Module.CurrentModule.GlobalTextManager.FindText("str_game_key_text", "anycontrol").ToString();
            }
            return null;
        }

        private void HandleCancelInput()
        {
            PartyUpgradeTroopVM upgradePopUp = this._dataSource.UpgradePopUp;
            if (upgradePopUp != null && upgradePopUp.IsOpen)
            {
                this._dataSource.UpgradePopUp.ExecuteCancel();
                return;
            }
            this._partyState.PartyScreenLogic.Reset();
            PartyScreenManager.CloseScreen(false, true);
        }

        private void HandleDoneInput()
        {
            PartyUpgradeTroopVM upgradePopUp = this._dataSource.UpgradePopUp;
            if (upgradePopUp != null && upgradePopUp.IsOpen)
            {
                this._dataSource.UpgradePopUp.ExecuteDone();
                return;
            }
            this._dataSource.ExecuteDone();
        }
    }
}
