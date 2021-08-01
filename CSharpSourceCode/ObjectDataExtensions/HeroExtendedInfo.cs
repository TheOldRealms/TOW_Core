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
        [SaveableField(0)] public List<string> AcquiredAbilities = new List<string>();
        [SaveableField(1)] public List<string> AcquiredAttributes = new List<string>();
        [SaveableField(2)] public float CurrentWindsOfMagic = 0;
        [SaveableField(3)] public float MaxWindsOfMagic = 0;
        [SaveableField(4)] public float WindsOfMagicRechargeRate = 1;
        [SaveableField(5)] public int Corruption = 0; //between 0 and 100, 0 = pure af, 100 = fallen to chaos
        [SaveableField(6)] private CharacterObject _baseCharacter;
        [SaveableField(7)] private CharacterObject _templateCharacterOrigin;
        //maybe not yet
        //public List<string> KnownLores;

        private HeroExtendedInfo() { }

        public HeroExtendedInfo(CharacterObject character)
        {
            _baseCharacter = character;
            _templateCharacterOrigin = character.TemplateCharacter;
        }
        public List<string> AllAbilities 
        { 
            get
            {
                var list = new List<string>();
                if (_baseCharacter != null) list.AddRange(_baseCharacter.GetAbilities());
                if(list.Count == 0)
                {
                    var templateList = _templateCharacterOrigin?.GetAbilities();
                    if (templateList != null && templateList.Count > 0) list = templateList;
                }
                list.AddRange(AcquiredAbilities);
                return list;
            } 
        }

        public List<string> AllAttributes
        {
            get
            {
                var list = new List<string>();
                if (_baseCharacter != null) list.AddRange(_baseCharacter.GetAttributes());
                if (list.Count == 0)
                {
                    var templateList = _templateCharacterOrigin?.GetAttributes();
                    if (templateList != null && templateList.Count > 0) list = templateList;
                }
                list.AddRange(AcquiredAttributes);
                return list;
            }
        }

    }
    public class HeroExtendedInfoInfoDefiner : SaveableTypeDefiner
    {
        public HeroExtendedInfoInfoDefiner() : base(1_543_132) { }
        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(HeroExtendedInfo), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(Dictionary<string, HeroExtendedInfo>));
        }
    }
}
