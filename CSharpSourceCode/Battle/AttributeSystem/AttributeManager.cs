using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ObjectDataExtensions
{
    public class AttributeManager
    {
        private static string ModuleName = "TOW_Core";
        private static string AttributeFileName = "tow_characterattributes.xml";
        private static Dictionary<string, List<string>> _attributesDictionary = new Dictionary<string, List<string>>();

        internal static List<string> GetAttributesFor(string id)
        {
            var list = new List<string>();
            _attributesDictionary.TryGetValue(id, out list);
            return list;
        }

        internal static List<string> GetAllCharacterIds()
        {
            return _attributesDictionary.Keys.ToList();
        }

        public static void LoadAttributes()
        {
            var files = Directory.GetFiles(ModuleHelper.GetModuleFullPath(ModuleName), AttributeFileName, SearchOption.AllDirectories);
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
                    _attributesDictionary.Add(character.Attributes["name"].Value, attributes);
                }
            }
        }


    }
}
