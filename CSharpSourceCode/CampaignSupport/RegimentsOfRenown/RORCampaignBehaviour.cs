using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TOW_Core.Utilities.Extensions;
using static TaleWorlds.CampaignSystem.Overlay.GameOverlays;

namespace TOW_Core.CampaignSupport.RegimentsOfRenown
{
    public class RORCampaignBehaviour : CampaignBehaviorBase
    {
        private Dictionary<Settlement, Dictionary<CharacterObject, int>> _rorSettlementDetails;

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, OnNewGame);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, SettlementEntered);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            obj.AddGameMenuOption("town", "ror_enter", "Regiments of Renown Recruitment",
                delegate (MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return Settlement.CurrentSettlement.IsRoRSettlement();
                },
                delegate (MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("ror_center");
                }
                , false, 4, false);

            obj.AddGameMenuOption("village", "ror_enter", "Regiments of Renown Recruitment",
                delegate (MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return Settlement.CurrentSettlement.IsRoRSettlement() && !Settlement.CurrentSettlement.IsRaided && !Settlement.CurrentSettlement.IsUnderRaid;
                },
                delegate (MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("ror_center");
                }
                , false, 4, false);

            obj.AddGameMenu("ror_center", "{ROR_HEADER}", centerinit, MenuOverlayType.SettlementWithBoth);

            obj.AddGameMenuOption("ror_center", "ror_recruit_1", "{LINETEXT}", 
                delegate(MenuCallbackArgs args) 
                {
                    return IsRecruitmentOptionValid(args, 0);
                },
                delegate (MenuCallbackArgs args)
                {
                    TryRecruitTroopsAtIndex(0);
                },
                false, -1, false);

            obj.AddGameMenuOption("ror_center", "ror_recruit_2", "{LINETEXT}",
                delegate (MenuCallbackArgs args)
                {
                    return IsRecruitmentOptionValid(args, 1);
                },
                delegate (MenuCallbackArgs args)
                {
                    TryRecruitTroopsAtIndex(1);
                },
                false, -1, false);

            obj.AddGameMenuOption("ror_center", "ror_recruit_3", "{LINETEXT}",
                delegate (MenuCallbackArgs args)
                {
                    return IsRecruitmentOptionValid(args, 2);
                },
                delegate (MenuCallbackArgs args)
                {
                    TryRecruitTroopsAtIndex(2);
                },
                false, -1, false);

            obj.AddGameMenuOption("ror_center", "ror_leave", "Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    SetGenericStateMenu(args.MenuContext.GameMenu.StringId);
                }, 
                false, -1, false);
        }

        private bool IsRecruitmentOptionValid(MenuCallbackArgs args, int index)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            if (Clan.PlayerClan.Tier < 3) return false;
            var kvp = _rorSettlementDetails[Settlement.CurrentSettlement].ElementAtOrDefault(index);
            if (kvp.Value > 0)
            {
                var cost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(kvp.Key, Hero.MainHero, false);
                cost *= kvp.Value;
                MBTextManager.SetTextVariable("MEN_COUNT", kvp.Value);
                MBTextManager.SetTextVariable("TROOP_NAME", kvp.Key.Name);
                MBTextManager.SetTextVariable("TOTAL_AMOUNT", cost);
                MBTextManager.SetTextVariable("LINETEXT", "Recruit {MEN_COUNT} {TROOP_NAME} ({TOTAL_AMOUNT}{GOLD_ICON})");
                return true;
            }
            else return false;
        }

        private void centerinit(MenuCallbackArgs args)
        {
            var template = Settlement.CurrentSettlement.GetRoRTemplate();
            args.MenuTitle = new TextObject(template.RegimentHQName);
            var intro = new TextObject("You have arrived at the {HQ_NAME}. {NEWLINE} {NEWLINE} {BLURB} {NEWLINE} {NEWLINE} {REFUSE} {NEWLINE} {NEWLINE} {EMPTY}");
            MBTextManager.SetTextVariable("HQ_NAME", template.RegimentHQName);
            MBTextManager.SetTextVariable("BLURB", template.MenuHeaderText);
            if(Clan.PlayerClan.Tier < 3) MBTextManager.SetTextVariable("REFUSE", "They refuse to deal with an unknown clan like yours. Come back when you have some renown.");
            else MBTextManager.SetTextVariable("REFUSE", " ");
            if (!HasAvailableRoRUnits(Settlement.CurrentSettlement)) MBTextManager.SetTextVariable("EMPTY", "Currently there are no available Regiments of Renown to recruit. Check back in a week.");
            else MBTextManager.SetTextVariable("EMPTY", " ");
            MBTextManager.SetTextVariable("ROR_HEADER", intro);
        }

        private void WeeklyTick() => FillSettlements();

        private void SettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty != null &&
                mobileParty.IsLordParty && 
                !mobileParty.IsDisbanding && 
                mobileParty.LeaderHero != null && 
                !mobileParty.Party.IsStarving &&
                mobileParty.MapFaction.IsKingdomFaction &&
                settlement.IsRoRSettlement() &&
                mobileParty.Party.NumberOfAllMembers < mobileParty.LimitedPartySize &&
                (mobileParty.UnlimitedWage || mobileParty.PaymentLimit >= mobileParty.TotalWage) &&
                mobileParty.LeaderHero.Gold > HeroHelper.StartRecruitingMoneyLimit(mobileParty.LeaderHero) && 
                (mobileParty.LeaderHero == mobileParty.LeaderHero.Clan.Leader || mobileParty.LeaderHero.Clan.Gold > HeroHelper.StartRecruitingMoneyLimitForClanLeader(mobileParty.LeaderHero)))
            {
                for(int i = 0; i < _rorSettlementDetails[settlement].Count; i++)
                {
                    var kvp = _rorSettlementDetails[settlement].ElementAt(i);
                    if (mobileParty.Party.NumberOfAllMembers < mobileParty.LimitedPartySize && kvp.Value > 0)
                    {
                        var cost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(kvp.Key, mobileParty.LeaderHero, false);
                        var num = MathF.Min(mobileParty.LimitedPartySize - mobileParty.Party.NumberOfAllMembers, kvp.Value);
                        cost *= num;
                        if (mobileParty.LeaderHero.Gold > cost)
                        {
                            GiveGoldAction.ApplyBetweenCharacters(mobileParty.LeaderHero, null, cost);
                            mobileParty.AddElementToMemberRoster(kvp.Key, num);
                            _rorSettlementDetails[settlement][kvp.Key] -= num;
                            CampaignEventDispatcher.Instance.OnTroopRecruited(mobileParty.LeaderHero, settlement, settlement.Notables.FirstOrDefault(), kvp.Key, num);
                        }
                    }
                }
            }
        }

        private void OnNewGame(CampaignGameStarter arg1, int arg2)
        {
            _rorSettlementDetails = new Dictionary<Settlement, Dictionary<CharacterObject, int>>();
            foreach(var settlement in Settlement.All)
            {
                if (settlement.IsRoRSettlement())
                {
                    _rorSettlementDetails.Add(settlement, new Dictionary<CharacterObject, int>());
                }
            }
            FillSettlements();
        }

        private void FillSettlements()
        {
            foreach(var settlement in _rorSettlementDetails.Keys)
            {
                var dictionary = _rorSettlementDetails[settlement];
                dictionary.Clear();
                var template = settlement.GetRoRTemplate();
                var basetroop = MBObjectManager.Instance.GetObject<CharacterObject>(template.BaseTroopId);
                List<CharacterObject> list = new List<CharacterObject>();
                list.Add(basetroop);
                list.AddRange(basetroop.UpgradeTargets);
                var selected = list.TakeRandom(3);
                foreach(var item in selected)
                {
                    dictionary.Add(item, GetWeeklyTroopNumber(item.Tier));
                }
            }
        }

        private int GetWeeklyTroopNumber(int tier)
        {
            float ratio = 10 / tier;
            return MBRandom.RandomInt(1, (int)ratio * 3);
        }

        private bool HasAvailableRoRUnits(Settlement settlement)
        {
            Dictionary<CharacterObject, int> dictionary;
            _rorSettlementDetails.TryGetValue(settlement, out dictionary);
            if(dictionary != null)
            {
                return dictionary.Any(x => x.Value > 0);
            }
            return false;
        }

        private void TryRecruitTroopsAtIndex(int index)
        {
            var kvp = _rorSettlementDetails[Settlement.CurrentSettlement].ElementAt(index);
            var cost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(kvp.Key, Hero.MainHero, false);
            cost *= kvp.Value;
            if(Hero.MainHero.Gold >= cost)
            {
                if((MobileParty.MainParty.LimitedPartySize - MobileParty.MainParty.Party.NumberOfAllMembers) >= kvp.Value)
                {
                    GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, cost);
                    MobileParty.MainParty.AddElementToMemberRoster(kvp.Key, kvp.Value);
                    _rorSettlementDetails[Settlement.CurrentSettlement][kvp.Key] = 0;
                    MBTextManager.SetTextVariable("GOLD_AMOUNT", MathF.Abs(cost));
                    //InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("str_gold_removed_with_icon", null).ToString(), "event:/ui/notification/coins_negative"));
                    CampaignEventDispatcher.Instance.OnTroopRecruited(Hero.MainHero, Settlement.CurrentSettlement, Settlement.CurrentSettlement.Notables.FirstOrDefault(), kvp.Key, kvp.Value);
                }
                else
                {
                    InformationManager.AddQuickInformation(new TextObject("Not enough room in party."));
                }
            }
            else
            {
                InformationManager.AddQuickInformation(new TextObject("Not enough gold."));
            }
            GameMenu.SwitchToMenu("ror_center");
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_rorSettlementDetails", ref _rorSettlementDetails);
        }

        private void SetGenericStateMenu(string currentMenuId)
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                GameMenu.SwitchToMenu("village");
            }
            else if (Settlement.CurrentSettlement.IsTown)
            {
                GameMenu.SwitchToMenu("town");
            }
            else
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
        }
    }

    public class RoRSaveableTypeDefiner : SaveableTypeDefiner
    {
        public RoRSaveableTypeDefiner() : base(516023) { }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(Dictionary<Settlement, Dictionary<CharacterObject, int>>));
        }
    }
}
