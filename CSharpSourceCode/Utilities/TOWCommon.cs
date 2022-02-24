using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TOW_Core.Utilities
{
    public static class TOWCommon
    {
        private static Random _random = new Random();
        /// <summary>
        /// Print a message to the MB2 message window.
        /// </summary>
        /// <param name="text">The text that you want to print to the console.</param>
        public static void Say(string text)
        {
            InformationManager.DisplayMessage(new InformationMessage(text, new Color(134, 114, 250)));
        }

        public static string GetWindsIconAsText()
        {
            return "<img src=\"winds_icon_45\"/>";
        }

        public static void Log(string message, LogLevel severity)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Log(severity, message);
        }

        public static void CopyEquipmentToClipBoard(SPInventoryVM vm)
        {
            string text = "";
            text += GetText(vm.CharacterWeapon1Slot) + ",";
            text += GetText(vm.CharacterWeapon2Slot) + ",";
            text += GetText(vm.CharacterWeapon3Slot) + ",";
            text += GetText(vm.CharacterWeapon4Slot) + ",";
            text += GetText(vm.CharacterHelmSlot) + ",";
            text += GetText(vm.CharacterTorsoSlot) + ",";
            text += GetText(vm.CharacterCloakSlot) + ",";
            text += GetText(vm.CharacterGloveSlot) + ",";
            text += GetText(vm.CharacterBootSlot) + ",";
            text += GetText(vm.CharacterMountSlot) + ",";
            text += GetText(vm.CharacterMountArmorSlot);
            Clipboard.SetText(text);
            InformationManager.DisplayMessage(new InformationMessage("Equipment items copied!", Colors.Green));
        }
        
        private static string GetText(SPItemVM slot)
        {
            if (slot.StringId != "" && slot.StringId != null) return "Item." + slot.StringId;
            else return "none";
        }

        public static string GetRandomScene()
        {
            var filterednames = new List<string>();
            string pickedname = "towmm_menuscene_01";
            var path = BasePath.Name + "Modules/TOR_Environment/SceneObj/";
            if (Directory.Exists(path))
            {
                var dirnames = Directory.GetDirectories(path);
                filterednames = dirnames.Where(x =>
                {
                    string[] s = x.Split('/');
                    var name = s[s.Length - 1];
                    if (name.StartsWith("towmm_")) return true;
                    else return false;
                }).ToList();
            }
            if (filterednames.Count > 0)
            {
                var index = _random.Next(0, filterednames.Count);
                pickedname = filterednames[index];
                string[] s = pickedname.Split('/');
                pickedname = s[s.Length - 1];

            }
            return pickedname;
        }
    }
}
