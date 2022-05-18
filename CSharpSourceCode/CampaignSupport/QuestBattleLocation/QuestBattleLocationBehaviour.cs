using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class QuestBattleLocationBehaviour : CampaignBehaviorBase
    {
        private QuestBattleComponent _component;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, onGameStart);
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, onMissionEnded);
        }

        private void onMissionEnded(IMission obj)
        {
            if (_component != null && _component.IsQuestBattleUnderway)
            {
                var mission = obj as Mission;
                if (mission.MissionResult != null && mission.MissionResult.BattleResolved && mission.MissionResult.PlayerVictory)
                {
                    _component.OnQuestBattleComplete(true);
                    var list = new List<InquiryElement>();
                    var item = MBObjectManager.Instance.GetObject<ItemObject>(_component.QuestBattleTemplate.RewardItemId);
                    list.Add(new InquiryElement(item, item.Name.ToString(), new ImageIdentifier(item)));
                    var inq = new MultiSelectionInquiryData("Victory!", "You are Victorious! Claim your reward!", list, false, 1, "OK", null, onRewardClaimed, null);
                    InformationManager.ShowMultiSelectionInquiry(inq);
                }
                else
                {
                    _component.OnQuestBattleComplete(false);
                    var inq = new InquiryData("Defeated!", "The enemy proved more than a match for you. Better luck next time!", true, false, "OK", null, null, null);
                    InformationManager.ShowInquiry(inq);
                }
            }
        }

        private void onRewardClaimed(List<InquiryElement> obj)
        {
            var item = obj[0].Identifier as ItemObject;
            Hero.MainHero.PartyBelongedTo.Party.ItemRoster.AddToCounts(item, 1);
        }

        private void onGameStart(CampaignGameStarter obj)
        {
            obj.AddGameMenu("questlocation_menu", "{=!}{LOCATION_DESCRIPTION}", this.root_menu_init, GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.None, null);
            obj.AddGameMenuOption("questlocation_menu", "doquestbattle", "{QUEST_TEXT} (battle)", this.doquestbattle_condition, this.doquestbattle_consequence);
            obj.AddGameMenuOption("questlocation_menu", "root_leave", "{=!}Leave...", delegate(MenuCallbackArgs args)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, this.root_leave, true, -1, false);
        }

        private bool doquestbattle_condition(MenuCallbackArgs args)
        {
            if (_component != null && _component.QuestBattleTemplate != null && _component.IsActive)
            {
                MBTextManager.SetTextVariable("QUEST_TEXT", _component.QuestBattleTemplate.QuestBattleSolveText);
                args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                return true;
            }

            return false;
        }

        private void doquestbattle_consequence(MenuCallbackArgs args)
        {
            if (PlayerEncounter.Battle == null)
            {
                _component.SpawnDefenderParty();
                
            }
            PlayerEncounter.StartBattle();
            PlayerEncounter.Update();

            _component.StartBattle();
            CampaignMission.OpenBattleMission(_component.QuestBattleTemplate.SceneName);
        }

        private void root_leave(MenuCallbackArgs args)
        {
            if (_component != null)
            {
                _component.CleanQuestParty();
            }
            PlayerEncounter.LeaveSettlement();
            PlayerEncounter.Finish(true);
            _component = null;
        }

        private void root_menu_init(MenuCallbackArgs args)
        {
            Settlement settlement = (Settlement.CurrentSettlement == null) ? MobileParty.MainParty.CurrentSettlement : Settlement.CurrentSettlement;
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            Campaign.Current.GameMenuManager.MenuLocations.Add(settlement.LocationComplex.GetLocationWithId("questbattle_location"));
            if (args.MapState != null && args.MapState.Handler != null) args.MapState.Handler.TeleportCameraToMainParty();
            PlayerEncounter.EnterSettlement();
            _component = settlement.GetComponent<QuestBattleComponent>();
            if (_component != null)
            {
                if (_component.IsActive)
                {
                    args.MenuContext.SetBackgroundMeshName(_component.BackgroundMeshName);
                    if (_component.QuestBattleTemplate != null)
                    {
                        var text = _component.QuestBattleTemplate.QuestBattleDescription;
                        MBTextManager.SetTextVariable("LOCATION_DESCRIPTION", text);
                    }
                }
                else
                {
                    //might want a different background when there are no active quest at this location.
                    args.MenuContext.SetBackgroundMeshName(_component.BackgroundMeshName);
                    if (_component.QuestBattleTemplate != null)
                    {
                        var text = _component.QuestBattleTemplate.InactiveDescription;
                        MBTextManager.SetTextVariable("LOCATION_DESCRIPTION", text);
                    }
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }
    }
}