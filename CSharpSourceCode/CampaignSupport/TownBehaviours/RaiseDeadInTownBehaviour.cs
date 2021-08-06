using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Localization;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    public class RaiseDeadInTownBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Initialize);
        }

        private void Initialize(CampaignGameStarter obj)
        {
            obj.AddGameMenuOption("town", "graveyard", "Go to the graveyard.", new GameMenuOption.OnConditionDelegate(raisedeadcondition), new GameMenuOption.OnConsequenceDelegate(raisedeadconsequence), false, 4, false);
            obj.AddGameMenu("graveyard", "{GRAVEYARD_INTRODUCTION}", new OnInitDelegate(graveyardmenuinit), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none, null);
            obj.AddGameMenuOption("graveyard", "attempt", "Attempt to raise dead from the corpses in the ground. Be aware that the generic population might not be tolerant to this activity.", new GameMenuOption.OnConditionDelegate(attemptcondition), new GameMenuOption.OnConsequenceDelegate(attemptconsequence), false, -1, false);
            obj.AddGameMenuOption("graveyard", "leaveoption", "Leave", new GameMenuOption.OnConditionDelegate(leavecondition), delegate (MenuCallbackArgs x)
            {
                GameMenu.SwitchToMenu("town");
            }, true, -1, false);
        }


        private bool raisedeadcondition(MenuCallbackArgs args)
        {
            bool shouldBeDisabled;
            TextObject disabledText;
            bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroAccessLocation(Settlement.CurrentSettlement, "center", out shouldBeDisabled, out disabledText);
            disabledText = new TextObject("The Graveyard's massive iron gates are closed shut.");
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            if (canPlayerDo)
            {
                canPlayerDo = Hero.MainHero.CanRaiseDead();
            }
            return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
        }

        private void raisedeadconsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu("graveyard");
        }

        private void graveyardmenuinit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("Graveyard");
            var intro = new TextObject("You have arrived at {SETTLEMENT_NAME}'s Graveyard. The commonfolk's tombs are ripe for the taking. Morr is going to be furious tonight!");
            MBTextManager.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name);
            MBTextManager.SetTextVariable("GRAVEYARD_INTRODUCTION", intro);
        }

        private bool attemptcondition(MenuCallbackArgs args)
        {
            throw new NotImplementedException();
        }

        private void attemptconsequence(MenuCallbackArgs args)
        {
            throw new NotImplementedException();
        }

        private bool leavecondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }
    }
}
