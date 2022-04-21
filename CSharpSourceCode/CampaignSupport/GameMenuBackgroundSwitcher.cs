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
        /** Janky fix for https://github.com/TheOldRealms/TOW_Core/issues/213
         *
         *  May potentially lead to issues where the wrong background mesh shows
         *  up for village_looted, village_raid_ended_leaded_by_someone_else, and
         *  raiding_village events.
         *  
         *  Though the above is only true for cases where Settlement.CurrentSettlement
         *  is null.
         *  
         *  TODO: Replace SettlementMemo with logic for finding the settlement 
         *  closest to the player party.
         */
        private static Settlement SettlementMemo = null;

        [GameMenuInitializationHandler("village_looted")]
        [GameMenuInitializationHandler("village_raid_ended_leaded_by_someone_else")]
        [GameMenuInitializationHandler("raiding_village")]
        private static void game_menu_ui_village_hostile_raid_on_init_tow(MenuCallbackArgs args)
        {
            var settlement = Settlement.CurrentSettlement
                ?? GameMenuBackgroundSwitcher.SettlementMemo;
            GameMenuBackgroundSwitcher.SettlementMemo = settlement;
            if (settlement == null || settlement.Culture == null)
            {
                args.MenuContext.SetBackgroundMeshName("wait_raiding_village");
                return;
            }

            var culture = settlement.Culture;
            switch (culture.StringId)
            {
                case "empire":
                    args.MenuContext.SetBackgroundMeshName("empire_looted_village");
                    return;
                case "khuzait":
                    args.MenuContext.SetBackgroundMeshName("vampire_looted_village");
                    return;
            }
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
            if (culture == null)
            {
                args.MenuContext.SetBackgroundMeshName("wait_captive_male");
                return;
            }

            switch (culture.StringId)
            {
                case "empire":
                    args.MenuContext.SetBackgroundMeshName("empire_captive");
                    return;
                case "khuzait":
                    args.MenuContext.SetBackgroundMeshName("vampire_captive");
                    return;
            }
        }
    }
}
