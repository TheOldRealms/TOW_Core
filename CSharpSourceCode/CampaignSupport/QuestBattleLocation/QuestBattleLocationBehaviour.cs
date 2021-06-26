using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class QuestBattleLocationBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, onGameStart);
        }

        private void onGameStart(CampaignGameStarter obj)
        {
            obj.AddGameMenu("questlocation_menu", "{=!}Test", this.root_menu_init, GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.none, null);
            obj.AddGameMenuOption("questlocation_menu", "root_leave", "{=!}Leave...", delegate (MenuCallbackArgs args)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, this.root_leave, true, -1, false);
        }

        private void root_leave(MenuCallbackArgs args)
        {
            PlayerEncounter.LeaveSettlement();
            PlayerEncounter.Finish(true);
        }

        private void root_menu_init(MenuCallbackArgs args)
        {
            Settlement settlement = (Settlement.CurrentSettlement == null) ? MobileParty.MainParty.CurrentSettlement : Settlement.CurrentSettlement;
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            Campaign.Current.GameMenuManager.MenuLocations.Add(settlement.LocationComplex.GetLocationWithId("questbattle_location"));
            PlayerEncounter.EnterSettlement();
            PlayerEncounter.LocationEncounter = new QuestBattleEncounter(settlement);
            if (args.MapState != null && args.MapState.Handler != null) args.MapState.Handler.TeleportCameraToMainParty();
            /*
            var text = "You have arrived at a {TEMPLENAME} of the {RELIGIONNAME}. The local folk from the surrounding settlements come here on a pilgrimage to pray at the altar of {GODNAME}.";
            MBTextManager.SetTextVariable("TEMPLENAME", religion.TownTempleName);
            MBTextManager.SetTextVariable("RELIGIONNAME", religion.Name);
            MBTextManager.SetTextVariable("GODNAME", religion.Pantheon.Find(x => x.Name == religion.MainGodName).LongName);
            MBTextManager.SetTextVariable("SHRINEROOTTEXT", text);
            */
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }
    }
}
