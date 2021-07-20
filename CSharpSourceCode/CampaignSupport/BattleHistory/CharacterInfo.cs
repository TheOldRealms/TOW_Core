using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace TOW_Core.CampaignSupport.BattleHistory
{
    public class CharacterInfo
    {
        public string Name;
        public float Age;
        public string CultureName;
        public int Level;

        public CharacterInfo()
        {

        }

        public CharacterInfo(BasicCharacterObject basicCharacterObject)
        {
            Name = basicCharacterObject.GetName().ToString();
            Age = basicCharacterObject.Age;
            CultureName = basicCharacterObject.Culture.ToString();
            Level = basicCharacterObject.Level;
        }
    }
}
