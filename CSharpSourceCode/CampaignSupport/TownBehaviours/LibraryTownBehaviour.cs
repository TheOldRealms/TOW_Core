using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    class LibraryTownBehaviour : CampaignBehaviorBase
    {
        private List<string> _spells;
        private int _spellCost = 50000;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddMenu);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, SpawnTrainer);
        }

        private void SpawnTrainer(Dictionary<string, int> unusedPoints)
        {
            
        }

        private void AddMenu(CampaignGameStarter obj)
        {
            _spells = Abilities.AbilityFactory.GetAllSpellNamesAsList();
            obj.AddGameMenuOption("town", "gotolibrary", "Go to the library", new GameMenuOption.OnConditionDelegate(_libraryCondition), new GameMenuOption.OnConsequenceDelegate(_libraryConsequence), false, 4, false);
            obj.AddGameMenu("library", "{LIBRARY_DESCRIPTION}", _initLibrary, GameOverlays.MenuOverlayType.SettlementWithBoth);
            foreach(var spellname in _spells)
            {
                var template = Abilities.AbilityFactory.GetTemplate(spellname);
                if(template != null)
                {
                    obj.AddGameMenuOption("library", "learn" + spellname, "Learn " + template.Name +" (" + _spellCost +" {GOLD_ICON})", (MenuCallbackArgs) => !Hero.MainHero.HasAbility("spellname"), (MenuCallbackArgs) => 
                    { 
                        if(Hero.MainHero.Gold > _spellCost)
                        {
                            if (!Hero.MainHero.HasAbility(spellname))
                            {
                                Hero.MainHero.ChangeHeroGold(-_spellCost);
                                Hero.MainHero.AddAbility(spellname);
                                InformationManager.AddQuickInformation(new TextObject("Learned " + template.Name));
                            }
                        }
                        else
                        {
                            InformationManager.AddQuickInformation(new TextObject("You cant afford that."));
                        }
                        GameMenu.SwitchToMenu("library");
                    });
                }
            }
            obj.AddGameMenuOption("library", "leaveoption", "Leave", new GameMenuOption.OnConditionDelegate((MenuCallbackArgs x) => true), delegate (MenuCallbackArgs x)
            {
                GameMenu.SwitchToMenu("town");
            }, true, -1, false);
        }

        private bool _libraryCondition(MenuCallbackArgs args)
        {
            bool shouldBeDisabled;
            TextObject disabledText;
            bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroAccessLocation(Settlement.CurrentSettlement, "center", out shouldBeDisabled, out disabledText);
            disabledText = new TextObject("The Library's doors remain closed to you.");
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            if (canPlayerDo)
            {
                canPlayerDo = Hero.MainHero.IsSpellCaster();
            }
            return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
        }

        private void _libraryConsequence(MenuCallbackArgs args)
        {
            GameMenu.ActivateGameMenu("library");
        }

        private void _initLibrary(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("Forbidden Library");
            var intro = new TextObject("You have arrived at {SETTLEMENT_NAME}'s Library. Forbidden knowledge lies inside for those with magical affinity.");
            MBTextManager.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name);
            MBTextManager.SetTextVariable("LIBRARY_DESCRIPTION", intro);
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }
    }
}
