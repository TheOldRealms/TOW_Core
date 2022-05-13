using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Helpers;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameMenu;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class ConversationPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_wanderer_introduction_on_condition")]
        public static bool WandererString(ref bool __result, ref Dictionary<CharacterObject, CharacterObject> ____previouslyMetWandererTemplates)
        {
			if (CharacterObject.OneToOneConversationCharacter != null && CharacterObject.OneToOneConversationCharacter.IsHero && CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer && CharacterObject.OneToOneConversationCharacter.HeroObject.HeroState != Hero.CharacterStates.Prisoner)
			{
				StringHelpers.SetCharacterProperties("CONVERSATION_CHARACTER", Hero.OneToOneConversationHero.CharacterObject, null);
				string stringId = Hero.OneToOneConversationHero.Template.StringId;
				CharacterObject characterObject;
				____previouslyMetWandererTemplates.TryGetValue(Hero.OneToOneConversationHero.Template, out characterObject);
				if (characterObject == null || characterObject == Hero.OneToOneConversationHero.CharacterObject)
				{
					if (characterObject == null)
					{
						____previouslyMetWandererTemplates[Hero.OneToOneConversationHero.Template] = Hero.OneToOneConversationHero.CharacterObject;
					}
					MBTextManager.SetTextVariable("IMPERIALCAPITAL", new TextObject("Altdorf"));
					MBTextManager.SetTextVariable("WANDERER_BACKSTORY_A", GameTexts.FindText("backstory_a", stringId), false);
					MBTextManager.SetTextVariable("WANDERER_BACKSTORY_B", GameTexts.FindText("backstory_b", stringId), false);
					MBTextManager.SetTextVariable("WANDERER_BACKSTORY_C", GameTexts.FindText("backstory_c", stringId), false);
					MBTextManager.SetTextVariable("BACKSTORY_RESPONSE_1", GameTexts.FindText("response_1", stringId), false);
					MBTextManager.SetTextVariable("BACKSTORY_RESPONSE_2", GameTexts.FindText("response_2", stringId), false);
					MBTextManager.SetTextVariable("WANDERER_BACKSTORY_D", GameTexts.FindText("backstory_d", stringId), false);
					StringHelpers.SetCharacterProperties("MET_WANDERER", Hero.OneToOneConversationHero.CharacterObject, null);
					if (CampaignMission.Current.Location != null && CampaignMission.Current.Location.StringId != "tavern")
					{
						MBTextManager.SetTextVariable("WANDERER_PREBACKSTORY", GameTexts.FindText("spc_prebackstory_generic", null), false);
					}
					__result = true;
				}
			}
            else __result = false;
            return false;
        }

		[HarmonyPostfix]
		[HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_lord_introduction_on_condition")]
		public static void LordStrings(ref bool __result)
		{
			if (Hero.OneToOneConversationHero != null && Campaign.Current.ConversationManager.CurrentConversationIsFirst && Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsRebel && Hero.OneToOneConversationHero.Clan.MapFaction.IsKingdomFaction)
			{
				string text = "you should never see this";
				if (Hero.OneToOneConversationHero.MapFaction.Leader == Hero.OneToOneConversationHero)
				{
					if(Hero.OneToOneConversationHero.MapFaction.StringId == "averland")
                    {
						text = "I am {CONVERSATION_CHARACTER.LINK} of the house of {CLAN_NAME_CUSTOM}, elector count of Averland.";
					}
					else if (Hero.OneToOneConversationHero.MapFaction.StringId == "stirland")
					{
						text = "I am {CONVERSATION_CHARACTER.LINK} of the house of {CLAN_NAME_CUSTOM}, elector count of Stirland.";
					}
					else if(Hero.OneToOneConversationHero.MapFaction.StringId == "sylvania")
                    {
						text = "I am {CONVERSATION_CHARACTER.LINK} of the {CLAN_NAME_CUSTOM}, ruler of Sylvania.";
					}
				}
				else
				{
					text = "I am {CONVERSATION_CHARACTER.LINK}, of the {CLAN_NAME_CUSTOM}.";
				}
				MBTextManager.SetTextVariable("CLAN_NAME_CUSTOM", Hero.OneToOneConversationHero.Clan.Name, false);
				MBTextManager.SetTextVariable("LORD_INTRODUCTION_STRING", text, false);
			}
		}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SettlementMenuOverlayVM), "ExecuteOnSetAsActiveContextMenuItem")]
		public static void RemoveQuickTalk(SettlementMenuOverlayVM __instance)
        {
			var itemToRemove = __instance.ContextList.First(x => x.ActionText == GameTexts.FindText("str_menu_overlay_context_list", "QuickConversation").ToString());
			if (itemToRemove != null) __instance.ContextList.Remove(itemToRemove);
        }
	}
}
