using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TOW_Core.Abilities.SpellBook
{
    public class LoreObjectVM : ViewModel
    {
        private string _name;
        private LoreObject _lore;
        private SpellBookVM _parent;
        private MBBindingList<SpellItemVM> _spells;
        private bool _isVisible;
        private bool _isSelected;
        private string _spriteName;

        public LoreObjectVM(SpellBookVM parent, LoreObject lore, Hero _currentHero)
        {
            _parent = parent;
            _lore = lore;
            _spells = new MBBindingList<SpellItemVM>();
            var spells = AbilityFactory.GetAllTemplates().Where(x => x.AbilityType == AbilityType.Spell && x.BelongsToLoreID == _lore.ID).OrderBy(x => (int)x.SpellTier);
            foreach(var spell in spells)
            {
                _spells.Add(new SpellItemVM(spell, _currentHero));
            }
            IsVisible = true;
            RefreshValues();
        }

        private void ExecuteSelectLoreObject()
        {
            _parent.OnLoreObjectSelected(this);
        }

        public override void RefreshValues()
        {
            Name = _lore.Name;
            SpriteName = _lore.SpriteName;
            base.RefreshValues();
        }

        [DataSourceProperty]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public string SpriteName
        {
            get
            {
                return this._spriteName;
            }
            set
            {
                if (value != this._spriteName)
                {
                    this._spriteName = value;
                    base.OnPropertyChangedWithValue(value, "SpriteName");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<SpellItemVM> SpellList
        {
            get
            {
                return this._spells;
            }
            set
            {
                if (value != this._spells)
                {
                    this._spells = value;
                    base.OnPropertyChangedWithValue(value, "SpellList");
                }
            }
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }
            set
            {
                if (value != this._isVisible)
                {
                    this._isVisible = value;
                    base.OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    base.OnPropertyChangedWithValue(value, "IsSelected");
                    IsVisible = !value;
                }
            }
        }
    }
}
