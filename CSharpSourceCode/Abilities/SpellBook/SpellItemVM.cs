using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellItemVM : ViewModel
    {
        private string _spellSpriteName;
        private string _spellName;
        private Hero _hero;
        private AbilityTemplate _spellTemplate;
        private MBBindingList<StatItemVM> _statItems;
        private bool _isDisabled;
        private bool _isKnown;
        private string _disabledReason;

        public SpellItemVM(AbilityTemplate template, Hero currentHero)
        {
            _spellTemplate = template;
            _hero = currentHero;
            SpellName = template.Name;
            SpellSpriteName = template.SpriteName;
            SpellStatItems = template.GetStats();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            IsKnown = _hero.HasAbility(_spellTemplate.StringID);
            IsDisabled = !IsKnown;
            if (IsDisabled)
            {
                var info = _hero.GetExtendedInfo();
                if(!info.KnownLores.Any(x=>x.ID == _spellTemplate.BelongsToLoreID))
                {
                    DisabledReason = "Unfamiliar lore";
                }
                else if(_spellTemplate.SpellTier > (int)info.SpellCastingLevel)
                {
                    DisabledReason = "Insufficient caster level";
                }
                else
                {
                    DisabledReason = "Can learn";
                }
            }
            base.RefreshValues();
        }

        [DataSourceProperty]
        public string SpellName
        {
            get
            {
                return this._spellName;
            }
            set
            {
                if (value != this._spellName)
                {
                    this._spellName = value;
                    base.OnPropertyChangedWithValue(value, "SpellName");
                }
            }
        }

        [DataSourceProperty]
        public string SpellSpriteName
        {
            get
            {
                return this._spellSpriteName;
            }
            set
            {
                if (value != this._spellSpriteName)
                {
                    this._spellSpriteName = value;
                    base.OnPropertyChangedWithValue(value, "SpellSpriteName");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StatItemVM> SpellStatItems
        {
            get
            {
                return this._statItems;
            }
            set
            {
                if (value != this._statItems)
                {
                    this._statItems = value;
                    base.OnPropertyChangedWithValue(value, "SpellStatItems");
                }
            }
        }

        [DataSourceProperty]
        public bool IsDisabled
        {
            get
            {
                return this._isDisabled;
            }
            set
            {
                if (value != this._isDisabled)
                {
                    this._isDisabled = value;
                    base.OnPropertyChangedWithValue(value, "IsDisabled");
                }
            }
        }

        [DataSourceProperty]
        public bool IsKnown
        {
            get
            {
                return this._isKnown;
            }
            set
            {
                if (value != this._isKnown)
                {
                    this._isKnown = value;
                    base.OnPropertyChangedWithValue(value, "IsKnown");
                }
            }
        }

        [DataSourceProperty]
        public string DisabledReason
        {
            get
            {
                return this._disabledReason;
            }
            set
            {
                if (value != this._disabledReason)
                {
                    this._disabledReason = value;
                    base.OnPropertyChangedWithValue(value, "DisabledReason");
                }
            }
        }
    }
}
