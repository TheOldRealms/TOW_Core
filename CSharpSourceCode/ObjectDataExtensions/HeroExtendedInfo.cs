using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using TOW_Core.Abilities;
using TOW_Core.Abilities.SpellBook;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.ObjectDataExtensions
{
    public class HeroExtendedInfo
    {
        [SaveableField(0)] public List<string> AcquiredAbilities = new List<string>();
        [SaveableField(1)] public List<string> AcquiredAttributes = new List<string>();
        [SaveableField(2)] public float CurrentWindsOfMagic = 0;
        [SaveableField(3)] public int Corruption = 0; //between 0 and 100, 0 = pure af, 100 = fallen to chaos
        [SaveableField(4)] public SpellCastingLevel SpellCastingLevel = SpellCastingLevel.Minor;
        [SaveableField(5)] private CharacterObject _baseCharacter;
        [SaveableField(6)] private List<string> _knownLores = new List<string>();
        public float MaxWindsOfMagic
        {
            get
            {
                if (!(Game.Current.GameType is Campaign)) return 30;
                else
                {
                    var hero = _baseCharacter.HeroObject;
                    var intelligence = hero.GetAttributeValue(DefaultCharacterAttributes.Intelligence);
                    return intelligence * 10;
                }
            }
        }
        public float WindsOfMagicRechargeRate
        {
            get
            {
                if (!(Game.Current.GameType is Campaign)) return 0.2f;
                else
                {
                    var hero = _baseCharacter.HeroObject;
                    var intelligence = hero.GetAttributeValue(DefaultCharacterAttributes.Intelligence);
                    return intelligence * 0.5f;
                }
            }
        }

        public List<LoreObject> KnownLores
        {
            get
            {
                List<LoreObject> list = new List<LoreObject>();
                foreach(var item in _knownLores)
                {
                    list.Add(LoreObject.GetLore(item));
                }
                return list;
            }
        }

        public List<string> AllAbilities
        {
            get
            {
                var list = new List<string>();
                if (_baseCharacter != null) list.AddRange(_baseCharacter.GetAbilities());
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
                list.AddRange(AcquiredAttributes);
                return list;
            }
        }

        private HeroExtendedInfo() { }

        public HeroExtendedInfo(CharacterObject character)
        {
            _baseCharacter = character;
        }

        public void AddKnownLore(string loreId)
        {
            if(LoreObject.GetLore(loreId) != null) _knownLores.Add(loreId);
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
