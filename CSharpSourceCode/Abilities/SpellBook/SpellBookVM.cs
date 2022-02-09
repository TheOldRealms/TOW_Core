using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellBookVM : ViewModel
    {
        private Action _closeAction;
        private HeroViewModel _currentCharacter;
        private MBBindingList<SpellCastingStatItemVM> _stats;
        private MBBindingList<LoreObjectVM> _lores;
        private LoreObjectVM _currentLore;

        public SpellBookVM(Action closeAction)
        {
            _closeAction = closeAction;
            _stats = new MBBindingList<SpellCastingStatItemVM>();
            _lores = new MBBindingList<LoreObjectVM>();
            Initialize();
            RefreshValues();
        }

        private void Initialize()
        {
            CurrentCharacter = new HeroViewModel();
            CurrentCharacter.FillFrom(Hero.MainHero);
            CurrentCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default(EquipmentElement));
            CurrentCharacter.SetEquipment(EquipmentIndex.HorseHarness, default(EquipmentElement));
            CurrentCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default(EquipmentElement));

            var info = Hero.MainHero.GetExtendedInfo();
            StatItems.Add(new SpellCastingStatItemVM("Hero name: ", Hero.MainHero.Name.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Spell casting level: ", info.SpellCastingLevel.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Maximum Winds of Magic: ", info.MaxWindsOfMagic.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Current Winds of Magic: ", info.CurrentWindsOfMagic.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Winds of Magic recharge rate: ", info.WindsOfMagicRechargeRate.ToString()));
            string lorestext = "";
            for(int i = 0; i < info.KnownLores.Count; i++)
            {
                lorestext += info.KnownLores[i].Name;
                if (i != info.KnownLores.Count - 1) lorestext += ", ";
            }
            StatItems.Add(new SpellCastingStatItemVM("Known Magic Schools: ", lorestext));

            var lores = LoreObject.GetAll();
            foreach(var lore in lores)
            {
                LoreObjects.Add(new LoreObjectVM(this, lore));
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        private void ExecuteClose()
        {
            _closeAction();
        }

        internal void OnLoreObjectSelected(LoreObjectVM loreObjectVM)
        {
            CurrentLore = loreObjectVM;
        }

        [DataSourceProperty]
        public HeroViewModel CurrentCharacter
        {
            get
            {
                return this._currentCharacter;
            }
            set
            {
                if (value != this._currentCharacter)
                {
                    this._currentCharacter = value;
                    //this.CurrentCharacterNameText = this._currentCharacter.HeroNameText;
                    base.OnPropertyChangedWithValue(value, "CurrentCharacter");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<SpellCastingStatItemVM> StatItems
        {
            get
            {
                return this._stats;
            }
            set
            {
                if (value != this._stats)
                {
                    this._stats = value;
                    base.OnPropertyChangedWithValue(value, "StatItems");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<LoreObjectVM> LoreObjects
        {
            get
            {
                return this._lores;
            }
            set
            {
                if (value != this._lores)
                {
                    this._lores = value;
                    base.OnPropertyChangedWithValue(value, "LoreObjects");
                }
            }
        }

        [DataSourceProperty]
        public LoreObjectVM CurrentLore
        {
            get
            {
                return this._currentLore;
            }
            set
            {
                if (value != this._currentLore)
                {
                    this._currentLore = value;
                    base.OnPropertyChangedWithValue(value, "CurrentLore");
                }
            }
        }
    }
}
