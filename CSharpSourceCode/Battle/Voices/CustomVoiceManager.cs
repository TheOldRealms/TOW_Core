using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.ModuleManager;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Voices
{
    public class CustomVoiceManager
    {
        private static string ModuleName = "TOW_Core";
        private static string VoicesFileName = "tow_charactervoices.xml";
        private static Dictionary<string, string> _voicesDictionary = new Dictionary<string, string>();

        internal static string GetVoiceClassNameFor(string id)
        {
            string s = null;
            _voicesDictionary.TryGetValue(id, out s);
            return s;
        }

        public static void LoadVoices()
        {
            var files = Directory.GetFiles(ModuleHelper.GetModuleFullPath(ModuleName), VoicesFileName, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                XmlDocument voicesXml = new XmlDocument();
                voicesXml.Load(file);
                XmlNodeList characters = voicesXml.GetElementsByTagName("Character");

                foreach (XmlNode character in characters)
                {
                    _voicesDictionary.Add(character.Attributes["name"].Value, character.Attributes["voice"].Value);
                }
            }
        }
    }
}
