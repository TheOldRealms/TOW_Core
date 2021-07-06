using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;

namespace TOW_Core.CampaignSupport
{
    public class GameMenuBackgroundSwitcher
    {
        [GameMenuInitializationHandler("village_looted")]
        [GameMenuInitializationHandler("village_raid_ended_leaded_by_someone_else")]
        [GameMenuInitializationHandler("raiding_village")]
        private static void game_menu_ui_village_hostile_raid_on_init_tow(MenuCallbackArgs args)
        {
            var culture = Settlement.CurrentSettlement.Culture;
            var found = false;
            if (culture != null)
            {
                if(culture.StringId == "empire")
                {
                    args.MenuContext.SetBackgroundMeshName("empire_looted_village");
                    found = true;
                }
                else if(culture.StringId == "khuzait")
                {
                    args.MenuContext.SetBackgroundMeshName("vampire_looted_village");
                    found = true;
                }
            }
            if(!found) args.MenuContext.SetBackgroundMeshName("wait_raiding_village");
        }

        [GameMenuInitializationHandler("town_arena")]
        private static void game_menu_town_menu_arena_on_init_tow(MenuCallbackArgs args)
        {
            args.MenuContext.SetBackgroundMeshName("generic_arena");
            //args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/arena");
        }

        [GameMenuInitializationHandler("prisoner_wait")]
        [GameMenuInitializationHandler("taken_prisoner")]
		[GameMenuInitializationHandler("menu_captivity_end_no_more_enemies")]
		[GameMenuInitializationHandler("menu_captivity_end_by_ally_party_saved")]
		[GameMenuInitializationHandler("menu_captivity_end_by_party_removed")]
		[GameMenuInitializationHandler("menu_captivity_end_wilderness_escape")]
		[GameMenuInitializationHandler("menu_escape_captivity_during_battle")]
		[GameMenuInitializationHandler("menu_released_after_battle")]
		[GameMenuInitializationHandler("menu_captivity_end_propose_ransom_wilderness")]
        private static void wait_menu_ui_prisoner_wait_on_init_tow(MenuCallbackArgs args)
        {
            var culture = Hero.MainHero.Culture;
            var found = false;
            if (culture != null)
            {
                if (culture.StringId == "empire")
                {
                    args.MenuContext.SetBackgroundMeshName("empire_captive");
                    found = true;
                }
                else if (culture.StringId == "khuzait")
                {
                    args.MenuContext.SetBackgroundMeshName("vampire_captive");
                    found = true;
                }
            }
            if (!found) args.MenuContext.SetBackgroundMeshName("wait_captive_male");
        }
    }
}
