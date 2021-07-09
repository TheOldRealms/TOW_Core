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
		private static Dictionary<string, Banner> _cache = new Dictionary<string, Banner>();

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
				_cache.Add(__instance.StringId, new Banner(code));
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Kingdom), "Deserialize")]
		public static void Postfix2(MBObjectManager objectManager, XmlNode node, Kingdom __instance)
		{
			string code = node?.Attributes?.GetNamedItem("banner_key").Value;
			if (code != null)
			{
				_cache.Add(__instance.StringId, new Banner(code));
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Clan),"Banner", MethodType.Getter)]
		public static void Postfix3(ref Banner __result, Clan __instance)
        {
			Banner banner = null;
			_cache.TryGetValue(__instance.StringId, out banner);
			if (banner != null)
			{
				__result = banner;
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Kingdom), "Banner", MethodType.Getter)]
		public static void Postfix5(ref Banner __result, Kingdom __instance)
		{
			Banner banner = null;
			_cache.TryGetValue(__instance.StringId, out banner);
			if (banner != null)
			{
				__result = banner;
			}
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
