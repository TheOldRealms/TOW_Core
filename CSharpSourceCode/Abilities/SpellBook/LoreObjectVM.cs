using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace TOW_Core.Abilities.SpellBook
{
    public class LoreObjectVM : ViewModel
    {
        private string _name;
        private string _symbolIconName;
        private LoreObject _lore;
        private SpellBookVM _parent;
        private MBBindingList<SpellItemVM> _spells;

        public LoreObjectVM(SpellBookVM parent, LoreObject lore)
        {
            _parent = parent;
            _lore = lore;
            Name = _lore.Name;
            SymbolIconName = lore.IconSprite;
        }

        private void ExecuteSelectLoreObject()
        {
            _parent.OnLoreObjectSelected(this);
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
        public string SymbolIconName
        {
            get
            {
                return this._symbolIconName;
            }
            set
            {
                if (value != this._symbolIconName)
                {
                    this._symbolIconName = value;
                    base.OnPropertyChangedWithValue(value, "SymbolIconName");
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
    }
}
