using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellBookMapIconVM : ViewModel
    {
        private BasicTooltipViewModel _hint;
        private bool _isVisible;

        public SpellBookMapIconVM()
        {
            IconHint = new BasicTooltipViewModel(delegate ()
            {
                return "Open Spell Book";
            });
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            if(Hero.MainHero.IsSpellCaster()) IsVisible = true;
        }

        public void ExecuteOpenSpellBook()
        {
            var state = Game.Current.GameStateManager.CreateState<SpellBookState>();
            Game.Current.GameStateManager.PushState(state);
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    base.OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel IconHint
        {
            get
            {
                return _hint;
            }
            set
            {
                if (value != _hint)
                {
                    _hint = value;
                    base.OnPropertyChangedWithValue(value, "IconHint");
                }
            }
        }
    }
}
