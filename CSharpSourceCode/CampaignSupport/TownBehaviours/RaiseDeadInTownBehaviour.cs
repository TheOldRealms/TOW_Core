using Helpers;
using SandBox;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOW_Core.CampaignSupport.Missions;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    public class RaiseDeadInTownBehaviour : CampaignBehaviorBase
    {
        private CharacterObject _skeleton;
        private CampaignTime _startWaitTime = CampaignTime.Now;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Initialize);
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, SettlementEntered);
        }

        private void SwitchToMenuIfThereIsAnInterrupt(string currentMenuId)
        {
            string genericStateMenu = Campaign.Current.Models.EncounterGameMenuModel.GetGenericStateMenu();
            if (genericStateMenu != currentMenuId)
            {
                if (!string.IsNullOrEmpty(genericStateMenu))
                {
                    GameMenu.SwitchToMenu(genericStateMenu);
                    return;
                }
                GameMenu.ExitToLast();
            }
        }

        private void MakeNecromancers()
        {
            foreach(var hero in Hero.AllAliveHeroes)
            {
                if(hero.Culture.StringId == "khuzait" && !hero.IsNecromancer() && (hero.IsNoble || hero.IsWanderer))
                {
                    hero.AddAttribute("Necromancer");
                }
            }
        }

        private void SettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party == null || settlement == null || hero == null || !hero.IsNecromancer() || hero.CharacterObject.IsPlayerCharacter) return;
            if (party.MemberRoster.TotalManCount < party.Party.PartySizeLimit)
            {
                if (_skeleton != null)
                {
                    party.MemberRoster.AddToCounts(_skeleton, Math.Min(20, party.Party.PartySizeLimit - party.MemberRoster.TotalManCount));
                }
            }
        }

        private void Initialize(CampaignGameStarter obj)
        {
            obj.AddGameMenuOption("town", "graveyard", "Go to the graveyard", 
                graveyardaccesscondition,
                delegate (MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("graveyard");
                }
                , false, 4, false);
            
            obj.AddGameMenu("graveyard", "{GRAVEYARD_INTRODUCTION}",
                delegate (MenuCallbackArgs args)
                {
                    args.MenuTitle = new TextObject("Graveyard");
                    var intro = new TextObject("You have arrived at {SETTLEMENT_NAME}'s Graveyard. Graves, tombstones and family crypts litter the peaceful hillside.");
                    MBTextManager.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name);
                    MBTextManager.SetTextVariable("GRAVEYARD_INTRODUCTION", intro);
                },
                GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.None, null);

            obj.AddGameMenuOption("graveyard", "raise_dead_attempt", "Raise dead from the corpses in the ground (wait 8 hours).", 
                raisedeadattemptcondition,
                delegate(MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("raising_dead");
                }
                , false, -1, false);

            obj.AddGameMenuOption("graveyard", "graveyard_leave", "Leave", 
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, 
                delegate (MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("town");
                }, true, -1, false);

            obj.AddWaitGameMenu("raising_dead", "The commonfolk's graves are ripe for the taking! You spend time to raise corpses from the ground. Morr is going to be furious tonight!", 
                delegate(MenuCallbackArgs args)
                {
                    _startWaitTime = CampaignTime.Now;
                    PlayerEncounter.Current.IsPlayerWaiting = true;
                    args.MenuContext.GameMenu.StartWait();
                }, 
                null, 
                raisingdeadconsequence, 
                raisingdeadtick, 
                GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f, GameMenu.MenuFlags.None, null);

            obj.AddGameMenuOption("raising_dead", "raising_dead_leave", "Leave",
                delegate (MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true, -1, false);

            obj.AddGameMenu("graveyard_interrupt", "{GRAVEYARD_INTERRUPT}",
                delegate (MenuCallbackArgs args)
                {
                    args.MenuTitle = new TextObject("Caught in the act");
                    var text = new TextObject("The local nightwatch is onto you. Face the consequences of your vile actions.");
                    MBTextManager.SetTextVariable("GRAVEYARD_INTERRUPT", text);
                },
                GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.None, null);

            obj.AddGameMenuOption("graveyard_interrupt", "interrupt_battle", "Defend yourself",
                delegate (MenuCallbackArgs args)
                {
                    if (Hero.MainHero.HitPoints > Hero.MainHero.MaxHitPoints * 0.2f)
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                        return true;
                    }
                    else return false;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    PlayerEncounter.RestartPlayerEncounter(PartyBase.MainParty, Settlement.CurrentSettlement.MilitiaPartyComponent.Party, true);
                    if (PlayerEncounter.Battle == null)
                    {
                        PlayerEncounter.StartBattle();
                        PlayerEncounter.Update();
                    }
                    TorMissionManager.OpenGraveyardMission();
                }, false, -1, false);

            obj.AddGameMenuOption("graveyard_interrupt", "interrupt_surrender", "Surrender",
                delegate (MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    var militiaparty = Settlement.CurrentSettlement.MilitiaPartyComponent.Party;
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    PlayerEncounter.Finish(true);
                    TakePrisonerAction.Apply(militiaparty, Hero.MainHero);
                    
                }, true, -1, false);

            _skeleton = MBObjectManager.Instance.GetObject<CharacterObject>("tor_vc_skeleton");
            MakeNecromancers();
        }

        private bool graveyardaccesscondition(MenuCallbackArgs args)
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

        private bool raisedeadattemptcondition(MenuCallbackArgs args)
        {
            if (Hero.MainHero.CanRaiseDead())
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }
            else return false;
        }

        private void raisingdeadconsequence(MenuCallbackArgs args)
        {
            PlayerEncounter.Current.IsPlayerWaiting = false;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("graveyard");
        }

        private void raisingdeadtick(MenuCallbackArgs args, CampaignTime dt)
        {
            float progress = args.MenuContext.GameMenu.Progress;
            int diff = (int)_startWaitTime.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    if (_skeleton != null && MobileParty.MainParty.MemberRoster.TotalManCount <= MobileParty.MainParty.Party.PartySizeLimit)
                    {
                        int raisePower = Math.Max(1, (int)Hero.MainHero.GetExtendedInfo().SpellCastingLevel);
                        MobileParty.MainParty.MemberRoster.AddToCounts(_skeleton, 2 * raisePower);
                    }
                    var rng = MBRandom.RandomFloatRanged(0, 1);
                    if(rng > 0.85f)
                    {
                        InterruptWait(args);
                    }
                }
            }
        }

        private void InterruptWait(MenuCallbackArgs args)
        {
            PlayerEncounter.Current.IsPlayerWaiting = false;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("graveyard_interrupt");
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }
    }
}
