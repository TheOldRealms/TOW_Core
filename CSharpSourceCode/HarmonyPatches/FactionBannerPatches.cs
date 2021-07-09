using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HarmonyLib;
using SandBox.ViewModelCollection.Nameplate;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class FactionBannerPatches
    {
		private static Dictionary<string, string> _dict = new Dictionary<string, string>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Banner),"Serialize")]
        public static bool Prefix(ref string __result, Banner __instance)
        {
			__result = SerializeBanner(__instance);
			return false;
        }

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Clan),"Deserialize")]
		public static void Postfix(MBObjectManager objectManager, XmlNode node, Clan __instance)
        {
			string code = node?.Attributes?.GetNamedItem("banner_key").Value;
			if (code != null)
			{
				_dict.Add(__instance.StringId, code);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Kingdom), "Deserialize")]
		public static void Postfix2(MBObjectManager objectManager, XmlNode node, Kingdom __instance)
		{
			string code = node?.Attributes?.GetNamedItem("banner_key").Value;
			if (code != null)
			{
				_dict.Add(__instance.StringId, code);
			}
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(PartyNameplateVM), "PartyBanner", MethodType.Setter)]
		public static bool Prefix2(ref PartyNameplateVM __instance, ref ImageIdentifierVM value)
		{
			if (__instance.Party.MapFaction != null) 
			{
				string code = "";
				_dict.TryGetValue(__instance.Party.MapFaction.StringId, out code);
				if(code != "")
                {
					var bannerCode = BannerCode.CreateFrom(code);
                    if (bannerCode != null && bannerCode.Code != null)
                    {
						value = new ImageIdentifierVM(bannerCode, true);
					}
				}
			}
			return true;
		}

		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(SettlementNameplateVM), "Banner", MethodType.Setter)]
		public static bool Prefix3(SettlementNameplateVM __instance, ref ImageIdentifierVM value)
        {
			if (__instance?.Settlement?.OwnerClan != null)
			{
				string code = "";
				_dict.TryGetValue(__instance.Settlement.OwnerClan.StringId, out code);
				if (code != "")
				{
					var bannerCode = BannerCode.CreateFrom(code);
					if (bannerCode != null && bannerCode.Code != null)
					{
						value = new ImageIdentifierVM(bannerCode, true);
					}
				}
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EncyclopediaClanPageVM), "Banner", MethodType.Setter)]
		public static bool Prefix4(EncyclopediaClanPageVM __instance, ref ImageIdentifierVM value, Clan ____clan)
		{
			if (____clan != null)
			{
				string code = "";
				_dict.TryGetValue(____clan.StringId, out code);
				if (code != "")
				{
					var bannerCode = BannerCode.CreateFrom(code);
					if (bannerCode != null && bannerCode.Code != null)
					{
						value = new ImageIdentifierVM(bannerCode, true);
					}
				}
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EncyclopediaFactionPageVM), "Banner", MethodType.Setter)]
		public static bool Prefix4(EncyclopediaFactionPageVM __instance, ref ImageIdentifierVM value, Kingdom ____faction)
		{
			if (____faction != null)
			{
				string code = "";
				_dict.TryGetValue(____faction.StringId, out code);
				if (code != "")
				{
					var bannerCode = BannerCode.CreateFrom(code);
					if (bannerCode != null && bannerCode.Code != null)
					{
						value = new ImageIdentifierVM(bannerCode, true);
					}
				}
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EncyclopediaFactionVM), "ImageIdentifier", MethodType.Setter)]
		public static bool Prefix5(EncyclopediaFactionVM __instance, ref ImageIdentifierVM value, IFaction ____faction)
		{
			if (____faction != null)
			{
				string code = "";
				_dict.TryGetValue(____faction.StringId, out code);
				if (code != "")
				{
					var bannerCode = BannerCode.CreateFrom(code);
					if (bannerCode != null && bannerCode.Code != null)
					{
						value = new ImageIdentifierVM(bannerCode, true);
					}
				}
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SettlementNameplatePartyMarkerItemVM), "Visual", MethodType.Setter)]
		public static bool Prefix6(SettlementNameplatePartyMarkerItemVM __instance, ref ImageIdentifierVM value)
		{
			if (__instance.Party?.MapFaction != null)
			{
				string code = "";
				_dict.TryGetValue(__instance.Party.MapFaction.StringId, out code);
				if (code != "")
				{
					var bannerCode = BannerCode.CreateFrom(code);
					if (bannerCode != null && bannerCode.Code != null)
					{
						value = new ImageIdentifierVM(bannerCode, true);
					}
				}
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SettlementMenuOverlayVM), "SettlementOwnerBanner", MethodType.Setter)]
		public static bool Prefix7(SettlementMenuOverlayVM __instance, ref ImageIdentifierVM value, SettlementComponent ____settlementComponent)
		{
			if (____settlementComponent?.Settlement?.MapFaction != null)
			{
				string code = "";
				_dict.TryGetValue(____settlementComponent.Settlement.MapFaction.StringId, out code);
				if (code != "")
				{
					var bannerCode = BannerCode.CreateFrom(code);
					if (bannerCode != null && bannerCode.Code != null)
					{
						value = new ImageIdentifierVM(bannerCode, true);
					}
				}
			}
			return true;
		}

		private static string SerializeBanner(Banner banner)
        {
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (BannerData bannerData in banner.BannerDataList)
			{
				try
				{
					bool flag2 = !flag;
					if (flag2)
					{
						stringBuilder.Append(".");
					}
					flag = false;
					stringBuilder.Append(bannerData.MeshId);
					stringBuilder.Append('.');
					stringBuilder.Append(bannerData.ColorId);
					stringBuilder.Append('.');
					stringBuilder.Append(bannerData.ColorId2);
					stringBuilder.Append('.');
					stringBuilder.Append((int)bannerData.Size.x);
					stringBuilder.Append('.');
					stringBuilder.Append((int)bannerData.Size.y);
					stringBuilder.Append('.');
					stringBuilder.Append((int)bannerData.Position.x);
					stringBuilder.Append('.');
					stringBuilder.Append((int)bannerData.Position.y);
					stringBuilder.Append('.');
					stringBuilder.Append(bannerData.DrawStroke ? 1 : 0);
					stringBuilder.Append('.');
					stringBuilder.Append(bannerData.Mirror ? 1 : 0);
					stringBuilder.Append('.');
					float num = bannerData.RotationValue / 0.00278f;
					int value = (int)short.Parse(Math.Round((double)num, MidpointRounding.AwayFromZero).ToString());
					stringBuilder.Append(value);
				}
				catch (Exception ex)
				{
					TOW_Core.Utilities.TOWCommon.Log(ex.Message, NLog.LogLevel.Error);
				}
			}
			return stringBuilder.ToString();
		}
    }
}
