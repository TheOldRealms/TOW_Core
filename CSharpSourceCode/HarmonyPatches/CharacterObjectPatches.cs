using HarmonyLib;
using Helpers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Library;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CharacterObjectPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterObject), "TroopWage", MethodType.Getter)]
        public static bool TroopWagePrefix(ref int __result, CharacterObject __instance)
        {
            if (__instance.IsUndead())
            {
                __result = 0;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterObject), "Tier", MethodType.Getter)]
        public static bool TierPrefix(ref int __result, CharacterObject __instance)
        {
            if (__instance.IsHero)
            {
                __result = 0;
            }
            else
            {
                __result = Math.Min(Math.Max(MathF.Ceiling(((float)__instance.Level - 5f) / 5f), 0), 9);
            }
            return false;
        }

        //Copied and modified from DesertionCampaignBehaviour.PartiesCheckDesertionDueToPartySizeExceedsPaymentRatio
        //reason is to support tier 9 troops. Game is crashing when T9 troops are trying to desert.
        //Must be reviewed if TW changes underlying code signature
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DesertionCampaignBehavior), "PartiesCheckDesertionDueToPartySizeExceedsPaymentRatio")]
        public static bool DesertionTierPrefix(MobileParty mobileParty, ref TroopRoster desertedTroopList)
        {
			int partySizeLimit = mobileParty.Party.PartySizeLimit;
			if ((mobileParty.IsLordParty || mobileParty.IsCaravan) && mobileParty.Party.NumberOfAllMembers > partySizeLimit && mobileParty != MobileParty.MainParty && mobileParty.MapEvent == null)
			{
				int num = mobileParty.Party.NumberOfAllMembers - partySizeLimit;
				for (int i = 0; i < num; i++)
				{
					CharacterObject character = mobileParty.MapFaction.BasicTroop;
					int num2 = 99;
					bool flag = false;
					for (int j = 0; j < mobileParty.MemberRoster.Count; j++)
					{
						CharacterObject characterAtIndex = mobileParty.MemberRoster.GetCharacterAtIndex(j);
						if (!characterAtIndex.IsHero && characterAtIndex.Level < num2 && mobileParty.MemberRoster.GetElementNumber(j) > 0)
						{
							num2 = characterAtIndex.Level;
							character = characterAtIndex;
							flag = (mobileParty.MemberRoster.GetElementWoundedNumber(j) > 0);
						}
					}
					if (num2 < 99)
					{
						if (flag)
						{
							mobileParty.MemberRoster.AddToCounts(character, -1, false, -1, 0, true, -1);
						}
						else
						{
							mobileParty.MemberRoster.AddToCounts(character, -1, false, 0, 0, true, -1);
						}
					}
				}
			}
			bool flag2 = mobileParty.PaymentLimit > 0 && !mobileParty.UnlimitedWage && mobileParty.PaymentLimit < mobileParty.TotalWage;
			if (mobileParty.Party.NumberOfAllMembers > mobileParty.LimitedPartySize || flag2)
			{
				int numberOfDeserters = Campaign.Current.Models.PartyDesertionModel.GetNumberOfDeserters(mobileParty);
				int num3 = 0;
				while (num3 < numberOfDeserters && mobileParty.MemberRoster.TotalRegulars > 0)
				{
					int stackNo = -1;
					int num4 = 9;
					int num5 = 1;
					int num6 = 0;
					while (num6 < mobileParty.MemberRoster.Count && mobileParty.MemberRoster.TotalRegulars > 0)
					{
						CharacterObject characterAtIndex2 = mobileParty.MemberRoster.GetCharacterAtIndex(num6);
						int elementNumber = mobileParty.MemberRoster.GetElementNumber(num6);
						if (!characterAtIndex2.IsHero && elementNumber > 0 && characterAtIndex2.Tier <= num4)
						{
							num4 = characterAtIndex2.Tier;
							stackNo = num6;
							num5 = Math.Min(elementNumber, numberOfDeserters - num3);
						}
						num6++;
					}
					MobilePartyHelper.DesertTroopsFromParty(mobileParty, stackNo, num5, 0, ref desertedTroopList);
					num3 += num5;
				}
			}
			return false;
		}
    }
}
