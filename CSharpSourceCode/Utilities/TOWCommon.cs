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

        public static void Log(string message, LogLevel severity)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Log(severity, message);
        }

        public static void CopyEquipmentToClipBoard(SPInventoryVM vm)
        {
            XElement xelement = new XElement("EquipmentRoster");
            AddItem(xelement, vm.CharacterWeapon1Slot.ItemRosterElement.EquipmentElement.Item, "Item0");
            AddItem(xelement, vm.CharacterWeapon2Slot.ItemRosterElement.EquipmentElement.Item, "Item1");
            AddItem(xelement, vm.CharacterWeapon3Slot.ItemRosterElement.EquipmentElement.Item, "Item2");
            AddItem(xelement, vm.CharacterWeapon4Slot.ItemRosterElement.EquipmentElement.Item, "Item3");
            AddItem(xelement, vm.CharacterHelmSlot.ItemRosterElement.EquipmentElement.Item, "Head");
            AddItem(xelement, vm.CharacterCloakSlot.ItemRosterElement.EquipmentElement.Item, "Cape");
            AddItem(xelement, vm.CharacterTorsoSlot.ItemRosterElement.EquipmentElement.Item, "Body");
            AddItem(xelement, vm.CharacterGloveSlot.ItemRosterElement.EquipmentElement.Item, "Gloves");
            AddItem(xelement, vm.CharacterBootSlot.ItemRosterElement.EquipmentElement.Item, "Leg");
            AddItem(xelement, vm.CharacterMountSlot.ItemRosterElement.EquipmentElement.Item, "Horse");
            AddItem(xelement, vm.CharacterMountArmorSlot.ItemRosterElement.EquipmentElement.Item, "HorseHarness");

            XElement xelement2 = new XElement("EquipmentRoster");
            xelement2.Add(new XAttribute("civilian", true));
            AddItem(xelement2, vm.CharacterWeapon1Slot.ItemRosterElement.EquipmentElement.Item, "Item0");
            AddItem(xelement2, vm.CharacterWeapon2Slot.ItemRosterElement.EquipmentElement.Item, "Item1");
            AddItem(xelement2, vm.CharacterWeapon3Slot.ItemRosterElement.EquipmentElement.Item, "Item2");
            AddItem(xelement2, vm.CharacterWeapon4Slot.ItemRosterElement.EquipmentElement.Item, "Item3");
            AddItem(xelement2, vm.CharacterHelmSlot.ItemRosterElement.EquipmentElement.Item, "Head");
            AddItem(xelement2, vm.CharacterCloakSlot.ItemRosterElement.EquipmentElement.Item, "Cape");
            AddItem(xelement2, vm.CharacterTorsoSlot.ItemRosterElement.EquipmentElement.Item, "Body");
            AddItem(xelement2, vm.CharacterGloveSlot.ItemRosterElement.EquipmentElement.Item, "Gloves");
            AddItem(xelement2, vm.CharacterBootSlot.ItemRosterElement.EquipmentElement.Item, "Leg");
            AddItem(xelement2, vm.CharacterMountSlot.ItemRosterElement.EquipmentElement.Item, "Horse");
            AddItem(xelement2, vm.CharacterMountArmorSlot.ItemRosterElement.EquipmentElement.Item, "HorseHarness");
            string text = xelement.ToString() + "\r\n" + xelement2.ToString();

            Clipboard.SetText(text);
            InformationManager.DisplayMessage(new InformationMessage("Equipment XML copied!", Colors.Green));
        }

        private static void AddItem(XElement element, ItemObject item, string slot)
        {
            if (item != null)
            {
                XElement xelement = new XElement("equipment");
                xelement.Add(new XAttribute("slot", slot));
                xelement.Add(new XAttribute("id", "Item." + item.StringId));
                element.Add(xelement);
            }
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
