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
        public string EquipmentSetId;
    }
}
