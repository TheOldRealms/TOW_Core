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

        public SpellBookVM(Action closeAction)
        {
            _closeAction = closeAction;
            _stats = new MBBindingList<SpellCastingStatItemVM>();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            CurrentCharacter = new HeroViewModel();
            CurrentCharacter.FillFrom(Hero.MainHero);
            CurrentCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default(EquipmentElement));
            CurrentCharacter.SetEquipment(EquipmentIndex.HorseHarness, default(EquipmentElement));
            CurrentCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default(EquipmentElement));
            StatItems.Clear();
            var info = Hero.MainHero.GetExtendedInfo();
            StatItems.Add(new SpellCastingStatItemVM("Hero name: ", Hero.MainHero.Name.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Spell casting level: ", info.SpellCastingLevel.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Maximum Winds of Magic: ", info.MaxWindsOfMagic.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Current Winds of Magic: ", info.CurrentWindsOfMagic.ToString()));
            StatItems.Add(new SpellCastingStatItemVM("Winds of Magic recharge rate: ", info.WindsOfMagicRechargeRate.ToString()));
            //StatItems.Add(new SpellCastingStatItemVM("Known Lores of Magic: ", Hero.MainHero.Name.ToString()));
        }

        private void ExecuteClose()
        {
            _closeAction();
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
    }
}
