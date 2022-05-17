using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
        private BasicTooltipViewModel _spellHint;
        private bool _isTrainerMode;
        private bool _canLearn = false;
        private string _learnText;

        public SpellItemVM(AbilityTemplate template, Hero currentHero, bool isTrainerMode = false)
        {
            _spellTemplate = template;
            _hero = currentHero;
            _isTrainerMode = isTrainerMode;
            SpellName = template.Name;
            SpellSpriteName = template.SpriteName;
            SpellStatItems = template.GetStats();
            SpellHint = new BasicTooltipViewModel(GetHintText);
            LearnText = "Learn " + _spellTemplate.GoldCost + "<img src=\"General\\Icons\\Coin@2x\"/>";
            RefreshValues();
        }

        private string GetHintText()
        {
            return _spellTemplate.TooltipDescription;
        }

        private void ExecuteLearnSpell()
        {
            // Deduct gold from the party leader if possible. Needed because
            // companions in a party do not actually own any gold.
            var sugarDaddy = _hero.IsPartyLeader ? _hero
                : _hero.PartyBelongedTo != null ? _hero.PartyBelongedTo.Owner
                    : _hero;
            if(sugarDaddy.Gold >= _spellTemplate.GoldCost)
            {
                sugarDaddy.ChangeHeroGold(-_spellTemplate.GoldCost);
                _hero.AddAbility(_spellTemplate.StringID);
                InformationManager.AddQuickInformation(new TextObject("Successfully learned spell: " + _spellTemplate.Name));
            }
            else
            {
                InformationManager.AddQuickInformation(new TextObject("Not enough gold"));
            }
            RefreshValues();
        }

        public override void RefreshValues()
        {
            IsKnown = _hero.HasAbility(_spellTemplate.StringID);
            IsDisabled = !IsKnown;
            if (IsDisabled)
            {
                var info = _hero.GetExtendedInfo();
                CanLearn = _isTrainerMode && _spellTemplate.SpellTier <= (int)info.SpellCastingLevel && _hero.HasKnownLore(_spellTemplate.BelongsToLoreID);
                if (!info.KnownLores.Any(x=>x.ID == _spellTemplate.BelongsToLoreID))
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
                    CanLearn = _isTrainerMode && _spellTemplate.SpellTier <= (int)info.SpellCastingLevel && _hero.HasKnownLore(_spellTemplate.BelongsToLoreID);
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

        [DataSourceProperty]
        public BasicTooltipViewModel SpellHint
        {
            get
            {
                return this._spellHint;
            }
            set
            {
                if (value != this._spellHint)
                {
                    this._spellHint = value;
                    base.OnPropertyChangedWithValue(value, "SpellHint");
                }
            }
        }

        [DataSourceProperty]
        public bool CanLearn
        {
            get
            {
                return this._canLearn;
            }
            set
            {
                if (value != this._canLearn)
                {
                    this._canLearn = value;
                    base.OnPropertyChangedWithValue(value, "CanLearn");
                }
            }
        }

        [DataSourceProperty]
        public string LearnText
        {
            get
            {
                return this._learnText;
            }
            set
            {
                if (value != this._learnText)
                {
                    this._learnText = value;
                    base.OnPropertyChangedWithValue(value, "LearnText");
                }
            }
        }
    }
}
