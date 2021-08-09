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
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    public class RaiseDeadInTownBehaviour : CampaignBehaviorBase
    {
        private CharacterObject _skeleton;
        private float _tickFrequency = 2f;
        private double _elapsedTime = 0;
        private int _progress = 1;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Initialize);
            CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, HourlyTick);
        }

        private void HourlyTick(Settlement obj)
        {
            if (obj == null || !obj.IsFortification) return;
            foreach(var party in obj.Parties)
            {
                if (party.LeaderHero == null || !party.LeaderHero.CanRaiseDead() || party == MobileParty.MainParty) continue;
                if (party.MemberRoster.TotalManCount < party.Party.PartySizeLimit)
                {
                    if (_skeleton != null)
                    {
                        MobileParty.MainParty.MemberRoster.AddToCounts(_skeleton, Math.Min(10,party.Party.PartySizeLimit - party.MemberRoster.TotalManCount));
                    }
                }
            }
        }

        private void Initialize(CampaignGameStarter obj)
        {
            obj.AddGameMenuOption("town", "graveyard", "Go to the graveyard", new GameMenuOption.OnConditionDelegate(raisedeadcondition), new GameMenuOption.OnConsequenceDelegate(raisedeadconsequence), false, 4, false);
            obj.AddGameMenu("graveyard", "{GRAVEYARD_INTRODUCTION}", new OnInitDelegate(graveyardmenuinit), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none, null);
            obj.AddGameMenuOption("graveyard", "attempt", "Raise dead from the corpses in the ground (wait 12 hours).", new GameMenuOption.OnConditionDelegate(attemptcondition), new GameMenuOption.OnConsequenceDelegate(attemptconsequence), false, -1, false);
            obj.AddGameMenuOption("graveyard", "leaveoption", "Leave", new GameMenuOption.OnConditionDelegate(leavecondition), delegate (MenuCallbackArgs x)
            {
                GameMenu.SwitchToMenu("town");
            }, true, -1, false);

            obj.AddWaitGameMenu("raising_dead", "The commonfolk's graves are ripe for the taking! You spend time to raise corpses from the ground. Morr is going to be furious tonight!", raisingdeadinit, raisingdeadcondition, raisingdeadconsequence, raisingdeadtick, GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption, GameOverlays.MenuOverlayType.None);
            _skeleton = MBObjectManager.Instance.GetObject<CharacterObject>("tow_skeleton_warrior");
        }

        private void raisingdeadtick(MenuCallbackArgs args, CampaignTime dt)
        {
            _elapsedTime += dt.ToHours;
            //1 every 2 hours for now. Later we want to tie this into the intelligence skill or something.
            if(_elapsedTime / _progress > _tickFrequency)
            {
                if(_skeleton != null && MobileParty.MainParty.MemberRoster.TotalManCount <= MobileParty.MainParty.Party.PartySizeLimit)
                {
                    MobileParty.MainParty.MemberRoster.AddToCounts(_skeleton, 1);
                }
                _progress += 1;
            }
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_progress/6);
        }

        private void raisingdeadconsequence(MenuCallbackArgs args)
        {
            _progress = 1;
            _elapsedTime = 0;
            GameMenu.SwitchToMenu("graveyard");
        }

        private bool raisingdeadcondition(MenuCallbackArgs args) => true;

        private void raisingdeadinit(MenuCallbackArgs args) { }

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
            var intro = new TextObject("You have arrived at {SETTLEMENT_NAME}'s Graveyard. Graves, tombstones and family crypts litter the peaceful hillside.");
            MBTextManager.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name);
            MBTextManager.SetTextVariable("GRAVEYARD_INTRODUCTION", intro);
        }

        private bool attemptcondition(MenuCallbackArgs args)
        {
            return Hero.MainHero.CanRaiseDead();
        }

        private void attemptconsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu("raising_dead");
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
