using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.ObjectDataExtensions
{
    public class HeroExtendedInfo
    {
        [SaveableField(0)]
        public List<string> AcquiredAbilities = new List<string>();
        [SaveableField(1)]
        public List<string> AcquiredAttributes = new List<string>();
        [SaveableField(2)]
        public float CurrentWindsOfMagic;
        [SaveableField(3)]
        public float MaxWindsOfMagic;
        //maybe not yet
        //public List<string> KnownLores;

        private CharacterObject _baseCharacter;
        public List<string> AllAbilities 
        { 
            get
            {
                var list = _baseCharacter.GetAbilities();
                list.AddRange(AcquiredAbilities);
                return list;
            } 
        }

        public List<string> AllAttributes
        {
            get
            {
                var list = _baseCharacter.GetAttributes();
                list.AddRange(AcquiredAttributes);
                return list;
            }
        }

    }
}
