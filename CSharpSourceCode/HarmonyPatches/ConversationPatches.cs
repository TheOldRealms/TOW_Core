using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Helpers;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class ConversationPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_wanderer_introduction_on_condition")]
        public static bool Prefix(ref bool __result, ref Dictionary<CharacterObject, CharacterObject> ____previouslyMetWandererTemplates)
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
    }
}
