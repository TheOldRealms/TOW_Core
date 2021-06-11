using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.ModuleManager;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.Battle.Voices
{
    public class CustomVoiceManager
    {
        private readonly string ModuleName = "TOW_Core";
        private readonly string VoicesFileName = "tow_charactervoices.xml";

        public CustomVoiceManager()
        {

        }

        public void LoadVoices()
        {
            var files = Directory.GetFiles(ModuleHelper.GetModuleFullPath(ModuleName), VoicesFileName, SearchOption.AllDirectories);
            Dictionary<string, string> voicesDictionary = new Dictionary<string, string>();
            foreach (var file in files)
            {
                XmlDocument voicesXml = new XmlDocument();
                voicesXml.Load(file);
                XmlNodeList characters = voicesXml.GetElementsByTagName("Character");

                foreach (XmlNode character in characters)
                {
                    voicesDictionary.Add(character.Attributes["name"].Value, character.Attributes["voice"].Value);
                }
            }

            AgentExtensions.SetVoicesDictionary(voicesDictionary);
        }
    }
}
