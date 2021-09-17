using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace TOW_Core.HarmonyPatches
{
	[HarmonyPatch]
    public static class SummonRaisePartyRosterFixPatch
    {
		/*
		[HarmonyPrefix]
		[HarmonyPatch(typeof(TroopRoster), "AddToCountsAtIndex")]
		public static bool FindIndexOfTroop(int index)
		{
			if (index < 0) return false;
			return true;
		}
		*/
	}
}
