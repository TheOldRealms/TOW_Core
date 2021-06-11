using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AttributeSystem
{
    public class AttributeManager
    {
        private readonly string ModuleName = "TOW_Core";
        private readonly string AttributeFileName = "tow_characterattributes.xml";
        
        public AttributeManager()
        {

        }

        public void LoadAttributes()
        {
            var files = Directory.GetFiles(ModuleHelper.GetModuleFullPath(ModuleName), AttributeFileName, SearchOption.AllDirectories);
            Dictionary<string, List<string>> attributesDictionary = new Dictionary<string, List<string>>();
            foreach (var file in files)
            {
                XmlDocument attributeXml = new XmlDocument();
                attributeXml.Load(file);
                XmlNodeList characters = attributeXml.GetElementsByTagName("Character");
                
                foreach (XmlNode character in characters)
                {
                    List<string> attributes = new List<string>();
                    foreach (XmlNode attributeNode in character.ChildNodes)
                    {
                        if (attributeNode.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        attributes.Add(attributeNode.Attributes["id"].Value);
                    }
                    attributesDictionary.Add(character.Attributes["name"].Value, attributes);
                }
            }

            AgentExtensions.SetAttributesDictionary(attributesDictionary);
        }
    }
}
