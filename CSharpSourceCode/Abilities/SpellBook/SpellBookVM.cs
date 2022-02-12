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
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellBookVM : ViewModel
    {
        private Action _closeAction;
        private HeroViewModel _currentCharacter;
        private List<Hero> _heroes;
        private Hero _currentHero;
        private MBBindingList<StatItemVM> _stats;
        private MBBindingList<LoreObjectVM> _lores;
        private LoreObjectVM _currentLore;

        public SpellBookVM(Action closeAction, List<Hero> heroes)
        {
            _closeAction = closeAction;
            _stats = new MBBindingList<StatItemVM>();
            _lores = new MBBindingList<LoreObjectVM>();
            _heroes = heroes;
            Initialize();
            RefreshValues();
        }

        private void Initialize()
        {
            _currentHero = _heroes.First(x=>x == Hero.MainHero);
            if (_currentHero == null) _currentHero = _heroes[0];
            CurrentCharacter = new HeroViewModel();
            CurrentCharacter.FillFrom(_currentHero);
            CurrentCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default(EquipmentElement));
            CurrentCharacter.SetEquipment(EquipmentIndex.HorseHarness, default(EquipmentElement));
            CurrentCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default(EquipmentElement));

            var info = _currentHero.GetExtendedInfo();
            StatItems.Add(new StatItemVM("Hero name: ", _currentHero.Name.ToString()));
            StatItems.Add(new StatItemVM("Spell casting level: ", info.SpellCastingLevel.ToString()));
            StatItems.Add(new StatItemVM("Maximum Winds of Magic: ", info.MaxWindsOfMagic.ToString() + TOWCommon.GetWindsIconAsText()));
            StatItems.Add(new StatItemVM("Current Winds of Magic: ", info.CurrentWindsOfMagic.ToString() + TOWCommon.GetWindsIconAsText()));
            StatItems.Add(new StatItemVM("Winds of Magic recharge rate: ", info.WindsOfMagicRechargeRate.ToString() + TOWCommon.GetWindsIconAsText() + "\\ hour"));
            string lorestext = "";
            for(int i = 0; i < info.KnownLores.Count; i++)
            {
                lorestext += info.KnownLores[i].Name;
                if (i != info.KnownLores.Count - 1) lorestext += ", ";
            }
            StatItems.Add(new StatItemVM("Known Magic Schools: ", lorestext));

            var lores = LoreObject.GetAll();
            foreach(var lore in lores)
            {
                LoreObjects.Add(new LoreObjectVM(this, lore, _currentHero));
            }
            CurrentLore = LoreObjects[0];
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
            CurrentLore.IsSelected = false;
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
                    base.OnPropertyChangedWithValue(value, "CurrentCharacter");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StatItemVM> StatItems
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
                    CurrentLore.IsSelected = true;
                }
            }
        }
    }
}
