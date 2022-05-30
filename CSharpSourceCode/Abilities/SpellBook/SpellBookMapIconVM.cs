using TaleWorlds.CampaignSystem.Party;
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
            RefreshValues();
        }

        public void ExecuteOpenSpellBook()
        {
            var state = Game.Current.GameStateManager.CreateState<SpellBookState>();
            Game.Current.GameStateManager.PushState(state);
        }

        public override void RefreshValues()
        {
            if (MobileParty.MainParty.HasSpellCasterMember()) IsVisible = true;
            else IsVisible = false;
            base.RefreshValues();
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
