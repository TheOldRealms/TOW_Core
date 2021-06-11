using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using System.Xml.Serialization;
using TaleWorlds.Library;
using System.IO;

namespace TOW_Core.CharacterCreation
{
    [Serializable]
    public class CharacterCreationOption
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Culture;
        [XmlAttribute]
        public int StageNumber;
        public string[] SkillsToIncrease;
        public string AttributeToIncrease;
        public string OptionText;
        public string OptionFlavourText;
        [XmlAttribute]
        public string CharacterIdToCopyEquipmentFrom;
        
        public static void WriteSampleXML()
        {
            List<CharacterCreationOption> list = new List<CharacterCreationOption>();
            var sampleOption = new CharacterCreationOption();
            sampleOption.Id = Guid.NewGuid().ToString();
            sampleOption.Culture = "empire";
            sampleOption.StageNumber = 1;
            sampleOption.SkillsToIncrease = new string[] { "Trade", "Charm" };
            sampleOption.AttributeToIncrease = DefaultCharacterAttributes.Endurance.StringId;
            sampleOption.OptionText = "Placeholder Option: please edit me";
            sampleOption.OptionFlavourText = "Placeholder Option Flavour text. Please edit me in tow_cc_options.xml";
            sampleOption.CharacterIdToCopyEquipmentFrom = "emp_lord";
            list.Add(sampleOption);

            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_cc_options.xml");

            XmlSerializer ser = new XmlSerializer(typeof(List<CharacterCreationOption>));
            ser.Serialize(File.OpenWrite(path), list);
        }
    }
}
